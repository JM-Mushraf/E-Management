# Smart Expense Tracker API

Hi! Welcome to my submission for the **Diligent Software Engineering Apprenticeship 2026** take-home assessment.

I designed and built this RESTful API using **ASP.NET Core (.NET 10)** and **C#**. The goal of this application is to manage personal and business expenses—supporting creation, category filtering, keyword searching, monthly and category summary calculations, and deletion—while showcasing clean software architecture, solid engineering practices, and maintainability without introducing unnecessary complexity.

---

## My Architectural Design & Philosophy

Even though this is a lightweight assignment, I chose to organize the codebase into a proper **Layered Clean Architecture**. I wanted to demonstrate how to build scalable .NET applications while intentionally avoiding over-engineering (no heavy databases, EF Core, Redis, or Docker containers).

### How Requests Flow Through My System

```text
Client (Swagger / Postman)
       ↓
src/SmartExpenseTracker (Web API & Controllers)
       ↓
src/SmartExpenseTracker.Service (Business Logic, Summaries, Search & Validations)
       ↓
src/SmartExpenseTracker.Store (Expense Repository & Persistence Abstractions)
       ↓
src/SmartExpenseTracker.Data (Low-level JSON File I/O with Thread Safety)
       ↓
expenses.json (Local File Storage)
```

Shared models, DTOs, custom exception types, and constants live inside `src/SmartExpenseTracker.Common`, which acts as the shared kernel for all layers.

---

## Project Structure & Layer Breakdown

Per Diligent's exact submission structure requirements, the solution is organized with source projects inside `src/` and test suites inside `tests/`:

```text
your-repo/
├── README.md                           # Documentation, setup, run & test guide
├── AI_NOTES.md                          # Mandatory AI collaboration disclosure
├── SmartExpenseTracker.slnx            # .NET Solution file
│
├── src/                                # Source code directory
│   ├── SmartExpenseTracker/            # Web API & Presentation Layer
│   │   ├── Controllers/
│   │   │   └── ExpensesController.cs   # REST API endpoints (v1)
│   │   ├── Middleware/
│   │   │   └── ExceptionHandlingMiddleware.cs # Global error handler (400, 404, 500)
│   │   └── Program.cs                  # Dependency Injection & Swagger configuration
│   │
│   ├── SmartExpenseTracker.Common/     # Shared Kernel & DTOs
│   ├── SmartExpenseTracker.Data/       # Low-level JSON File I/O with SemaphoreSlim locking
│   ├── SmartExpenseTracker.Store/      # Repository Store Layer
│   └── SmartExpenseTracker.Service/    # Business Logic & Calculations
│
└── tests/                              # Test suite directory
    └── SmartExpenseTracker.Tests/      # xUnit Test Suite
```

---

## Key Features Implemented

1. **Add Expense (`POST /api/v1/expenses`)**: Creates an expense entry with model validation (`Title`, `Amount > 0`, `Category`).
2. **Paginated Expenses List (`GET /api/v1/expenses`)**: Returns paginated expense results (`PagedResponseDto<T>`) with metadata (`pageNumber`, `pageSize`, `totalItems`, `totalPages`, `hasNextPage`, `hasPreviousPage`).
3. **Search Expenses (`GET /api/v1/expenses/search?query=chair`)**: Case-insensitively searches expense titles or categories.
4. **Filter by Category (`GET /api/v1/expenses?category=Food` or `GET /api/v1/expenses/category/Food`)**: Filters expenses by category name.
5. **Category Summary (`GET /api/v1/expenses/summary`)**: Calculates overall total amount, total expense count, and per-category breakdown (`TotalAmount` and `Count`).
6. **Monthly Summary (`GET /api/v1/expenses/summary/monthly?year=2026`)**: Groups expenses by year and month, providing monthly totals, item counts, and month names.
7. **Delete Expense (`DELETE /api/v1/expenses/{id}`)**: Deletes an expense entry by Guid, returning `204 No Content` or `404 Not Found` if missing.
8. **Global Exception Handling**: Thin controller delegates exception handling to central middleware.

---

## How to Run My Application

### Prerequisites
Make sure you have the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) installed on your machine.

### 1. Build the Solution
Open your terminal in the root folder and run:
```bash
dotnet build
```

### 2. Run the Web API
You can run the API directly from the root folder using:
```bash
dotnet run --project src/SmartExpenseTracker
```

Alternatively, navigate into the project directory:
```bash
cd src/SmartExpenseTracker
dotnet run
```

### 3. Open Swagger UI
Once running, open your web browser to test the API interactively:
- **HTTPS**: [https://localhost:7136](https://localhost:7136)
- **HTTP**: [http://localhost:5230](http://localhost:5230)

*Note: Swagger UI is configured as the root landing page (`/`), so opening either URL will load Swagger UI immediately.*

---

## How to Run My Unit Tests

I wrote an xUnit test suite covering data persistence, file storage, service business logic, keyword search, monthly math, and error handling.

To execute all tests from the root directory, run:
```bash
dotnet test
```

---

## API Reference (v1)

| HTTP Method | Route Endpoint | Description |
| :--- | :--- | :--- |
| **`POST`** | `/api/v1/expenses` | Creates a new expense entry (returns `201 Created`). |
| **`GET`** | `/api/v1/expenses` | Gets paginated expenses (accepts `?pageNumber=1&pageSize=10&category=Food`). |
| **`GET`** | `/api/v1/expenses/search` | Searches expenses by title or category keywords (`?query=chair`). |
| **`GET`** | `/api/v1/expenses/category/{category}` | Gets paginated expenses filtered by category route parameter. |
| **`GET`** | `/api/v1/expenses/summary` | Calculates overall total, item count, and category breakdown. |
| **`GET`** | `/api/v1/expenses/summary/monthly` | Calculates monthly totals grouped by year and month (`?year=2026`). |
| **`DELETE`** | `/api/v1/expenses/{id}` | Deletes expense by unique Guid ID (returns `204 NoContent`). |

### Sample JSON Payload (`POST /api/v1/expenses`)
```json
{
  "title": "Ergonomic Desk Chair",
  "amount": 299.99,
  "category": "Office Supplies",
  "date": "2026-07-31T10:00:00.000Z"
}
```

---

## Data Storage Implementation

Per the assignment specifications, I did not use a database server or EF Core. Instead, data is stored in a local `expenses.json` file in the application execution directory. 

To make `expenses.json` behave like a reliable database:
- Missing files are automatically created with an empty `[]` array.
- File reads/writes use `SemaphoreSlim` async locks to ensure thread safety and prevent file lock collisions under concurrent API calls.
- Malformed or corrupt JSON files are handled gracefully without crashing the server.

Thank you for reviewing my project! Feel free to explore the code or run the test suite.
