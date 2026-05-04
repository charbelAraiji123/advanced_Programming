using Final_Project_Adv.Infrastructure.Data;
using Final_Project_Adv.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────────────────────
// 1. DATABASE
// ─────────────────────────────────────────────────────────────────────────────

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 33))
    ));

// ─────────────────────────────────────────────────────────────────────────────
// 2. SERVICES
// ─────────────────────────────────────────────────────────────────────────────

builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddSingleton<JwtService>();

builder.Services.AddScoped<IAdminServices, AdminServices>();
builder.Services.AddScoped<IManagerServices, ManagerServices>();
builder.Services.AddScoped<IEmployeeServices, EmployeeServices>();
builder.Services.AddScoped<ProgressService>();
// ─────────────────────────────────────────────────────────────────────────────
// 3. JWT AUTHENTICATION
// ─────────────────────────────────────────────────────────────────────────────

var jwtKey = builder.Configuration["JwtSettings:SecretKey"]!;
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]!;
var jwtAudience = builder.Configuration["JwtSettings:Audience"]!;

builder.Services.AddAuthentication(options =>
{
    // Cookie is the default for MVC views (login page etc.)
    options.DefaultScheme = "CookieAuth";
    options.DefaultChallengeScheme = "CookieAuth";
})
.AddCookie("CookieAuth", options =>
{
    options.LoginPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero  // no grace period on expiry
    };
});

// ─────────────────────────────────────────────────────────────────────────────
// 4. SESSION
// ─────────────────────────────────────────────────────────────────────────────

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ─────────────────────────────────────────────────────────────────────────────
// 5. MVC + JSON
// ─────────────────────────────────────────────────────────────────────────────

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Enums as strings in API responses (e.g. "Pending" not 0)
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ─────────────────────────────────────────────────────────────────────────────
// 6. SWAGGER WITH JWT SUPPORT
// ─────────────────────────────────────────────────────────────────────────────

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Final Project Advanced API",
        Version = "v1",
        Description = "Task Management System — use /api/Account/Login to get a JWT token, " +
                      "then click Authorize and paste it."
    });

    // Define the Bearer security scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description =
            "Paste your JWT token here.\n\n" +
            "Get one by calling POST /api/Account/Login with your email and password."
    });

    // Apply it globally to every endpoint
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─────────────────────────────────────────────────────────────────────────────
// 7. BUILD THE APP
// ─────────────────────────────────────────────────────────────────────────────

var app = builder.Build();

// ─────────────────────────────────────────────────────────────────────────────
// 8. MIDDLEWARE PIPELINE  (order matters)
// ─────────────────────────────────────────────────────────────────────────────

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Final Project API v1");
        options.DisplayRequestDuration();         // shows how long each call took
        options.DefaultModelsExpandDepth(-1);     // collapse schema section by default
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();          // ← must be before Authentication

app.UseAuthentication();
app.UseAuthorization();

// ─────────────────────────────────────────────────────────────────────────────
// 9. ROUTES
// ─────────────────────────────────────────────────────────────────────────────

// MVC convention route (for Razor views — Account/Login, Manager/Dashboard etc.)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Attribute-routed API controllers ([Route("api/...")])
app.MapControllers();

app.Run();