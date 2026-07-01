# Terraform configuration for deploying the C# reference implementation to Google Cloud

This Terraform root deploys the C# Data IO reference implementation to Google Cloud Run. It builds the application image from `ExtensionAppDataIO/Containerfile`, pushes it to Artifact Registry, provisions a Cloud Run service, and renders hosted manifest files into `.terraform/` for later upload to the DocuSign Developer Console.

## Specific cloud prerequisites

Before deploying on Google Cloud, complete the following setup:

1. Sign in to a Google Cloud project that can create Cloud Run and Artifact Registry resources.
1. Install and initialize the Google Cloud CLI.
1. Authenticate locally:

   ```sh
   gcloud auth login
   gcloud auth application-default login
   gcloud config set project <YOUR_PROJECT_ID>
   ```

1. Install Terraform.
1. Install and start Docker so Terraform can build and push the application image.

If you prefer Podman, expose a Docker-compatible API endpoint and pass that endpoint through the `docker_host` Terraform input. This configuration still uses the Docker provider and related Docker resources, so Podman is not a drop-in local runtime.

## What this configuration deploys

The GCP Terraform root provisions:

- A Google Artifact Registry Docker repository for the application image
- A Google Cloud Run service for the C# web application
- Generated hosted manifest files written into `.terraform/`

The containerized application listens on port `8080` inside the container. Cloud Run is configured with `ASPNETCORE_URLS=http://+:8080` so the deployed service binds to the expected port.

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

If you do not provide explicit values for these variables, Terraform generates them for the Cloud Run runtime configuration.

The generated hosted manifest files are templated separately. They only substitute these hosted manifest placeholders:

- `CLIENT_ID`
- `CLIENT_SECRET`
- `PROXY_BASE_URL`

## Deploying

From the C# repository root:

```sh
cd terraform/gcp
terraform init -upgrade
terraform plan -out app.tfplan
terraform apply app.tfplan
```

Use `terraform output` after apply to retrieve:

- `application_service_url`
- `output_manifest_files_paths`

Upload the generated hosted manifest files from `.terraform/` to the DocuSign Developer Console.
