using Prueba_Apis.Model;
using Prueba_Apis.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prueba_Apis.Services
{
    public class VentaService
    {
        #region Servicios
        private readonly DatabaseService _database;
        #endregion

        #region Constructor
        public VentaService()
        {
            _database = DatabaseService.Instance;
        }
        #endregion

        #region CREATE (Crear)
        public void RegistrarVenta(ObservableCollection<ProductoCarrito> Carrito, decimal Total)
        {
            try
            {
                // ✅ INCLUYE Descripcion y Tipo
                string sql = @"
            INSERT INTO Ventas (ProductoId, Cantidad, PrecioUnitario, Total, FechaVenta, Descripcion, Tipo)
            VALUES (@ProductoId, @Cantidad, @PrecioUnitario, @Total, @FechaVenta, @Descripcion, @Tipo);";

                using (var connection = _database.GetConnection())
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var command = new SQLiteCommand(sql, connection))
                        {
                            foreach (var item in Carrito)
                            {
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@ProductoId", item.Id);
                                command.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                                command.Parameters.AddWithValue("@PrecioUnitario", item.Precio);
                                command.Parameters.AddWithValue("@Total", item.Subtotal);
                                command.Parameters.AddWithValue("@FechaVenta", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                command.Parameters.AddWithValue("@Descripcion", $"Venta de {item.Nombre}"); // ✅ AGREGADO
                                command.Parameters.AddWithValue("@Tipo", "Venta"); // ✅ AGREGADO
                                command.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al registrar la venta: {ex.Message}", ex);
            }
        }
        #endregion

        #region READ (Leer)
        public List<Ventas> ObtenerVentas()
        {
            List<Ventas> ventas = new List<Ventas>();
            try
            {
                string sql = @"
                    SELECT v.Id, v.ProductoId, p.Descripcion AS ProductoNombre, v.Cantidad, v.PrecioUnitario, v.Total as Monto, v.FechaVenta, 'Ventas' AS Tipo
                    FROM Ventas v
                    JOIN Productos p ON v.ProductoId = p.Id
                    ORDER BY v.FechaVenta DESC LIMIT 5;";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ventas.Add(new Ventas
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    ProductoId = Convert.ToInt32(reader["ProductoId"]),
                                    ProductoNombre = reader["ProductoNombre"].ToString(),
                                    Cantidad = Convert.ToInt32(reader["Cantidad"]),
                                    PrecioUnitario = Convert.ToDecimal(reader["PrecioUnitario"]),
                                    Total = Convert.ToDecimal(reader["Total"]),
                                    FechaVenta = DateTime.Parse(reader["FechaVenta"].ToString())
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener las ventas: {ex.Message}", ex);
            }
            return ventas;
        }

        #endregion

        #region UPDATE (Actualizar)
        // Métodos de actualización si es necesario
        public void ActualizarVenta(Ventas venta)
        {
            try
            {
                string sql = @"
                    UPDATE Ventas
                    SET ProductoId = @ProductoId,
                        Cantidad = @Cantidad,
                        PrecioUnitario = @PrecioUnitario,
                        Total = @Total,
                        FechaVenta = @FechaVenta
                    WHERE Id = @Id;";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ProductoId", venta.ProductoId);
                        command.Parameters.AddWithValue("@Cantidad", venta.Cantidad);
                        command.Parameters.AddWithValue("@PrecioUnitario", venta.PrecioUnitario);
                        command.Parameters.AddWithValue("@Total", venta.Total);
                        command.Parameters.AddWithValue("@FechaVenta", venta.FechaVenta.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@Id", venta.Id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar la venta: {ex.Message}", ex);
            }
        }
        #endregion

        #region DELETE (Eliminar)
        public void EliminarVenta(int ventaId)
        {
            try
            {
                string sql = "DELETE FROM Ventas WHERE Id = @Id;";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", ventaId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar la venta: {ex.Message}", ex);
            }
        }
        #endregion

        #region Estadisticas
        public decimal ObtenerTotalVentasPorFecha(DateTime fecha)
        {
            decimal totalVentas = 0;
            try
            {
                string sql = @"
                    SELECT SUM(Total) AS TotalVentas
                    FROM Ventas
                    WHERE DATE(FechaVenta) = @Fecha AND Tipo = 'Venta';";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Fecha", fecha.ToString("yyyy-MM-dd"));
                        var result = command.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            totalVentas = Convert.ToDecimal(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el total de ventas: {ex.Message}", ex);
            }
            return totalVentas;
        }

        public int ObtenerCantidadVentasPorFecha(DateTime fecha)
        {
            int cantidadVentas = 0;
            try
            {
                string sql = @"
                    SELECT COUNT(*) AS CantidadVentas
                    FROM Ventas
                    WHERE DATE(FechaVenta) = @Fecha;";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Fecha", fecha.ToString("yyyy-MM-dd"));
                        var result = command.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            cantidadVentas = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener la cantidad de ventas: {ex.Message}", ex);
            }
            return cantidadVentas;
        }

        public decimal ObtenerTotalVentas()
        {
            decimal totalVentas = 0;
            try
            {
                string sql = "SELECT SUM(Total) AS TotalVentas FROM Ventas WHERE Tipo = 'Venta';";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        var result = command.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            totalVentas = Convert.ToDecimal(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el total de ventas: {ex.Message}", ex);
            }
            return totalVentas;
        }

        public int ObtenerCantidadTotalVentas()
        {
            int cantidadVentas = 0;
            try
            {
                string sql = "SELECT COUNT(*) AS CantidadVentas FROM Ventas WHERE Tipo = 'Venta';";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        var result = command.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            cantidadVentas = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener la cantidad total de ventas: {ex.Message}", ex);
            }
            return cantidadVentas;
        }

        public decimal ObtenerPromedioVenta()
        {
            decimal promedioVenta = 0;
            try
            {
                string sql = "SELECT AVG(Total) AS PromedioVenta FROM Ventas;";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        var result = command.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            promedioVenta = Convert.ToDecimal(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el promedio de ventas: {ex.Message}", ex);
            }
            return promedioVenta;
        }

        public decimal ObtenerVentaMasAlta()
        {
            decimal ventaMasAlta = 0;
            try
            {
                string sql = "SELECT MAX(Total) AS VentaMasAlta FROM Ventas;";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        var result = command.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            ventaMasAlta = Convert.ToDecimal(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener la venta más alta: {ex.Message}", ex);
            }
            return ventaMasAlta;
        }

        public decimal ObtenerVentaMasBaja()
        {
            decimal ventaMasBaja = 0;
            try
            {
                string sql = "SELECT MIN(Total) AS VentaMasBaja FROM Ventas;";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        var result = command.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            ventaMasBaja = Convert.ToDecimal(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener la venta más baja: {ex.Message}", ex);
            }
            return ventaMasBaja;
        }

        public List<Ventas> ObtenerVentasPorProducto(int productoId)
        {
            List<Ventas> ventas = new List<Ventas>();
            try
            {
                string sql = @"
                    SELECT v.Id, v.ProductoId, p.Nombre AS ProductoNombre, v.Cantidad, v.PrecioUnitario, v.Total, v.FechaVenta
                    FROM Ventas v
                    JOIN Productos p ON v.ProductoId = p.Id
                    WHERE v.ProductoId = @ProductoId
                    ORDER BY v.FechaVenta DESC;";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ProductoId", productoId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ventas.Add(new Ventas
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    ProductoId = Convert.ToInt32(reader["ProductoId"]),
                                    ProductoNombre = reader["ProductoNombre"].ToString(),
                                    Cantidad = Convert.ToInt32(reader["Cantidad"]),
                                    PrecioUnitario = Convert.ToDecimal(reader["PrecioUnitario"]),
                                    Total = Convert.ToDecimal(reader["Total"]),
                                    FechaVenta = DateTime.Parse(reader["FechaVenta"].ToString())
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener las ventas por producto: {ex.Message}", ex);
            }
            return ventas;
        }

        #endregion
    }
}
