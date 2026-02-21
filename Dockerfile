# 1. Base runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
LABEL service="competitions"
USER $APP_UID
WORKDIR /app
EXPOSE 3050

# 2. Build stage - Use --platform=$BUILDPLATFORM to run SDK natively on the runner
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy and restore specifically for the target architecture (-a $TARGETARCH)
COPY ["competitions.csproj", "./"]
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