# SaveMail
 

SaveMail is a self-hosted web application that allows users to store and archive emails locally. Built with a modern tech stack, it leverages ASP.NET for the backend REST API and Next.js for the WebUI.

## Table of Contents

- [Features](#features)
- [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Installation](#installation)
    - [Backend](#backend)
        - [Docker Backend](#docker-backend)
        - [Run Backend Locally](#run-backend-locally)
    - [Frontend](#frontend)
- [Configuration Variables](#configuration-variables)
- [Faster support](#need-an-issue-fixed-faster-or-a-question-answered-faster)
- [API Documentation](#api-documentation)
- [Tech Stack](#tech-stack)
- [Contributing](#contributing)
- [License](#license)

## Features

- Email retention: Emails are kept even if they are deleted from the IMAP server.
- Multi-user support: Add as many users as required and all will have access to their own independent mailboxes.
- Integrated WebUI: Responsive design, optimized for desktop and mobile devices.
- API Integration: RESTful API backend with well-documented endpoints if you want to use your own custom frontend.
- Docker Support: Easily deployable using Docker.

## Getting Started

### Prerequisites

Ensure you have the following installed:

- Node.js (v14.x or later)
- .NET Core (v8.0 or later)

**OR**

- Docker

### Backend

#### Docker Backend

Ensure you have a Postgres database running and edit the `DefaultConnection` json connection string to the database in the `savemail/Backend/appsettings.json` file.

Then run

```bash
docker run -p 80:80 -p 443:443 stephanelatil/savemail-backend
```

#### Run Backend Locally

Clone the repository:

```bash
git clone https://github.com/stephanelatil/savemail.git
cd savemail
```

Navigate to the backend directory and build the .NET application:

```bash
cd ../Backend
dotnet restore
dotnet publish
```

Finally run the application:

```bash
dotnet ./bin/Release/net8.0/Backend.dll
```

### Frontend

The frontend is in development! Contribute or come back later when it's funtional.

## Configuration Variables

The application relies on several environment variables that need to be configured:

#### For Backend:

You can configure it directly in `savemail/Backend/appsettings.json` or use the environment variables (for docker).

| Environment Variable | Use | Default |
| --- | --- | --- |
| `SAVEMAIL_ConnectionStrings_Host` | The Host IP or domain of the database | `localhost` |
| `SAVEMAIL_ConnectionStrings_Username` | The database Username to use to connect | `postgres` |
| `SAVEMAIL_ConnectionStrings_Password` | The database password | `P0stgres` |

## Need an issue fixed faster or a question answered faster?

[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/stephanelatil)

## API Documentation

The SaveMail API is documented using Swagger. Once the backend is running, you can access the API documentation at the `backend_url/swagger`.

Additional documentation will be added later :)

## Tech Stack

- Frontend: Next.js (React Framework)
- Backend: ASP.NET Core (Web API)
- Database: Postgres SQL
- Authentication: ASP.NET Identity
- Containerization: Docker

## Contributing
We welcome contributions to SaveMail! Here's how you can help:

Fork the repository.
Create a new branch for your feature or bugfix (git checkout -b feature-name).
Commit your changes (git commit -am 'Add new feature').
Push to the branch (git push origin feature-name).
Create a new Pull Request.
Please make sure to follow the coding standards and write tests for any new features.

## License
This project is licensed under the Apache 2.0 License - see the LICENSE file for details.
