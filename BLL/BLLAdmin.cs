using SERV;
using System.Web.UI;

namespace BLL
{
    public static class BLLAdmin
    {
        public static bool VerificarSesion(Page page)
        {
            return SessionHelper.VerificarSesion(page);
        }

        public static bool VerificarDB(Page page)
        {
            return SessionHelper.VerificarDB(page);
        }

        public static void RegistrarAcceso(string usuario)
        {
            Bitacora.Registrar(usuario, "ACCESO", "Admin.aspx");
        }

        public static bool VerificarRol(Page page, string rol)
        {
            return SessionHelper.VerificarRol(page, rol);
        }

        public static void Registrar(string usuario, string accion, string detalle)
        {
            Bitacora.Registrar(usuario, accion, detalle);
        }
    }
}
