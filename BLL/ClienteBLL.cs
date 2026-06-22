using System.Collections.Generic;
using System.Data.SqlClient;
using BE;
using DAL;

namespace BLL
{
    public static class ClienteBLL
    {
        static readonly MP_CLIENTE mapper = new MP_CLIENTE();

        public static Cliente GetByIdUsuario(int idUsuario)
        {
            return mapper.ObtenerPorIdUsuario(idUsuario);
        }

        public static int Crear(SqlConnection con, SqlTransaction tx,
            int idUsuario, string nombre, string apellido, string email,
            string telefono, string direccion)
        {
            Cliente c = new Cliente
            {
                IdUsuario = idUsuario,
                Nombre    = nombre,
                Apellido  = apellido,
                Email     = email,
                Telefono  = telefono,
                Direccion = direccion
            };
            return mapper.InsertarEnTransaccion(con, tx, c);
        }

        public static List<Cliente> GetAll()
        {
            return mapper.Listar();
        }
    }
}
