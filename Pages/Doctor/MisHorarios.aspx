<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="MisHorarios.aspx.vb" Inherits="Sistema_Citas_MedicasV2.MisHorarios" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
     <div class="d-flex justify-content-between align-items-center mb-3">
    <h2>Mis Horarios</h2>
    <div class="input-group" style="max-width:360px;">
      <asp:TextBox ID="txtFecha" runat="server" CssClass="form-control" placeholder="dd/MM/yyyy HH:mm"></asp:TextBox>
      <asp:Button ID="btnAgregar" runat="server" Text="Agregar" CssClass="btn btn-primary" />
    </div>
  </div>

  <asp:GridView ID="gvHorarios" runat="server" CssClass="table table-striped table-sm"
      AutoGenerateColumns="False" EmptyDataText="No tienes horarios creados.">
    <Columns>
      <asp:BoundField DataField="Id" HeaderText="#" />
      <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
      <asp:CheckBoxField DataField="Disponible" HeaderText="Disponible" />
    </Columns>
  </asp:GridView>
</asp:Content>
