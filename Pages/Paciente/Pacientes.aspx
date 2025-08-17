<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="Pacientes.aspx.vb" Inherits="Sistema_Citas_MedicasV2.Pacientes" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
      <h2 class="mb-3">Mis Citas</h2>
  <asp:GridView ID="gvCitas" runat="server" CssClass="table table-striped table-sm"
      AutoGenerateColumns="False" EmptyDataText="No tienes citas registradas.">
    <Columns>
      <asp:BoundField DataField="Id" HeaderText="#" />
      <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
      <asp:BoundField DataField="DoctorNombre" HeaderText="Doctor" />
      <asp:BoundField DataField="Estado" HeaderText="Estado" />
    </Columns>
  </asp:GridView>
</asp:Content>
