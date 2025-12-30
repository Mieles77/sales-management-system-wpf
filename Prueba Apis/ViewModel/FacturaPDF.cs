using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Prueba_Apis.Models;
using System.Collections.Generic;
using System.Linq;

namespace Prueba_Apis.ViewModel
{
    public class FacturaPDF
    {
        public void GenerarFactura(string rutaArchivo, IEnumerable<ProductoCarrito> productosCarrito, decimal totalVenta)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            // AGRUPACIÓN: Esto resuelve el error de "de dónde sacar la info para la tabla"
            var itemsFactura = productosCarrito
                .GroupBy(p => p.Codigo)
                .Select(g => new
                {
                    Codigo = g.Key,
                    Nombre = g.First().Nombre,
                    Cantidad = g.Count(),
                    PrecioUnitario = g.First().Precio,
                    Subtotal = g.First().Subtotal
                }).ToList();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                    page.Header().Row(row => {
                        row.RelativeItem().Text("FACTURA DE VENTA").FontSize(20).Bold();
                        row.RelativeItem().AlignRight().Text(System.DateTime.Now.ToString("dd/MM/yyyy"));
                    });

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);  // Cantidad
                            columns.RelativeColumn();    // Descripción
                            columns.ConstantColumn(80);  // Unitario
                            columns.ConstantColumn(80);  // Subtotal
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).Text("Cant.");
                            header.Cell().BorderBottom(1).Text("Producto");
                            header.Cell().BorderBottom(1).Text("Precio U.");
                            header.Cell().BorderBottom(1).Text("Subtotal");
                        });

                        // Aquí usamos los datos AGRUPADOS
                        foreach (var item in itemsFactura)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Cantidad.ToString());
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Nombre);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.PrecioUnitario.ToString("C0"));
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Subtotal.ToString("C0"));
                        }
                    });

                    page.Footer().AlignRight().Text($"TOTAL: {totalVenta:C0}").FontSize(16).Bold();
                });
            })
            .GeneratePdf(rutaArchivo);
        }
    }
}