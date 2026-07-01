# Terraform configuration for deploying the C# reference implementation to AWS

This Terraform root deploys the C# Data IO reference implementation to AWS App Runner. It builds the application image from `ExtensionAppDataIO/Containerfile`, pushes it to Amazon ECR, provisions the App Runner service, and renders hosted manifest files into `.terraform/` for later upload to the DocuSign Developer Console.

## Specific cloud prerequisites

Before deploying on AWS, complete the following setup:

1. Sign in to an AWS account that can create App Runner, ECR, and IAM resources.
1. Install the AWS CLI.
1. Configure AWS credentials locally:

   ```sh
   aws configure
   ```

1. Install Terraform.
1. Install and start Docker so Terraform can build and push the application image.

If you prefer Podman, expose a Docker-compatible API endpoint and pass that endpoint through the `docker_host` Terraform input. This configuration still uses the Docker provider and related Docker resources, so Podman is not a drop-in local runtime.

The Terraform AWS provider can use the shared AWS credentials created by `aws configure` or any other supported AWS authentication method.

## What this configuration deploys

The AWS Terraform root provisions:

- An Amazon ECR repository for the application image
- An AWS App Runner service for the C# web application
- Generated hosted manifest files written into `.terraform/`

The containerized application listens on port `8080` inside the container. App Runner is configured with `ASPNETCORE_URLS=http://+:8080` so the deployed service binds to the expected port.

## Application sources used by Terraform

Terraform builds from the repository root and uses `ExtensionAppDataIO/Containerfile` as the container build file.

The generated hosted manifests are based on these source manifest trees:

- `manifests/authorizationCode/ReadOnlyManifest.json`
- `manifests/authorizationCode/ReadWriteManifest.json`
- `manifests/clientCredentials/ReadOnlyManifest.json`
- `manifests/clientCredentials/ReadWriteManifest.json`

During `terraform apply`, the rendered hosted manifests are emitted into `.terraform/` with filenames that include the manifest source directory name.

## Runtime configuration injected by Terraform

Terraform injects the ASP.NET Core authentication settings that the C# app binds from configuration:

- `AuthSettings__JwtSecretKey`
- `AuthSettings__OAuthClientId`
- `AuthSettings__OAuthClientSecret`
- `AuthSettings__AuthorizationCode`

If you do not provide explicit values for these variables, Terraform generates them for the App Runner runtime configuration.

The generated hosted manifest files are templated separately. They only substitute these hosted manifest placeholders:

- `CLIENT_ID`
- `CLIENT_SECRET`
- `PROXY_BASE_URL`

The hosted manifest outputs do not receive `AuthSettings__JwtSecretKey` or `AuthSettings__AuthorizationCode` values directly.

## Deploying

From the C# repository root:

```sh
cd terraform/aws
terraform init -upgrade
terraform plan -out app.tfplan
terraform apply app.tfplan
```

Use `terraform output` after apply to retrieve:

- `application_service_url`
- `output_manifest_files_paths`

Upload the generated hosted manifest files from `.terraform/` to the DocuSign Developer Console.

