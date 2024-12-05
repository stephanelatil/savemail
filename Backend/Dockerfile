# Backend Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the backend code and build
COPY . ./
RUN dotnet publish -c Release -o out

# Production stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS production
WORKDIR /app

# Copy built artifacts from build stage
COPY --from=build /app/out .

# Expose the port the app runs on
EXPOSE 5000

# Set the environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

# Start the application
ENTRYPOINT ["dotnet", "Backend.dll"]