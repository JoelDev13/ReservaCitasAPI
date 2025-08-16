using Application.DTOs.Auth;
using Application.Interfaces;
using Infrastructure.Persistencia.Repositorios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly IAutenticacionService _servicio;
        private readonly ILogService _logService;

        public AuthController(IAutenticacionService servicio)
        {
            _servicio = servicio;
            _logService = Infrastructure.Servicios.LogService.Instance;
        }

        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] RegistroUsuarioDTO dto)
        {
            try
            {
                var resultado = _servicio.Registrar(dto);
                if (!resultado)
                {
                    _logService.LogError($"Intento de registro fallido para email: {dto.Email}");
                    return BadRequest("Usuario ya existe o error en el registro");
                }

                _logService.LogConfiguracion($"USUARIO REGISTRADO", 0);
                return Ok("Registro exitoso");
            }
            catch (Exception ex)
            {
                _logService.LogError("Error en registro de usuario", ex);
                return StatusCode(500, "Error interno del servidor");
            }

        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUsuarioDTO dto)
        {
            try
            {
                var respuesta = _servicio.Login(dto);
                if (respuesta == null)
                {
                     _logService.LogError($"Intento de login fallido para: {dto.Email}");
                      return Unauthorized("Credenciales invalidas");   
                }
                _logService.LogLogin(dto.Email);
                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                _logService.LogError("Error en login", ex);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("solo-admin")]
        [Authorize(Roles = "Administrador")]
        public IActionResult SoloAdmin()
        {
            return Ok("Acceso concedido solo a administradores");
        }
    }
}

