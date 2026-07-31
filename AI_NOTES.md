# AI Notes & Development Disclosure

This document provides a transparent overview of how AI tools were used during the development of the **Smart Expense Tracker API**, highlighting what I planned and wrote myself, what was AI-assisted, how I refactored AI outputs, and the technical suggestions I chose to reject.

---

## 1. Code Ownership: Human-Written vs. AI-Assisted

### Written & Planned by Me (Human)
- **DTOs, Domain Models & Constants**: I planned and wrote the entire domain model (`Expense`, `ExpenseStorageSettings`), DTOs (`CreateExpenseRequestDto`, `ExpenseResponseDto`, `ExpenseSummaryResponseDto`, `CategoryExpenseSummaryDto`, `PagedRequestDto`, `PagedResponseDto`), and central constants (`ValidationMessages`, `JsonFileNames`).
- **Layered Architecture & Interfaces**: I designed the 5-project layered architecture and enforced strict interface segregation by organizing contracts and logic into `Abstractions` and `Implementations` folders across all layers.
- **Service & Store Implementations**: I wrote the business logic in `ExpenseService`, repository operations in `ExpenseStore`, and file handling contracts.
- **Thread-Safe Concurrency (Locking)**: The initial AI sample suggested simple direct file reading and writing. Realizing the risk of file lock collisions under concurrent API calls, I designed and implemented `SemaphoreSlim` async locking inside `JsonFileProvider` to ensure thread-safe file I/O.

### AI-Assisted Components
- **Swagger Configuration**: AI assisted with boilerplate options setup for OpenAPI and Swagger UI.
- **Automated xUnit Tests**: AI assisted with scaffolding automated unit test cases inside `SmartExpenseTracker.Tests` to test repository and service methods.
- **Planning Refinement**: I used AI as a sounding board to review my architectural phase breakdown before writing code.

---

## 2. What I Validated, Tested, and Changed in AI Outputs

- **Manual API Testing & Adding Pagination for Scalability**:
  I manually executed and tested all endpoints in Swagger UI. While the initial AI suggestion returned simple unpaginated lists, I realized that returning all records at once wouldn't scale well for large datasets. I refactored the retrieval endpoints to support optional **Pagination** (`PagedResponseDto<T>`) with metadata (`pageNumber`, `pageSize`, `totalItems`, `totalPages`, `hasNextPage`, `hasPreviousPage`).

- **Centralized Global Exception Handling**:
  Instead of wrapping every controller action in redundant `try-catch` blocks, I refactored error handling into a single `ExceptionHandlingMiddleware` that intercepts custom exceptions and formats clean HTTP 400 (Bad Request), 404 (Not Found), and 500 (Internal Server Error) JSON responses.

- **Strict DTO vs. Domain Model Separation**:
  Instead of using models directly in API controllers or using DTOs for data persistence, I strictly separated domain models (`Expense`) for internal data storage from client-facing DTOs for external API communication.

- **Centralized Validation & DataAnnotations**:
  I replaced inline validation strings with a centralized `ValidationMessages` constant class and applied DTO-level DataAnnotation attributes (`[Required]`, `[StringLength]`, `[Range]`) to catch invalid inputs at the API border.

---

## 3. AI Suggestions I Decided NOT to Use (and Why)

1. **In-Memory Data Storage**:
   - **Reason**: Storing data purely in-memory loses all expenses whenever the application restarts or reloads. I chose file-backed JSON storage (`expenses.json`) so data persists reliably across application restarts.

2. **Unvalidated DTOs / Raw Requests**:
   - **Reason**: Allowing unvalidated DTOs into the service layer can cause corrupted data. I enforced strict DataAnnotation attributes on all DTOs to reject bad requests at the API boundary.

3. **Heavy Database / EF Core Setup**:
   - **Reason**: Suggestions to introduce Entity Framework Core or SQL Server were rejected to strictly respect assignment constraints (*"NO DATABASE"*) and avoid over-engineering.
