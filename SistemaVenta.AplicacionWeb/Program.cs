using SistemaVenta.IOC;
using SistemaVenta.AplicacionWeb.Utilidades.Automapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Agregamos el metodo que creamos como dependencia en IOC
// Todas la inyecciones de dependencias las llamamos en otra clase aparte
builder.Services.InyectarDependencias(builder.Configuration);


/* LA ÚLTIMA VERSIÓN DEL NUGGET AUTOMAPER YA TRAE IMPLEMENTADO EL NUGGER INYECTION DEPENDENCY DE MICROSOFT */
builder.Services.AddAutoMapper(typeof(AutoMapperProfile)); // Inyectamos toda la configuracion del automapper en nuestra dependencia


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
