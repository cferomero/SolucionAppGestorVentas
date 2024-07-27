using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace SistemaVenta.DAL.Implementacion
{
    public class GenericRepository<TEntidad> : IGenericRepository<TEntidad> where TEntidad : class
    {
        // creamos el constructor de la clase
        private readonly DbventaContext _dbVentaContext;
        public GenericRepository(DbventaContext dbVentaContext)
        {
            // obtenemos el contexto de la conexion
            _dbVentaContext = dbVentaContext;
        }
        // Añadimos la clase del repositorio y le clonamos todos los metodos de la interfaz
        public async Task<TEntidad> Obtener(Expression<Func<TEntidad, bool>> filtro)
        {
            try
            {
                // le pasamos una variable asincrona, el dbContext y que me encuentre el primero o cualquiera
                TEntidad entidad = await _dbVentaContext.Set<TEntidad>().FirstOrDefaultAsync(filtro);
                return entidad;
            }
            catch
            {
                throw;
            }
        }
        public async Task<TEntidad> Crear(TEntidad entidad)
        {
            try
            {
                _dbVentaContext.Set<TEntidad>().Add(entidad);
                await _dbVentaContext.SaveChangesAsync();
                return entidad;
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> Editar(TEntidad entidad)
        {
            try
            {
                _dbVentaContext.Update(entidad);
                await _dbVentaContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> Eliminar(TEntidad entidad)
        {
            try
            {
                _dbVentaContext.Remove(entidad);
                await _dbVentaContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IQueryable<TEntidad>> Consultar(Expression<Func<TEntidad, bool>> filtro = null)
        {
            IQueryable<TEntidad> queryEntidad = filtro == null ? _dbVentaContext.Set<TEntidad>() : _dbVentaContext.Set<TEntidad>().Where(filtro);
            return queryEntidad;
        }
    }
}
