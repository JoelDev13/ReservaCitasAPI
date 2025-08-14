using Application.Interfaces;
using Application.Servicios;
using Domain.Interfaces;
using Infrastructure.Persistencia.Contexto;
using Infrastructure.Persistencia.Repositorios;
using Infrastructure.Seguridad;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ProyectoFinal.Domain.Interfaces;
using ProyectoFinal.Infrastructure.Repositories;
using System.Security.Claims;
using Infrastructure.Persistencia.Repositorios.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//aqui consiguo la cadena de conexion de appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// aqui inyecto el dbcontext y lo paso a la cadena
builder.Services.AddDbContext<CitasDbContext>(options =>
    options.UseNpgsql(connectionString));


// Inyeccion del repositorio y UnitOfWork
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IConfiguracionService, ConfiguracionService>();
builder.Services.AddScoped<Application.Servicios.CitaService>();

// Lee la clave JWT desde configuracion
var claveSecreta = builder.Configuration["JWT:ClaveSecreta"];
// Registra tu servicio de generacion de JWT
builder.Services.AddSingleton<IGeneradorJWT>(new GeneradorJWT(claveSecreta));

builder.Services.AddScoped<IAutenticacionService, ServiceAutenticacion>();
builder.Services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();



// aqui convierto la cadena secreta a un arreglo de bytes
var key = System.Text.Encoding.UTF8.GetBytes(claveSecreta);



builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });


var app = builder.Build();


// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
