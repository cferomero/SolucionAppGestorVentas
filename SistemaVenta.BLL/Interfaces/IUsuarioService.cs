using SistemaVenta.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.BLL.Interfaces
{
    public interface IUsuarioService
    {
        // crear metodos del servicio del usuario
        // asincronos = Task<>

        Task<List<Usuario>> Lista();
        Task<Usuario> Crear(Usuario entidad, Stream Foto = null, string NombreFoto = "", string UrlPlantillaCorreo = "");
        Task<Usuario> Editar(Usuario entidad, Stream Foto = null, string NombreFoto = "");
        Task<bool> Eliminar(int IdUsuario);
        Task<Usuario> OtenerPorCredenciales(string correo, string clave);
        Task<Usuario> OtenerPorId(int IdUsuario);

        // Estos metodos van en el formulario del perfil del usuario
        Task<bool> GuardarPerfil(Usuario entidad);
        Task<bool> CambiarClave(int IdUsuario, string ClaveActual, string ClaveNueva);
        Task<bool> ReestablecerClave(string correo, string UrlPlantillaCorreo);



    }
}
