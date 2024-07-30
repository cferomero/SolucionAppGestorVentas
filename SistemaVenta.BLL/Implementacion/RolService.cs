using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class RolService : IRolService
    {
        private readonly IGenericRepository<Rol> _repository;
        public RolService(IGenericRepository<Rol> repository)
        {
            _repository = repository;
        }

        // Metodo para devolver la lista de roles
        public async Task<List<Rol>> Lista()
        {
            IQueryable<Rol> query = await _repository.Consultar();
            return query.ToList();
        }
    }
}
