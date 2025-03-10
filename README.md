# SaveMail

<img src="https://raw.githubusercontent.com/stephanelatil/savemail/main/img/Logo.png?raw=true" width="200">

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
By default it will run on `http://localhost:5000`. You can edit this by passing the HOST and PORT environment variables.

Example: 

```bash
docker run -p 5000:5000 -e SAVEMAIL_ConnectionStrings_Host=<DB_HOST> -e SAVEMAIL_ConnectionStrings_Username=<DB_USER> -e SAVEMAIL_ConnectionStrings_Password=<DB_PASSWORD> -e HOST=thebackend.yourdomain.com -e PORT=12345 stephanelatil/savemail-backend
```

##### All Configuration Environment Variables

Environment variables are used to configure the backend.

**Hosting Specific**

| Variable                         | Description                              | Default   |
|----------------------------------|------------------------------------------|-----------|
| `HOST` | Hostname (IP or domain) where the backend is hosted | `localhost` |
| `PORT` | Port to listen on | `5000` |

**DB and app variables**

These can be set in the `appsettings.json` file or passed directly when running the Docker containers.

| Variable                         | Description                              | Default   |
|----------------------------------|------------------------------------------|-----------|
| `SAVEMAIL__ConnectionStrings__Host` | Database host (IP or domain) | `localhost` |
| `SAVEMAIL__ConnectionStrings__Username` | Database username | `postgres` |
| `SAVEMAIL__ConnectionStrings__Password` | Database password | **Required** |
| `SAVEMAIL__AttachmentsPath` | The path where the attachments will be stored | `./Attachments` |
| `SAVEMAIL__AppSecret` | A random string of characters used to generate an encryption key for OAuth tokens. Do not modify this once is is set or all access and refresh tokens will become invalid. | `ANY_RANDOM_ASSORTMENT_OF_CHARACTERS (Used to encrypt OAuth tokens in DB)` |
| `SAVEMAIL__Logging__LogLevel__Default` | The default logging level for all namespaces, except if specifically stated with another level in another environment variable | `Information` |
| `SAVEMAIL__Logging__LogLevel__Backend` | The logging level for all services of the Backend. If empty it will default to the default logging value of "Information" |  |
| `SAVEMAIL__Logging__LogLevel__Microsoft.AspNetCore` | The default logging level for the host application and endpoints. Uses the "Default" namespace value if not set. | `Information` |
| `SAVEMAIL__Logging__LogLevel__Microsoft.EntityFrameworkCore.Database.Command` | The default logging level for EF core database queries. Uses the "Default" namespace value if not set. Anything below `Warning` will make the logs VERY verbose. | `Warning` |
| `SAVEMAIL__Logging__LogLevel__{Some namespace here}` | The default logging level for the namespace. Uses the "Default" namespace value if not set |  |

**Added Features**

Other variables can be set for added features:

| Variable                         | Description                              | Default   |
|----------------------------------|------------------------------------------|-----------|
| `SAVEMAIL_OAuth2_GOOGLE_CLIENT_ID` | Google Client Id to enable Oauth linking. Otherwise Gmail addresses will not work! |  |
| `SAVEMAIL_OAuth2_GOOGLE_CLIENT_SECRET` | Google Client Secret to enable Oauth linking. Otherwise Gmail addresses will not work! |  |
| `SAVEMAIL__SendGrid__Key` | The Sendgrid Key used to send email verification, password reset emails etc. If no email sending service is present, this feature will be disabled | |
| `SAVEMAIL__SendGrid__FromEmail` | The email from which the email will be sent (use the same email as the one used with the sendgrid Key) | |
| `SAVEMAIL__SendGrid__FromName` | The name of the sender present at the top of sent emails | `SaveMail` |
| `SAVEMAIL__Brevo__Key` | The Brevo Key used to send email verification, password reset emails etc. If no email sending service is present, this feature will be disabled | |
| `SAVEMAIL__Brevo__SenderId` | The Brevo Sender Id used to send email verification, password reset emails etc. | |

#### Frontend

Start the frontend container with:
```bash
docker run -p 3000:3000 stephanelatil/savemail-frontend
```

Environment variables are used to configure the frontend.

| Variable                         | Description                              | Default   |
|----------------------------------|------------------------------------------|-----------|
| `LISTEN` | Hostname (IP or domain) where the frontend is hosted | `localhost` |
| `HOST` | Hostname (IP or domain) where the frontend is accessible. This is useful for  | `localhost` |
| `PORT` | Port to listen on | `3000` |
| `BACKEND_PORT` | The port the Backend is listening on | `3000` |

