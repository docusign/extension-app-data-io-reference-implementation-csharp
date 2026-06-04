# Global arguments
ARG SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:10.0
ARG RUNTIME_IMAGE=mcr.microsoft.com/dotnet/aspnet:10.0
ARG APP_DIR=/app
ARG SRC_DIR=/src
ARG UID=1000
ARG GID=1000
ARG PORT=8080
ARG BUILD_CONFIGURATION=Release
ARG PROJECT_NAME=ExtensionAppDataIO
ARG PROJECT_FILE=ExtensionAppDataIO.csproj
ARG PROJECT_CONTEXT_DIR=ExtensionAppDataIO

# Base stage
FROM ${RUNTIME_IMAGE} AS base
ARG APP_DIR
ARG UID
ARG GID
ARG PORT

USER root
RUN mkdir -p ${APP_DIR} && \
	chown -R ${UID}:${GID} ${APP_DIR}

USER ${UID}:${GID}
WORKDIR ${APP_DIR}

ENV PORT=${PORT}
ENV ASPNETCORE_HTTP_PORTS=${PORT}
EXPOSE ${PORT}

# Dependencies stage
FROM ${SDK_IMAGE} AS dependencies
ARG SRC_DIR
ARG UID
ARG GID
ARG PROJECT_NAME
ARG PROJECT_FILE
ARG PROJECT_CONTEXT_DIR

USER root
RUN mkdir -p ${SRC_DIR}/${PROJECT_NAME} /tmp/nuget && \
	chown -R ${UID}:${GID} ${SRC_DIR} /tmp/nuget

USER ${UID}:${GID}
WORKDIR ${SRC_DIR}

COPY --chown=${UID}:${GID} ${PROJECT_CONTEXT_DIR}/${PROJECT_FILE} ./${PROJECT_NAME}/

RUN dotnet restore ./${PROJECT_NAME}/${PROJECT_FILE} --packages /tmp/nuget

ENV NUGET_PACKAGES=/tmp/nuget

# Development stage
FROM dependencies AS development
ARG SRC_DIR
ARG UID
ARG GID
ARG PROJECT_NAME
ARG PROJECT_FILE
ARG PORT
ARG PROJECT_CONTEXT_DIR

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:${PORT}

COPY --chown=${UID}:${GID} ${PROJECT_CONTEXT_DIR}/ ${SRC_DIR}/${PROJECT_NAME}/
WORKDIR ${SRC_DIR}/${PROJECT_NAME}

CMD ["dotnet", "watch", "run", "--project", "ExtensionAppDataIO.csproj", "--no-launch-profile", "--urls", "http://+:8080"]

# Build stage
FROM dependencies AS build
ARG SRC_DIR
ARG UID
ARG GID
ARG PROJECT_NAME
ARG PROJECT_FILE
ARG BUILD_CONFIGURATION
ARG PROJECT_CONTEXT_DIR

COPY --chown=${UID}:${GID} ${PROJECT_CONTEXT_DIR}/ ${SRC_DIR}/${PROJECT_NAME}/
WORKDIR ${SRC_DIR}/${PROJECT_NAME}

RUN dotnet publish ./${PROJECT_FILE} -c ${BUILD_CONFIGURATION} -o ${SRC_DIR}/publish /p:UseAppHost=false --no-restore

# Production stage
FROM base AS production
ARG UID
ARG GID

ENV ASPNETCORE_ENVIRONMENT=Production

COPY --chown=${UID}:${GID} --from=build /src/publish .

ENTRYPOINT ["dotnet", "ExtensionAppDataIO.dll"]