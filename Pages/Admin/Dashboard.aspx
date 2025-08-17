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

  <asp:GridView ID="gvDoctores" runat="server" CssClass="table table-striped table-sm"
      AutoGenerateColumns="True" />
</asp:Content>
