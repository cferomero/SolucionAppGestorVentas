using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.BLL.Interfaces;
using Firebase.Auth;
using Firebase.Storage;
using SistemaVenta.Entity;
using SistemaVenta.DAL.Interfaces;

namespace SistemaVenta.BLL.Implementacion
{
    public class FireBaseService : IFireBaseservice
    {
        private readonly IGenericRepository<Configuracion> _repositorio;
        public FireBaseService(IGenericRepository<Configuracion> repositorio)
        {
            _repositorio = repositorio;
        }

        // *** Logica para subir el Storage
        public async Task<string> SubirStorage(Stream streamArchivo, string CarpetaDestino, string NombreArchivo)
        {
            // Subiendo el archivo a storage
            string urlImagen = "";
            try
            {
                IQueryable<Configuracion> query = await _repositorio.Consultar(c => c.Recurso.Equals("FireBase_Storage"));
                Dictionary<string, string> Config = query.ToDictionary(keySelector: c => c.Propiedad, elementSelector: c => c.Valor);

                // configuracion de firebase
                var auth = new FirebaseAuthProvider(new FirebaseConfig(Config["api_key"]));
                var a = await auth.SignInWithEmailAndPasswordAsync(Config["email"], Config["clave"]);

                // añadir el token de cancelacion de origen
                var cancellation = new CancellationTokenSource();
                var task = new FirebaseStorage(
                    Config["ruta"],
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                        ThrowOnCancel = true
                    }
                )
                .Child(Config[CarpetaDestino]) // creando la carpeta de destino
                .Child(NombreArchivo) // creando el archivo
                .PutAsync(streamArchivo, cancellation.Token);

                urlImagen = await task;
            }
            catch
            {
                urlImagen = "";
            }

            return urlImagen;
        }


        // **** Logica para eliminar el archivo del firebase
        public async Task<bool> EliminarStorage(string CarpetaDestino, string NombreArchivo)
        {
            // Eliminando el archivo subido en el storage
            try
            {
                IQueryable<Configuracion> query = await _repositorio.Consultar(c => c.Recurso.Equals("FireBase_Storage"));
                Dictionary<string, string> Config = query.ToDictionary(keySelector: c => c.Propiedad, elementSelector: c => c.Valor);

                // configuracion de firebase
                var auth = new FirebaseAuthProvider(new FirebaseConfig(Config["api_key"]));
                var a = await auth.SignInWithEmailAndPasswordAsync(Config["email"], Config["clave"]);

                // añadir el token de cancelacion de origen
                var cancellation = new CancellationTokenSource();
                var task = new FirebaseStorage(
                    Config["ruta"],
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                        ThrowOnCancel = true
                    }
                )
                .Child(Config[CarpetaDestino]) // creando la carpeta de destino
                .Child(NombreArchivo) // creando el archivo
                .DeleteAsync();

                await task;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