### Docker Compose

For a simplified deployment, create a `docker-compose.yml` file in your project directory:

```yaml
services:
  postgres_db:
    image: postgres:15
    container_name: postgres_db
    environment:
      POSTGRES_USER: <DB_USERNAME_HERE>
      POSTGRES_PASSWORD: <DB_PASSWORD_HERE>
      POSTGRES_DB: savemaildb
    volumes:
      - psql_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U $${POSTGRES_USER} -d $${POSTGRES_DB}"]
      interval: 5s
      timeout: 5s
      retries: 5
    

  backend:
    image: stephanelatil/savemail-backend
    container_name: backend
    environment:
      SAVEMAIL__ConnectionStrings__Host: postgres_db # The name of the DB container or the Hostname of the psql DB
      SAVEMAIL__ConnectionStrings__Username: <DB_USERNAME_HERE>
      SAVEMAIL__ConnectionStrings__Password: <DB_PASSWORD_HERE>
      SAVEMAIL__ConnectionStrings__Database: savemaildb #name of the postgres database (Should be the same as POSTGRES_DB)
      # SAVEMAIL__OAuth2__GOOGLE_CLIENT_ID: #Google Client Id to enable Oauth linking. Otherwise Gmail addresses will not work!
      # SAVEMAIL__OAuth2__GOOGLE_CLIENT_SECRET: #Google Client Secret to enable Oauth linking. Otherwise Gmail addresses will not work!
      # SAVEMAIL__AttachmentsPath: #The directory path where the attachments will be stored. by default ./Attachments is used. The path is relative to the Backend.dll location
      # SAVEMAIL__AppSecret: # A random string of characters used to generate an encryption key for OAuth tokens. Do not modify this once is is set or all access and refresh tokens will become invalid. They can be regenerated by re-authenticating all users (There will be a notification in the frontend or a flag in the "needsReauth" json field, when querying a MailBox though the API
      # SAVEMAIL__Logging__LogLevel__Default: Information # The default logging level for all namespaces, except if specifically stated with another level in another environment variable, Levels can be "Debug", "Information", "Warning", "Error", "Critical"
      # SAVEMAIL__Logging__LogLevel__Backend: # The logging level for all services of the Backend. If empty it will default to the default logging value of "Information"
      # SAVEMAIL__Logging__LogLevel__Microsoft.AspNetCore: # The default logging level for the host application and endpoints. Uses the "Default" namespace value if not set.
      # SAVEMAIL__Logging__LogLevel__Microsoft.EntityFrameworkCore.Database.Command: # The default logging level for EF core database queries. Uses the "Default" namespace value if not set. Anything below `Warning` will make the logs VERY verbose.
      # SAVEMAIL__Logging__LogLevel__ANY.NAMESPACE.HERE: # Logging level for a specific namespace. Uses the "Default" namespace value if not set. Useful for debugging

      # SAVEMAIL__OAuth2__GOOGLE_CLIENT_ID: #Google Client Id to enable Oauth linking. Otherwise Gmail addresses will not work!
      # SAVEMAIL__OAuth2__GOOGLE_CLIENT_SECRET: #Google Client Secret to enable Oauth linking. Otherwise Gmail addresses will not work!

      # SAVEMAIL__RequireEmailConfirmation: true # true/false
      # SAVEMAIL__SendGrid__Key: #The Sendgrid Key used to send email verification, password reset emails etc. If no email sending service is present, this feature will be disabled
      # SAVEMAIL__SendGrid__FromEmail: # The email from which the email will be sent (use the same email as the one used with the sendgrid Key)
      # SAVEMAIL__SendGrid__FromName: # The name of the sender present at the top of sent emails. Set to "SaveMail" by default
      # SAVEMAIL__Brevo__Key: # The Brevo Key used to send email verification, password reset emails etc. If no email sending service is present, this feature will be disabled
      # SAVEMAIL__Brevo__SenderId: # The Brevo Sender Id used to send email verification, password reset emails etc. 

    volumes:
      - attachments_vol:/app/Attachments
    ports:
      - "5000:5000"
    depends_on:
      postgres_db:
         condition: service_healthy

  frontend:
    image: stephanelatil/savemail-frontend
    container_name: frontend
    environment:
      PORT: "3000"
      HOST: "localhost" #The hostname to listen on
      BACKEND_PORT: "5000" #the port of the backend (used to link backend/frontend)
    depends_on:
      backend:
         condition: service_healthy
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
