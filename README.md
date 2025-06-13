# RaftlabAssignment (Using .NET Core 3.1)

This project is a sample `.NET Core 3.1` solution simulating a service that fetches and processes user data from an external public API ([https://reqres.in](https://reqres.in)). It demonstrates proper API client design using `HttpClient`, resilience with `Polly`, and in-memory caching. The project follows the repository and service pattern, along with Clean Architecture principles.

---

## 🧱 Solution Structure

```
RaftlabAssignment.sln
│
├── RaftlabAssignment.Domain          # Domain models (eg. User)
│   └── Entities
│
├── RaftlabAssignment.Infrastructure  # API client implementation
│   ├── Configuration
│   ├── Interfaces
│   ├── Models
│   └── Services
│
├── RaftlabAssignment.Api      # Demo console app for running the service (startup project to run Api directly)
│
└── RaftlabAssignment.Tests           # xUnit + Moq unit tests
```

---

## ✅ Prerequisites

- [.NET Core SDK 3.1](https://dotnet.microsoft.com/en-us/download/dotnet/3.1)
- Visual Studio 2022 (or later)

---

## 🚀 How to Build, Run & Test

### 🛠️ Build

```bash
dotnet build
```

Or via Visual Studio → Build Solution (`Ctrl+Shift+B`)

---

### ▶️ Run the Console App

```bash
cd RaftlabAssignment.Api
dotnet run
```

This will make a real-time call to `https://reqres.in/api/users` and display users.

---

### 🧪 Run Tests

```bash
cd RaftlabAssignment.Tests
dotnet test
```

Tests use **xUnit** and **Moq** to verify service behavior.

---

## 💡 Design Decisions

- **HttpClient via constructor injection**
- **Service pattern** — All business logic encapsulated in `ExternalUserService`.
- **DTOs vs Domain Models** — Raw API responses (`ApiUser`) mapped to internal `User` models.
- **Async/Await** — All external I/O is fully asynchronous.
- **Clean Architecture** — Core logic separated from infrastructure concerns.

---

## 🔁 Polly Integration (Retry Logic)

- **Polly** is used via a named `HttpClient` policy (e.g., retry on transient failures).
- Configured in `Startup` registration.
- Retry logic ensures:
  - Every request using this named client will retry up to 3 times on failure.
  - No need to manually retry in your code — Polly handles it.

```csharp
  services.AddHttpClient<IExternalUserService, ExternalUserService>()
      .AddPolicyHandler(GetRetryPolicy());
```

```csharp
  private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
  {
      return HttpPolicyExtensions
          .HandleTransientHttpError()
          .WaitAndRetryAsync(3, retryAttempt =>
              TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
  }
```
---

## 🧠 Caching (In-Memory)

- Uses `IMemoryCache` injected into the service.
- User data is cached by ID (`User_1`, `User_2`, etc.) with a 10-minute expiration.
- Prevents repeated API calls for the same user during the cache window.

```csharp
_cache.Set(cacheKey, user, TimeSpan.FromMinutes(10));
```

---

## 📁 Configuration

API base URL is configurable via `ApiSettings`:

```json
// appsettings.json
{
  "ApiSettings": {
    "BaseUrl": "https://reqres.in/api/",
    "ApiKey": "reqres-free-v1"
  }
}
```

Injected via `IOptions<ApiSettings>` into the service.

---

## 🧑‍💻 Author

Developed by Ravi Bhushan for Raftlab .NET Developer Technical Assessment.

---

## 📜 License

This is a test/demo project — no license required. Use for educational or evaluation purposes.
