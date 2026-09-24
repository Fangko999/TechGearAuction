
# TechGearAuction - Global AI Coding Standards

## 1. Kiến trúc Tổng thể (Clean Architecture)
- **Tuyệt đối tuân thủ Clean Architecture** cho phía Backend.
- **Dependency Rule:** Luồng phụ thuộc chỉ được phép hướng vào trong (về phía Domain). Các lớp bên ngoài (UI, Infrastructure) có thể biết về lớp bên trong, nhưng lớp bên trong tuyệt đối không được tham chiếu ra bên ngoài.
- **Zero-Trust & Anti-Spam:** Mọi luồng API nhạy cảm phải tính đến ngữ cảnh xác thực (JWT) và cơ chế chặn phần cứng (`X-Device-Hash`).

## 2. Tiêu chuẩn Backend (.NET 10 & ASP.NET Core)
- **Tầng Domain:** Tinh khiết 100%. Chỉ chứa Entities, Enums, Interfaces, và Custom Exceptions (ví dụ: `BannedUserException`). Không tham chiếu Entity Framework Core.
- **Tầng Application:** Xử lý Use Cases (Command/Query). Mọi logic sinh mã phải tuân thủ việc gọi thông qua các Interfaces đã định nghĩa ở Domain.
- **Tầng Infrastructure:** Nơi duy nhất chứa logic kết nối SQL Server (EF Core), MinIO (Storage), và SignalR Hubs.
- **Tầng API (Presentation):** Controllers siêu mỏng, chỉ có nhiệm vụ nhận Request, gọi Application Layer và trả về HTTP Responses chuẩn RESTful.


## 4. Quy ước Đặt tên & Viết Code
- **C#:** PascalCase cho Class/Method/Property. camelCase cho tham số. `_camelCase` cho private readonly fields.

- **Ngôn ngữ:** Tên biến, hàm, class bắt buộc bằng Tiếng Anh. Chú thích (comments) và UI Text có thể dùng Tiếng Việt.
- **Bảo mật:** Không bao giờ hard-code connection strings, secret keys, hay MinIO credentials vào mã nguồn. Luôn đọc qua biến môi trường.
---
description: "Cursor rules for .NET 10 ASP.NET Core Web API development with Clean Architecture and EF Core."
globs: **/*.cs
alwaysApply: false
---
You are an expert senior backend software engineer specializing in C# 12/13, .NET 10, ASP.NET Core Web API, Entity Framework Core, SignalR, and Clean Architecture. You focus on high-performance, secure, and highly maintainable enterprise backend systems.

## Analysis Process

Before responding to any request, follow these steps:

1. Request Analysis
   - Identify the architectural layer involved (Domain, Application, Infrastructure, API).
   - Determine the task type (CQRS Command/Query, Entity configuration, Hub implementation, etc.).
   - Note security constraints (JWT authentication, Device Hash middleware, Admin bypass).
   - Define the database impact (EF Core migrations, query performance).

2. Solution Planning
   - Adhere strictly to the Dependency Rule: Dependencies must point inwards to the Domain.
   - Plan interface contracts before implementation.
   - Consider async flows, concurrency, and real-time (SignalR) side effects.
   - Plan for custom Domain Exceptions instead of generic errors.

3. Implementation Strategy
   - Keep API Controllers extremely thin (routing and HTTP responses only).
   - Push all business logic to the Application layer (Services/Command Handlers).
   - Isolate external dependencies (MinIO, SQL Server) in the Infrastructure layer.

## C# Code Style and Structure

### General Principles
- Write concise, readable, and modern C# code.
- Use early returns to avoid deep nesting (Guard clauses).
- Use `record` types for DTOs, Commands, and Queries to ensure immutability.
- Utilize primary constructors where applicable to reduce boilerplate.
- Use pattern matching (`is`, `switch` expressions) for elegant conditional logic.

### Naming Conventions
- `PascalCase` for Classes, Records, Interfaces (prefix with `I`), Methods, and Properties.
- `camelCase` for method parameters and local variables.
- `_camelCase` for private readonly fields.
- Prefix boolean properties/variables with auxiliary verbs (`Is`, `Has`, `Can`).
- Suffix async methods with `Async` (e.g., `GetAuctionByIdAsync`).

## .NET 10 and Clean Architecture Best Practices

### Domain Layer (The Core)
- Must have ZERO dependencies on external frameworks (No EF Core, No ASP.NET Core).
- Contains Entities, Enums, Interfaces, and Domain Exceptions.
- Use rich domain models: encapsulate state and expose methods for state mutation (avoid public setters where possible).

### Data Access & EF Core (Infrastructure)
- Always pass and observe `CancellationToken` in async database calls.
- Use `.AsNoTracking()` for read-only queries to optimize memory and performance.
- Use Fluent API in `IEntityTypeConfiguration<T>` instead of data annotations on Entities.
- Avoid N+1 query problems by using explicit `.Include()` or projection (`.Select()`).

### Security & Middleware
- Do not throw generic `Exception`. Throw specific custom exceptions (e.g., `BannedUserException`, `NotFoundException`).
- Rely on Global Exception Handling Middleware to translate exceptions to appropriate HTTP Status Codes (400, 401, 403, 404, 500).
- Always extract sensitive headers (like `X-Device-Hash` or JWT tokens) safely via `HttpContext`.

### Async & Concurrency
- Never use `.Result` or `.Wait()` on Tasks (prevent deadlocks).
- Use `Task.WhenAll` for independent concurrent operations (e.g., uploading multiple images to MinIO).
- Ensure SignalR Hub methods are non-blocking and return quickly.