using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using Dapper;
using Prueba_Apis.Models;

namespace Prueba_Apis.Services
{
    /// <summary>
    /// Servicio para operaciones CRUD de productos
    /// </summary>
    public class ProductoService
    {
        #region Servicios
        private readonly DatabaseService _database;
        #endregion

        #region Constructor
        public ProductoService()
        {
            _database = DatabaseService.Instance;
        }
        #endregion

        #region CREATE (Crear)

        /// <summary>
        /// Registra un nuevo producto en la base de datos
        /// </summary>
        public int Registrar(Producto producto)
        {
            try
            {
                string sql = @"
                    INSERT INTO Productos (Codigo, Nombre, PrecioFabrica, PrecioVenta, Cantidad, FechaVencimiento, FechaRegistro)
                    VALUES (@Codigo, @Nombre, @PrecioFabrica, @PrecioVenta, @Cantidad, @FechaVencimiento, @FechaRegistro);
                    SELECT last_insert_rowid();
                ";

                using (var connection = _database.GetConnection())
                {
                    connection.Open();

                    // Ejecutar query y obtener el ID generado
                    int nuevoId = connection.QuerySingle<int>(sql, new
                    {
                        producto.Codigo,
                        producto.Nombre,
                        producto.PrecioFabrica,
                        producto.PrecioVenta,
                        producto.Cantidad,
                        FechaVencimiento = producto.FechaVencimiento?.ToString("yyyy-MM-dd"),
                        FechaRegistro = producto.FechaRegistro.ToString("yyyy-MM-dd HH:mm:ss")
                    });

                    producto.Id = nuevoId;
                    return nuevoId;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al registrar producto: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Registra múltiples productos en una sola transacción
        /// </summary>
        public void RegistrarVarios(List<Producto> productos)
        {
            using (var connection = _database.GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var producto in productos)
                        {
                            Registrar(producto);
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        #endregion

        #region READ (Leer)

        /// <summary>
        /// Obtiene todos los productos
        /// </summary>
        public List<Producto> ObtenerTodos()
        {
            try
            {
                string sql = "SELECT * FROM Productos ORDER BY Id DESC";

                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    var productos = connection.Query<Producto>(sql).ToList();

                    // Convertir fechas desde string
                    foreach (var producto in productos)
                    {
                        ConvertirFechas(producto);
                    }

                    return productos;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener productos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Busca un producto por ID
        /// </summary>
        public Producto ObtenerPorId(int id)
        {
            try
            {
                string sql = "SELECT * FROM Productos WHERE Id = @Id";

                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    var producto = connection.QueryFirstOrDefault<Producto>(sql, new { Id = id });

                    if (producto != null)
                    {
                        ConvertirFechas(producto);
                    }

                    return producto;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar producto: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Busca un producto por código
        /// </summary>
        public Producto ObtenerPorCodigo(string codigo)
        {
            try
            {
                string sql = "SELECT * FROM Productos WHERE Codigo = @Codigo";

                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    var producto = connection.QueryFirstOrDefault<Producto>(sql, new { Codigo = codigo });

                    if (producto != null)
                    {
                        ConvertirFechas(producto);
                    }

                    return producto;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar producto por código: {ex.Message}", ex);
            }
        }

        public Producto ObtenerPorNombreExacto(string nombre)
        {
            try
            {
                string sql = "SELECT * FROM Productos WHERE Nombre = @Nombre COLLATE NOCASE";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    var producto = connection.QueryFirstOrDefault<Producto>(sql, new { Nombre = nombre });
                    if (producto != null)
                    {
                        ConvertirFechas(producto);
                    }
                    return producto;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar producto por nombre: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Busca productos por nombre (búsqueda parcial)
        /// </summary>
        public List<Producto> BuscarPorNombre(string nombre)
        {
            try
            {
                string sql = "SELECT * FROM Productos WHERE Nombre LIKE @Nombre";

                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    var productos = connection.Query<Producto>(sql, new { Nombre = $"%{nombre}%" }).ToList();

                    foreach (var producto in productos)
                    {
                        ConvertirFechas(producto);
                    }

                    return productos;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar productos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene productos con bajo stock
        /// </summary>
        public List<Producto> ObtenerBajoStock(int cantidadMinima = 10)
        {
            try
            {
                string sql = "SELECT * FROM Productos WHERE Cantidad <= @Cantidad ORDER BY Cantidad ASC";

                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    var productos = connection.Query<Producto>(sql, new { Cantidad = cantidadMinima }).ToList();

                    foreach (var producto in productos)
                    {
                        ConvertirFechas(producto);
                    }

                    return productos;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener productos con bajo stock: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene productos próximos a vencer (30 días)
        /// </summary>
        public List<Producto> ObtenerProximosAVencer()
        {
            try
            {
                var todos = ObtenerTodos();
                var proximosAVencer = todos.Where(p => p.ProximoAVencer).ToList();
                return proximosAVencer;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener productos próximos a vencer: {ex.Message}", ex);
            }
        }


        // Guardar un nuevo movimiento (Venta o Entrada)
        // ✅ MÉTODO CORREGIDO
        public void RegistrarMovimiento(string descripcion, decimal monto, string tipo)
        {
            try
            {
                string sql = @"
            INSERT INTO Ventas (ProductoId, Cantidad, PrecioUnitario, Total, FechaVenta, Descripcion, Tipo) 
            VALUES (0, 0, 0, @monto, @fecha, @descripcion, @tipo)";

                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@monto", monto);
                        command.Parameters.AddWithValue("@fecha", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@descripcion", descripcion);
                        command.Parameters.AddWithValue("@tipo", tipo);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar movimiento: " + ex.Message);
            }
        }

        // ✅ CONSULTA CORREGIDA
        public List<MovimientoItem> ObtenerMovimientosRecientes()
        {
            try
            {
                string sql = @"
            SELECT Tipo, Descripcion, Total AS Monto, FechaVenta AS Fecha 
            FROM Ventas 
            ORDER BY FechaVenta DESC 
            LIMIT 10";

                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    var movimientos = new List<MovimientoItem>();

                    using (var command = new SQLiteCommand(sql, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            movimientos.Add(new MovimientoItem
                            {
                                Tipo = reader["Tipo"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                Monto = Convert.ToDecimal(reader["Monto"]),
                                Fecha = DateTime.Parse(reader["Fecha"].ToString())
                            });
                        }
                    }

                    return movimientos;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return new List<MovimientoItem>();
            }
        }

        #endregion

        #region UPDATE (Actualizar)

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        public bool Actualizar(Producto producto)
        {
            try
            {
                string sql = @"
                    UPDATE Productos 
                    SET Codigo = @Codigo,
                        Nombre = @Nombre,
                        PrecioFabrica = @PrecioFabrica,
                        PrecioVenta = @PrecioVenta,
                        Cantidad = @Cantidad,
                        FechaVencimiento = @FechaVencimiento
                    WHERE Id = @Id
                ";

                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    int filasAfectadas = connection.Execute(sql, new
                    {
                        producto.Id,
                        producto.Codigo,
                        producto.Nombre,
                        producto.PrecioFabrica,
                        producto.PrecioVenta,
                        producto.Cantidad,
                        FechaVencimiento = producto.FechaVencimiento?.ToString("yyyy-MM-dd")
                    });

                    return filasAfectadas > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar producto: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza solo el stock de un producto
        /// </summary>
        public bool ActualizarStock(int productoId, int nuevaCantidad)
        {
            try
            {
                string sql = "UPDATE Productos SET Cantidad = @Cantidad WHERE Id = @Id";

                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    int filasAfectadas = connection.Execute(sql, new { Id = productoId, Cantidad = nuevaCantidad });
                    return filasAfectadas > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar stock: {ex.Message}", ex);
            }
        }

        #endregion

        #region DELETE (Eliminar)

        /// <summary>
        /// Elimina un producto por ID
        /// </summary>
        public bool Eliminar(int id)
        {
            try
            {
                string sql = "DELETE FROM Productos WHERE Id = @Id";

                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    int filasAfectadas = connection.Execute(sql, new { Id = id });
                    return filasAfectadas > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar producto: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Elimina todos los productos (¡USAR CON CUIDADO!)
        /// </summary>
        public void EliminarTodos()
        {
            try
            {
                string sql = "DELETE FROM Productos";

                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    connection.Execute(sql);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar todos los productos: {ex.Message}", ex);
            }
        }

        #endregion

        #region ESTADÍSTICAS

        /// <summary>
        /// Obtiene el total de productos registrados
        /// </summary>
        public int ObtenerTotalProductos()
        {
            try
            {
                string sql = "SELECT COUNT(*) FROM Productos";

                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    return connection.QuerySingle<int>(sql);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al contar productos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el valor total del inventario
        /// </summary>
        public decimal ObtenerValorInventario()
        {
            try
            {
                string sql = "SELECT SUM(PrecioVenta * Cantidad) FROM Productos";

                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    return connection.QuerySingle<decimal>(sql);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al calcular valor del inventario: {ex.Message}", ex);
            }
        }

        public decimal ObtenerValorInventarioFabrica()
        {
            try
            {
                string sql = "SELECT SUM(PrecioFabrica * Cantidad) FROM Productos";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    return connection.QuerySingle<decimal>(sql);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al calcular valor del inventario de fábrica: {ex.Message}", ex);
            }
        }

        public decimal ObtenerGananciaTotalPotencial()
        {
            try
            {
                string sql = "SELECT SUM((PrecioVenta - PrecioFabrica) * Cantidad) FROM Productos";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    return connection.QuerySingle<decimal>(sql);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al calcular ganancia total potencial: {ex.Message}", ex);
            }
        }

        public decimal ObtenerGananciaTotalPorProducto(int productoId)
        {
            try
            {
                string sql = "SELECT (PrecioVenta - PrecioFabrica) * Cantidad FROM Productos WHERE Id = @Id";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    return connection.QuerySingle<decimal>(sql, new { Id = productoId });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al calcular ganancia total por producto: {ex.Message}", ex);
            }
        }

        public int ObtenerCantidadTotalProductos()
        {
            try
            {
                string sql = "SELECT SUM(Cantidad) FROM Productos";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    return connection.QuerySingle<int>(sql);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al calcular cantidad total de productos: {ex.Message}", ex);
            }
        }

        public int ObtenerCantidadProductosVencidos()
        {
            try
            {
                string sql = "SELECT COUNT(*) FROM Productos WHERE FechaVencimiento IS NOT NULL AND DATE(FechaVencimiento) < DATE('now')";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    return connection.QuerySingle<int>(sql);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al calcular cantidad de productos vencidos: {ex.Message}", ex);
            }
        }

        public decimal ObtenerValorFabricaProducto(int productoId)
        {
            try
            {
                string sql = "SELECT PrecioFabrica * Cantidad FROM Productos WHERE Id = @Id";
                using (var connection = _database.GetConnection())
                {
                    connection.Open();
                    return connection.QuerySingle<decimal>(sql, new { Id = productoId });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al calcular valor de fábrica del inventario: {ex.Message}", ex);
            }
        }

        #endregion

        #region Métodos auxiliares

        /// <summary>
        /// Convierte las fechas de string a DateTime
        /// </summary>
        private void ConvertirFechas(Producto producto)
        {
            // Dapper ya convierte automáticamente si las columnas están bien definidas
            // Este método es por si necesitas conversiones adicionales
        }

        #endregion
    }
}