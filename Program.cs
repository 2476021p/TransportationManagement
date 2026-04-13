using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Models;
using TransportationManagement.Interfaces;
using TransportationManagement.Repositories;
using TransportationManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. DB Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity Setup
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireUppercase = true;
	options.Password.RequireNonAlphanumeric = true;
	options.Password.RequiredLength = 6;
	options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. MVC
builder.Services.AddControllersWithViews();

// 4. Session Configuration (Essential for Login)
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(60); // Time penchaanu
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

// 5. Dependency Injection - Repositories
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
builder.Services.AddScoped<IFuelRepository, FuelRepository>();

// 6. Dependency Injection - Services
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

// Sequence is very important here!
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Default Route
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Account}/{action=Login}/{id?}");

// 7. Role Seeding - (DRIVER ROLE ADD CHESANU)
using (var scope = app.Services.CreateScope())
{
	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
	var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

	// Kachithanga "Driver" undali ikkada
	string[] roles = { "Admin", "FleetManager", "Driver", "MaintenanceEngineer" };

	foreach (var role in roles)
	{
		if (!await roleManager.RoleExistsAsync(role))
		{
			await roleManager.CreateAsync(new IdentityRole(role));
		}
	}

	// Optional: Auto-create Admin if not exists
	var adminEmail = "admin@transport.com";
	var adminUser = await userManager.FindByEmailAsync(adminEmail);
	if (adminUser == null)
	{
		var user = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "System Admin" };
		await userManager.CreateAsync(user, "Admin@123");
		await userManager.AddToRoleAsync(user, "Admin");
	}
}

app.Run();