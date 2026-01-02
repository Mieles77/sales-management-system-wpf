using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prueba_Apis.Model
{
    using System;

    namespace Prueba_Apis.Model
    {
        public class Usuario
        {
            public int Id { get; set; }
            public string Correo { get; set; }
            public string NombreUsuario { get; set; }
            public string Password { get; set; }
            public string FotoURL { get; set; }
            public DateTime FechaRegistro { get; set; } // <--- CRUCIAL: Para calcular los 15 días
            public bool EstaSuscrito { get; set; }      // <--- Para saber si ya pagó
        }
    }
}
