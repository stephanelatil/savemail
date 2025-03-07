# syntax= docker/dockerfile:1.7-labs

# Backend Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the backend code and build
COPY . ./
RUN dotnet publish -c Release -o out

# Production stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS production
WORKDIR /app

# Copy built artifacts from build stage
COPY --from=build --exclude=**/appsettings*.json /app/out .

RUN apt-get update \
    && apt-get install -y curl

ENV PORT=5000
ENV HOSTNAME=localhost

# Set the environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://${HOSTNAME}:${PORT}
ENV Logging__LogLevel__Microsoft.EntityFrameworkCore.Database.Command=Warning
ENV Logging__LogLevel__Microsoft.AspNetCore=Warning
ENV Logging__LogLevel__Default=Information
ENV AppSecret=123456789
ENV AttachmentsPath="./Attachments"

HEALTHCHECK --start-period=15s --start-interval=3s --interval=60s CMD /usr/bin/curl http://localhost:5000/status || exit 1

# Expose the port the app runs on
EXPOSE ${PORT}

# Start the application
ENTRYPOINT ["dotnet", "Backend.dll"]