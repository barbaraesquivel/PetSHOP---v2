using BE;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DAL
{
    public class MP_CLIENTE : MAPPER<BE.Cliente>
    {
        public override void Deletear(Cliente obj)
        {
            throw new NotImplementedException();
        }

        public override void Insertar(Cliente obj)
        {
            throw new NotImplementedException();
        }

        public override void Updatear(Cliente obj)
        {
            throw new NotImplementedException();
        }

        public override List<Cliente> Listar()
        {
            var lista = new List<Cliente>();
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT IdCliente, IdUsuario, Nombre, Apellido, Email, Telefono, Direccion, FechaAlta " +
                    "FROM Clientes ORDER BY FechaAlta DESC", con);
                SqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                    lista.Add(Mapear(r));
                r.Close();
            }
            return lista;
        }

        // ── Extras ──────────────────────────────────────────────────────────

        public Cliente ObtenerPorIdUsuario(int idUsuario)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT IdCliente, IdUsuario, Nombre, Apellido, Email, Telefono, Direccion, FechaAlta " +
                    "FROM Clientes WHERE IdUsuario = @idUsuario", con);
                cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                SqlDataReader r = cmd.ExecuteReader();
                if (r.Read())
                {
                    Cliente c = Mapear(r);
                    r.Close();
                    return c;
                }
                r.Close();
                return null;
            }
        }

        public int InsertarEnTransaccion(SqlConnection con, SqlTransaction tx, Cliente obj)
        {
            SqlCommand cmd = new SqlCommand(
                @"INSERT INTO Clientes (IdUsuario, Nombre, Apellido, Email, Telefono, Direccion, FechaAlta)
                  OUTPUT INSERTED.IdCliente
                  VALUES (@idUsuario, @nombre, @apellido, @email, @telefono, @direccion, GETDATE())",
                con, tx);
            cmd.Parameters.AddWithValue("@idUsuario", obj.IdUsuario);
            cmd.Parameters.AddWithValue("@nombre",    obj.Nombre);
            cmd.Parameters.AddWithValue("@apellido",  obj.Apellido);
            cmd.Parameters.AddWithValue("@email",     obj.Email);
            cmd.Parameters.AddWithValue("@telefono",  string.IsNullOrEmpty(obj.Telefono)  ? (object)DBNull.Value : obj.Telefono);
            cmd.Parameters.AddWithValue("@direccion", string.IsNullOrEmpty(obj.Direccion) ? (object)DBNull.Value : obj.Direccion);
            return (int)cmd.ExecuteScalar();
        }

        // ── Helper privado ───────────────────────────────────────────────────

        private static Cliente Mapear(SqlDataReader r)
        {
            return new Cliente
            {
                IdCliente = (int)r["IdCliente"],
                IdUsuario = (int)r["IdUsuario"],
                Nombre    = r["Nombre"].ToString(),
                Apellido  = r["Apellido"].ToString(),
                Email     = r["Email"].ToString(),
                Telefono  = r["Telefono"]  == DBNull.Value ? "" : r["Telefono"].ToString(),
                Direccion = r["Direccion"] == DBNull.Value ? "" : r["Direccion"].ToString(),
                FechaAlta = (DateTime)r["FechaAlta"]
            };
        }
    }
}
