using System;
using System.Data.SQLite;
using System.IO;

namespace Prueba_Apis.Services
{
    /// <summary>
    /// Servicio para gestionar la conexión y creación de la base de datos SQLite
    /// </summary>
    public class DatabaseService
    {
        private static DatabaseService _instance;
        private readonly string _connectionString;
        private readonly string _databasePath;

        // Patrón Singleton para tener una única instancia
        public static DatabaseService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DatabaseService();
                }
                return _instance;
            }
        }

        private DatabaseService()
        {
            // Ruta donde se guardará la base de datos
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MiInventario"
            );

            // Crear carpeta si no existe
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _databasePath = Path.Combine(appDataPath, "inventario.db");
            _connectionString = $"Data Source={_databasePath};Version=3;";

            // Crear base de datos si no existe
            InicializarBaseDeDatos();
        }

        /// <summary>
        /// Obtiene una nueva conexión a la base de datos
        /// </summary>
        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        /// <summary>
        /// Crea las tablas si no existen
        /// </summary>
        private void InicializarBaseDeDatos()
        {
            try
            {
                // Si el archivo no existe, SQLite lo creará automáticamente
                bool esNuevaDB = !File.Exists(_databasePath);

                using (var connection = GetConnection())
                {
                    connection.Open();

                    if (esNuevaDB)
                    {
                        CrearTablas(connection);
                        InsertarDatosIniciales(connection);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al inicializar la base de datos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Crea todas las tablas necesarias
        /// </summary>
        private void CrearTablas(SQLiteConnection connection)
        {
            string createTableProductos = @"
                CREATE TABLE IF NOT EXISTS Productos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Codigo TEXT,
                    Nombre TEXT NOT NULL,
                    PrecioFabrica REAL NOT NULL DEFAULT 0,
                    PrecioVenta REAL NOT NULL,
                    Cantidad INTEGER NOT NULL DEFAULT 0,
                    FechaVencimiento TEXT,
                    FechaRegistro TEXT NOT NULL
                );
            ";

            string createTableVentas = @"
                CREATE TABLE IF NOT EXISTS Ventas (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductoId INTEGER NOT NULL,
                    Cantidad INTEGER NOT NULL,
                    PrecioUnitario REAL NOT NULL,
                    Total REAL NOT NULL,
                    FechaVenta TEXT NOT NULL,
                    Tipo TEXT DEFAULT 'Venta',
                    Descripcion TEXT,
                    FOREIGN KEY (ProductoId) REFERENCES Productos(Id)
                );
            ";

            string createTableConfiguracion = @"
                CREATE TABLE IF NOT EXISTS Configuracion (
                    Clave TEXT PRIMARY KEY,
                    Valor TEXT NOT NULL
                );
            ";

            string createTableUsuarios = @"
                CREATE TABLE IF NOT EXISTS Usuarios (
                    Correo Text Primary Key,
                    Nombre Text,
                    Password Text,
                    FotoUrl Text);";

            using (var command = new SQLiteCommand(connection))
            {
                // Crear tabla de productos
                command.CommandText = createTableProductos;
                command.ExecuteNonQuery();

                // Crear tabla de ventas
                command.CommandText = createTableVentas;
                command.ExecuteNonQuery();

                // Crear tabla de configuración
                command.CommandText = createTableConfiguracion;
                command.ExecuteNonQuery();

                //Crear tabla de usuarios
                command.CommandText = createTableUsuarios;
                command.ExecuteNonQuery();
                // Crear índices para mejorar el rendimiento
                command.CommandText = "CREATE INDEX IF NOT EXISTS idx_productos_codigo ON Productos(Codigo);";
                command.ExecuteNonQuery();

                command.CommandText = "CREATE INDEX IF NOT EXISTS idx_ventas_fecha ON Ventas(FechaVenta);";
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Inserta datos de ejemplo (opcional)
        /// </summary>
        private void InsertarDatosIniciales(SQLiteConnection connection)
        {
            string insertDatosEjemplo = @"
                INSERT INTO Productos (Codigo, Nombre, PrecioFabrica, PrecioVenta, Cantidad, FechaRegistro)
                VALUES 
                    ('001', 'Laptop HP', 800.00, 1200.00, 5, datetime('now')),
                    ('002', 'Mouse Inalámbrico', 10.00, 25.00, 50, datetime('now')),
                    ('003', 'Teclado Mecánico', 40.00, 80.00, 15, datetime('now'));
            ";

            using (var command = new SQLiteCommand(insertDatosEjemplo, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Obtiene la ruta completa de la base de datos
        /// </summary>
        public string GetDatabasePath()
        {
            return _databasePath;
        }

        /// <summary>
        /// Verifica si la base de datos existe y está accesible
        /// </summary>
        public bool VerificarConexion()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    return connection.State == System.Data.ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Crea una copia de seguridad de la base de datos
        /// </summary>
        public void CrearBackup(string rutaBackup)
        {
            try
            {
                File.Copy(_databasePath, rutaBackup, true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear backup: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Restaura una copia de seguridad
        /// </summary>
        public void RestaurarBackup(string rutaBackup)
        {
            try
            {
                if (!File.Exists(rutaBackup))
                {
                    throw new FileNotFoundException("El archivo de backup no existe");
                }

                File.Copy(rutaBackup, _databasePath, true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al restaurar backup: {ex.Message}", ex);
            }
        }
    }
}