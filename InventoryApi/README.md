# Inventory & Order Management API

## Project Overview
A complete RESTful API for managing products, inventory, and orders with JWT authentication, built with ASP.NET Core 8.0, Entity Framework Core, and SQL Server.

## Features Implemented

### Core Functionality
✅ **Product Management**
- Create, read, update, delete products
- Product search with pagination
- SKU-based unique identification
- Price tracking

✅ **Inventory Management**
- Real-time stock tracking
- Automatic inventory deduction on purchase
- Inventory restocking on order cancellation
- Reorder level and quantity configuration

✅ **Order Management**
- Create orders with multiple items
- Automatic inventory validation
- Order status tracking (Pending → Processing → Shipped → Delivered)
- Order cancellation with inventory restoration
- User order history with pagination

✅ **Authentication & Authorization**
- JWT token-based authentication
- User registration and login
- Secure password hashing (SHA256)
- User profile endpoint
- Role-based access control

### Technical Features
✅ **Database**
- Entity Framework Core with SQL Server
- Proper foreign key relationships
- Cascade delete behavior
- Precision decimal columns for pricing

✅ **API Features**
- RESTful design
- Pagination support (configurable page size)
- Sorting capabilities
- CORS enabled
- Swagger/OpenAPI documentation with JWT bearer support
- Error handling and validation

✅ **Architecture**
- Repository Pattern for data access
- Service Layer for business logic
- Dependency Injection
- Clean separation of concerns

## Project Structure

```
InventoryAPI/
├── Models/                 # Domain entities
│   ├── User.cs
│   ├── Product.cs
│   ├── Inventory.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   └── OrderStatus.cs
├── Data/                   # Database context
│   └── ApplicationDbContext.cs
├── Repositories/           # Data access layer
│   ├── IRepository.cs      # Interfaces
│   ├── ProductRepository.cs
│   ├── InventoryRepository.cs
│   ├── OrderRepository.cs
│   └── UserRepository.cs
├── Services/              # Business logic layer
│   ├── IService.cs        # Interfaces
│   ├── ProductService.cs
│   ├── InventoryService.cs
│   ├── OrderService.cs
│   └── AuthService.cs
├── Controllers/           # API endpoints
│   ├── AuthController.cs
│   ├── ProductsController.cs
│   ├── InventoryController.cs
│   └── OrdersController.cs
├── DTOs/                  # Data transfer objects
│   ├── ProductDto.cs
│   ├── InventoryDto.cs
│   ├── OrderDto.cs
│   ├── UserDto.cs
│   └── PaginationDto.cs
├── Program.cs             # Application startup
├── appsettings.json
├── appsettings.Development.json
├── .env                   # Local environment variables
└── .env.example
```

## API Endpoints

### Authentication
```
POST   /api/auth/register       # Register new user
POST   /api/auth/login          # Login user
GET    /api/auth/profile        # Get current user (requires JWT)
```

### Products
```
GET    /api/products            # Get all products (paginated, no auth required)
GET    /api/products/{id}       # Get product by ID
POST   /api/products            # Create product (requires JWT)
PUT    /api/products/{id}       # Update product (requires JWT)
DELETE /api/products/{id}       # Delete product (requires JWT)
```

### Inventory
```
GET    /api/inventory/product/{productId}           # Get inventory for product
PUT    /api/inventory/product/{productId}           # Update inventory
GET    /api/inventory/check/{productId}/{quantity}  # Check stock availability
```

### Orders
```
GET    /api/orders                          # Get all orders (paginated)
GET    /api/orders/{id}                     # Get order by ID
GET    /api/orders/user/{userId}            # Get user's orders
POST   /api/orders                          # Create new order
PUT    /api/orders/{id}/status              # Update order status
PUT    /api/orders/{id}/cancel              # Cancel order
```

## Database Schema

