using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.DAL.Implementacion
{
    public class VentaRepository : GenericRepository<Venta>, IVentaRepository
    {
        private readonly DbventaContext _dbventaContext;
        public VentaRepository(DbventaContext dbventaContext) : base(dbventaContext) // lo basamos en dbventaContext ya que estamos usando de GenericRepository
        {
            _dbventaContext = dbventaContext;
        }
        public async Task<Venta> Registrar(Venta entidad)
        {
            Venta ventaGenerada = new Venta();

            // agregamos una transaccion, para cuando ocurra un error en la insercion nos reestablezca la transaccion
            using (var transaction = _dbventaContext.Database.BeginTransaction())
            {
                // iniciaar la transaccion
                try
                {
                    // iteramos para hallar el producto
                    foreach (DetalleVenta dv in entidad.DetalleVenta)
                    {
                        Producto producto_encontrado = _dbventaContext.Productos.Where(p => p.IdProducto == dv.IdProducto).First(); // recibimos el primero
                        producto_encontrado.Stock = producto_encontrado.Stock - dv.Cantidad; // disminuimos la cantidad del stock
                        _dbventaContext.Productos.Update(producto_encontrado); // actualizamos la cantidad en stock con el producto ya disminuido
                    }
                    await _dbventaContext.SaveChangesAsync(); // guardar todos los cambios realizados

                    NumeroCorrelativo numeroCorrelativo = _dbventaContext.NumeroCorrelativos.Where(n => n.Gestion == "venta").First();

                    numeroCorrelativo.UltimoNumero = numeroCorrelativo.UltimoNumero + 1; // aumentamos el valor del ultimo numero + 1
                    numeroCorrelativo.FechaActualizacion = DateTime.Now; // actualizamos la fecha de actualizacion

                    _dbventaContext.NumeroCorrelativos.Update(numeroCorrelativo);
                    await _dbventaContext.SaveChangesAsync();

                    string ceros = string.Concat(Enumerable.Repeat("0", numeroCorrelativo.CantidadDigitos.Value)); // añadirle los ceros
                    string numeroVenta = ceros + numeroCorrelativo.UltimoNumero.ToString(); // concatenamos los ceros con el ultimo numero
                    numeroVenta.Substring(numeroVenta.Length - numeroCorrelativo.CantidadDigitos.Value, numeroCorrelativo.CantidadDigitos.Value); // realizamos la conversion y guardamos solo 6  digitos

                    entidad.NumeroVenta = numeroVenta;

                    await _dbventaContext.Venta.AddAsync(entidad);
                    await _dbventaContext.SaveChangesAsync();

                    ventaGenerada = entidad;

                    transaction.Commit(); // hacemos el commit de las consultas hechas anteriormente

                }
                catch (Exception ex)
                {
                    // reestablecer la transaccion
                    transaction.Rollback();
                    throw ex;
                }
                return ventaGenerada;
            }
        }


        // METODO DEL REPORTE
        public async Task<List<DetalleVenta>> Reporte(DateTime FechaInicio, DateTime FechaFin)
        {
            List<DetalleVenta> listaResumen = await _dbventaContext.DetalleVenta
                .Include(v => v.IdVentaNavigation)
                .ThenInclude(u => u.IdUsuarioNavigation)
                .Include(v => v.IdVentaNavigation)
                .ThenInclude(tdv => tdv.IdTipoDocumentoVentaNavigation)
                .Where(dv => dv.IdVentaNavigation.FechaRegistro.Value.Date >= FechaInicio.Date && dv.IdVentaNavigation.FechaRegistro.Value.Date <= FechaFin.Date).ToListAsync();

            return listaResumen;

        }
    }
}
