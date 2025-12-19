# 📚 Readlog API

A modern RESTful API for managing book reviews and reading lists, built with Clean Architecture, Domain-Driven Design (DDD), and CQRS patterns.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Azure](https://img.shields.io/badge/Azure-Deployed-0078D4?logo=microsoftazure)
![License](https://img.shields.io/badge/License-MIT-green)
[![CI/CD](https://github.com/Degbogueur/readlog/actions/workflows/cicd.yml/badge.svg)](https://github.com/Degbogueur/readlog/actions)

## Live Demo

- **API**: [https://app-readlog.azurewebsites.net](https://app-readlog.azurewebsites.net)
- **Swagger Documentation**: [https://app-readlog.azurewebsites.net/swagger](https://app-readlog.azurewebsites.net/swagger)

## Features

- 📖 **Books Management**: CRUD operations with search, pagination, and sorting
- ⭐ **Reviews**: Rate and review books (one review per user per book)
- 📋 **Reading Lists**: Create personal reading lists with reading status tracking
- 🔐 **Authentication**: JWT-based auth with refresh token rotation
- 🗑️ **Soft Delete**: Data is preserved for audit purposes
- 📊 **Pagination**: Efficient data loading with customizable page sizes

## Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:
```
src/
├── Readlog.Domain/          # Entities, Value Objects, Domain Events
├── Readlog.Application/     # Use Cases, CQRS Handlers, Validators
├── Readlog.Infrastructure/  # EF Core, Repositories, External Services
└── Readlog.Api/             # Controllers, Middleware, Configuration
```

### Key Patterns & Practices

| Pattern | Implementation |
|---------|----------------|
| **Clean Architecture** | 4-layer separation (Domain, Application, Infrastructure, API) |
| **Domain-Driven Design** | Rich domain models, Value Objects, Domain Events |
| **CQRS** | Commands and Queries via MediatR |
| **Repository Pattern** | Abstracted data access |
| **Result Pattern** | Explicit error handling without exceptions |
| **Unit of Work** | Transactional consistency |

## Tech Stack

- **.NET 8** - LTS version
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM with SQL Server
- **MediatR** - CQRS implementation
- **FluentValidation** - Request validation
- **ASP.NET Core Identity** - User management
- **JWT Bearer** - Authentication
- **Swagger/OpenAPI** - API documentation
- **xUnit** - Testing framework
- **TestContainers** - Integration testing with Docker
- **GitHub Actions** - CI/CD pipeline
- **Azure App Service** - Cloud hosting
- **Azure SQL Database** - Managed database

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB for development)
- [Docker](https://www.docker.com/) (for integration tests)

### Installation

1. Clone the repository
```bash
   git clone https://github.com/Degbogueur/readlog.git
   cd readlog
```

2. Restore dependencies
```bash
   dotnet restore
```

3. Update the connection string in `appsettings.json`
```bash
"ConnectionStrings": {
  "DefaultConnection": "Server={server_name};Database={database_name};Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

4. Apply database migrations
```bash
   dotnet ef database update --project src/Readlog.Infrastructure --startup-project src/Readlog.Api
```

5. Run the application
```bash
   dotnet run --project src/Readlog.Api
```

6. Open Swagger UI
```
   https://localhost:5001/swagger
```

### Running Tests
```bash
# All tests
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Specific project
dotnet test tests/Readlog.Domain.Tests
dotnet test tests/Readlog.Application.Tests
dotnet test tests/Readlog.Api.Tests  # Requires Docker
```

## API Endpoints

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login and get tokens |
| POST | `/api/auth/refresh` | Refresh access token |
| POST | `/api/auth/revoke` | Revoke refresh token |

### Books

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/books` | Get all books (paginated) |
| GET | `/api/books/{id}` | Get book by ID |
| POST | `/api/books` | Create a new book |
| PUT | `/api/books/{id}` | Update a book |
| DELETE | `/api/books/{id}` | Delete a book |

### Reviews

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/books/{bookId}/reviews` | Get reviews for a book |
| GET | `/api/books/{bookId}/reviews/{id}` | Get review for a book by ID |
| POST | `/api/books/{bookId}/reviews` | Create a review |
| PUT | `/api/books/{bookId}/reviews/{id}` | Update your review |
| DELETE | `/api/books/{bookId}/reviews/{id}` | Delete your review |

### Reading Lists

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/reading-lists` | Get your reading lists |
| GET | `/api/reading-lists/{id}` | Get reading list by ID |
| POST | `/api/reading-lists` | Create a reading list |
| PUT | `/api/reading-lists/{id}/rename` | Rename a reading list |
| DELETE | `/api/reading-lists/{id}` | Delete a reading list |
| POST | `/api/reading-lists/{id}/books` | Add book to list |
| PUT | `/api/reading-lists/{id}/books/{bookId}/status` | Update book status |
| DELETE | `/api/reading-lists/{id}/books/{bookId}` | Remove book from list |

## Testing Strategy

| Layer | Tests | Tools |
|-------|-------|-------|
| **Domain** | ~124 unit tests | xUnit, FluentAssertions |
| **Application** | ~96 unit tests | xUnit, Moq, FluentAssertions |
| **Integration** | ~58 tests | xUnit, TestContainers, WebApplicationFactory |

## Project Structure
```
readlog/
├── .github/
│   └── workflows/
│       └── cicd.yml            # GitHub Actions pipeline
├── src/
│   ├── Readlog.Domain/
│   │   ├── Abstractions/       # Interfaces (IAuditable, ISoftDeletable)
│   │   ├── Entities/           # Book, Review, ReadingList, RefreshToken
│   │   ├── Enums/              # ReadingStatus
│   │   ├── Events/             # Domain events
│   │   ├── Exceptions/         # Domain exceptions
│   │   └── ValueObjects/       # ISBN, Rating
│   ├── Readlog.Application/
│   │   ├── Abstractions/       # IUnitOfWork, ICurrentUserService
│   │   ├── Behaviors/          # MediatR pipelines (ValidationBehavior)
│   │   ├── Extensions/         # QueryableExtensions 
│   │   ├── Features/           # Commands & Queries per feature
│   │   └── Shared/             # Constants, PagedResult, Result pattern
│   ├── Readlog.Infrastructure/
│   │   ├── Identity/           # JWT, Identity
│   │   ├── Data/               # DbContext, Configurations, Migrations
│   │   ├── Interceptors/       # Auditable, SoftDelete, DomainEvents
│   │   └── Repositories/       # Repository implementations
│   │   └── Services/           # AuthenticationService, CurrentUserService
│   └── Readlog.Api/
│       ├── Controllers/        # API controllers
│       ├── Extensions/         # Service extensions
│       ├── Handlers/           # Global exception handler
│       ├── Requests/           # Request DTOs
│       └── Responses/          # Response DTOs
└── tests/
    ├── Readlog.Domain.Tests/
    ├── Readlog.Application.Tests/
    └── Readlog.Api.Tests/
```

## Authentication Flow
```
1. Register/Login → Access Token + Refresh Token
2. Use Access Token in Authorization header
3. When Access Token expires → Use Refresh Token to get new pair
4. Logout → Revoke Refresh Token
```

## Deployment

The application is deployed to Azure using GitHub Actions:

1. **Push to `main`** triggers the CI/CD pipeline
2. **Build & Test** runs all tests
3. **Deploy** pushes to Azure App Service

### Azure Resources

- **App Service**: Hosts the API (Free tier)
- **Azure SQL**: Managed database (Free tier)

## License

This project is licensed under the MIT License

## Author

- GitHub: [@Degbogueur](https://github.com/Degbogueur)
- LinkedIn: [Komi Obed Degbo](https://linkedin.com/in/obed-degbo)

---

⭐ If you found this project helpful, please give it a star!