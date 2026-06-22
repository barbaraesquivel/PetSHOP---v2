using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class MP_PRODUCTO : MAPPER<BE.Producto>
    {
        // ── 4 métodos base ───────────────────────────────────────────────────

        // Inserta el producto y actualiza obj.IdProducto con el IDENTITY generado.
        public override void Insertar(Producto obj)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO Productos (Nombre, Descripcion, Precio, Categoria, Activo, HashVerificador)
                      OUTPUT INSERTED.IdProducto
                      VALUES (@n, @d, @p, @c, 1, '')", con);
                cmd.Parameters.AddWithValue("@n", obj.Nombre);
                cmd.Parameters.AddWithValue("@d", string.IsNullOrEmpty(obj.Descripcion) ? (object)DBNull.Value : obj.Descripcion);
                cmd.Parameters.AddWithValue("@p", obj.Precio);
                cmd.Parameters.AddWithValue("@c", obj.Categoria);
                obj.IdProducto = (int)cmd.ExecuteScalar();
            }
        }

        // Actualiza todos los campos editables, incluyendo HashVerificador.
        public override void Updatear(Producto obj)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"UPDATE Productos
                      SET Nombre=@n, Descripcion=@d, Precio=@p, Categoria=@c, HashVerificador=@h
                      WHERE IdProducto=@id", con);
                cmd.Parameters.AddWithValue("@n",  obj.Nombre);
                cmd.Parameters.AddWithValue("@d",  string.IsNullOrEmpty(obj.Descripcion) ? (object)DBNull.Value : obj.Descripcion);
                cmd.Parameters.AddWithValue("@p",  obj.Precio);
                cmd.Parameters.AddWithValue("@c",  obj.Categoria);
                cmd.Parameters.AddWithValue("@h",  obj.HashVerificador);
                cmd.Parameters.AddWithValue("@id", obj.IdProducto);
                cmd.ExecuteNonQuery();
            }
        }

        // Baja lógica: Activo=0 y Eliminado=1.
        public override void Deletear(Producto obj)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Productos SET Activo=0, Eliminado=1 WHERE IdProducto=@id", con);
                cmd.Parameters.AddWithValue("@id", obj.IdProducto);
                cmd.ExecuteNonQuery();
            }
        }

        public override List<Producto> Listar()
        {
            var lista = new List<Producto>();
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT IdProducto, Nombre, Descripcion, Precio, Categoria, Stock, Activo, HashVerificador, Eliminado " +
                    "FROM Productos ORDER BY Nombre", con);
                SqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                    lista.Add(Mapear(r));
                r.Close();
            }
            return lista;
        }

        // ── Extras ──────────────────────────────────────────────────────────

        public DataTable ListarComoTabla()
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT IdProducto, Nombre, Descripcion, Precio, Categoria, Stock, Activo FROM Productos ORDER BY Nombre", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public Producto ObtenerPorId(int id)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT IdProducto, Nombre, Descripcion, Precio, Categoria FROM Productos WHERE IdProducto=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader r = cmd.ExecuteReader();
                if (r.Read())
                {
                    Producto p = new Producto
                    {
                        IdProducto  = (int)r["IdProducto"],
                        Nombre      = r["Nombre"].ToString(),
                        Descripcion = r["Descripcion"] == DBNull.Value ? "" : r["Descripcion"].ToString(),
                        Precio      = (decimal)r["Precio"],
                        Categoria   = r["Categoria"].ToString()
                    };
                    r.Close();
                    return p;
                }
                r.Close();
                return null;
            }
        }

        public (string Nombre, int Stock) ObtenerNombreYStock(int id)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT Nombre, Stock FROM Productos WHERE IdProducto=@id", con);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader r = cmd.ExecuteReader();
                if (r.Read())
                {
                    string nombre = r["Nombre"].ToString();
                    int    stock  = (int)r["Stock"];
                    r.Close();
                    return (nombre, stock);
                }
                r.Close();
                return (null, 0);
            }
        }

        public void ActualizarStock(int id, int nuevoStock)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Productos SET Stock=@s WHERE IdProducto=@id", con);
                cmd.Parameters.AddWithValue("@s",  nuevoStock);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarHash(int id, string hash)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Productos SET HashVerificador=@h WHERE IdProducto=@id", con);
                cmd.Parameters.AddWithValue("@h",  hash);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable GetAlertaStock()
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT IdProducto, Nombre, Stock FROM Productos WHERE Activo=1 AND Eliminado=0 AND Stock <= 5 ORDER BY Stock, Nombre", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // ── Helper privado ───────────────────────────────────────────────────

        private static Producto Mapear(SqlDataReader r)
        {
            return new Producto
            {
                IdProducto      = (int)r["IdProducto"],
                Nombre          = r["Nombre"].ToString(),
                Descripcion     = r["Descripcion"] == DBNull.Value ? "" : r["Descripcion"].ToString(),
                Precio          = (decimal)r["Precio"],
                Categoria       = r["Categoria"].ToString(),
                Stock           = (int)r["Stock"],
                Activo          = (bool)r["Activo"],
                HashVerificador = r["HashVerificador"].ToString(),
                Eliminado       = (bool)r["Eliminado"]
            };
        }
    }
}
