<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="Pacientes.aspx.vb" Inherits="Sistema_Citas_MedicasV2.Pacientes" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
      <h2 class="mb-3">Mis Citas</h2>


<!-- Mensaje de estado -->
<asp:Label ID="lblMensaje" runat="server" CssClass="alert d-none" 
    Visible="false" style="display: none !important;"></asp:Label>

<!-- Sección de reserva de citas -->
<div class="card mb-4">
    <div class="card-header bg-primary text-white">
        <h5 class="mb-0">Reservar Nueva Cita</h5>
    </div>
    <div class="card-body">
        <div class="row g-3">
            <div class="col-md-4">
                <label class="form-label">Doctor</label>
                <asp:DropDownList ID="ddlDoctores" runat="server" CssClass="form-select"
                    AutoPostBack="true" OnSelectedIndexChanged="ddlDoctores_SelectedIndexChanged">
                </asp:DropDownList>
            </div>
            <div class="col-md-4">
                <label class="form-label">Horario Disponible</label>
                <asp:DropDownList ID="ddlHorarios" runat="server" CssClass="form-select">
                </asp:DropDownList>
            </div>
            <div class="col-md-4">
                <label class="form-label">&nbsp;</label>
                <asp:Button ID="btnReservar" runat="server" Text="Reservar Cita" 
                    CssClass="btn btn-success w-100" OnClick="btnReservar_Click" />
            </div>
        </div>
    </div>
</div>

<!-- Mejorar el GridView de citas -->
<asp:GridView ID="gvCitas" runat="server" CssClass="table table-striped table-sm"
    AutoGenerateColumns="False" EmptyDataText="No tienes citas registradas."
    DataKeyNames="Id" OnRowDeleting="gvCitas_RowDeleting" 
    OnRowDataBound="gvCitas_RowDataBound">
    <Columns>
        <asp:BoundField DataField="Id" HeaderText="#" />
        <asp:BoundField DataField="Fecha" HeaderText="Fecha" 
            DataFormatString="{0:dd/MM/yyyy HH:mm}" />
        <asp:BoundField DataField="DoctorNombre" HeaderText="Doctor" />
        <asp:BoundField DataField="Estado" HeaderText="Estado" />
        <asp:CommandField ShowDeleteButton="True" ButtonType="Button" 
            DeleteText="Cancelar" ControlStyle-CssClass="btn btn-danger btn-sm" />
    </Columns>
</asp:GridView>

<!-- Información importante -->
<div class="alert alert-info mt-3">
    <strong>💡 Información importante:</strong><br />
    - Puedes cancelar citas hasta 24 horas antes<br />
    - Las citas en estado "Completada" o "No Presentado" no se pueden cancelar<br />
    - Recuerda llegar 10 minutos antes de tu cita
</div>


</asp:Content>
