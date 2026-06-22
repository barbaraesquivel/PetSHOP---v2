using DAL;
using SEGURIDAD;
using SERV;
using System;
using System.Data;

namespace BLL
{
    public class BLLUSUARIO
    {
        MP_USUARIO mapperUsuario = new MP_USUARIO();

        public void AgregarUsuario(BE.USUARIO usuario, string auditor)
        {
            usuario.Password = Encriptacion.HashSHA256(usuario.Password);
            mapperUsuario.Insertar(usuario);
            Bitacora.Registrar(auditor, "AGREGAR_USUARIO", "Nuevo usuario: " + usuario.Nombre);
        }

        public DataTable ListarUsuarios()
        {
            return mapperUsuario.ListarComoTabla();
        }

        public BE.USUARIO ObtenerParaEditar(int id)
        {
            return mapperUsuario.ObtenerPorId(id);
        }

        public string EliminarUsuario(int id, string auditor)
        {
            BE.USUARIO u = mapperUsuario.ObtenerPorId(id);
            mapperUsuario.Deletear(new BE.USUARIO { IdUsuario = id });
            Bitacora.Registrar(auditor, "ELIMINAR_USUARIO", "Usuario eliminado: " + u.Nombre);
            return u.Nombre;
        }

        public void CambiarRol(int id, string rol, string auditor)
        {
            mapperUsuario.ActualizarRol(id, rol);
            Bitacora.Registrar(auditor, "EDITAR_USUARIO", "Id:" + id + " nuevo rol: " + rol);
        }

        public BE.USUARIO Autenticar(string usuario, string clave)
        {
            string hash = Encriptacion.HashSHA256(clave);
            return mapperUsuario.ObtenerPorCredenciales(usuario, hash);
        }

        public void Registrar(string nombre, string regNombre, string apellido, string email,
                              string telefono, string direccion, string pass)
        {
            if (mapperUsuario.ExisteNombre(nombre))
                throw new Exception("Ese nombre de usuario ya esta en uso. Elegi otro.");

            BE.USUARIO u = new BE.USUARIO
            {
                Nombre        = nombre,
                Password      = Encriptacion.HashSHA256(pass),
                Rol           = "Usuario",
                NombrePersona = regNombre,
                Apellido      = apellido,
                Email         = email,
                Telefono      = telefono,
                Direccion     = direccion
            };
            mapperUsuario.InsertarCompleto(u);
            Bitacora.Registrar(nombre, "REGISTRO", "Nuevo usuario: " + regNombre + " " + apellido + " <" + email + ">");
        }
    }
}
