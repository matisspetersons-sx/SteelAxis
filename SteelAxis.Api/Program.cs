using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using SteelAxis.Directory;
using SteelAxis.Directory.Interfaces;
using SteelAxis.Directory.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Entra External ID (CIAM) authentication for API
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(
        options => builder.Configuration.Bind("AzureAdB2C", options),
        options =>
        {
            builder.Configuration.Bind("AzureAdB2C", options);
            options.RequireHttpsMetadata = false;
        });

// Add authorization
builder.Services.AddAuthorization();

// Add HttpContextAccessor for tenant resolution
builder.Services.AddHttpContextAccessor();

// Add Directory Database Context
builder.Services.AddDbContext<DirectoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DirectoryConnection")));

// Register Directory Services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ITenantManagementService, TenantManagementService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
