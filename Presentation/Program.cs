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
using Infrastructure.Persistencia.Repositorios.Interfaces;
using Infrastructure.Servicios;

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


// Registro de las dependencias
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IConfiguracionService, ConfiguracionService>();
builder.Services.AddScoped<CitaService>();
builder.Services.AddScoped<IAutenticacionService, ServiceAutenticacion>();
builder.Services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
builder.Services.AddScoped<ICitaService, CitaService>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<ILogService>(provider => LogService.Instance);
// Lee la clave JWT desde configuracion
var claveSecreta = builder.Configuration["JWT:ClaveSecreta"]
?? throw new InvalidOperationException("La clave JWT no está configurada");
// Registra tu servicio de generacion de JWT
builder.Services.AddSingleton<IGeneradorJWT>(new GeneradorJWT(claveSecreta));


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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", policy =>
    {
        policy.AllowAnyOrigin();
        policy.AllowAnyMethod();
        policy.AllowAnyHeader();
    });
});


var app = builder.Build();


// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAnyOrigin");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
