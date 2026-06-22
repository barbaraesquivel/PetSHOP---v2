using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;
using BE;
using BLL;

public partial class Admin : System.Web.UI.Page
{

    BLL.BLLUSUARIO gestorUsuario = new BLL.BLLUSUARIO();
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!BLLAdmin.VerificarSesion(this)) return;

        string rol = Session["Rol"].ToString();

        if (rol != "Admin" && rol != "WebMaster")
        {
            pnlDenegado.Visible  = true;
            pnlContenido.Visible = false;
            return;
        }

        bool bloqueado = Application["SistemaBlockeado"] != null && (bool)Application["SistemaBlockeado"];
        if (bloqueado)
        {
            if (rol == "WebMaster")
                Response.Redirect("WebMaster.aspx", false);
            else
                Response.Redirect("Error.aspx?motivo=integridad", false);
            return;
        }

        if (!BLLAdmin.VerificarDB(this))
        {
            pnlDenegado.Visible  = true;
            pnlContenido.Visible = false;
            return;
        }

        pnlContenido.Visible = true;
        pnlDenegado.Visible  = false;
        lblAdminUser.Text    = Session["Usuario"].ToString();

        if (rol == "WebMaster")
        {
            pnlFormUsuario.Visible  = false;
            pnlFormProducto.Visible = false;
            lblModo.Text    = "Modo solo lectura (WebMaster). Sin permisos de modificacion.";
            lblModo.Visible = true;
        }

        if (!IsPostBack)
        {
            CargarUsuarios();
            CargarProductos();
            CargarClientes();
            CargarAlertaStock();
            CargarPedidos();
            BLLAdmin.RegistrarAcceso(Session["Usuario"].ToString());
        }
    }

    // CARGA DE GRILLAS

    private void CargarUsuarios()
    {
        try
        {
            gvUsuarios.DataSource = gestorUsuario.ListarUsuarios();
            gvUsuarios.DataBind();
            ReaplicarSeleccionUsuario();
        }
        catch (Exception ex)
        {
            MostrarMensaje("Error al cargar usuarios: " + ex.Message, true);
        }
    }

    private void CargarProductos()
    {
        try
        {
            gvProductos.DataSource = BLLProducto.ListarProductos();
            gvProductos.DataBind();
            ReaplicarSeleccionProducto();
        }
        catch (Exception ex)
        {
            MostrarMensaje("Error al cargar productos: " + ex.Message, true);
        }
    }

    // vuelve a resaltar la fila seleccionada despues de cada postback
    private void ReaplicarSeleccionUsuario()
    {
        int selId;
        if (!int.TryParse(hfSelectedUserId.Value, out selId) || selId <= 0) return;
        for (int i = 0; i < gvUsuarios.Rows.Count; i++)
        {
            if ((int)gvUsuarios.DataKeys[i].Value == selId)
            {
                string rowClientId = gvUsuarios.Rows[i].ClientID;
                Page.ClientScript.RegisterStartupScript(GetType(), "reselUser",
                    "var r=document.getElementById('" + rowClientId + "'); if(r) r.classList.add('fila-seleccionada');", true);
                break;
            }
        }
    }

    private void ReaplicarSeleccionProducto()
    {
        int selId;
        if (!int.TryParse(hfSelectedProdId.Value, out selId) || selId <= 0) return;
        for (int i = 0; i < gvProductos.Rows.Count; i++)
        {
            if ((int)gvProductos.DataKeys[i].Value == selId)
            {
                string rowClientId = gvProductos.Rows[i].ClientID;
                Page.ClientScript.RegisterStartupScript(GetType(), "reselProd",
                    "var r=document.getElementById('" + rowClientId + "'); if(r) r.classList.add('fila-seleccionada');", true);
                break;
            }
        }
    }

    // Hace las filas clickeables: JS puro actualiza el HiddenField sin postback
    protected void gvUsuarios_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            string id = gvUsuarios.DataKeys[e.Row.RowIndex].Value.ToString();
            e.Row.Attributes["onclick"] = "seleccionarFila(this,'" + id + "','hfSelectedUserId')";
            e.Row.Style["cursor"] = "pointer";
            e.Row.ToolTip = "Clic para seleccionar";
        }
    }

    protected void gvProductos_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            string id = gvProductos.DataKeys[e.Row.RowIndex].Value.ToString();
            e.Row.Attributes["onclick"] = "seleccionarFila(this,'" + id + "','hfSelectedProdId')";
            e.Row.Style["cursor"] = "pointer";
            e.Row.ToolTip = "Clic para seleccionar";
        }
    }

    // ACCIONES USUARIOS

    protected void btnAgregarUsuario_Click(object sender, EventArgs e)
    {
        string nombre = txtNombreU.Text.Trim().ToLower();
        string pass   = txtPassU.Text;
        string rol    = ddlRolU.SelectedValue;

        if (nombre == "" || pass == "")
        {
            MostrarMensaje("El nombre y la contrasena son obligatorios.", true);
            return;
        }

        BE.USUARIO usuario = new USUARIO();
        usuario.Nombre = nombre;
        usuario.Password = pass;
        usuario.Rol = rol;

        try
        {
            gestorUsuario.AgregarUsuario(usuario, Session["Usuario"].ToString());
            txtNombreU.Text = "";
            txtPassU.Text   = "";
            MostrarMensaje("Usuario '" + nombre + "' agregado correctamente.", false);
            CargarUsuarios();
        }
        catch (Exception ex)
        {
            MostrarMensaje("Error al agregar usuario: " + ex.Message, true);
        }
    }

    protected void btnEditarUsuario_Click(object sender, EventArgs e)
    {
        int id;
        if (!int.TryParse(hfSelectedUserId.Value, out id) || id <= 0)
        {
            MostrarMensaje("Seleccione un usuario de la tabla primero.", true);
            return;
        }

        try
        {
            BE.USUARIO u = gestorUsuario.ObtenerParaEditar(id);
            if (u != null)
            {
                hfIdUserEdit.Value       = id.ToString();
                lblNombreUserEdit.Text   = u.Nombre;
                ddlEditRol.SelectedValue = u.Rol;
                pnlEditarUsuario.Visible = true;
                Page.ClientScript.RegisterStartupScript(GetType(), "scrollUser",
                    "document.getElementById('pnlEditarUsuario').scrollIntoView({behavior:'smooth'});", true);
            }
        }
        catch (Exception ex)
        {
            MostrarMensaje("Error al cargar usuario: " + ex.Message, true);
        }
    }

    protected void btnEliminarUsuario_Click(object sender, EventArgs e)
    {
        int id;
        if (!int.TryParse(hfSelectedUserId.Value, out id) || id <= 0)
        {
            MostrarMensaje("Seleccione un usuario de la tabla primero.", true);
            return;
        }

        if (id == (int)Session["IdUsuario"])
        {
            MostrarMensaje("No puede eliminarse a si mismo.", true);
            return;
        }

        try
        {
            string nombre = gestorUsuario.EliminarUsuario(id, Session["Usuario"].ToString());
            hfSelectedUserId.Value = "0";
            MostrarMensaje("Usuario '" + nombre + "' eliminado.", false);
            CargarUsuarios();
        }
        catch (Exception ex)
        {
            string msg = ex.Message.Contains("REFERENCE") || ex.Message.Contains("FK")
                ? "No se puede eliminar: el usuario tiene pedidos asociados."
                : "Error al eliminar: " + ex.Message;
            MostrarMensaje(msg, true);
        }
    }

    protected void btnGuardarUsuario_Click(object sender, EventArgs e)
    {
        int    id  = int.Parse(hfIdUserEdit.Value);
        string rol = ddlEditRol.SelectedValue;

        try
        {
            gestorUsuario.CambiarRol(id, rol, Session["Usuario"].ToString());
            pnlEditarUsuario.Visible = false;
            MostrarMensaje("Usuario actualizado correctamente.", false);
            CargarUsuarios();
        }
        catch (Exception ex)
        {
            MostrarMensaje("Error al guardar: " + ex.Message, true);
        }
    }

    protected void btnCancelarEditUser_Click(object sender, EventArgs e)
    {
        pnlEditarUsuario.Visible = false;
    }

    // ACCIONES PRODUCTOS

    protected void btnAgregarProducto_Click(object sender, EventArgs e)
    {
        string  nombre    = txtNombreP.Text.Trim();
        string  desc      = txtDescP.Text.Trim();
        string  precioStr = txtPrecioP.Text.Trim().Replace(",", ".");
        string  categoria = ddlCatP.SelectedValue;
        decimal precio;

        if (nombre == "" || !decimal.TryParse(precioStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out precio))
        {
            MostrarMensaje("Nombre y precio son obligatorios. El precio debe ser numerico.", true);
            return;
        }

        try
        {
            BLLProducto.AgregarProducto(nombre, desc, precio, categoria, Session["Usuario"].ToString());
            txtNombreP.Text = "";
            txtDescP.Text   = "";
            txtPrecioP.Text = "";
            MostrarMensaje("Producto '" + nombre + "' agregado correctamente.", false);
            CargarProductos();
        }
        catch (Exception ex)
        {
            MostrarMensaje("Error al agregar producto: " + ex.Message, true);
        }
    }

    protected void btnEditarProducto_Click(object sender, EventArgs e)
    {
        int id;
        if (!int.TryParse(hfSelectedProdId.Value, out id) || id <= 0)
        {
            MostrarMensaje("Seleccione un producto de la tabla primero.", true);
            return;
        }

        try
        {
            BE.Producto p = BLLProducto.ObtenerParaEditar(id);
            if (p != null)
            {
                hfIdProdEdit.Value        = id.ToString();
                lblIdProdEdit.Text        = id.ToString();
                txtEditNombreP.Text       = p.Nombre;
                txtEditDescP.Text         = p.Descripcion;
                txtEditPrecioP.Text       = p.Precio.ToString("N2");
                ddlEditCatP.SelectedValue = p.Categoria;
                pnlEditarProducto.Visible = true;
                Page.ClientScript.RegisterStartupScript(GetType(), "scrollProd",
                    "document.getElementById('pnlEditarProducto').scrollIntoView({behavior:'smooth'});", true);
            }
        }
        catch (Exception ex)
        {
            MostrarMensaje("Error al cargar producto: " + ex.Message, true);
        }
    }

    protected void btnEliminarProducto_Click(object sender, EventArgs e)
    {
        int id;
        if (!int.TryParse(hfSelectedProdId.Value, out id) || id <= 0)
        {
            MostrarMensaje("Seleccione un producto de la tabla primero.", true);
            return;
        }

        try
        {
            string nombreProd = BLLProducto.EliminarProducto(id, Session["Usuario"].ToString());
            hfSelectedProdId.Value = "0";
            MostrarMensaje("Producto '" + nombreProd + "' desactivado.", false);
            CargarProductos();
        }
        catch (Exception ex)
        {
            MostrarMensaje("Error: " + ex.Message, true);
        }
    }

    protected void btnGuardarProducto_Click(object sender, EventArgs e)
    {
        int     id        = int.Parse(hfIdProdEdit.Value);
        string  nombre    = txtEditNombreP.Text.Trim();
        string  desc      = txtEditDescP.Text.Trim();
        string  precioStr = txtEditPrecioP.Text.Trim().Replace(",", ".");
        string  categoria = ddlEditCatP.SelectedValue;
        decimal precio;

        if (nombre == "" || !decimal.TryParse(precioStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out precio))
        {
            MostrarMensaje("Nombre y precio son obligatorios. El precio debe ser numerico.", true);
            return;
        }

        try
        {
            BLLProducto.ActualizarProducto(id, nombre, desc, precio, categoria, Session["Usuario"].ToString());
            pnlEditarProducto.Visible = false;
            MostrarMensaje("Producto actualizado. Hash recalculado.", false);
            CargarProductos();
        }
        catch (Exception ex)
        {
            MostrarMensaje("Error al guardar: " + ex.Message, true);
        }
    }

    protected void btnCancelarEditProd_Click(object sender, EventArgs e)
    {
        pnlEditarProducto.Visible = false;
    }

    // STOCK

    protected void btnGestionarStock_Click(object sender, EventArgs e)
    {
        int id;
        if (!int.TryParse(hfSelectedProdId.Value, out id) || id <= 0)
        {
            MostrarMensaje("Seleccione un producto de la tabla primero.", true);
            return;
        }

        try
        {
            var stock = BLLProducto.ObtenerStock(id);
            if (stock.Item1 != null)
            {
                hfIdStockEdit.Value        = id.ToString();
                lblNombreStock.Text        = stock.Item1;
                lblStockActual.Text        = stock.Item2.ToString();
                txtNuevoStock.Text         = stock.Item2.ToString();
                pnlActualizarStock.Visible = true;
                Page.ClientScript.RegisterStartupScript(GetType(), "scrollStock",
                    "document.getElementById('" + pnlActualizarStock.ClientID + "').scrollIntoView({behavior:'smooth'});", true);
            }
        }
        catch (Exception ex)
        {
            MostrarMensaje("Error al cargar stock: " + ex.Message, true);
        }
    }

    protected void btnActualizarStock_Click(object sender, EventArgs e)
    {
        int id = int.Parse(hfIdStockEdit.Value);
        int nuevoStock;
        if (!int.TryParse(txtNuevoStock.Text.Trim(), out nuevoStock) || nuevoStock < 0)
        {
            MostrarMensaje("Stock invalido. Debe ser un numero entero mayor o igual a 0.", true);
            return;
        }

        try
        {
            var resultado = BLLProducto.ActualizarStock(id, nuevoStock, Session["Usuario"].ToString());
            pnlActualizarStock.Visible = false;
            MostrarMensaje("Stock de '" + resultado.Item1 + "' actualizado: " + resultado.Item2 + " -> " + nuevoStock + ".", false);
            CargarProductos();
            CargarAlertaStock();
        }
        catch (Exception ex)
        {
            MostrarMensaje("Error al actualizar stock: " + ex.Message, true);
        }
    }

    protected void btnCancelarStock_Click(object sender, EventArgs e)
    {
        pnlActualizarStock.Visible = false;
    }

    private void CargarAlertaStock()
    {
        try
        {
            DataTable dt = BLLProducto.GetAlertaStock();
            pnlAlertaStock.Visible = dt.Rows.Count > 0;
            if (dt.Rows.Count > 0)
            {
                gvAlertaStock.DataSource = dt;
                gvAlertaStock.DataBind();
            }
        }
        catch { }
    }

    // CLIENTES

    private void CargarClientes()
    {
        try
        {
            List<Cliente> clientes = ClienteBLL.GetAll();
            gvClientes.DataSource = clientes;
            gvClientes.DataBind();
        }
        catch (Exception ex)
        {
            MostrarMensaje("Error al cargar clientes: " + ex.Message, true);
        }
    }

    // PEDIDOS

    private void CargarPedidos()
    {
        try
        {
            DataTable dt = PedidoBLL.GetAllAdmin();
            gvPedidos.DataSource = dt;
            gvPedidos.DataBind();
        }
        catch (Exception ex)
        {
            MostrarMensaje("Error al cargar pedidos: " + ex.Message, true);
        }
    }

    protected void gvPedidos_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow) return;

        DataRowView drv    = (DataRowView)e.Row.DataItem;
        string      estado = drv["Estado"].ToString();

        Button btnAvanzar  = (Button)e.Row.FindControl("btnAvanzarEstado");
        Button btnCancelar = (Button)e.Row.FindControl("btnCancelarPedidoAdmin");

        if (btnAvanzar != null)
        {
            string siguiente = PedidoBLL.GetSiguienteEstado(estado);
            btnAvanzar.Text    = siguiente != null ? PedidoBLL.GetEtiquetaAvance(estado) : "-";
            btnAvanzar.Enabled = siguiente != null;
        }

        if (btnCancelar != null)
            btnCancelar.Visible = PedidoBLL.PuedeCancelarAdmin(estado);
    }

    protected void gvPedidos_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        try
        {
            int idPedido;
            if (!int.TryParse(e.CommandArgument.ToString(), out idPedido) || idPedido <= 0)
            {
                MostrarMensajePedido("Pedido invalido. Recarga la pagina e intenta de nuevo.", true);
                return;
            }

            if (e.CommandName == "AvanzarEstado")
            {
                try
                {
                    string nuevoEstado = PedidoBLL.AvanzarEstado(idPedido, Session["Usuario"].ToString());
                    MostrarMensajePedido("Pedido #" + idPedido + " avanzado a: " + nuevoEstado, false);
                    CargarPedidos();
                    CargarAlertaStock();
                }
                catch (Exception ex)
                {
                    MostrarMensajePedido("Error al avanzar estado: " + ex.Message, true);
                }
            }
            else if (e.CommandName == "CancelarPedido")
            {
                try
                {
                    PedidoBLL.Cancelar(idPedido, Session["Usuario"].ToString(), esAdmin: true);
                    MostrarMensajePedido("Pedido #" + idPedido + " cancelado. Stock restaurado.", false);
                    pnlDetallePedidoAdmin.Visible = false;
                    CargarPedidos();
                    CargarAlertaStock();
                }
                catch (Exception ex)
                {
                    MostrarMensajePedido("Error al cancelar: " + ex.Message, true);
                }
            }
            else if (e.CommandName == "VerDetalle")
            {
                try
                {
                    DataTable dt = PedidoBLL.GetDetalleByPedido(idPedido);
                    lblDetallePedidoTitulo.Text     = "Detalle del pedido #" + idPedido;
                    gvDetallePedidoAdmin.DataSource = dt;
                    gvDetallePedidoAdmin.DataBind();
                    pnlDetallePedidoAdmin.Visible   = true;
                    Page.ClientScript.RegisterStartupScript(GetType(), "scrollDetalle",
                        "document.getElementById('" + pnlDetallePedidoAdmin.ClientID + "').scrollIntoView({behavior:'smooth'});", true);
                }
                catch (Exception ex)
                {
                    MostrarMensajePedido("Error al ver detalle: " + ex.Message, true);
                }
            }
        }
        catch (Exception ex)
        {
            MostrarMensaje(ex.GetType().Name + ": " + ex.Message, true);
        }
    }

    protected void btnCerrarDetallePedido_Click(object sender, EventArgs e)
    {
        pnlDetallePedidoAdmin.Visible = false;
    }

    private void MostrarMensajePedido(string texto, bool esError)
    {
        lblMensajePedido.Text     = texto;
        lblMensajePedido.CssClass = esError ? "msg msg-err" : "msg msg-ok";
        lblMensajePedido.Visible  = true;
    }

    // MENSAJES

    private void MostrarMensaje(string texto, bool esError)
    {
        lblMensaje.Text     = texto;
        lblMensaje.CssClass = esError ? "msg msg-err" : "msg msg-ok";
        lblMensaje.Visible  = true;
    }

    protected void gvPedidos_SelectedIndexChanged(object sender, EventArgs e)
    {

    }
}
