using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class USUARIO
    {
        public int    IdUsuario    { get; set; }
        public string Nombre      { get; set; }   // login username (NombreUsuario en BD)
        public string Password    { get; set; }
        public string Rol         { get; set; }
        public string NombrePersona { get; set; } // primer nombre visible (Nombre en BD)
        public string Apellido    { get; set; }
        public string Email       { get; set; }
        public string Telefono    { get; set; }
        public string Direccion   { get; set; }
    }
}
