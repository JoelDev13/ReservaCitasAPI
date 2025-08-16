using Application.DTOs.Cita;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitasController : ControllerBase
    {
        private readonly ICitaService _citaService;
        private readonly ILogService _logService;

        public CitasController(ICitaService citaService)
        {
            _citaService = citaService;
            _logService = Infrastructure.Servicios.LogService.Instance;
        }

        // Endpoint para generar slots segun configuracion
        [HttpPost("generar-slots/{configuracionId}")]
        public async Task<IActionResult> GenerarSlots(int configuracionId)
        {
            try
            {
                await _citaService.GenerarSlotsPorConfiguracionAsync(configuracionId);
                return Ok(new { message = "Slots generados correctamente" });
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error generando slots para configuracion {configuracionId}", ex);
                return BadRequest(new { error = ex.Message });
            }
        }

        // Endpoint para reservar una cita
        [HttpPost("reservar")]
        public async Task<IActionResult> ReservarCita([FromBody] ReservarCitaDTO dto)
        {
            try
            {
                var resultado = await _citaService.ReservarCitaAsync(dto);

                if (resultado)
                    return Ok(new { mensaje = "Cita reservada exitosamente" });
                return BadRequest(new { mensaje = "No se pudo reservar la cita" });

            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("cancelar")]
        public async Task<IActionResult> CancelarCita([FromBody] CancelarCitaDTO dto)
        {
            try
            {
                var resultado = await _citaService.CancelarCitaAsync(dto);
                if (resultado)
                    return Ok(new { mensaje = "Cita cancelada exitosamente" });
                return BadRequest(new { mensaje = "No se pudo cancelar la cita" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logService.LogError("Error cancelando cita", ex);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpGet("horarios-disponibles")]
        public async Task<IActionResult> ObtenerHorariosDisponibles([FromQuery] DateTime fecha, [FromQuery] int turnoId)
        {
            try
            {
                var horarios = await _citaService.ObtenerHorariosDisponiblesAsync(fecha, turnoId);
                return Ok(horarios);
            }
            catch (Exception ex)
            {
                _logService.LogError("Error obteniendo horarios disponibles", ex);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpGet("mis-citas/{usuarioId}")]
        public async Task<IActionResult> ObtenerMisCitas(int usuarioId)
        {
            try
            {
                var citas = await _citaService.ObtenerCitasUsuarioAsync(usuarioId);
                return Ok(citas);
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error obteniendo citas del usuario {usuarioId}", ex);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }
}
