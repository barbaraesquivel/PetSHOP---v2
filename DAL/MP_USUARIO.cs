using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class MP_USUARIO : MAPPER<BE.USUARIO>
    {
        public override void Deletear(USUARIO obj)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Usuarios WHERE IdUsuario=@id", con);
                cmd.Parameters.AddWithValue("@id", obj.IdUsuario);
                cmd.ExecuteNonQuery();
            }
        }

        public override void Insertar(USUARIO obj)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Usuarios (NombreUsuario, PasswordHash, Rol) VALUES (@n, @h, @r)", con);
                cmd.Parameters.AddWithValue("@n", obj.Nombre);
                cmd.Parameters.AddWithValue("@h", obj.Password);
                cmd.Parameters.AddWithValue("@r", obj.Rol);
                cmd.ExecuteNonQuery();
            }
        }

        public override List<USUARIO> Listar()
        {
            var lista = new List<USUARIO>();
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT IdUsuario, NombreUsuario, Rol FROM Usuarios ORDER BY NombreUsuario", con);
                SqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                    lista.Add(new USUARIO
                    {
                        IdUsuario = (int)r["IdUsuario"],
                        Nombre    = r["NombreUsuario"].ToString(),
                        Rol       = r["Rol"].ToString()
                    });
                r.Close();
            }
            return lista;
        }

        public override void Updatear(USUARIO obj)
        {
            throw new NotImplementedException();
        }

        // ── Extras ──────────────────────────────────────────────────────────

        public DataTable ListarComoTabla()
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT IdUsuario, NombreUsuario, Rol FROM Usuarios ORDER BY NombreUsuario", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public USUARIO ObtenerPorId(int id)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT IdUsuario, NombreUsuario, Rol FROM Usuarios WHERE IdUsuario=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader r = cmd.ExecuteReader();
                if (r.Read())
                {
                    USUARIO u = new USUARIO
                    {
                        IdUsuario = (int)r["IdUsuario"],
                        Nombre    = r["NombreUsuario"].ToString(),
                        Rol       = r["Rol"].ToString()
                    };
                    r.Close();
                    return u;
                }
                r.Close();
                return null;
            }
        }

        public void ActualizarRol(int id, string rol)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Usuarios SET Rol=@r WHERE IdUsuario=@id", con);
                cmd.Parameters.AddWithValue("@r",  rol);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public USUARIO ObtenerPorCredenciales(string usuario, string hash)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT IdUsuario, NombreUsuario, Rol FROM Usuarios WHERE NombreUsuario=@u AND PasswordHash=@h", con);
                cmd.Parameters.AddWithValue("@u", usuario);
                cmd.Parameters.AddWithValue("@h", hash);
                SqlDataReader r = cmd.ExecuteReader();
                if (r.Read())
                {
                    USUARIO u = new USUARIO
                    {
                        IdUsuario = (int)r["IdUsuario"],
                        Nombre    = r["NombreUsuario"].ToString(),
                        Rol       = r["Rol"].ToString()
                    };
                    r.Close();
                    return u;
                }
                r.Close();
                return null;
            }
        }

        public bool ExisteNombre(string nombre)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(1) FROM Usuarios WHERE NombreUsuario=@n", con);
                cmd.Parameters.AddWithValue("@n", nombre);
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public void InsertarCompleto(USUARIO obj)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO Usuarios (NombreUsuario, PasswordHash, Rol, Nombre, Apellido, Email, Telefono, Direccion)
                      VALUES (@n, @h, @r, @nombre, @apellido, @email, @telefono, @direccion)", con);
                cmd.Parameters.AddWithValue("@n",        obj.Nombre);
                cmd.Parameters.AddWithValue("@h",        obj.Password);
                cmd.Parameters.AddWithValue("@r",        obj.Rol);
                cmd.Parameters.AddWithValue("@nombre",   string.IsNullOrEmpty(obj.NombrePersona) ? (object)DBNull.Value : obj.NombrePersona);
                cmd.Parameters.AddWithValue("@apellido", string.IsNullOrEmpty(obj.Apellido)      ? (object)DBNull.Value : obj.Apellido);
                cmd.Parameters.AddWithValue("@email",    string.IsNullOrEmpty(obj.Email)         ? (object)DBNull.Value : obj.Email);
                cmd.Parameters.AddWithValue("@telefono", string.IsNullOrEmpty(obj.Telefono)      ? (object)DBNull.Value : obj.Telefono);
                cmd.Parameters.AddWithValue("@direccion",string.IsNullOrEmpty(obj.Direccion)     ? (object)DBNull.Value : obj.Direccion);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
