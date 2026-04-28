
using Microsoft.EntityFrameworkCore;
using Moka.src.Authentication.Domain.Interfaces;
using Moka.src.Authentication.Infrastructure;
using Moka.src.Authentication.Services;
using Moka.src.Brokerage.Application.Interfaces;
using Moka.src.Brokerage.Application.Services;
using Moka.src.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<IBrokerageService, BrokerageService>();

builder.Services.AddControllers();
var app = builder.Build();

app.MapControllers();
app.UseHttpsRedirection();
app.Run();
