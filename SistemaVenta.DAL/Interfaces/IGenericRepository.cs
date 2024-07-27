using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SistemaVenta.DAL.Interfaces
{
    public interface IGenericRepository<TEntidad> where TEntidad : class // lo creamos como generico y le decimos qe TEntidad es una clase
    {
        // declaramos los metodos y los declaramos todos como asincronos
        Task<TEntidad> Obtener(Expression<Func<TEntidad, bool>> filtro);
        Task<TEntidad> Crear(TEntidad entidad);
        Task<bool> Editar(TEntidad entidad);
        Task<bool> Eliminar(TEntidad entidad);
        Task<IQueryable<TEntidad>> Consultar(Expression<Func<TEntidad, bool>> filtro = null);
    }
}
