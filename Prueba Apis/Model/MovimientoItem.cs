using System;
using System.Collections.ObjectModel;

public class MovimientoItem
{
    public string Tipo { get; set; } // "Venta" o "Entrada"
    public string Descripcion { get; set; }
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
}
