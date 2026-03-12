# 1. Base runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
LABEL service="competitions"
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# 2. Build stage - Use --platform=$BUILDPLATFORM to run SDK natively on the runner
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
ARG BUILD_CONFIGURATION=Release
ARG GITLAB_TOKEN
WORKDIR /src

# Copy project file + NuGet config, authenticate, then restore
# (nuget.config defines the gitlab source URL; we inject credentials here)
COPY ["competitions.csproj", "nuget.config", "./"]
RUN dotnet nuget update source gitlab \
        --username gitlab-ci-token \
        --password "$GITLAB_TOKEN" \
        --store-password-in-clear-text
RUN dotnet restore "competitions.csproj" -a $TARGETARCH

# Copy the rest of the source
COPY . .

# Build for the target architecture without using the emulator
RUN dotnet build "competitions.csproj" -c $BUILD_CONFIGURATION -a $TARGETARCH -o /app/build

# 3. Publish stage
FROM build AS publish
# -a $TARGETARCH ensures the published output is for the correct CPU
RUN dotnet publish "competitions.csproj" -c $BUILD_CONFIGURATION -a $TARGETARCH -o /app/publish /p:UseAppHost=false

# 4. Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "competitions.dll"]