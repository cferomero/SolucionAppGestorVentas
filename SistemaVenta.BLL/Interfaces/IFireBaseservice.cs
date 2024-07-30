using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.BLL.Interfaces
{
    public interface IFireBaseservice
    {
        // crear los metodos de la interfaz
        Task<string> SubirStorage(Stream streamArchivo, string CarpetaDestino, string NombreArchivo);
        Task<bool> EliminarStorage(string CarpetaDestino, string NombreArchivo);
    }
}
