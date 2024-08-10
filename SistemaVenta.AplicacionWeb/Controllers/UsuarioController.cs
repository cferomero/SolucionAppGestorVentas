using Microsoft.AspNetCore.Mvc;

using AutoMapper;
using Newtonsoft.Json;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class UsuarioController : Controller
    {
        // Llamamos las interfaces del rol y el usuario y el automapper
        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolService;
        private readonly IMapper _mapper;

        public UsuarioController(IMapper mapper, IUsuarioService usuarioService, IRolService rolService)
        {
            _mapper = mapper;
            _usuarioService = usuarioService;
            _rolService = rolService;
        }

        public IActionResult Index()
        {
            return View();
        }

        // CREACION DE METODOS Y PETICIONES

        // Lista roles
        [HttpGet]
        public async Task<IActionResult> ListaRoles()
        {
            // metodo para desplegar toda la lista de roles, cuando queramos crear un nuevo usuario
            List<VMRol> VMListaRoles = _mapper.Map<List<VMRol>>(await _rolService.Lista());
            return StatusCode(StatusCodes.Status200OK, VMListaRoles);
        }



        // Usuarios
        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            // metodo para desplegar toda la lista de los usuarios
            List<VMUsuario> VMUsuarioLista = _mapper.Map<List<VMUsuario>>(await _usuarioService.Lista());
            return StatusCode(StatusCodes.Status200OK, new { data = VMUsuarioLista }); // le estamos pasando un nuevo formato a la lista
            // le pasamos un objeto c# para despues poder usar el tipo DataTable
        }



        // crear
        [HttpPost]
        public async Task<IActionResult> Crear([FromForm] IFormFile foto, [FromForm] string modelo)
        {
            GenericResponse<VMUsuario> GenericResponse = new GenericResponse<VMUsuario>();
            try
            {
                // Recibimos los parametros
                VMUsuario VMUsuario = JsonConvert.DeserializeObject<VMUsuario>(modelo); // pasamos una instancia del viewModel y deserializamos el modelo del parametro
                string nombreFoto = "";
                Stream fotoStream = null;

                if (foto != null)
                {
                    // Validando si la Foto no es nula
                    string nombre_en_codigo = Guid.NewGuid().ToString("N");
                    string extension = Path.GetExtension(foto.FileName); // obteniendo la extension de la foto
                    nombreFoto = string.Concat(nombre_en_codigo, extension); // dandole un nuevo nombre a la foto, nombre y extension del archivo
                    fotoStream = foto.OpenReadStream();
                }

                // url para la plantilla del correo
                string urlPlantillaCorreo = $"{this.Request.Scheme}://{this.Request.Host}/Plantilla/EnviarClave?correo=[correo]&clave=[clave]";  // añadiendo la url de la plantilla

                Usuario usuario_creado = await _usuarioService.Crear(_mapper.Map<Usuario>(VMUsuario), fotoStream, nombreFoto, urlPlantillaCorreo);

                // convirtiendo usuario_creado a ViewModel
                VMUsuario = _mapper.Map<VMUsuario>(usuario_creado);

                // obtener la respuesta
                GenericResponse.Estado = true;
                GenericResponse.Objeto = VMUsuario;
            }
            catch (Exception ex)
            {
                // obtener la respuesta
                GenericResponse.Estado = false;
                GenericResponse.Mensaje = ex.Message;
            }

            // retornar una respuesta
            return StatusCode(StatusCodes.Status200OK, GenericResponse);
        }




        // Editar
        [HttpPut]
        public async Task<IActionResult> Editar([FromForm] IFormFile foto, [FromForm] string modelo)
        {
            GenericResponse<VMUsuario> GenericResponse = new GenericResponse<VMUsuario>();
            try
            {
                // Recibimos los parametros
                VMUsuario VMUsuario = JsonConvert.DeserializeObject<VMUsuario>(modelo); // pasamos una instancia del viewModel y deserializamos el modelo del parametro
                string nombreFoto = "";
                Stream fotoStream = null;

                if (foto != null)
                {
                    // Validando si la Foto no es nula
                    string nombre_en_codigo = Guid.NewGuid().ToString("N");
                    string extension = Path.GetExtension(foto.FileName); // obteniendo la extension de la foto
                    nombreFoto = string.Concat(nombre_en_codigo, extension); // dandole un nuevo nombre a la foto, nombre y extension del archivo
                    fotoStream = foto.OpenReadStream();
                }

                Usuario usuario_editado = await _usuarioService.Editar(_mapper.Map<Usuario>(VMUsuario), fotoStream, nombreFoto);

                // convirtiendo usuario_creado a ViewModel
                VMUsuario = _mapper.Map<VMUsuario>(usuario_editado);

                // obtener la respuesta
                GenericResponse.Estado = true;
                GenericResponse.Objeto = VMUsuario;
            }
            catch (Exception ex)
            {
                // obtener la respuesta
                GenericResponse.Estado = false;
                GenericResponse.Mensaje = ex.Message;
            }

            // retornar una respuesta
            return StatusCode(StatusCodes.Status200OK, GenericResponse);
        }




        // Eliminar
        [HttpDelete]
        public async Task<IActionResult> Eliminar(int IdUsuario)
        {
            GenericResponse<string> GenericResponse = new GenericResponse<string>();
            try
            {
                GenericResponse.Estado = await _usuarioService.Eliminar(IdUsuario);
            }
            catch (Exception ex)
            {
                // obtener la respuesta
                GenericResponse.Estado = false;
                GenericResponse.Mensaje = ex.Message;
            }

            // retornar una respuesta
            return StatusCode(StatusCodes.Status200OK, GenericResponse);

        }
    }
}
