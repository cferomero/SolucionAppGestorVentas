using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.DAL.DBContext;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.DAL.Implementacion;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.BLL.Implementacion;

namespace SistemaVenta.IOC
{
    public static class Dependencia
    {
        public static void InyectarDependencias(this IServiceCollection Services, IConfiguration Configuration)
        {
            // Agregando la cadena de conexión a nuestro contexto de EntityFramework
            Services.AddDbContext<DbventaContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("CadenaSQL"));
            });


            // *** INYECCION DEPENDENCIA PARA NUESTRO REPOSITORY GENERIC *****

            // utlizamos e implementamos la interfaz y la clase generica de manera trasient
            // Trasient, que varia sus valores segun sea necesario
            Services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            Services.AddScoped<IVentaRepository, VentaRepository>();

            Services.AddScoped<ICorreoService, CorreoService>();
            Services.AddScoped<IFireBaseservice, FireBaseService>();

            Services.AddScoped<IUtilidadesService, UtilidadesService>();
            Services.AddScoped<IRolService, RolService>();
        }
    }
}
