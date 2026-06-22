using System;
using BLL;

public partial class _Default : System.Web.UI.Page
{
    BLLUSUARIO gestorUsuario = new BLLUSUARIO();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack && Session["Usuario"] != null)
            Response.Redirect("Menu.aspx", false);
    }

    protected void btnIngresar_Click(object sender, EventArgs e)
    {
        string usuario = txtUsuario.Text.Trim().ToLower();

        if (Application["DBDisponible"] != null && !(bool)Application["DBDisponible"])
        {
            lblError.Text    = "No esta disponible el sistema. Intente mas tarde.";
            lblError.Visible = true;
            return;
        }

        try
        {
            BE.USUARIO u = gestorUsuario.Autenticar(usuario, txtContrasena.Text);

            if (u != null)
            {
                Session["IdUsuario"] = u.IdUsuario;
                Session["Usuario"]   = u.Nombre;
                Session["Rol"]       = u.Rol;

                bool bloqueado = Application["SistemaBlockeado"] != null && (bool)Application["SistemaBlockeado"];
                if (bloqueado && u.Rol != "WebMaster")
                {
                    Session.Clear();
                    Session.Abandon();
                    BLLAdmin.Registrar(usuario, "LOGIN_BLOQUEADO", "Sistema bloqueado - acceso denegado");
                    Response.Redirect("Error.aspx?motivo=integridad", false);
                    return;
                }

                BLLAdmin.Registrar(usuario, "LOGIN", "Login exitoso - Rol: " + u.Rol);
                if (bloqueado)
                    Response.Redirect("WebMaster.aspx", false);
                else
                    Response.Redirect("Menu.aspx", false);
            }
            else
            {
                BLLAdmin.Registrar(usuario, "LOGIN_FALLO", "Credenciales incorrectas");
                lblError.Text    = "Usuario o contrasena incorrectos.";
                lblError.Visible = true;
            }
        }
        catch (Exception ex)
        {
            BLLAdmin.Registrar(usuario, "LOGIN_ERROR", ex.Message);
            lblError.Text    = "Error BD: " + ex.Message;
            lblError.Visible = true;
        }
    }

    protected void btnIrRegistro_Click(object sender, EventArgs e)
    {
        lblError.Visible      = false;
        pnlLogin.Visible      = false;
        pnlRegistro.Visible   = true;
        lblRegMensaje.Visible = false;
    }

    protected void btnIrLogin_Click(object sender, EventArgs e)
    {
        pnlRegistro.Visible   = false;
        pnlLogin.Visible      = true;
        lblRegMensaje.Visible = false;
    }

    protected void btnRegistrar_Click(object sender, EventArgs e)
    {
        string nombre    = txtRegUsuario.Text.Trim().ToLower();
        string regNombre = txtRegNombre.Text.Trim();
        string apellido  = txtRegApellido.Text.Trim();
        string email     = txtRegEmail.Text.Trim();
        string telefono  = txtRegTelefono.Text.Trim();
        string direccion = txtRegDireccion.Text.Trim();
        string pass      = txtRegPass.Text;
        string conf      = txtRegConfirm.Text;

        if (nombre == "" || regNombre == "" || apellido == "" || email == "" || pass == "")
        {
            MostrarRegError("Usuario, nombre, apellido, email y contrasena son obligatorios.");
            return;
        }

        if (pass.Length < 4)
        {
            MostrarRegError("La contrasena debe tener al menos 4 caracteres.");
            return;
        }

        if (pass != conf)
        {
            MostrarRegError("Las contrasenas no coinciden.");
            return;
        }

        try
        {
            gestorUsuario.Registrar(nombre, regNombre, apellido, email, telefono, direccion, pass);

            txtRegUsuario.Text   = "";
            txtRegNombre.Text    = "";
            txtRegApellido.Text  = "";
            txtRegEmail.Text     = "";
            txtRegTelefono.Text  = "";
            txtRegDireccion.Text = "";
            txtRegPass.Text      = "";
            txtRegConfirm.Text   = "";
            pnlRegistro.Visible  = false;
            pnlLogin.Visible     = true;
            lblError.Text        = "Cuenta creada correctamente. Ya podes iniciar sesion.";
            lblError.CssClass    = "ok";
            lblError.Visible     = true;
        }
        catch (Exception ex)
        {
            MostrarRegError(ex.Message);
        }
    }

    private void MostrarRegError(string texto)
    {
        lblRegMensaje.Text     = texto;
        lblRegMensaje.CssClass = "error";
        lblRegMensaje.Visible  = true;
    }
}
