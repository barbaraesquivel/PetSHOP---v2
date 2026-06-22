using System;
using System.Data;
using DAL;
using SERV;

namespace BLL
{
    public static class PedidoBLL
    {
        static readonly MP_PEDIDO mapper = new MP_PEDIDO();

        // ── Máquina de estados (lógica pura) ────────────────────────────────

        public static string GetSiguienteEstado(string estado)
        {
            switch (estado)
            {
                case "Pendiente":        return "Confirmado";
                case "Confirmado":       return "EnPreparacion";
                case "EnPreparacion":    return "ListoParaRetirar";
                case "ListoParaRetirar": return "Retirado";
                default:                 return null;
            }
        }

        public static string GetEtiquetaAvance(string estado)
        {
            switch (estado)
            {
                case "Pendiente":        return "Confirmar";
                case "Confirmado":       return "Iniciar preparacion";
                case "EnPreparacion":    return "Listo para retirar";
                case "ListoParaRetirar": return "Marcar retirado";
                default:                 return "";
            }
        }

        public static bool PuedeCancelarAdmin(string estado)
        {
            return estado != "ListoParaRetirar" && estado != "Retirado" && estado != "Cancelado";
        }

        public static bool PuedeCancelarCliente(string estado)
        {
            return estado == "Pendiente";
        }

        // ── Consultas ────────────────────────────────────────────────────────

        public static DataTable GetByCliente(int idCliente)
        {
            return mapper.GetByCliente(idCliente);
        }

        public static DataTable GetDetalleByPedido(int idPedido)
        {
            return mapper.GetDetalleByPedido(idPedido);
        }

        public static DataTable GetAllAdmin()
        {
            return mapper.GetAllAdmin();
        }

        // ── Cambios de estado ────────────────────────────────────────────────

        public static string AvanzarEstado(int idPedido, string usuario)
        {
            string estadoActual = mapper.GetEstadoActual(idPedido);
            string nuevoEstado  = GetSiguienteEstado(estadoActual);

            if (nuevoEstado == null)
                throw new Exception("El pedido #" + idPedido + " no puede avanzar desde: " + estadoActual);

            mapper.ActualizarEstado(idPedido, nuevoEstado, usuario);
            Bitacora.Registrar(usuario, "PEDIDO_AVANCE", "Pedido #" + idPedido + " -> " + nuevoEstado);
            return nuevoEstado;
        }

        public static string Cancelar(int idPedido, string usuario, bool esAdmin)
        {
            string estadoActual = mapper.GetEstadoActual(idPedido);

            if (esAdmin)
            {
                if (!PuedeCancelarAdmin(estadoActual))
                    throw new Exception("No se puede cancelar un pedido en estado: " + estadoActual);
            }
            else
            {
                if (!PuedeCancelarCliente(estadoActual))
                    throw new Exception("Solo puede cancelar pedidos en estado Pendiente. Estado actual: " + estadoActual);
            }

            mapper.CancelarConRestaurarStock(idPedido, usuario);
            Bitacora.Registrar(usuario, "PEDIDO_CANCELADO",
                "Pedido #" + idPedido + ": " + estadoActual + " -> Cancelado");
            return estadoActual;
        }
    }
}
