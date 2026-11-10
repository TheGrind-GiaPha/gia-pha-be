# VGMS Backend — Gia Phả Management System

Backend khung cho **Vietnamese Genealogy Management System (VGMS)**, dựng theo cấu trúc **3-layer + Repository + DAO** của môn **PRN232**, không dùng Clean Architecture tự phát.

> Trạng thái: **Skeleton**. Hiện chỉ có lát cắt `Clan` để chứng minh luồng 3 layer, Repository, DAO, Singleton, EF Core migration và CRUD API. ERD chi tiết (Person, Union, ParentChild, Branch, ChangeRequest, AuditLog...) sẽ được bổ sung ở bước tiếp theo.

---

## 1. Kiến trúc 3 lớp

```
┌─────────────────────────────────────────────────────────────┐
│ Presentation Layer                                          │
│ APIs/Controllers/ClansController.cs                         │
│  - nhận HTTP request, validate DTO, trả HTTP response       │
│  - KHÔNG truy cập DbContext, KHÔNG chứa business logic      │
└──────────────────────────────┬──────────────────────────────┘
                               │ IClanService
┌──────────────────────────────▼──────────────────────────────┐
│ Business Logic Layer                                        │
│ Services/Implementations/ClanService.cs                     │
│  - validate nghiệp vụ, check trùng tên, map DTO             │
│  - điều phối RepositoryManager                              │
└──────────────────────────────┬──────────────────────────────┘
                               │ IRepositoryManager
┌──────────────────────────────▼──────────────────────────────┐
│ Data Access Layer                                           │
│ Repositories/  →  DataAccessObjects/DAOs/  →  DbContext     │
│  - Repository: abstraction cho business layer               │
│  - DAO: thao tác EF Core trực tiếp                          │
│  - DbContext: mapping entity ↔ SQL Server                   │
└──────────────────────────────┬──────────────────────────────┘
                               │
                        SQL Server (LocalDB)
```

### Luồng xử lý một request

```
HTTP Request
  → ClansController
  → IClanService (ClanService)
  → IRepositoryManager
  → IClanRepository (ClanRepository)
  → IClanDao (ClanDao)
  → GiaPhaDbContext
  → SQL Server
```

**Quy tắc bất biến:**

- Controller không được inject `GiaPhaDbContext` hoặc `IClanDao`.
- Service không được inject `GiaPhaDbContext`.
- Repository không chứa business logic; chỉ truy vấn/persist.
- DAO không chứa business logic; chỉ thao tác EF Core.
- Chỉ `RepositoryManager.SaveAsync()` gọi `SaveChangesAsync()`.

---

## 2. Cấu trúc solution

```
gia-pha-be/
├── GiaPha.sln
├── BusinessObjects/                 # Entity + DTO + exception dùng chung
│   ├── Entities/
│   │   ├── BaseEntity.cs
│   │   └── Clan.cs
│   ├── DTOs/Clans/
│   │   ├── ClanDto.cs
│   │   ├── CreateClanRequest.cs
│   │   └── UpdateClanRequest.cs
│   └── Exceptions/
│       ├── ConflictException.cs
│       └── NotFoundException.cs
├── DataAccessObjects/               # DbContext + DAO + EF configuration + migrations
│   ├── Context/GiaPhaDbContext.cs
│   ├── Configurations/ClanConfiguration.cs
│   ├── DAOs/
│   │   ├── IGenericDao.cs
│   │   ├── GenericDao.cs
│   │   ├── IClanDao.cs
│   │   └── ClanDao.cs
│   ├── Migrations/
│   └── Extensions/DataAccessServiceCollectionExtensions.cs
├── Repositories/                    # Repository pattern
│   ├── Interfaces/
│   │   ├── IRepositoryBase.cs
│   │   ├── IClanRepository.cs
│   │   └── IRepositoryManager.cs
│   ├── Implementations/
│   │   ├── RepositoryBase.cs
│   │   ├── ClanRepository.cs
│   │   └── RepositoryManager.cs
│   └── Extensions/RepositoryServiceCollectionExtensions.cs
├── Services/                        # Business logic + Singleton logger
│   ├── Interfaces/IClanService.cs
│   ├── Implementations/ClanService.cs
│   ├── Logging/
│   │   ├── ILoggerManager.cs
│   │   └── LoggerManager.cs
│   └── Extensions/ServiceCollectionExtensions.cs
├── APIs/                            # Presentation layer + composition root
│   ├── Controllers/ClansController.cs
│   ├── Middleware/GlobalExceptionHandler.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── Properties/launchSettings.json
├── Tests/                           # xUnit + Moq
│   └── Services/ClanServiceTests.cs
├── .github/workflows/backend-ci.yml
├── Directory.Build.props
├── .editorconfig
├── .gitignore
└── README.md
```

### Quan hệ phụ thuộc

```
APIs ───────────────► Services ───────► Repositories ───────► DataAccessObjects
  │                      │                    │                      │
  └──────────────────────┴────────────────────┴──────────────────────► BusinessObjects
```

- `BusinessObjects` không phụ thuộc project nào.
- `DataAccessObjects` → `BusinessObjects`.
- `Repositories` → `BusinessObjects` + `DataAccessObjects`.
- `Services` → `BusinessObjects` + `Repositories`.
- `APIs` → cả 4 project (composition root).
- `Tests` → `BusinessObjects`, `Repositories`, `Services`.

---

