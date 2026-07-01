locals {
  application_build_context_requested = (
    startswith(var.application_build_context, "/") ||
    can(regex("^[A-Za-z]:[/\\\\]", var.application_build_context))
  ) ? var.application_build_context : abspath(join(local.file_path_separator, compact([path.cwd, var.application_build_context])))
  # On Windows, Docker provider can mis-handle '#' in paths. Prefer a safe junction under the detected workspace root when available.
  application_build_context_safe = join(local.file_path_separator, [local.application_build_context_requested, "extappdataio-safe"])
  application_build_context = fileexists(join(local.file_path_separator, [local.application_build_context_safe, "ExtensionAppDataIO", "Containerfile"])) ? local.application_build_context_safe : local.application_build_context_requested
  application_runtime_image = var.application_build_base_image_name != "" ? var.application_build_base_image_name : var.application_build_runtime_image_name

  application_image_name = join(local.docker_registry_separator, compact([
    local.artifact_registry_login_server,
    local.artifact_registry_project,
    local.artifact_registry_repository,
    join(local.docker_image_tag_separator, compact([
      var.application_name,
      var.application_build_image_tag
    ])),
  ]))

  application_image_name_without_registry = trimprefix(
    module.image.app_image_name,
    join("", compact([
      local.artifact_registry_login_server,
      local.docker_registry_separator,
    ]))
  )

  application_build_arguments = {
    PORT                = tostring(var.application_port)
    SDK_IMAGE           = var.application_build_sdk_image_name
    RUNTIME_IMAGE       = local.application_runtime_image
    PROJECT_CONTEXT_DIR = "ExtensionAppDataIO"
    PROJECT_FILE        = "ExtensionAppDataIO.csproj"
    PROJECT_NAME        = "ExtensionAppDataIO"
  }

  application_build_dockerfile = {
    docker = "ExtensionAppDataIO/Containerfile"
    podman = "ExtensionAppDataIO/Containerfile"
  }
}

resource "local_file" "docker_config" {
  filename = "${path.module}/.docker/config.json"
  content = jsonencode({
    auths = {
      (local.artifact_registry_url) = {
        auth = base64encode("_json_key_base64:${local.application_service_account_private_key}")
      }
    }
  })
}

module "image" {
  source = "../common/modules/docker"

  base_image_name = local.application_runtime_image

  app_image_name               = local.application_image_name
  app_image_build_context      = local.application_build_context
  app_image_build_dockerfile   = lookup(local.application_build_dockerfile, var.container_tool, null)
  app_image_build_target_stage = var.application_environment_mode
  app_image_build_paths        = var.application_build_paths
  app_image_build_args         = local.application_build_arguments
  app_image_build_labels       = var.application_build_labels

  do_push_app_image = false
}

resource "terraform_data" "login_container_registry" {
  input = {
    container_registry_login_server = local.artifact_registry_login_server

    docker_config = dirname(local_file.docker_config.filename)
  }

  triggers_replace = [
    local_file.docker_config.id,
    google_artifact_registry_repository.this.id,
    google_artifact_registry_repository_iam_binding.readers.etag,
    google_artifact_registry_repository_iam_binding.writers.etag,
  ]

}


resource "terraform_data" "push_docker_image" {
  input = {
    application_image_name = join(local.docker_registry_separator, compact([
      terraform_data.login_container_registry.output.container_registry_login_server,
      local.application_image_name_without_registry,
    ]))

    container_tool = var.container_tool
    docker_config  = terraform_data.login_container_registry.output.docker_config
  }

  triggers_replace = [
    terraform_data.login_container_registry.id,
    module.image.app_image_repo_digest,
  ]

  provisioner "local-exec" {
    environment = {
      DOCKER_CONFIG = self.input.docker_config
    }
    command = "${self.input.container_tool} push ${self.input.application_image_name}"
  }

  provisioner "local-exec" {
    when       = destroy
    on_failure = continue
    command    = "gcloud artifacts docker images delete ${self.input.application_image_name} --quiet"
  }
}
