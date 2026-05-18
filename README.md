# Math Test System

A containerized, microservices-oriented distributed system built on **.NET 10** for automated mass grading of arithmetic tasks. Designed as an enterprise-grade solution with a strict Microsoft technology stack.

---

## Downloads

Pre-built Windows executables are attached to every [GitHub Release](../../releases/latest). No installation or .NET runtime required — just download and run.

| App | Description |
|---|---|
| `MathTestSystem.TeacherApp.exe` | Upload XML exam files and view grading results |
| `MathTestSystem.StudentApp.exe` | Log in as a student and review your exam analytics |

---

## 1. Quick Start

```bash
# Clone the repository
git clone https://github.com/BojanDimitrovski1710/MathTestSystem.git

# Spin up the full backend (API Gateway, Grading Service, Student Service, SQL Server)
docker compose up -d
```

> **Note:** The API Gateway is the single public entry point at `http://localhost:5000`. Backend services run on an isolated internal Docker network and are not directly reachable from the host. Once the backend is running, open `MathTestSystem.TeacherApp` or `MathTestSystem.StudentApp` in Visual Studio to run the desktop clients.

---

## 2. Tech Stack

| Layer | Technology |
|---|---|
| API Gateway | YARP (Yet Another Reverse Proxy) |
| Backend Services | ASP.NET Core Web API — .NET 10 |
| Security | ASP.NET Core Identity + JWT Bearer |
| Database & ORM | SQL Server 2022 + Entity Framework Core |
| Math Engine | Custom .NET Class Library (zero external dependencies) |
| Desktop Clients | WPF — .NET 10, MVVM pattern |
| Testing | xUnit + NSubstitute |
| CI/CD | GitHub Actions + Docker Compose |

---

## 3. System Architecture & Design Decisions

The system is split across four independently deployable units, each with a single responsibility.

```
┌────────────────────────────────────────────────────┐
│              Docker Internal Network               │
│                                                    │
│  ┌──────────────┐       ┌───────────────────────┐  │
│  │  API Gateway │ --->  │   Grading Service     │  │
│  │  YARP :5000  │       │   (write-heavy)       │  │
│  │  (public)    │       └───────────────────────┘  │
│  │              │       ┌───────────────────────┐  │
│  │              │ --->  │   Student Service     │  │
│  └──────────────┘       │   (read-heavy)        │  │
│         │               └───────────────────────┘  │
│         ▼                        │                 │
│  ┌──────────────┐                │                 │
│  │  SQL Server  │<---------------┘                 │
│  └──────────────┘                                  │
└────────────────────────────────────────────────────┘
         ▲
    WPF Clients
```

**API Gateway (YARP)** — the sole public entry point and the independent integration point for third-party applications. It handles JWT authentication, routes requests to the correct downstream service, and shields internal topology from clients.

**Grading Service** — owns the write path. It accepts aggregated XML files, validates them against an XSD schema, evaluates every arithmetic expression, persists results in bulk, and returns a structured grade report. Expressions are evaluated in parallel with results cached per unique expression string to avoid redundant computation on mass uploads.

**Student Service** — owns the read path. It exposes optimized endpoints for student analytics dashboards and teacher views of student results. It does not perform any grading logic.

**Separation of Write and Read** — keeping grading and analytics in separate services means a large batch upload cannot degrade the responsiveness of the student-facing dashboard. This maps to the CQRS principle applied at the service level.

**Bounded Contexts** — the WPF applications share no assemblies with the backend. They maintain their own local model classes and communicate exclusively over HTTP through the gateway, ensuring client and server can evolve independently.

**Network Isolation** — only the API Gateway publishes a host port (`5000`). Grading Service and Student Service are on the internal bridge network only; they are not reachable without going through the gateway.

---

## 4. Project Structure

```
MathTestSystem/
│
├── Clients/                            # ── CLIENT APPLICATIONS ──────────────────────────
│   ├── MathTestSystem.TeacherApp/      # WPF teacher client — login, XML upload, results
│   │   ├── ViewModels/                 # MVVM — login, upload, results view models
│   │   ├── Views/                      # WPF XAML windows and controls
│   │   ├── Services/                   # HTTP client wrappers calling the gateway
│   │   ├── Models/                     # Local DTOs (no shared assemblies with backend)
│   │   ├── Commands/                   # ICommand implementations
│   │   └── State/                      # Application state management
│   └── MathTestSystem.StudentApp/      # WPF student client — login, analytics dashboard
│       ├── ViewModels/
│       ├── Views/
│       ├── Services/
│       ├── Models/
│       ├── Commands/
│       └── State/
│
├── Gateways/                           # ── API GATEWAY ───────────────────────────────────
│   └── MathTestSystem.ApiGateway/      # YARP reverse proxy — sole public entry point
│       ├── Controllers/                # Auth controller — JWT issuance and login
│       └── Models/                     # Login/token request-response models
│                                       # Route config in appsettings.json (YARP)
│
├── Services/                           # ── BACKEND MICROSERVICES ─────────────────────────
│   ├── MathTestSystem.GradingService/  # Write-heavy — accepts XML, grades, persists
│   │   ├── Controllers/                # POST /api/exams — accepts XML uploads
│   │   ├── Services/                   # ExamGradingService — orchestrates grading pipeline
│   │   ├── Parsing/                    # ExamXmlParser + XmlProcessor — XSD validation
│   │   ├── Models/                     # Grading response models
│   │   └── Schemas/                    # TeacherExam.xsd — enforced XML contract
│   ├── MathTestSystem.GradingService.Tests/
│   ├── MathTestSystem.StudentService/  # Read-heavy — analytics endpoints
│   │   ├── Controllers/                # GET endpoints for students and teachers
│   │   └── Models/                     # Dashboard and summary response models
│   └── MathTestSystem.StudentService.Tests/
│
├── BuildingBlocks/                     # ── SHARED BUILDING BLOCKS ────────────────────────
│   ├── MathTestSystem.Domain/          # Entities, repository interfaces, result codes
│   │   ├── Entities/                   # Teacher, Student, Exam, ExamTask
│   │   ├── Interfaces/                 # Repository contracts
│   │   └── Constants/                  # ResultCodes — structured error strings
│   ├── MathTestSystem.Infrastructure/  # EF Core DbContext, repositories, Identity, JWT
│   │   ├── Data/                       # AppDbContext + AppUser
│   │   ├── Repositories/               # Concrete EF Core repository implementations
│   │   ├── Auth/                       # JWT generation helper
│   │   ├── Extensions/                 # DI service registration helpers
│   │   └── Migrations/                 # EF Core migration history
│   ├── MathTestSystem.MathProcessor/   # Independent math engine (zero NuGet dependencies)
│   │   ├── Services/                   # Tokenizer, Shunting-Yard evaluator
│   │   ├── Models/                     # Token types, EvaluationResult
│   │   └── Interfaces/                 # IExpressionEvaluator
│   └── MathTestSystem.MathProcessor.Tests/
│
├── docker-compose.yml                  # Full backend stack (Gateway + Services + SQL Server)
└── MathTestSystem.slnx                 # Global solution file
```

