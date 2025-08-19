using Application.DTOs.Configuracion;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfiguracionesController : ControllerBase
    {
        private readonly IConfiguracionService _service;
        private readonly ILogService _logService;

        public ConfiguracionesController(IConfiguracionService service, ILogService logService)
        {
            _service = service;
            _logService = logService;
        }

        // Crea una nueva configuracion para una fecha y turno
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] ConfiguracionCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.CrearConfiguracionAsync(dto);
                return CreatedAtAction(nameof(ObtenerActivas), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message }); // 409 por duplicacion
            }
        }

        // Lista las configuraciones activas (fecha >= hoy)
       
        [HttpGet("activas")]
        public async Task<IActionResult> ObtenerActivas()
        {
            var result = await _service.ObtenerConfiguracionesActivasAsync();
            return Ok(result);
        }

        // Endpoint para obtener fechas disponibles
        [HttpGet("fechas-disponibles")]
        public async Task<IActionResult> ObtenerFechasDisponibles()
        {
            try
            {
                var configuraciones = await _service.ObtenerConfiguracionesActivasAsync();
                
                // Necesito obtener informacion de los turnos para mostrar nombres y horarios
                var turnos = await _service.ObtenerTurnosAsync(); // Necesito agregar este metodo
                
                var fechas = configuraciones
                    .Select(c => new { 
                        fecha = c.Fecha.ToString("yyyy-MM-dd"),
                        fechaFormateada = c.Fecha.ToString("dd/MM/yyyy"),
                        turnoId = c.TurnoId,
                        duracion = c.DuracionCitaMinutos,
                        estaciones = c.CantidadEstaciones
                    })
                    .GroupBy(f => f.fecha)
                    .Select(g => new {
                        fecha = g.Key,
                        fechaFormateada = g.First().fechaFormateada,
                        turnos = g.Select(t => {
                            var turno = turnos.FirstOrDefault(tr => tr.Id == t.turnoId);
                            return new {
                                turnoId = t.turnoId,
                                turnoNombre = turno?.Nombre ?? "Turno Desconocido",
                                duracion = t.duracion,
                                estaciones = t.estaciones,
                                horaInicio = turno?.HoraInicio.ToString(@"hh\:mm") ?? "00:00",
                                horaFin = turno?.HoraFin.ToString(@"hh\:mm") ?? "00:00"
                            };
                        }).ToList()
                    })
                    .OrderBy(f => f.fecha)
                    .ToList();

                return Ok(fechas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error obteniendo fechas disponibles" });
            }
        }

        // Endpoint para leer logs del sistema
        [HttpGet("logs")]
        public async Task<IActionResult> ObtenerLogs()
        {
            try
            {
                var logs = await _logService.LeerLogsAsync();
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error leyendo logs del sistema" });
            }
        }

        // Endpoint para eliminar una configuracion
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var result = await _service.EliminarConfiguracionAsync(id);
                if (result)
                {
                    return Ok(new { message = "Configuracion eliminada exitosamente" });
                }
                return NotFound(new { message = "Configuracion no encontrada" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error eliminando configuración {id}", ex);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }
}
