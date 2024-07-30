using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.BLL.Interfaces
{
    public interface IUtilidadesService
    {
        // crear metodos
        string GenerarClave(); // Este metodo nos retorna un codigo para que el usuario pueda logearse
        string ConvertirSha256(string texto); // metodo para encriptar datos
    }
}
