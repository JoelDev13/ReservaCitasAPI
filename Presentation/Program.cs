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

// Aqui obtengo la cadena de conexion de appsettings.json
// Lo hice asi porque quiero que la configuracion sea flexible
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Aqui inyecto el dbcontext y lo paso a la cadena de conexion
// Lo hice asi porque necesito que Entity Framework sepa como conectarse a la base de datos
builder.Services.AddDbContext<CitasDbContext>(options =>
    options.UseNpgsql(connectionString));

// Aqui registro todas las dependencias del sistema
// Lo hice asi porque quiero usar inyeccion de dependencias en lugar de crear objetos manualmente
// Esto hace que el codigo sea mas testeable y mantenible

// Registro el repositorio generico para operaciones CRUD basicas
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Registro el Unit of Work para manejar transacciones
// Lo hice asi porque quiero que las operaciones complejas sean atomicas
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Registro el servicio de configuracion
// Lo hice asi porque quiero que el admin pueda configurar el sistema
builder.Services.AddScoped<IConfiguracionService, ConfiguracionService>();

// Registro el servicio de autenticacion
// Lo hice asi porque necesito manejar login y registro de usuarios
builder.Services.AddScoped<IAutenticacionService, ServiceAutenticacion>();

// Registro el repositorio de usuarios
// Lo hice asi porque necesito acceso especifico a los usuarios
builder.Services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();

// Registro el servicio de citas
// Lo hice asi porque aqui esta toda la logica de negocio de las reservas
builder.Services.AddScoped<ICitaService, CitaService>();

// Registro el servicio de email
// Lo hice asi porque quiero que se envien emails de confirmacion
// Pero si falla, no debe afectar las reservas
builder.Services.AddScoped<IEmailService, EmailService>();

// Registro el servicio de logging como Singleton
// Lo hice asi porque quiero que solo haya una instancia del servicio de logging
// Esto es parte del patron Singleton que requiere la evaluacion
builder.Services.AddSingleton<ILogService>(provider => LogService.Instance);

// Aqui leo la clave JWT desde la configuracion
// Lo hice asi porque la clave debe ser secreta y configurable
var claveSecreta = builder.Configuration["JWT:ClaveSecreta"]
?? throw new InvalidOperationException("La clave JWT no est configurada");

// Registro mi servicio de generacion de JWT
// Lo hice asi porque necesito generar tokens para la autenticacion
builder.Services.AddSingleton<IGeneradorJWT>(new GeneradorJWT(claveSecreta));

// Aqui convierto la cadena secreta a un arreglo de bytes
// Lo hice asi porque JWT necesita la clave en formato de bytes
var key = System.Text.Encoding.UTF8.GetBytes(claveSecreta);

// Aqui configuro la autenticacion JWT
// Lo hice asi porque quiero que la API sea segura y solo usuarios autenticados puedan acceder
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

// Aqui configuro CORS para permitir que el frontend se conecte al backend
// Lo hice asi porque el frontend y backend estan en puertos diferentes
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

// Aqui configuro el pipeline de la aplicacion
// Lo hice asi porque quiero que la aplicacion funcione correctamente

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Habilito CORS para que el frontend pueda hacer peticiones
app.UseCors("AllowAnyOrigin");

app.UseHttpsRedirection();

// Habilito la autenticacion y autorizacion
// Lo hice asi porque quiero que la API sea segura
app.UseAuthentication();
app.UseAuthorization();

// Mapeo los controllers para que la API funcione
app.MapControllers();

app.Run();