## 3. Repository + DAO + Singleton

### Repository pattern

`IRepositoryBase<T>` giữ đúng mẫu trong slide PRN232:

```csharp
IQueryable<T> FindAll(bool trackChanges);
IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges);
void Create(T entity);
void Update(T entity);
void Delete(T entity);
```

`RepositoryManager` gom repository và là nơi duy nhất gọi `SaveChangesAsync`:

```csharp
public interface IRepositoryManager
{
    IClanRepository Clan { get; }
    Task<int> SaveAsync(CancellationToken cancellationToken = default);
}
```

### DAO

`IGenericDao<T>` / `GenericDao<T>` bọc EF Core; `ClanDao` là DAO riêng cho `Clan`.

```
ClanRepository → IClanDao → GenericDao<Clan> → GiaPhaDbContext
```

### Singleton pattern

`ILoggerManager` được đăng ký **Singleton** trong `Services/Extensions/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<ILoggerManager, LoggerManager>();
```

Lý do: logger là stateless, dùng chung toàn ứng dụng. Các thành phần phụ thuộc `DbContext` (`RepositoryManager`, repository, service) được đăng ký **Scoped**, không phải Singleton — nếu đăng ký Singleton sẽ gây lỗi lifetime và rò rỉ DbContext.

---

## 4. Yêu cầu môi trường

- **.NET 8 SDK** (runtime 8.0+)
- **SQL Server LocalDB** hoặc SQL Server 2016+
- `dotnet-ef` local tool (đã khai báo trong `dotnet-tools.json`)

Kiểm tra LocalDB:

```powershell
sqllocaldb info
```

Nếu chưa có LocalDB, cài SQL Server Express LocalDB hoặc đổi `DefaultConnection` sang SQL Server thật.

---

## 5. Chạy dự án

### 5.1. Restore tool + package

```powershell
cd C:\Users\ThienNLH3\Desktop\gia-pha-be
dotnet tool restore
dotnet restore GiaPha.sln
```

### 5.2. Tạo database + migration

```powershell
dotnet tool run dotnet-ef database update `
  --project DataAccessObjects/GiaPha.DataAccessObjects.csproj `
  --startup-project APIs/GiaPha.APIs.csproj
```

Lệnh này tạo database `GiaPha` trong LocalDB và seed 2 dòng họ mẫu.

Nếu muốn tạo lại migration:

```powershell
dotnet tool run dotnet-ef migrations add TenMigration `
  --project DataAccessObjects/GiaPha.DataAccessObjects.csproj `
  --startup-project APIs/GiaPha.APIs.csproj `
  --output-dir Migrations
```

### 5.3. Chạy API

```powershell
dotnet run --project APIs/GiaPha.APIs.csproj
```

- API: <http://localhost:5000>
- Swagger: <http://localhost:5000/swagger>
- Health: <http://localhost:5000/health>

### 5.4. Chạy test

```powershell
dotnet test GiaPha.sln
```

---

## 6. API hiện có

| Method | Route | Mô tả |
|---|---|---|
| `GET` | `/health` | Health check + DB check |
| `GET` | `/api/clans?search=` | Danh sách dòng họ, tìm theo tên |
| `GET` | `/api/clans/{id}` | Chi tiết dòng họ |
| `POST` | `/api/clans` | Tạo dòng họ |
| `PUT` | `/api/clans/{id}` | Cập nhật dòng họ |
| `DELETE` | `/api/clans/{id}` | Xóa dòng họ |

Ví dụ:

```http
POST http://localhost:5000/api/clans
Content-Type: application/json

{
  "name": "Gia phả họ Lê",
  "description": "Dòng họ Lê tại Nam Định"
}
```

---

## 7. Những gì chưa làm (cố ý)

Skeleton này **chưa** gắn các phần sau; sẽ làm ở các task tiếp theo:

- **JWT/Identity** (đăng nhập, phân quyền theo clan/chi).
- **OData** endpoint (nếu cần cho đúng yêu cầu assignment).
- **ERD đầy đủ**: `Person`, `Union`, `ParentChild`, `Branch`, `ChangeRequest`, `AuditLog`, `ClanEvent`...
- **Clan isolation** (global query filter theo `ClanId`).
- **Import Excel**, **phả đồ PDF**, **lunar calendar**, **kinship lookup**.
- **Frontend React** (repo `gia-pha-fe`).

---

## 8. Quy ước Git

- `main`: nhánh ổn định, demo.
- `develop`: nhánh tích hợp.
- `feature/<ten-task>`: nhánh tính năng, PR vào `develop`.
- Commit message: `feat:`, `fix:`, `chore:`, `docs:`, `test:`.

Repo: <https://github.com/TheGrind-GiaPha/gia-pha-be>

---

## 9. Ghi chú về PRN232

Cấu trúc này bám theo yêu cầu assignment:

- 3-layer architecture.
- Repository pattern + DAO, không truy cập DB trực tiếp từ controller.
- Singleton pattern (`ILoggerManager`).
- Entity Framework Core + SQL Server.
- Data annotations validation.
- DTO tách khỏi entity.
- Solution BE riêng; frontend sẽ nằm ở solution/repo `gia-pha-fe`.

Khi làm Assignment 01/02, có thể bổ sung `SystemAccount`, `NewsArticle`, `Category`, `Tag`, `NewsTag` theo đúng sơ đồ database trong file assignment. Không trộn các entity đó vào domain VGMS nếu không cần.
