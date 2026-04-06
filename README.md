# Transportation Management System
### Smart Transportation & Fleet Operational Management System
**ASP.NET Core MVC | C# | Entity Framework Core | SQL Server | ASP.NET Identity**

---

## Project Structure

```
TransportationManagement/
├── Properties/
│   └── launchSettings.json
├── wwwroot/
│   ├── css/
│   │   └── site.css
│   └── js/
│       └── site.js
├── Controllers/
│   ├── AccountController.cs
│   ├── AdminController.cs
│   ├── DriverController.cs
│   ├── FuelController.cs
│   ├── HomeController.cs
│   ├── MaintenanceController.cs
│   ├── TripController.cs
│   └── VehicleController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DbInitializer.cs
├── Interfaces/
│   ├── IDriverRepository.cs
│   ├── IFuelRepository.cs
│   ├── IMaintenanceRepository.cs
│   ├── ITripRepository.cs
│   └── IVehicleRepository.cs
├── Models/
│   ├── ApplicationUser.cs
│   ├── Driver.cs
│   ├── FuelEntry.cs
│   ├── MaintenanceRecord.cs
│   ├── Trip.cs
│   └── Vehicle.cs
├── Repositories/
│   ├── DriverRepository.cs
│   ├── FuelRepository.cs
│   ├── MaintenanceRepository.cs
│   ├── TripRepository.cs
│   └── VehicleRepository.cs
├── Services/
│   ├── DriverService.cs
│   ├── FuelService.cs
│   ├── MaintenanceService.cs
│   ├── TripService.cs
│   └── VehicleService.cs
├── ViewModels/
│   └── ViewModels.cs
├── Views/
│   ├── Account/Login.cshtml
│   ├── Admin/Dashboard.cshtml, Users.cshtml, CreateUser.cshtml, EditUser.cshtml, ...
│   ├── Driver/ (Index, AddDriver, Edit, GetDriverDetails, GetAssignedTrips, Delete)
│   ├── Fuel/ (Index, AddFuelEntry, Edit, GenerateFuelReport, GetFuelConsumption, Delete)
│   ├── Home/Index.cshtml
│   ├── Maintenance/ (Index, ScheduleMaintenance, UpdateServiceRecord, GetMaintenanceHistory, Delete)
│   ├── Trip/ (Index, CreateTrip, UpdateTripStatus, GetTripPlan, Delete)
│   ├── Vehicle/ (Index, AddVehicle, UpdateVehicle, GetVehicleDetails, Delete)
│   └── Shared/_Layout.cshtml
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── TransportationManagement.csproj
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or SQL Server Express or LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended) or VS Code

---

## Setup Steps

### Step 1 — Clone / Extract the project
Extract the zip and open the folder in Visual Studio 2022.

### Step 2 — Configure the database connection
Open **`appsettings.json`** and update the connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TransportationManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

For SQL Server Express, change to:
```
Server=.\\SQLEXPRESS;Database=TransportationManagementDB;Trusted_Connection=True;
```

### Step 3 — Apply Migrations (runs automatically on startup)
The app calls `context.Database.Migrate()` on first run, so the database and tables
are created automatically. If you want to run manually:

```bash
# In Package Manager Console (Visual Studio)
Add-Migration InitialCreate
Update-Database

# OR in terminal
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Step 4 — Run the application

**In Visual Studio:** Press **F5** or click the green Run button.

**In terminal:**
```bash
cd TransportationManagement
dotnet run
```

Open browser at: `http://localhost:5000`

---

## Default Login Credentials

| Role               | Email                    | Password   |
|--------------------|--------------------------|------------|
| Admin              | admin@transport.com      | Admin@123  |

The admin can create additional users with any of the 4 roles from the **Manage Users** screen.

---

## Roles & Access

| Role                  | Access                                          |
|-----------------------|-------------------------------------------------|
| **Admin**             | Full access — dashboard, all modules, user mgmt |
| **FleetManager**      | Vehicles, Drivers, Trips, Maintenance, Fuel     |
| **Driver**            | View & update trip status only                  |
| **MaintenanceEngineer** | Maintenance module only                       |

---

## Modules

1. **Vehicle Registration & Fleet Catalog** — Add/Edit/Delete vehicles, track status (ACTIVE / IN_SERVICE / RETIRED)
2. **Driver Management & Trip Assignment** — Manage drivers, assign trips, view assigned trips per driver
3. **Trip Scheduling & Route Planning** — Create trips with origin, destination, planned route (text), update status (PLANNED / IN_PROGRESS / COMPLETED)
4. **Maintenance Scheduling & Service History** — Schedule service, update service records, view history per vehicle
5. **Fuel Usage Analytics & Compliance Reporting** — Log fuel entries, view consumption per vehicle, generate full fuel report

---

## Architecture

- **MVC Pattern** — Controllers → Services → Repositories → DbContext
- **Repository Pattern** — Each entity has an Interface + Implementation
- **Service Layer** — Business logic decoupled from controllers
- **ASP.NET Identity** — Role-based authentication & authorization
- **Entity Framework Core** — Code-first with migrations

---
