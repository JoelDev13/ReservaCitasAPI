using Application.DTOs.Auth;
using Infrastructure.Persistencia.Repositorios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly IAutenticacionService _servicio;

        public AuthController(IAutenticacionService servicio)
        {
            _servicio = servicio;
        }

        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] RegistroUsuarioDTO dto)
        {
            var resultado = _servicio.Registrar(dto);
            if (!resultado) return BadRequest("Usuario ya existe");
            return Ok("Registro exitoso");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUsuarioDTO dto)
        {
            var respuesta = _servicio.Login(dto);
            if (respuesta == null) return Unauthorized("Credenciales inválidas");
            return Ok(respuesta);
        }

        [HttpGet("solo-admin")]
        [Authorize(Roles = "Administrador")]
        public IActionResult SoloAdmin()
        {
            return Ok("Acceso concedido solo a administradores");
        }
    }
}

