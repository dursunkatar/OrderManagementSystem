# E-Commerce Order Management System (OMS)

This project is a comprehensive Order Management System designed for e-commerce platforms. It's built following Clean Architecture and Domain-Driven Design principles.

## 🌟 Features

- **Order Management**
  - Order creation
  - Status tracking (Pending, Completed, Cancelled)
  - Order history viewing

- **Cart Management**
  - Adding products to cart
  - Updating cart contents
  - Cart total calculation

- **User Management**
  - User registration and login
  - JWT-based authentication
  - Role-based authorization (Customer, Admin)

- **Performance Optimizations**
  - Redis caching
  - Pagination
  - Asynchronous operations

- **Event-Driven Architecture**
  - Order status change notifications with RabbitMQ
  - Microservice-ready design

## 🔧 Technical Details

### Technologies

- **Backend**: .NET 9 / ASP.NET Core Web API
- **Database**: SQL Server / Entity Framework Core
- **Cache**: Redis
- **Messaging**: RabbitMQ
- **Authentication**: JWT (JSON Web Token)
- **Testing**: xUnit, Moq

### Architecture

The project is designed with a layered structure following Clean Architecture principles:

- **OMS.Domain**: Core entities and domain logic
- **OMS.Application**: Application services, DTOs, and business logic
- **OMS.Infrastructure**: Database, cache, and external service integrations
- **OMS.API**: HTTP API and controllers
- **OMS.Tests**: Unit tests

## 📋 Usage

### Requirements

- .NET 9 SDK
- SQL Server
- Redis (optional, can be disabled in configuration)
- RabbitMQ (optional, can be disabled in configuration)

### Installation

1. Clone the repository:
```
git clone https://github.com/username/order-management-system.git
```

2. Create the database:
```sql
CREATE DATABASE OrderManagementSystem;
```

3. Run the OrderManagementSystem.sql script

4. Configure the connection string:
Update the `ConnectionStrings` setting in the `appsettings.json` file according to your database server.

5. Run the application:
```
dotnet run --project OMS.API
```

### API Documentation

API documentation is available through Swagger UI:
```
https://localhost:5001/swagger
```

## 📊 API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/auth/register` | POST | Register a new user |
| `/api/auth/login` | POST | User login |
| `/api/cart` | GET | Get user's cart |
| `/api/cart/items` | POST | Add product to cart |
| `/api/cart/items` | PUT | Update cart item |
| `/api/cart/items/{productId}` | DELETE | Remove product from cart |
| `/api/cart` | DELETE | Clear cart |
| `/api/orders` | POST | Create order |
| `/api/orders/from-cart` | POST | Create order from cart |
| `/api/orders/{id}` | GET | Get order details |
| `/api/orders/my-orders` | GET | Get user's orders |
| `/api/orders/{id}/complete` | PUT | Complete order |
| `/api/orders/{id}/cancel` | PUT | Cancel order |

## 🧪 Tests

To run unit tests:
```
dotnet test OMS.Tests
```

## 🔒 Security Measures

- JWT-based authentication
- Role-based authorization
- Rate limiting (API request throttling)
- Secure algorithm for password hashing (MD5)
- Login attempt monitoring and prevention

