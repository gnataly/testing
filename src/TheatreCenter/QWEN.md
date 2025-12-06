# Theatre Center Project

## Project Overview
The Theatre Center is a .NET 8.0 web application built with ASP.NET Core that manages theatre-related information including theatres, musicals, actors, roles, and shows. The project follows a clean architecture pattern with separation of concerns across multiple projects.

### Architecture
The solution consists of multiple projects:
- **TheatreCenter.Backend**: Main web API application using ASP.NET Core with Entity Framework Core
- **TheatreCenter.Domain**: Contains domain models, interfaces, enums, and business entities
- **TheatreCenter.Data**: Data access layer with Entity Framework Core implementation and repositories
- **TheatreCenter.Services**: Business logic layer with service implementations
- **TheatreCenter.DTOs**: Data Transfer Objects for API communication
- **TheatreCenter.UnitTests**: Comprehensive test suite with unit, integration, and end-to-end tests

### Technologies Used
- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core with PostgreSQL
- JWT Authentication
- Serilog for logging
- Swagger/OpenAPI for API documentation
- Docker & Docker Compose for containerization
- MSTest and xUnit for testing
- Moq for mocking

### Key Features
- Theatre management (create, read, update, delete)
- Musical/Show management
- Actor and cast management
- Role assignment to actors
- Theme classification
- User favorites system
- Account management
- Search and filtering capabilities
- API versioning
- Comprehensive test coverage

## Building and Running

### Local Development
1. **Prerequisites**: .NET 8.0 SDK, PostgreSQL database
2. **Restore packages**: `dotnet restore`
3. **Build solution**: `dotnet build`
4. **Run migrations**: `dotnet ef database update`
5. **Start the application**: `dotnet run` in the `TheatreCenter.Backend` directory

### Using Docker
1. **Build and run with Docker Compose**:
   ```bash
   docker-compose up --build
   ```
2. The API will be available at `http://localhost:5000`

### Local Build Script
Use the provided `build.bat` script to build and publish the application:
```cmd
build.bat
```
This creates a deployment directory with all necessary files.

## Configuration
- **Connection String**: Configured in `appsettings.json` pointing to PostgreSQL database
- **JWT Settings**: Secret, issuer, audience, and expiration configured in appsettings
- **Logging**: Serilog configured for file-based logging with daily rolling
- **Ports**: Configured to run on port 5000 (HTTP) and 5001 (HTTPS)

## Testing
The project includes a comprehensive testing strategy:
- **Unit Tests**: Test individual components and services in isolation
- **Integration Tests**: Test interactions between components and database operations
- **End-to-End Tests**: Test complete API workflows

### Running Tests
1. **Local**: `dotnet test TheatreCenter.UnitTests/TheatreCenter.UnitTests.csproj`
2. **Docker**: The `docker-compose.yml` includes test runner services that execute tests in containers
3. **Specific test categories**: Use test filters like `--filter "Category=Unit"`

## Development Conventions
- Follows clean architecture principles with clear separation of concerns
- Dependency injection used throughout the application
- Repository pattern for data access
- Service layer for business logic
- DTOs for API communication to prevent over-exposure of domain models
- Comprehensive logging using Serilog
- Input validation using Data Annotations and IValidatableObject
- Asynchronous programming patterns throughout
- Comprehensive exception handling with logging
- API versioning to support multiple client versions

## Database
- Uses PostgreSQL as the primary data store
- Entity Framework Core for ORM
- Migrations managed through EF Core
- Seed data can be loaded via SQL scripts in the test directory

## API Documentation
- Swagger/OpenAPI documentation available at `/swagger` endpoint
- API versioning for backward compatibility
- Proper HTTP status codes and error responses
- JWT authentication for protected endpoints