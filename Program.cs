
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Moka.src.Authentication.Application;
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
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<Moka.src.Authentication.Services.AuthenticationService>();
builder.Services.AddScoped<RegisterUseCase>();
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<IBrokerageService, BrokerageService>();

builder.Services
    .AddAuthentication(JwtAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, JwtAuthenticationHandler>(JwtAuthenticationHandler.SchemeName, null);
builder.Services.AddAuthorization();
builder.Services.AddControllers();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