---

## 5. The Math Engine

The arithmetic processor lives in `MathTestSystem.MathProcessor` — a pure .NET class library with no NuGet dependencies. It is independently testable and reusable.

- **Lexical tokenizer** — scans the raw expression string and produces a typed token stream (numbers, operators, parentheses).
- **Shunting-Yard Algorithm** — converts infix notation to postfix (Reverse Polish Notation), correctly handling operator precedence and associativity (`*` and `/` before `+` and `-`).
- **Stack-based evaluator** — executes the postfix token stream using a `decimal` operand stack, eliminating floating-point precision errors.
- **Result codes** — every failure mode (division by zero, invalid character, mismatched parentheses, empty expression) returns a structured error code rather than an exception, so the grading service can mark a task as errored without crashing the batch.

During mass grading, unique expressions across all students are extracted up front and evaluated in parallel. Results are cached in a `Dictionary<string, EvaluationResult>` so each unique expression is evaluated exactly once regardless of how many students submitted the same task.

---

## 6. XML Format

The system accepts a teacher-scoped aggregated XML file. Multiple students and multiple exams per student are supported in a single upload.

```xml
<Teacher ID="11111">
  <Students>
    <Student ID="12345">
      <Exam Id="1">
        <Task id="1"> 2+3/6-4 = 74 </Task>
        <Task id="2"> 6*2+3-4 = 22 </Task>
      </Exam>
    </Student>
    <Student ID="54321">
      <Exam Id="1">
        <Task id="1"> 2+3 = 5 </Task>
      </Exam>
    </Student>
  </Students>
</Teacher>
```

The file is validated against a compiled XSD schema before any processing begins. Schema violations return a structured `XML_SCHEMA_VALIDATION_FAILED` error immediately.

---

## 7. Engineering Assumptions

The assignment explicitly instructed to make assumptions for unclarities. These are documented here.

**Operator precedence** — expressions follow standard BODMAS/PEMDAS rules. `2+3/6-4` evaluates as `2 + (3/6) - 4 = -1.5`, not left-to-right.

**Fault isolation on partial failures** — if a single task's expression is malformed (e.g. division by zero), that task is marked with an error code and excluded from the score denominator. The rest of the exam and all other students in the batch continue processing normally.

**Multi-teacher student grading** — a student may appear in XML uploads from multiple teachers. Each exam is tagged with the uploading teacher's identity, so the student dashboard correctly groups results by teacher rather than showing only the first teacher the student was registered under.

**Shared database (MVP scope)** — a single SQL Server instance is shared between Grading Service and Student Service. In a production deployment these would be isolated databases synchronized via an event-driven message broker (e.g. Azure Service Bus or RabbitMQ).

**Authentication scope** — teacher and student accounts are created automatically the first time they appear in an uploaded XML. Credentials are `username = ID, password = ID` (e.g. username `12345`, password `12345`). This is intentionally simplified for the MVP.

---

## 8. Testing

Tests are built with **xUnit** and **NSubstitute** and run in CI on every push and pull request to `main` and `dev`.

```bash
# Run all test projects from the solution root
dotnet test
```

| Project | Coverage |
|---|---|
| `MathProcessor.Tests` | Tokenizer, Shunting-Yard, operator precedence, error codes |
| `GradingService.Tests` | XML parsing, schema validation, grading logic, score calculation, persistence |
| `StudentService.Tests` | Controller endpoints, 404 handling, dashboard aggregation, multi-teacher grouping |

The CI pipeline has two jobs — `test` (runs on `ubuntu-latest`) and `build-apps` (runs on `windows-latest` to build the WPF projects which require the Windows SDK).

---

## 9. Running Tests Locally

```bash
dotnet test MathTestSystem.MathProcessor.Tests/MathTestSystem.MathProcessor.Tests.csproj
dotnet test MathTestSystem.GradingService.Tests/MathTestSystem.GradingService.Tests.csproj
dotnet test MathTestSystem.StudentService.Tests/MathTestSystem.StudentService.Tests.csproj
```

---

## 10. Resetting the Database

```bash
# Tear down containers and delete the SQL Server volume
docker compose down -v

# Bring everything back up — migrations apply automatically on startup
docker compose up -d
```
