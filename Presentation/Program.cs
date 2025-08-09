using Domain.Interfaces;
using Infrastructure.Persistencia.Contexto;
using Infrastructure.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Domain.Interfaces;
using ProyectoFinal.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//aqui consiguo la cadena de conexion de appsettings.json
var connectionString = app.Configuration.GetConnectionString("DefaultConnection");

// aqui inyecto el dbcontext y lo paso a la cadena
builder.Services.AddDbContext<CitasDbContext>(options =>
    options.UseNpgsql(connectionString));

// Inyeccion del repositorio y UnitOfWork
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