### Users Table
- Id (PK)
- Email (Unique)
- PasswordHash
- FirstName
- LastName
- CreatedAt
- UpdatedAt

### Products Table
- Id (PK)
- Name
- Description
- Price (decimal)
- Sku (Unique)
- CreatedAt
- UpdatedAt

### Inventory Table
- Id (PK)
- ProductId (FK)
- QuantityOnHand
- ReorderLevel
- ReorderQuantity
- LastUpdated

### Orders Table
- Id (PK)
- UserId (FK)
- Status (Pending, Processing, Shipped, Delivered, Cancelled, Failed)
- TotalAmount (decimal)
- OrderDate
- ShippedDate
- DeliveredDate
- UpdatedAt

### OrderItems Table
- Id (PK)
- OrderId (FK)
- ProductId (FK)
- Quantity
- UnitPrice (decimal)

## Configuration

### Environment Variables (.env)
```
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5068
ConnectionStrings__DefaultConnection=<connection-string>
Jwt__Secret=<your-secret-key>
Jwt__Issuer=InventoryAPI
Jwt__Audience=InventoryAPIUsers
```

### Appsettings Configuration
- Default connection string points to SQL Server
- JWT authentication configured with 24-hour token expiration
- CORS enabled for all origins (can be restricted in production)
- SQL Server is the default database provider

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (local or remote)
- Visual Studio Code or Visual Studio

### Installation & Setup

1. **Configure database connection:**
   - Edit `appsettings.json` or set `ConnectionStrings__DefaultConnection` in `.env`

2. **Load environment variables:**
   ```bash
   set -a
   source .env
   set +a
   ```

3. **Create and seed database:**
   ```bash
   dotnet ef database update
   ```

4. **Run the application:**
   ```bash
   dotnet run
   ```

5. **Access Swagger UI:**
   - Open browser: http://localhost:5068/swagger

### Testing the API

1. **Register a user:**
   ```json
   POST /api/auth/register
   {
     "email": "user@example.com",
     "password": "SecurePass123!",
     "firstName": "John",
     "lastName": "Doe"
   }
   ```

2. **Login:**
   ```json
   POST /api/auth/login
   {
     "email": "user@example.com",
     "password": "SecurePass123!"
   }
   ```

3. **Create a product:**
   ```json
   POST /api/products (with JWT token)
   {
     "name": "Laptop",
     "description": "High-performance laptop",
     "price": 999.99,
     "sku": "LAP-001"
   }
   ```

4. **Create an order:**
   ```json
   POST /api/orders (with JWT token)
   {
     "userId": 1,
     "items": [
       {
         "productId": 1,
         "quantity": 2
       }
     ]
   }
   ```

## Security Considerations

- Passwords are hashed using SHA256 (consider using bcrypt for production)
- JWT tokens expire after 24 hours
- All endpoints except `/products` (GET) and auth endpoints require JWT
- CORS is open to all origins (configure in production)
- Connection string should use environment variables in production
- JWT secret should be stored securely (use Azure Key Vault or similar)

## Future Enhancements

- [ ] Integration tests and unit tests
- [ ] Background jobs for inventory management
- [ ] Email notifications on order status changes
- [ ] Payment gateway integration
- [ ] Shipping provider integration
- [ ] Advanced filtering and search
- [ ] Audit logging
- [ ] Rate limiting
- [ ] API versioning
- [ ] GraphQL endpoint

## Dependencies

- Microsoft.EntityFrameworkCore (8.0.1)
- Microsoft.EntityFrameworkCore.SqlServer (8.0.1)
- Microsoft.AspNetCore.Authentication.JwtBearer (8.0.1)
- System.IdentityModel.Tokens.Jwt (7.1.0)
- Swashbuckle.AspNetCore (6.6.2)

## Notes

- Build passes without errors ✅
- All packages installed ✅
- JWT authentication configured ✅
- Database context ready for migrations ✅
- CORS enabled for development ✅
