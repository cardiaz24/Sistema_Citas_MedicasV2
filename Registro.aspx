<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="Registro.aspx.vb" Inherits="Sistema_Citas_MedicasV2.Registro" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card shadow-lg p-4" style="max-width: 400px; width: 100%;">
        <div class="card-body">
            <h2 class="h4 mb-3 text-center">Create an Account</h2>

            <div class="form-floating">
                <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" TextMode="SingleLine" placeholder="Name"></asp:TextBox>
                <label for="MainContent_txtNombre">Nombre</label>
            </div>
            <div class="form-floating">
                <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control" TextMode="SingleLine" placeholder="Name"></asp:TextBox>
                <label for="MainContent_txtNombre">Apellido</label>
            </div>
            <div class="form-floating">
                <asp:TextBox ID="txtDireccion" runat="server" CssClass="form-control"
                    TextMode="MultiLine" Rows="2" placeholder="Dirección"></asp:TextBox>
                <label for="MainContent_txtDireccion">Dirección</label>
            </div>

            <div class="form-floating">
                <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control"
                    TextMode="Phone" placeholder="Teléfono"></asp:TextBox>
                <label for="MainContent_txtTelefono">Teléfono</label>
                <asp:RegularExpressionValidator ID="regexTelefono" runat="server"
                    ControlToValidate="txtTelefono"
                    ValidationExpression="^[\d\s\-\+\(\)]{10,20}$"
                    ErrorMessage="Formato de teléfono inválido"
                    Display="Dynamic" CssClass="text-danger" />
            </div>
            <div class="form-floating">
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="Email"></asp:TextBox>
                <label for="MainContent_txtEmail">Email</label>
            </div>

            <div class="form-floating">
                <asp:TextBox ID="txtPass" runat="server" CssClass="form-control" TextMode="Password" placeholder="Password"></asp:TextBox>
                <label for="MainContent_txtPass">Contraseña</label>
                <asp:RequiredFieldValidator ID="RequiredFieldValidatorPass"
                    ControlToValidate="txtPass"
                    Display="Dynamic"
                    ErrorMessage="La contraseña es requerida"
                    runat="server" />
            </div>

            <asp:Button CssClass="btn btn-primary w-100 py-2" ID="btnRegistrar" runat="server" Text="Registrarse" />
        </div>

        <a href="Login.aspx">¿Ya estas registrado?</a>
    </div>
    <asp:Label ID="lblError" runat="server" Text="" CssClass="error"></asp:Label>
</asp:Content>

