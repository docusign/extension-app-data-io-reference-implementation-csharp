data "aws_region" "current" {
  count = var.region == null ? 1 : 0
}
data "aws_caller_identity" "current" {}
data "aws_ecr_authorization_token" "current" {}

locals {
  region = var.region != null ? var.region : data.aws_region.current[0].name

  application_jwt_secret_key      = var.application_jwt_secret_key != "" ? var.application_jwt_secret_key : module.generate_jwt_secret_key[0].random_bytes
  application_oauth_client_id     = var.application_oauth_client_id != "" ? var.application_oauth_client_id : module.generate_oauth_client_id[0].random_bytes
  application_oauth_client_secret = var.application_oauth_client_secret != "" ? var.application_oauth_client_secret : module.generate_oauth_client_secret[0].random_bytes
  application_authorization_code  = var.application_authorization_code != "" ? var.application_authorization_code : module.generate_authorization_code[0].random_bytes

  file_path_separator = "/"

  output_manifest_files_directory = abspath(join(local.file_path_separator, compact([path.cwd, var.output_manifest_files_directory])))
  output_manifest_files_paths     = module.manifest[*].output_file_path

  tags = merge(
    {
      application = var.application_name
    },
    var.tags
  )
}

module "generate_jwt_secret_key" {
  count = var.application_jwt_secret_key == "" ? 1 : 0

  source = "../common/modules/generate"
}

module "generate_oauth_client_id" {
  count = var.application_oauth_client_id == "" ? 1 : 0

  source = "../common/modules/generate"
}

module "generate_oauth_client_secret" {
  count = var.application_oauth_client_secret == "" ? 1 : 0

  source = "../common/modules/generate"
}

module "generate_authorization_code" {
  count = var.application_authorization_code == "" ? 1 : 0

  source = "../common/modules/generate"
}

module "manifest" {
  count = length(var.manifest_files_paths)

  source = "../common/modules/template"

  input_file_path  = abspath(join(local.file_path_separator, [path.cwd, var.manifest_files_paths[count.index]]))
  output_file_path = join(local.file_path_separator, [
    local.output_manifest_files_directory,
    "${basename(dirname(var.manifest_files_paths[count.index]))}.${basename(var.manifest_files_paths[count.index])}"
  ])
  
  client_id     = local.application_oauth_client_id
  client_secret = local.application_oauth_client_secret
  base_url      = local.application_service_url
}
