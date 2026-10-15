# Alpine keeps the first Docker build smaller while remaining an official .NET image.
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY TaskTracker.Api/TaskTracker.Api.csproj TaskTracker.Api/
RUN dotnet restore TaskTracker.Api/TaskTracker.Api.csproj

COPY TaskTracker.Api/ TaskTracker.Api/
WORKDIR /src/TaskTracker.Api
RUN dotnet publish TaskTracker.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskTracker.Api.dll"]
