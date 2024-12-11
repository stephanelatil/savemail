# SaveMail

![SaveMail Icon](https://raw.githubusercontent.com/stephanelatil/savemail/main/img/Logo.png?raw=true "Savemail")

SaveMail is a self-hosted web application for storing and archiving emails locally. Designed for convenience and security, it supports multi-user access and comes with a responsive web interface. Built on a modern tech stack, it leverages **ASP.NET Core** for the backend REST API and **Next.js** for the WebUI.

## Table of Contents

- [Features](#features)
- [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Installation](#installation)
        - [Backend](#backend)
        - [Frontend](#frontend)
    - [Docker Compose](#docker-compose)
- [API Documentation](#api-documentation)
- [Tech Stack](#tech-stack)
- [Contributing](#contributing)
- [License](#license)
- [Support](#support)

## Features

- **Email Retention**: Archive emails locally, even if deleted from the IMAP server.
- **Multi-user Support**: Individualized user accounts with secure, isolated mailboxes.
- **Modern Web Interface**: Accessible via any modern browser; supports dark and light modes.
- **API Integration**: Full-featured RESTful API for custom integrations.
- **Docker Support**: Simplified deployment with pre-built containers.
- **Secure by Design**: Built-in 2FA and ASP.NET Identity for authentication.

## Getting Started

Using the Docker deployment is **strongly** recommended. Information on the docker deployment is available in the [Docker Deployment](#docker-deployment) section. Otherwise you can run the services individually yourself with the instructions below.

### Prerequisites

Ensure you have the following installed before proceeding or use the [docker container deployment](#docker-deployment):

- **Node.js** (v14.x or later)
- **.NET Core SDK** (v8.0 or later)
- **PostgreSQL**

### Installation

#### Backend

1. **Clone the repository**:
   ```bash
   git clone https://github.com/stephanelatil/savemail.git
   cd savemail
   ```

2. **Navigate to the backend directory**:
   ```bash
   cd Backend
   ```

3. **Restore and publish**:
   ```bash
   dotnet restore
   dotnet publish -c Release
   ```

4. **Database setup**:
   Ensure PostgreSQL is running and configured. Update connection details in `appsettings.json` or via environment variables. Initialize the database:
   ```bash
   dotnet ef database update
   ```

5. **Run the application with the correct environment variables**:
   ```bash
    ASPNETCORE_ENVIRONMENT=Production ASPNETCORE_URLS='http://+:5000' dotnet ./bin/Release/net8.0/Backend.dll
   ```

#### Frontend

1. **Navigate to the frontend directory**:
   ```bash
   cd ../Frontend
   ```

2. **Install dependencies**:
   ```bash
   npm install
   npm install next
   ```

3. **Run the production server**:
   ```bash
   npn run start
   ```

   Access the WebUI at `http://localhost:3000`.

---

### Docker Deployment

SaveMail provides pre-built Docker images for easy deployment. Make sure to also run a Postgres database running (preferably in a container)

#### Backend

Start the backend container with:
```bash
docker run -p 5000:5000 -e SAVEMAIL_ConnectionStrings_Host=<DB_HOST> -e SAVEMAIL_ConnectionStrings_Username=<DB_USER> -e SAVEMAIL_ConnectionStrings_Password=<DB_PASSWORD> stephanelatil/savemail-backend
```
By default it will run on `http://localhost:5000`. You can edit this by passing the HOSTNAME and PORT environment variables.

Example: 

```bash
docker run -p 5000:5000 -e SAVEMAIL_ConnectionStrings_Host=<DB_HOST> -e SAVEMAIL_ConnectionStrings_Username=<DB_USER> -e SAVEMAIL_ConnectionStrings_Password=<DB_PASSWORD> -e HOSTNAME=thebackend.yourdomain.com -e PORT=12345 stephanelatil/savemail-backend
```

##### All Configuration Environment Variables

Environment variables are used to configure the backend.

**Hosting Specific**

| Variable                         | Description                              | Default   |
|----------------------------------|------------------------------------------|-----------|
| `HOSTNAME` | Hostname (IP or domain) where the backend is hosted | `localhost` |
| `PORT` | Port to listen on | `5000` |

**DB and app variables**

These can be set in the `appsettings.json` file or passed directly when running the Docker containers.

| Variable                         | Description                              | Default   |
|----------------------------------|------------------------------------------|-----------|
| `SAVEMAIL_ConnectionStrings_Host` | Database host (IP or domain) | `localhost` |
| `SAVEMAIL_ConnectionStrings_Username` | Database username | `postgres` |
| `SAVEMAIL_ConnectionStrings_Password` | Database password | **Required** |
| `SAVEMAIL_AttachmentsPath` | The path where the attachments will be stored | `./Attachments` |
| `SAVEMAIL_AppSecret` | A random string of characters used to generate an encryption key for OAuth tokens. Do not modify this once is is set or all access and refresh tokens will become invalid. | `ANY_RANDOM_ASSORTMENT_OF_CHARACTERS (Used to encrypt OAuth tokens in DB)` |

**Added Features**

Other variables can be set for added features:

| Variable                         | Description                              | Default   |
|----------------------------------|------------------------------------------|-----------|
| `SAVEMAIL_OAuth2_GOOGLE_CLIENT_ID` | Google Client Id to enable Oauth linking. Otherwise Gmail addresses will not work! |  |
| `SAVEMAIL_OAuth2_GOOGLE_CLIENT_SECRET` | Google Client Secret to enable Oauth linking. Otherwise Gmail addresses will not work! |  |

#### Frontend

Start the frontend container with:
```bash
docker run -p 3000:3000 stephanelatil/savemail-frontend
```

Environment variables are used to configure the frontend.

| Variable                         | Description                              | Default   |
|----------------------------------|------------------------------------------|-----------|
| `HOSTNAME` | Hostname (IP or domain) where the frontend is hosted | `localhost` |
| `PORT` | Port to listen on | `3000` |

### Docker Compose

For a simplified deployment, create a `docker-compose.yml` file in your project directory:

```yaml
services:
  postgres_db:
    image: postgres:15
    container_name: postgres_db
    environment:
      POSTGRES_USER: <DB_USER>
      POSTGRES_PASSWORD: <DB_PASSWORD>
      POSTGRES_DB: savemaildb
    volumes:
      - psql_data:/var/lib/postgresql/data

  backend:
    image: stephanelatil/savemail-backend
    container_name: backend
    environment:
      SAVEMAIL_ConnectionStrings_Host: postgres_db
      SAVEMAIL_ConnectionStrings_Username: <DB_USER>
      SAVEMAIL_ConnectionStrings_Password: <DB_PASSWORD>
    volumes:
        - attachments_vol:/app/Attachments
    depends_on:
      postgres:
         condition: service_healthy

  frontend:
      image: stephanelatil/savemail-frontend
    container_name: frontend
      environment:
        - PORT=3000
        - NEXT_PUBLIC_FRONTEND_URL="http://<YOUR_IP_HOSTNAME_HERE>:3000"
        - NEXT_PUBLIC_BACKEND_URL="http://<YOUR_IP_HOSTNAME_HERE>:5000"
      depends_on:
         - backend
      ports:
         - "3000:3000"

volumes:
    attachments_vol:
    psql_data:
```

Run all services with:
```bash
docker-compose up -d
```


## API Documentation

SaveMail's backend API is documented using **Swagger**. Once the backend is running, access the documentation at:
```
http://<backend_url_and_port>/swagger
```


## Tech Stack

- **Frontend**: Next.js (React Framework)
- **Backend**: ASP.NET Core (Web API)
- **Database**: PostgreSQL
- **Authentication**: ASP.NET Identity
- **Containerization**: Docker, Docker Compose


## Contributing

Contributions are welcome! Follow these steps:

1. Fork the repository.
2. Create a new branch:
   ```bash
   git checkout -b feature-name
   ```
3. Commit your changes:
   ```bash
   git commit -am 'Add new feature'
   ```
4. Push the branch:
   ```bash
   git push origin feature-name
   ```
5. Open a Pull Request.

Ensure your code adheres to standards and includes tests for new features.

## License

This project is licensed under the Apache 2.0 License. See the `LICENSE` file for details.


## Support

Need help, an issue fixed or a feature faster? Feel free to:
[![Buy Me a Coffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/stephanelatil)
