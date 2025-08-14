using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entidades;
using Domain.Interfaces;
using Infrastructure.Persistencia.Repositorios.Interfaces;

namespace Application.Servicios
{
    public class ServiceAutenticacion : IAutenticacionService
    {
        private readonly IRepositorioUsuario _repositorio;
        private readonly IGeneradorJWT _generadorJwt;
        

        public ServiceAutenticacion(IRepositorioUsuario repositorio, IGeneradorJWT generadorJwt)
        {
            _repositorio = repositorio;
            _generadorJwt = generadorJwt;
        }

        public bool Registrar(RegistroUsuarioDTO dto)
        {
            if (_repositorio.ObtenerPorNombre(dto.Nombre) != null)
                return false;

            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(dto.ContrasenaHash),
                Rol = dto.RolUsuario
            };

            _repositorio.Agregar(usuario);
            return _repositorio.GuardarCambios();
        }

        public RespuestaLoginDTO? Login(LoginUsuarioDTO dto)
        {
            var usuario = _repositorio.ObtenerPorNombre(dto.Nombre);
            if (usuario == null) return null;

            if (!BCrypt.Net.BCrypt.Verify(dto.ContrasenaHash, usuario.ContrasenaHash))
                return null;

            var token = _generadorJwt.GenerarToken(usuario);
            return new RespuestaLoginDTO { Token = token };
        }
    }
}

