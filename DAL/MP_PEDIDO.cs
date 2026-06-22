using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class MP_PEDIDO : MAPPER<BE.Pedido>
    {
        public override void Insertar(Pedido obj) { throw new NotImplementedException(); }
        public override void Updatear(Pedido obj) { throw new NotImplementedException(); }
        public override void Deletear(Pedido obj) { throw new NotImplementedException(); }
        public override List<Pedido> Listar()     { throw new NotImplementedException(); }

        // ── Extras ──────────────────────────────────────────────────────────

        public DataTable GetByCliente(int idCliente)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(
                    @"SELECT IdPedido, FechaPedido, Total, Estado,
                             ISNULL(ModificadoPor, '') AS ModificadoPor
                      FROM Pedidos
                      WHERE IdCliente = @id
                      ORDER BY FechaPedido DESC", con);
                da.SelectCommand.Parameters.AddWithValue("@id", idCliente);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public DataTable GetDetalleByPedido(int idPedido)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(
                    @"SELECT NombreProducto, PrecioUnitario, Cantidad, Subtotal
                      FROM DetallePedido
                      WHERE IdPedido = @id
                      ORDER BY IdDetalle", con);
                da.SelectCommand.Parameters.AddWithValue("@id", idPedido);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public DataTable GetAllAdmin()
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(
                    @"SELECT p.IdPedido,
                             ISNULL(c.Nombre + ' ' + c.Apellido, 'Sin cliente') AS NombreCliente,
                             p.FechaPedido, p.Total, p.Estado,
                             ISNULL(p.ModificadoPor, '') AS ModificadoPor
                      FROM Pedidos p
                      LEFT JOIN Clientes c ON p.IdCliente = c.IdCliente
                      ORDER BY p.FechaPedido DESC", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public string GetEstadoActual(int idPedido)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT Estado FROM Pedidos WHERE IdPedido=@id", con);
                cmd.Parameters.AddWithValue("@id", idPedido);
                object result = cmd.ExecuteScalar();
                if (result == null)
                    throw new Exception("Pedido #" + idPedido + " no encontrado.");
                return result.ToString();
            }
        }

        public void ActualizarEstado(int idPedido, string nuevoEstado, string usuario)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"UPDATE Pedidos
                      SET Estado=@estado, ModificadoPor=@usuario, FechaModif=GETDATE()
                      WHERE IdPedido=@id", con);
                cmd.Parameters.AddWithValue("@estado",  nuevoEstado);
                cmd.Parameters.AddWithValue("@usuario", usuario);
                cmd.Parameters.AddWithValue("@id",      idPedido);
                cmd.ExecuteNonQuery();
            }
        }

        public void CancelarConRestaurarStock(int idPedido, string usuario)
        {
            using (SqlConnection con = ConexionBD.ObtenerConexion())
            {
                con.Open();
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    SqlCommand cmdItems = new SqlCommand(
                        "SELECT IdProducto, Cantidad FROM DetallePedido WHERE IdPedido=@id AND IdProducto IS NOT NULL",
                        con, tx);
                    cmdItems.Parameters.AddWithValue("@id", idPedido);
                    SqlDataReader r = cmdItems.ExecuteReader();
                    var items = new List<int[]>();
                    while (r.Read())
                        items.Add(new int[] { (int)r["IdProducto"], (int)r["Cantidad"] });
                    r.Close();

                    foreach (int[] item in items)
                    {
                        SqlCommand cmdStock = new SqlCommand(
                            "UPDATE Productos SET Stock = Stock + @cant WHERE IdProducto = @id",
                            con, tx);
                        cmdStock.Parameters.AddWithValue("@cant", item[1]);
                        cmdStock.Parameters.AddWithValue("@id",   item[0]);
                        cmdStock.ExecuteNonQuery();
                    }

                    SqlCommand cmdCancel = new SqlCommand(
                        @"UPDATE Pedidos
                          SET Estado='Cancelado', ModificadoPor=@usuario, FechaModif=GETDATE()
                          WHERE IdPedido=@id",
                        con, tx);
                    cmdCancel.Parameters.AddWithValue("@usuario", usuario);
                    cmdCancel.Parameters.AddWithValue("@id",      idPedido);
                    cmdCancel.ExecuteNonQuery();

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }
}
