<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="MisHorarios.aspx.vb" Inherits="Sistema_Citas_MedicasV2.MisHorarios" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
     <div class="d-flex justify-content-between align-items-center mb-3">
    <h2>Mis Horarios</h2>

<!-- Mensaje de estado -->
<asp:Label ID="lblMensaje" runat="server" CssClass="alert alert-info d-none" 
    Visible="false" style="display: none !important;"></asp:Label>

<!-- Mejorar el input de fecha -->
<div class="input-group" style="max-width:360px;">
    <asp:TextBox ID="txtFecha" runat="server" CssClass="form-control" 
        TextMode="DateTimeLocal" placeholder="Seleccione fecha y hora"></asp:TextBox>
    <asp:Button ID="btnAgregar" runat="server" Text="Agregar" CssClass="btn btn-primary" 
        OnClick="btnAgregar_Click" />
</div>

<!-- Botón para recargar -->
<asp:Button ID="btnRecargar" runat="server" Text="Actualizar" 
    CssClass="btn btn-secondary mb-3" OnClick="btnRecargar_Click" />

<!-- Mejorar el GridView -->
<asp:GridView ID="gvHorarios" runat="server" CssClass="table table-striped table-sm"
    AutoGenerateColumns="False" EmptyDataText="No tienes horarios creados."
    DataKeyNames="Id" OnRowDeleting="gvHorarios_RowDeleting" 
    OnRowDataBound="gvHorarios_RowDataBound">
    <Columns>
        <asp:BoundField DataField="Id" HeaderText="#" />
        <asp:BoundField DataField="Fecha" HeaderText="Fecha" 
            DataFormatString="{0:dd/MM/yyyy HH:mm}" />
        <asp:CheckBoxField DataField="Disponible" HeaderText="Disponible" />
        <asp:CommandField ShowDeleteButton="True" ButtonType="Button" 
            DeleteText="Eliminar" ControlStyle-CssClass="btn btn-danger btn-sm" />
    </Columns>
</asp:GridView>

<!-- Instrucciones -->
<div class="alert alert-info mt-3">
    <strong>💡 Instrucciones:</strong><br />
    - Solo puedes agregar horarios futuros<br />
    - Los horarios se muestran como disponibles para que los pacientes agenden citas<br />
    - No puedes eliminar horarios que ya tienen citas asignadas
</div>
</asp:Content>
