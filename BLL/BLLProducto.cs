using System.Data;
using DAL;
using SERV;

namespace BLL
{
    public static class BLLProducto
    {
        static readonly MP_PRODUCTO mapper = new MP_PRODUCTO();

        public static DataTable ListarProductos()
        {
            return mapper.ListarComoTabla();
        }

        public static BE.Producto ObtenerParaEditar(int id)
        {
            return mapper.ObtenerPorId(id);
        }

        public static void AgregarProducto(string nombre, string desc, decimal precio, string categoria, string auditor)
        {
            BE.Producto p = new BE.Producto
            {
                Nombre      = nombre,
                Descripcion = desc,
                Precio      = precio,
                Categoria   = categoria
            };
            mapper.Insertar(p);                                                          // p.IdProducto queda seteado
            mapper.ActualizarHash(p.IdProducto, Catalogo.CalcularHash(p.IdProducto, nombre, desc, precio, categoria));
            Bitacora.Registrar(auditor, "AGREGAR_PRODUCTO", "Producto: " + nombre);
        }

        public static string EliminarProducto(int id, string auditor)
        {
            BE.Producto p = mapper.ObtenerPorId(id);
            mapper.Deletear(new BE.Producto { IdProducto = id });
            Bitacora.Registrar(auditor, "DESACTIVAR_PRODUCTO", "Producto: " + p.Nombre);
            return p.Nombre;
        }

        public static void ActualizarProducto(int id, string nombre, string desc, decimal precio, string categoria, string auditor)
        {
            BE.Producto p = new BE.Producto
            {
                IdProducto      = id,
                Nombre          = nombre,
                Descripcion     = desc,
                Precio          = precio,
                Categoria       = categoria,
                HashVerificador = Catalogo.CalcularHash(id, nombre, desc, precio, categoria)
            };
            mapper.Updatear(p);
            Bitacora.Registrar(auditor, "EDITAR_PRODUCTO", "Id:" + id + " - " + nombre);
        }

        public static (string Nombre, int Stock) ObtenerStock(int id)
        {
            return mapper.ObtenerNombreYStock(id);
        }

        public static (string Nombre, int StockAnterior) ActualizarStock(int id, int nuevoStock, string auditor)
        {
            var actual = mapper.ObtenerNombreYStock(id);
            mapper.ActualizarStock(id, nuevoStock);
            Bitacora.Registrar(auditor, "STOCK_ACTUALIZADO",
                "Producto: " + actual.Nombre + " | Anterior: " + actual.Stock + " | Nuevo: " + nuevoStock);
            return (actual.Nombre, actual.Stock);
        }

        public static DataTable GetAlertaStock()
        {
            return mapper.GetAlertaStock();
        }
    }
}
