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

        public ConfiguracionesController(IConfiguracionService service)
        {
            _service = service;
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

        // Lista las configuraciones activas (fecha >= hoy).
       
        [HttpGet("activas")]
        public async Task<IActionResult> ObtenerActivas()
        {
            var result = await _service.ObtenerConfiguracionesActivasAsync();
            return Ok(result);
        }
    }
}