<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 1.0.0, < 2.0.0 |
| <a name="requirement_aws"></a> [aws](#requirement\_aws) | ~> 5.0 |
| <a name="requirement_docker"></a> [docker](#requirement\_docker) | ~> 3.0 |
| <a name="requirement_local"></a> [local](#requirement\_local) | ~> 2.5 |
| <a name="requirement_random"></a> [random](#requirement\_random) | ~> 3.6 |
| <a name="requirement_time"></a> [time](#requirement\_time) | ~> 0.12 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_aws"></a> [aws](#provider\_aws) | ~> 5.0 |
| <a name="provider_time"></a> [time](#provider\_time) | ~> 0.12 |

## Modules

| Name | Source | Version |
|------|--------|---------|
| <a name="module_generate_authorization_code"></a> [generate\_authorization\_code](#module\_generate\_authorization\_code) | ../common/modules/generate | n/a |
| <a name="module_generate_jwt_secret_key"></a> [generate\_jwt\_secret\_key](#module\_generate\_jwt\_secret\_key) | ../common/modules/generate | n/a |
| <a name="module_generate_oauth_client_id"></a> [generate\_oauth\_client\_id](#module\_generate\_oauth\_client\_id) | ../common/modules/generate | n/a |
| <a name="module_generate_oauth_client_secret"></a> [generate\_oauth\_client\_secret](#module\_generate\_oauth\_client\_secret) | ../common/modules/generate | n/a |
| <a name="module_image"></a> [image](#module\_image) | ../common/modules/docker | n/a |
| <a name="module_manifest"></a> [manifest](#module\_manifest) | ../common/modules/template | n/a |

## Resources

| Name | Type |
|------|------|
| [aws_apprunner_service.this](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/apprunner_service) | resource |
| [aws_ecr_repository.this](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/ecr_repository) | resource |
| [aws_ecr_repository_policy.this](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/ecr_repository_policy) | resource |
| [aws_iam_role.access](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/iam_role) | resource |
| [aws_iam_role.instance](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/iam_role) | resource |
| [aws_iam_role_policy_attachment.apprunner](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/resources/iam_role_policy_attachment) | resource |
| [time_sleep.access_iam_role_propagation](https://registry.terraform.io/providers/hashicorp/time/latest/docs/resources/sleep) | resource |
| [aws_caller_identity.current](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/caller_identity) | data source |
| [aws_ecr_authorization_token.current](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/ecr_authorization_token) | data source |
| [aws_iam_policy_document.app_role_assume_role_policy](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/iam_policy_document) | data source |
| [aws_iam_policy_document.apprunner](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/iam_policy_document) | data source |
| [aws_iam_policy_document.ecr](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/iam_policy_document) | data source |
| [aws_region.current](https://registry.terraform.io/providers/hashicorp/aws/latest/docs/data-sources/region) | data source |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_application_authorization_code"></a> [application\_authorization\_code](#input\_application\_authorization\_code) | The authorization code for the application. If empty, a random code will be generated. | `string` | `""` | no |
| <a name="input_application_build_base_image_name"></a> [application\_build\_base\_image\_name](#input\_application\_build\_base\_image\_name) | Deprecated compatibility alias for the runtime base image. If non-empty, it overrides application_build_runtime_image_name. | `string` | `""` | no |
| <a name="input_application_build_context"></a> [application\_build\_context](#input\_application\_build\_context) | The relative path to the build context for the application. The build context is the directory from which the Dockerfile is read. If it is empty the current working directory will be used. | `string` | `"../.."` | no |
| <a name="input_application_build_image_tag"></a> [application\_build\_image\_tag](#input\_application\_build\_image\_tag) | The tag to apply to the application build image. If empty the timestamp tag will be used. | `string` | `""` | no |
| <a name="input_application_build_labels"></a> [application\_build\_labels](#input\_application\_build\_labels) | The labels to apply to the application build image | `map(string)` | <pre>{<br/>  "org.opencontainers.image.authors": "DocuSign Inc.",<br/>  "org.opencontainers.image.description": "C# reference implementation for data input and output extension app workflows.",<br/>  "org.opencontainers.image.licenses": "MIT",<br/>  "org.opencontainers.image.source": "https://github.com/docusign/extension-app-data-io-reference-implementation-csharp",<br/>  "org.opencontainers.image.title": "Data IO Extension App Reference Implementation (C#)",<br/>  "org.opencontainers.image.vendor": "DocuSign Inc."<br/>}</pre> | no |
| <a name="input_application_build_paths"></a> [application\_build\_paths](#input\_application\_build\_paths) | Paths of files relative to the build context, changes to which lead to a rebuild of the image. Supported pattern matches are the same as for the `fileset` Terraform function (https://developer.hashicorp.com/terraform/language/functions/fileset). | `list(string)` | <pre>[<br/>  ".dockerignore",<br/>  "ExtensionAppDataIO/**/*.cs",<br/>  "ExtensionAppDataIO/**/*.cshtml",<br/>  "ExtensionAppDataIO/**/*.json",<br/>  "ExtensionAppDataIO/**/*.http",<br/>  "ExtensionAppDataIO/Controllers/**",<br/>  "ExtensionAppDataIO/Data/**",<br/>  "ExtensionAppDataIO/DataModels/**",<br/>  "ExtensionAppDataIO/Models/**",<br/>  "ExtensionAppDataIO/Properties/**",<br/>  "ExtensionAppDataIO/Services/**",<br/>  "ExtensionAppDataIO/Views/**",<br/>  "ExtensionAppDataIO/wwwroot/**",<br/>  "ExtensionAppDataIO/ExtensionAppDataIO.csproj",<br/>  "ExtensionAppDataIO/Containerfile",<br/>  "manifests/**"<br/>]</pre> | no |
| <a name="input_application_build_runtime_image_name"></a> [application\_build\_runtime\_image\_name](#input\_application\_build\_runtime\_image\_name) | The runtime image to use for the final application container stage | `string` | `"mcr.microsoft.com/dotnet/aspnet:10.0"` | no |
| <a name="input_application_build_sdk_image_name"></a> [application\_build\_sdk\_image\_name](#input\_application\_build\_sdk\_image\_name) | The SDK image to use for the application build stages | `string` | `"mcr.microsoft.com/dotnet/sdk:10.0"` | no |
| <a name="input_application_environment_mode"></a> [application\_environment\_mode](#input\_application\_environment\_mode) | The environment mode for the application | `string` | `"production"` | no |
| <a name="input_application_instance_cpu"></a> [application\_instance\_cpu](#input\_application\_instance\_cpu) | The number of CPU units to allocate to the application instance | `string` | `"256"` | no |
| <a name="input_application_instance_memory"></a> [application\_instance\_memory](#input\_application\_instance\_memory) | The amount of memory to allocate to the application instance | `string` | `"512"` | no |
| <a name="input_application_jwt_secret_key"></a> [application\_jwt\_secret\_key](#input\_application\_jwt\_secret\_key) | The secret key to use for signing JWT tokens. If empty, a random key will be generated. | `string` | `""` | no |
| <a name="input_application_name"></a> [application\_name](#input\_application\_name) | The name of the application | `string` | `"extension-app-data-io-cs"` | no |
| <a name="input_application_oauth_client_id"></a> [application\_oauth\_client\_id](#input\_application\_oauth\_client\_id) | The OAuth client ID for the application. If empty, a random client ID will be generated. | `string` | `""` | no |
| <a name="input_application_oauth_client_secret"></a> [application\_oauth\_client\_secret](#input\_application\_oauth\_client\_secret) | The OAuth client secret for the application. If empty, a random client secret will be generated. | `string` | `""` | no |
| <a name="input_application_port"></a> [application\_port](#input\_application\_port) | The port the application listens on | `number` | `8080` | no |
| <a name="input_container_tool"></a> [container\_tool](#input\_container\_tool) | The container tool to use for building and pushing images | `string` | `"docker"` | no |
| <a name="input_do_force_delete_repository"></a> [do\_force\_delete\_repository](#input\_do\_force\_delete\_repository) | Whether to delete the ECR repository even if it contains images | `bool` | `true` | no |
| <a name="input_do_scan_images"></a> [do\_scan\_images](#input\_do\_scan\_images) | Whether images are scanned after being pushed to the ECR repository | `bool` | `true` | no |
| <a name="input_docker_host"></a> [docker\_host](#input\_docker\_host) | The Docker host (e.g. 'tcp://127.0.0.1:2376' or 'unix:///var/run/docker.sock') to connect to. If empty, the default Docker host will be used | `string` | `null` | no |
| <a name="input_manifest_files_paths"></a> [manifest\_files\_paths](#input\_manifest\_files\_paths) | The list of manifest files relative paths to generate | `list(string)` | <pre>[<br/>  "../../manifests/authorizationCode/ReadOnlyManifest.json",<br/>  "../../manifests/authorizationCode/ReadWriteManifest.json",<br/>  "../../manifests/clientCredentials/ReadOnlyManifest.json",<br/>  "../../manifests/clientCredentials/ReadWriteManifest.json"<br/>]</pre> | no |
| <a name="input_output_manifest_files_directory"></a> [output\_manifest\_files\_directory](#input\_output\_manifest\_files\_directory) | The directory to output the generated manifest files | `string` | `".terraform"` | no |
| <a name="input_region"></a> [region](#input\_region) | The AWS region | `string` | `"us-east-1"` | no |
| <a name="input_repository_image_tag_mutability"></a> [repository\_image\_tag\_mutability](#input\_repository\_image\_tag\_mutability) | The image tag mutability setting for the ECR repository | `string` | `"MUTABLE"` | no |
| <a name="input_tags"></a> [tags](#input\_tags) | A map of the tags to apply to various resources | `map(string)` | `{}` | no |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_application_service_url"></a> [application\_service\_url](#output\_application\_service\_url) | The base URL of the application service |
| <a name="output_output_manifest_files_paths"></a> [output\_manifest\_files\_paths](#output\_output\_manifest\_files\_paths) | The absolute paths to the output manifest files |
<!-- END_TF_DOCS -->
