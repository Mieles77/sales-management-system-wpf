using Prueba_Apis.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prueba_Apis.Services
{
    public class NotificacionService
    {
        public static List<Notificacion> GenerarNotificaciones()
        {
            var lista = new List<Notificacion>();
            var productoService = new ProductoService();
            var ventaService = new VentaService();

            
            var bajoStock = productoService.ObtenerBajoStock();
            foreach (var p in bajoStock)
            {
                lista.Add(new Notificacion
                {
                    Titulo = "Stock Bajo",
                    Mensaje = $"Quedan solo {p.Cantidad} unidades de {p.Nombre}.",
                    Fecha = DateTime.Now.ToString("t")
                });
            }

            
            var ventas = ventaService.ObtenerVentas();
            if (ventas.Any())
            {
                var ultima = ventas.First();
                lista.Add(new Notificacion
                {
                    Titulo = "Venta Registrada",
                    Mensaje = $"Se vendió {ultima.Cantidad}x {ultima.ProductoNombre} por ${ultima.Total:N0}.",
                    Fecha = ultima.FechaVenta.ToString("t")
                });
            }

            
            var vencidos = productoService.ObtenerCantidadProductosVencidos();
            if (vencidos > 0)
            {
                lista.Add(new Notificacion
                {
                    Titulo = "Productos Vencidos",
                    Mensaje = $"Hay {vencidos} productos vencidos en inventario.",
                    Fecha = DateTime.Now.ToString("t")
                });
            }

            var proximosAVencer = productoService.ObtenerProximosAVencer();
            foreach(var p in proximosAVencer)
            {
                lista.Add(new Notificacion
                {
                    Titulo = "Producto a Vencer",
                    Mensaje = $"Aun quedan {p.Cantidad} producto de {p.Nombre} que se venceran el dia {p.FechaVencimiento}.",
                    Fecha = DateTime.Now.ToString("t")
                });
            }
            return lista;
        }
    }

}
