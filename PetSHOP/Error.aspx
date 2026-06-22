<%@ Page Language="C#" AutoEventWireup="true" %>
<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        string motivo = Request.QueryString["motivo"];
        if (motivo == "integridad")
        {
            lblTitulo.Text  = "Sistema bloqueado";
            lblMensaje.Text = "Se produjo un error inesperado. Por favor intente mas tarde.";
        }
        else
        {
            lblTitulo.Text  = "No esta disponible el sistema";
            lblMensaje.Text = "Se produjo un error inesperado. Por favor intente mas tarde.";

            if (Session["_DebugError"] != null)
            {
                lblMensaje.Text += "<br/><br/><strong style='color:#cc0000'>Detalle del error:</strong><br/>"
                                 + Server.HtmlEncode(Session["_DebugError"].ToString());
                Session.Remove("_DebugError");
            }
        }
    }
</script>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Error - PetShop</title>
    <style>
        body { font-family: Arial, sans-serif; background-color: #f0f0f0; }
        .caja { width: 520px; margin: 100px auto; background: white; border: 1px solid #ccc; padding: 20px; }
        h2 { color: #cc0000; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="caja">
            <h2><asp:Label ID="lblTitulo" runat="server" /></h2>
            <hr />
            <p><asp:Label ID="lblMensaje" runat="server" /></p>
            <p><a href="Default.aspx">Volver al inicio</a></p>
        </div>
    </form>
</body>
</html>
