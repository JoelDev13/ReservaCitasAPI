using Application.DTOs.Cita;
using Microsoft.AspNetCore.Mvc;


namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitasController : ControllerBase
    {
        private readonly Application.Servicios.CitaService _citaService;

        public CitasController(Application.Servicios.CitaService citaService)
        {
            _citaService = citaService;
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
                if (!resultado) return BadRequest("No se pudo reservar la cita");
                return Ok("Cita reservada correctamente");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
