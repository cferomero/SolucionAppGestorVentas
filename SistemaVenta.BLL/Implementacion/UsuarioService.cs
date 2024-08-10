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
            IQueryable<Usuario> query = await _repository.Consultar();
            return query.Include(r => r.IdRolNavigation).ToList(); // incluimos tambien la logica del usuario y del rol
        }

        // Logica par crear un usuario
        public async Task<Usuario> Crear(Usuario entidad, Stream Foto = null, string NombreFoto = "", string UrlPlantillaCorreo = "")
        {
            Usuario usuario_existe = await _repository.Obtener(u => u.Correo == entidad.Correo); // comprobar si el usuario existe

            if (usuario_existe != null)
                throw new TaskCanceledException("El correo ya existe"); // si el correo esta asignado a un usuario, enviamos un error


            try
            {
                // Asignamos la clave generada y la encriptamos
                string clave_generada = _utilidadesService.GenerarClave();
                entidad.Clave = _utilidadesService.ConvertirSha256(clave_generada);
                entidad.NombreFoto = NombreFoto;

                if(Foto != null)
                {
                    // validamos si la foto existe le asignamos una url
                    string urlFoto = await _fireBaseservice.SubirStorage(Foto, "carpeta_usuario", NombreFoto);
                    entidad.UrlFoto = urlFoto;
                }


                // Creamos una instancia de usuario como usuario_creado y validamos que exista
                Usuario usuario_creado = await _repository.Crear(entidad);
                if(usuario_creado.IdUsuario == 0)
                {
                    throw new TaskCanceledException("No se pudo crear el usuario");
                }


                // validar el envio del correo
                if (UrlPlantillaCorreo != "")
                {
                    UrlPlantillaCorreo = UrlPlantillaCorreo.Replace("[correo]", usuario_creado.Correo).Replace("[clave]", clave_generada); // pasamos parametros y le enviamos los datos del usuario

                    // leer la view del correo
                    string htmlCorreo = "";
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPlantillaCorreo); // llamamos la url de la plantilla de enviar correo
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse(); // obtenemos la respuesta de la solicitud anterior

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream dataStream = response.GetResponseStream())
                        {
                            // es una respuesta de la solicitud a la url de la plantilla
                            StreamReader readerStream = null;
                            if (response.CharacterSet == null)
                            {
                                readerStream = new StreamReader(dataStream);
                            }
                            else
                            {
                                readerStream = new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));
                            }

                            htmlCorreo = readerStream.ReadToEnd();
                            response.Close();
                            readerStream.Close();
                        }
                    }

                    // Creando la cuenta
                    if (htmlCorreo != "")
                    {
                        await _correoService.EnviarCorreo(usuario_creado.Correo, "Cuenta creada", htmlCorreo);
                    }
                }
                IQueryable<Usuario> query = await _repository.Consultar(u => u.IdUsuario == usuario_creado.IdUsuario);
                usuario_creado = query.Include(r => r.IdRolNavigation).First();
                return usuario_creado;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        // Logica par editar un usuario
        public async Task<Usuario> Editar(Usuario entidad, Stream Foto = null, string NombreFoto = "")
        {
            Usuario usuario_existe = await _repository.Obtener(u => u.Correo == entidad.Correo && u.IdUsuario != entidad.IdUsuario);

            if (usuario_existe != null)
                throw new TaskCanceledException("El correo ya existe");

            try
            {
                IQueryable<Usuario> queryUsuario = await _repository.Consultar(u => u.IdUsuario == entidad.IdUsuario);
                 Usuario usuario_editar = queryUsuario.First();

                usuario_editar.Nombre = entidad.Nombre;
                usuario_editar.Correo = entidad.Correo;
                usuario_editar.Telefono = entidad.Telefono;
                usuario_editar.IdRol = entidad.IdRol;

                if(usuario_editar.NombreFoto == "")
                {
                    usuario_editar.NombreFoto = NombreFoto;
                }

                if(Foto != null)
                {
                    string urlFoto = await _fireBaseservice.SubirStorage(Foto, "carpeta_usuario", usuario_editar.NombreFoto);
                    usuario_editar.UrlFoto = urlFoto;
                }

                bool respuesta = await _repository.Editar(usuario_editar);
                if (!respuesta)
                {
                    throw new TaskCanceledException("No se pudo modificar el usuario");
                }

                Usuario usuario_editado = queryUsuario.Include(r => r.IdRolNavigation).First();
                return usuario_editado;
            }
            catch
            {
                throw;
            }
        }
        
        
        // Logica para eliminar un usuario
        public async Task<bool> Eliminar(int IdUsuario)
        {
            try
            {
                // factorizar codigo
                Usuario usuario_encontrado = await _repository.Obtener(u => u.IdUsuario == IdUsuario) ??  throw new TaskCanceledException("El usuario no existe");
                //Usuario usuario_encontrado = await _repository.Obtener(u => u.IdUsuario == IdUsuario);
                //if(usuario_encontrado == null)
                //{
                //    throw new TaskCanceledException("El usuario no existe");
                //}

                string nombreFoto = usuario_encontrado.NombreFoto;
                bool respuesta = await _repository.Eliminar(usuario_encontrado);

                if(respuesta)
                {
                    await _fireBaseservice.EliminarStorage("carpeta_usuario", nombreFoto);
                }
                return true;

            }
            catch
            {
                throw;
            }
        }


        // Logica para obtener usuario por correo y clave
        public async Task<Usuario> OtenerPorCredenciales(string correo, string clave)
        {
            string clave_encriptada = _utilidadesService.ConvertirSha256(clave); // encriptamos la clave
            Usuario usuario_encontrado = await _repository.Obtener(u => u.Correo.Equals(correo) && u.Clave.Equals(clave_encriptada));
            return usuario_encontrado;

        }


        // Logica para obtener usuario por Id
        public async Task<Usuario> OtenerPorId(int IdUsuario)
        {
            IQueryable<Usuario> query = await _repository.Consultar(u => u.IdUsuario == IdUsuario);
            Usuario resultado = query.Include(r => r.IdRolNavigation).FirstOrDefault();
            return resultado;
        }





        // **** ESTOS METODOS VAN EN EL FORMULARIO DEL PERFIL DEL USUARIO **********

        public async Task<bool> GuardarPerfil(Usuario entidad)
        {
            try
            {
                Usuario usuario_encontrado = await _repository.Obtener(u => u.IdUsuario == entidad.IdUsuario) ?? throw new TaskCanceledException("El usuario no existe");

                usuario_encontrado.Correo = entidad.Correo;
                usuario_encontrado.Telefono = entidad.Telefono;

                bool respuesta = await _repository.Editar(usuario_encontrado);
                return respuesta;
            }
            catch
            {
                throw;
            }
        }


        // Logica para cambiar la clave
        public async Task<bool> CambiarClave(int IdUsuario, string ClaveActual, string ClaveNueva)
        {
            try
            {
                // validamos si el usuario existe o no
                Usuario usuario_encontrado = await _repository.Obtener(u => u.IdUsuario == IdUsuario) ?? throw new TaskCanceledException("El usuario no existe");

                // validamos si la clave ingresada es correcta
                if(usuario_encontrado.Clave != _utilidadesService.ConvertirSha256(ClaveActual))
                {
                    throw new TaskCanceledException("La contraseña ingresada como actual no es correcta");
                }

                // hacemos la asignacion de la clave nueva
                usuario_encontrado.Clave = _utilidadesService.ConvertirSha256(ClaveNueva);


                // generamos una respuesta al cambio de la clave
                bool respuesta = await _repository.Editar(usuario_encontrado);
                return respuesta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        // Logica para reestablecer la contraseña
        public async Task<bool> ReestablecerClave(string correo, string UrlPlantillaCorreo)
        {
            try
            {
                Usuario usuario_encontrado = await _repository.Obtener(u => u.Correo == correo) ?? throw new TaskCanceledException("No encontramos ningun usuario asociado");

                string clave_generada = _utilidadesService.GenerarClave();
                usuario_encontrado.Clave = _utilidadesService.ConvertirSha256(clave_generada);



                // cear plantilla y enviar correo
                UrlPlantillaCorreo = UrlPlantillaCorreo.Replace("[clave]", clave_generada); // pasamos parametros y le enviamos los datos del usuario

                // leer la view del correo
                string htmlCorreo = "";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPlantillaCorreo); // llamamos la url de la plantilla de enviar correo
                HttpWebResponse response = (HttpWebResponse)request.GetResponse(); // obtenemos la respuesta de la solicitud anterior

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        // es una respuesta de la solicitud a la url de la plantilla
                        StreamReader readerStream = null;
                        if (response.CharacterSet == null)
                        {
                            readerStream = new StreamReader(dataStream);
                        }
                        else
                        {
                            readerStream = new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));
                        }

                        htmlCorreo = readerStream.ReadToEnd();
                        response.Close();
                        readerStream.Close();
                    }
                }

                
                // Creando el correo
                bool correo_enviado = false;

                // validando que el html no este vacio
                if (htmlCorreo != "")
                {
                    correo_enviado = await _correoService.EnviarCorreo(correo, "Contraseña reestablecida", htmlCorreo);
                }
                // verificando si el correo se encio correctamente
                if (!correo_enviado)
                {
                    throw new TaskCanceledException("Tenemos problemas. Por favor inténtalo de nuevo más tarde.");
                }

                bool respuesta = await _repository.Editar(usuario_encontrado);
                return respuesta;
            }
            catch
            {
                throw;
            }
        }
    }
}
