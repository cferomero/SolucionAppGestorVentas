using SistemaVenta.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVenta.BLL.Interfaces
{
    public interface IRolService
    {
        // creando metodos sincronos
        Task<List<Rol>> Lista();

    }
}
