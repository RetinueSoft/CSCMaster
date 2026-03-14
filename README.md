# CSCMaster

**CSCMaster** is an ASP.NET Core (NET 8) solution that provides a web-based UI and a backend API for managing and importing CSC enrolment data.

## 🧩 Solution Structure

This repository contains two main projects:

- `CSCMasterAPI/` - **ASP.NET Core Web API**
  - Provides authentication (JWT) and endpoints for:
    - user/member login (JWT + refresh tokens)
    - fetching district/member reports
    - uploading enrolment CSV/ZIP files to import enrolment data
  - Uses Entity Framework Core with **SQL Server** (connection string in `appsettings.json`).

- `CSCMasterUI/` - **ASP.NET Core MVC Web UI**
  - Provides web pages and UI controllers for login, dashboards, and enrolment upload.
  - Uses standard MVC views and Razor runtime compilation.

## ⚙️ Key Features

- JWT-based authentication (API)
- Refresh token support (in-memory)
- Upload and import enrolment data via CSV or ZIP (password-protected ZIP)
- District/member reporting endpoints
- Swagger/OpenAPI support for API exploration (enabled in Development)

## 🧱 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local or remote)
- (Optional) Visual Studio 2022/2023 or VS Code

## 🚀 Getting Started

### 1) Configure the database

Update the connection string in `CSCMasterAPI/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS01;Database=CSCMaster;Integrated Security=True;Encrypt=false;"
}
```

Adjust the server name / credentials as needed.

### 2) Apply database migrations

From the root folder, run:

```bash
cd CSCMasterAPI
dotnet ef database update
```

### 3) Run API and UI

#### Option A: Run via Visual Studio

Open `CSCMaster.sln` or `CSCMasterUI.sln` and run both projects (API + UI).

#### Option B: Run via CLI

```bash
cd CSCMasterAPI
dotnet run
```

In a separate terminal:

```bash
cd CSCMasterUI
dotnet run
```

### 4) Explore API

When running in Development, Swagger UI is available at:

```
https://localhost:<api-port>/swagger
```

## 🔐 Authentication

The API uses **JWT Bearer tokens**. The `AuthController` exposes:

- `POST /Auth/login` — authenticate and receive `accessToken` + `refreshToken`
- `POST /Auth/refresh` — refresh an access token using a valid refresh token

> Note: JWT signing key is hardcoded in `CSCMasterAPI/Program.cs` (`M0MjGA3c4w6FhXpavzFwOuDchrBo9JSZ`). For production, replace it with a securely stored secret.

## 🗂️ Enrolment Upload

The endpoint `POST /Enrolments/UploadEnrolmentData` accepts:

- `multipart/form-data` upload with a file (`.csv` or `.zip` containing `.csv`)
- JSON metadata describing the member (sent in form field `Member`)

ZIP files are expected to use the password: `123` (hardcoded). Change this in code if needed.

## 🧩 Notes / TODOs

- Refresh tokens are stored in-memory; the app will lose valid refresh tokens on restart.
- JWT key and ZIP password are hardcoded for development.
- UI currently does not enforce strict authorization; it redirects on HTTP 401.

## 🛠️ Project Structure (High Level)

```
CSCMaster/
  CSCMasterAPI/      # Web API + EF Core models, controllers, services
  CSCMasterUI/       # MVC UI, views, controllers, static content
```

---

If you need help understanding a specific feature (login flow, enrolment import, or report endpoints), let me know and I can add a detailed section to this README.