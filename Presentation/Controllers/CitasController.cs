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
    }
}
