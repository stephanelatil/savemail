# syntax= docker/dockerfile:1.7-labs

# Backend Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the backend code and build
COPY Controllers .
COPY Migrations .
COPY Models .
COPY Properties .
COPY Services .
COPY Utils .
COPY Program.cs .
RUN dotnet publish -c Release -o out

# Production stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS production
WORKDIR /app

# Copy built artifacts from build stage
COPY --from=build --exclude=**/appsettings*.json /app/out .

RUN apt-get update \
    && apt-get install -y curl

COPY start.sh .
COPY health.sh .

ENV PORT=5000
ENV HOST=localhost

EXPOSE $PORT

# Set the environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:${PORT}
ENV ASPNETCORE_HTTP_PORTS=${PORT}
ENV SAVEMAIL__Logging__LogLevel__Microsoft.EntityFrameworkCore.Database.Command=Warning
ENV SAVEMAIL__Logging__LogLevel__Microsoft.AspNetCore=Warning
ENV SAVEMAIL__Logging__LogLevel__Default=Information
ENV SAVEMAIL__AppSecret=123456789
ENV SAVEMAIL__AttachmentsPath="./Attachments"

HEALTHCHECK --start-period=15s --start-interval=3s --interval=60s CMD /app/health.sh

# Start the application
ENTRYPOINT ["./start.sh"]