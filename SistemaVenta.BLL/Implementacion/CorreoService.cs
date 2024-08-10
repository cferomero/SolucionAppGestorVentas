using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Mail;

using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class CorreoService : ICorreoService
    {
        private readonly IGenericRepository<Configuracion> _repository;
        public CorreoService(IGenericRepository<Configuracion> repository)
        {
            _repository = repository;   
        }

        // **** METODO PAR ENVIAR EL CORREO ******
        public async Task<bool> EnviarCorreo(string CorreoDestino, string Asunto, string MensajeCorreo)
        {
            try
            {
                // obtenemos nuestro repositorio para obtener las credenciales
                IQueryable<Configuracion> query = await _repository.Consultar(c => c.Recurso.Equals("Servicio_Correo"));
                Dictionary<string, string> Config = query.ToDictionary(keySelector: c => c.Propiedad, elementSelector: c => c.Valor); // Alamcenar en un diccionario los datos de la tabla CONFIGURACION

                var credenciales = new NetworkCredential(Config["correo"], Config["clave"]); // Credenciales del correo

                // definimos los valores para enviar el correo y el correo de origen
                var correo = new MailMessage()
                {
                    From = new MailAddress(Config["correo"], Config["alias"]),
                    Subject = Asunto,
                    Body = MensajeCorreo,
                    IsBodyHtml = true
                };

                // definimos los valores y el correo de destino
                correo.To.Add(new MailAddress(CorreoDestino));

                // configuramos el cliente-Servidor
                var clienteServidor = new SmtpClient()
                {
                    Host = Config["host"],
                    Port = int.Parse(Config["puerto"]),
                    Credentials = credenciales,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    EnableSsl = true
                };

                // Llamar al cliente-servidor y que envie el correo y lo anteriormente configurado
                clienteServidor.Send(correo);
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
