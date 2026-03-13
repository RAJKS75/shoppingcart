# ShoppingCart

A simple ASP.NET Core MVC web application for managing a shopping cart. This application allows users to browse products, add them to a cart, and place orders. It includes authentication via Microsoft Entra ID (Azure AD) and supports both SQL Server and MySQL databases.

## Features

- User authentication with Azure AD
- Product catalog
- Shopping cart functionality
- Order management
- Database support for SQL Server and MySQL
- Swagger API documentation

## Prerequisites

Before building and running the application, ensure you have the following installed:

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- A database server:
  - SQL Server (local or Azure SQL Database)
  - MySQL (local or cloud-hosted)
- Azure AD app registration for authentication (optional, but required for full functionality)

### Database Setup

1. **SQL Server**:
   - Install SQL Server or use Azure SQL Database.
   - Update the connection string in `appsettings.json` or `appsettings.Development.json`.

2. **MySQL**:
   - Install MySQL or use a cloud service like Azure Database for MySQL.
   - Set `"DatabaseProvider": "MySQL"` in `appsettings.json`.
   - Update the connection string accordingly.

### Azure AD Setup (Optional)

To enable authentication:

1. Register an application in Azure AD.
2. Note the Tenant ID, Client ID, and Client Secret.
3. Update the `AzureAd` section in `appsettings.json`:
   ```json
   "AzureAd": {
     "TenantId": "your-tenant-id",
     "ClientId": "your-client-id",
     "ClientSecret": "your-client-secret"
   }
   ```

## Building the Application

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd ShoppingCart
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the application:
   ```bash
   dotnet build
   ```

4. (Optional) Run database migrations:
   If using Entity Framework, apply migrations to set up the database schema:
   ```bash
   dotnet ef database update
   ```
   Note: If migrations are not present, you may need to create them first:
   ```bash
   dotnet ef migrations add InitialCreate
   ```

## Running Locally

1. Ensure your database is running and accessible.

2. Run the application:
   ```bash
   dotnet run
   ```

3. Open a web browser and navigate to `https://localhost:5001` (or the URL shown in the console).

4. Access the Swagger API documentation at `https://localhost:5001/swagger`.

## Deployment

### Option 1: Deploy to Azure App Service

1. Publish the application:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. Create an Azure App Service in the Azure portal.

3. Configure the App Service:
   - Set the runtime stack to .NET 10.0.
   - Add environment variables or app settings for:
     - Database connection string
     - Azure AD settings
     - `DatabaseProvider` if using MySQL

4. Deploy using Azure CLI or Visual Studio:
   - Via Azure CLI:
     ```bash
     az webapp deployment source config-zip --resource-group <resource-group> --name <app-name> --src ./publish.zip
     ```
   - Or use Visual Studio's Publish tool.

5. Ensure the database is accessible from Azure (e.g., allow Azure services in firewall settings).

### Option 2: Deploy to IIS

1. Publish the application for Windows:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained -o ./publish
   ```

2. Install the .NET Hosting Bundle on the IIS server.

3. Create a new website in IIS Manager:
   - Set the physical path to the `publish` folder.
   - Configure the application pool to use .NET CLR Version: No Managed Code (for .NET Core).

4. Update `web.config` if necessary for custom settings.

5. Ensure the database connection is configured in `appsettings.json` in the publish folder.

### Option 3: Deploy as Docker Container

1. Create a Dockerfile (if not present):
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
   WORKDIR /app
   EXPOSE 80

   FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
   WORKDIR /src
   COPY ["ShoppingCart.csproj", "."]
   RUN dotnet restore
   COPY . .
   RUN dotnet build -c Release -o /app/build

   FROM build AS publish
   RUN dotnet publish -c Release -o /app/publish

   FROM base AS final
   WORKDIR /app
   COPY --from=publish /app/publish .
   ENTRYPOINT ["dotnet", "ShoppingCart.dll"]
   ```

2. Build and run the Docker image:
   ```bash
   docker build -t shoppingcart .
   docker run -p 8080:80 shoppingcart
   ```

3. Deploy to a container registry like Azure Container Registry and use in Azure Container Instances or Kubernetes.

## Configuration

- `appsettings.json`: Main configuration file.
- `appsettings.Development.json`: Development-specific settings.
- Environment variables can override settings in production.

## API Endpoints

- `/`: Home page
- `/Products`: Product catalog
- `/Cart`: Shopping cart
- `/Home/CheckoutSuccess`: Order confirmation

Use Swagger at `/swagger` for detailed API documentation.

## Contributing

1. Fork the repository.
2. Create a feature branch.
3. Make changes and test.
4. Submit a pull request.

## License

This project is licensed under the MIT License.