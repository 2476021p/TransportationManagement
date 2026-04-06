using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Models;
using TransportationManagement.Interfaces;
using TransportationManagement.Repositories;
using TransportationManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// DB Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity - FIXED WITH PASSWORD OPTIONS
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
	// Relaxed password rules
	options.Password.RequireDigit = false;
	options.Password.RequireLowercase = false;
	options.Password.RequireUppercase = false;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequiredLength = 6;

	// Email must be unique
	options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// MVC
builder.Services.AddControllersWithViews();

// Session
builder.Services.AddSession();

// Dependency Injection - Repositories
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
builder.Services.AddScoped<IFuelRepository, FuelRepository>();

// Dependency Injection - Services
builder.Services.AddScoped<TripService>();
builder.Services.AddScoped<DriverService>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<MaintenanceService>();
builder.Services.AddScoped<FuelService>();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Default Route
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Account}/{action=Login}/{id?}");

// Role Seeding
using (var scope = app.Services.CreateScope())
{
	var roleManager = scope.ServiceProvider
		.GetRequiredService<RoleManager<IdentityRole>>();

	string[] roles = {
		"Admin",
		"FleetManager",
		"Driver",
		"MaintenanceEngineer"
	};

	foreach (var role in roles)
	{
		if (!await roleManager.RoleExistsAsync(role))
			await roleManager.CreateAsync(new IdentityRole(role));
	}
}

app.Run();
