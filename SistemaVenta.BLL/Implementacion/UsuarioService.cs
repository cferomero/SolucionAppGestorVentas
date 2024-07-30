using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.Net;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _repository;
        private readonly IFireBaseservice _fireBaseservice;
        private readonly IUtilidadesService _utilidadesService;
        private readonly ICorreoService _correoService;

        public UsuarioService(
            IGenericRepository<Usuario> repository, 
            IFireBaseservice fireBaseservice, 
            IUtilidadesService utilidadesService, 
            ICorreoService correoService
            )
        {
            _repository = repository;
            _fireBaseservice = fireBaseservice;
            _utilidadesService = utilidadesService;
            _correoService = correoService;
        }



        // *** METODOS DEL USUARIO ****
        public async Task<List<Usuario>> Lista()
        {
            //
            IQueryable<Usuario> query = await _repository.Consultar();
            return query.Include(r => r.IdRolNavigation).ToList(); // incluimos tambien la logica del usuario y del rol
        }
        public Task<Usuario> Crear(Usuario entidad, Stream Foto = null, string NombreFoto = "", string UrlPlantillaCorreo = "")
        {
            throw new NotImplementedException();
        }
        public Task<Usuario> Editar(Usuario entidad, Stream Foto = null, string NombreFoto = "")
        {
            throw new NotImplementedException();
        }
        public Task<bool> Eliminar(int IdUsuario)
        {
            throw new NotImplementedException();
        }
        public Task<Usuario> OtenerPorCredenciales(string correo, string clave)
        {
            throw new NotImplementedException();
        }
        public Task<Usuario> OtenerPorId(int IdUsuario)
        {
            throw new NotImplementedException();
        }


        // **** ESTOS METODOS VAN EN EL FORMULARIO DEL PERFIL DEL USUARIO

        public Task<bool> GuardarPerfil(Usuario entidad)
        {
            throw new NotImplementedException();
        }
        public Task<bool> CambiarClave(int IdUsuario, string ClaveActual, string ClaveNueva)
        {
            throw new NotImplementedException();
        }
        public Task<bool> ReestablecerClave(string correo, string UrlPlantillaCorreo)
        {
            throw new NotImplementedException();
        }
    }
}
