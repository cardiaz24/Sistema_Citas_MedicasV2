<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="Dashboard.aspx.vb" Inherits="Sistema_Citas_MedicasV2.Dashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
      <h2 class="mb-4">Administración</h2>

  <div class="card mb-4">
    <div class="card-header">Asignar rol de Doctor</div>
    <div class="card-body">
      <div class="row g-2 align-items-end">
        <div class="col-md-3">
          <label class="form-label">PersonaId</label>
          <asp:TextBox ID="txtPersonaId" runat="server" CssClass="form-control" />
        </div>
        <div class="col-md-4">
          <label class="form-label">Especialidad</label>
          <asp:TextBox ID="txtEspecialidad" runat="server" CssClass="form-control" />
        </div>
        <div class="col-md-3">
          <asp:Button ID="btnAsignarDoctor" runat="server" Text="Asignar Doctor" CssClass="btn btn-primary" />
        </div>
        <div class="col-md-12 mt-2">
          <asp:Label ID="lblMsg" runat="server" CssClass="text-success"></asp:Label>
        </div>
      </div>
    </div>
  </div>

    <div class="card-header">Buscar Persona para asignar rol</div>
    <div class="card-body">
        <div class="row g-2 align-items-end">
            <div class="col-md-4">
                <label class="form-label">Email de la persona</label>
                <asp:TextBox ID="txtEmailBusqueda" runat="server" CssClass="form-control" 
                    placeholder="email@ejemplo.com" />
            </div>
            <div class="col-md-2">
                <asp:Button ID="btnBuscarPersona" runat="server" Text="Buscar" 
                    CssClass="btn btn-outline-secondary" OnClick="btnBuscarPersona_Click" />
            </div>
        </div>
        
        <!-- Resultado de búsqueda -->
        <asp:Panel ID="pnlResultadoBusqueda" runat="server" Visible="false" CssClass="mt-3">
            <div class="alert alert-info">
                <strong>Persona encontrada:</strong>
                <asp:Label ID="lblPersonaInfo" runat="server"></asp:Label>
                <asp:HiddenField ID="hdnPersonaId" runat="server" />
            </div>
        </asp:Panel>
    </div>

    <!-- Botón para recargar -->
<asp:Button ID="btnRecargar" runat="server" Text="Actualizar Lista" 
    CssClass="btn btn-secondary mb-3" OnClick="btnRecargar_Click" />

<!-- Mejorar el GridView -->
<asp:GridView ID="gvDoctores" runat="server" CssClass="table table-striped table-sm"
    AutoGenerateColumns="False" OnRowDataBound="gvDoctores_RowDataBound">
    <Columns>
        <asp:BoundField DataField="Id" HeaderText="ID" />
        <asp:BoundField DataField="Nombre" HeaderText="Nombre" />
        <asp:BoundField DataField="Apellidos" HeaderText="Apellidos" />
        <asp:BoundField DataField="Email" HeaderText="Email" />
        <asp:BoundField DataField="Especialidad" HeaderText="Especialidad" />
        <asp:CheckBoxField DataField="Activo" HeaderText="Activo" />
        <asp:BoundField DataField="FechaCreacion" HeaderText="Registro" 
            DataFormatString="{0:dd/MM/yyyy}" />
    </Columns>
</asp:GridView>


</asp:Content>
