<%@ Page Title="About" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.vb" Inherits="Sistema_Citas_MedicasV2.About" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-5">
        <h2 class="mb-4">Acerca del Sistema de Citas Médicas</h2>

        <div class="row">
            <div class="col-md-6">
                <p class="lead">
                    Este sistema fue desarrollado como parte de un proyecto académico con el fin de ofrecer una solución práctica, moderna y funcional para la gestión de citas médicas.
                </p>
                <p>
                    Su diseño modular permite a <strong>pacientes</strong>, <strong>doctores</strong> y <strong>administradores</strong> interactuar con el sistema según sus permisos:
                </p>
                <ul>
                    <li><strong>Pacientes:</strong> registran y gestionan sus citas.</li>
                    <li><strong>Doctores:</strong> administran horarios disponibles.</li>
                    <li><strong>Administradores:</strong> supervisan la operación completa.</li>
                </ul>
                <p>
                    El sistema fue desarrollado con tecnologías como ASP.NET Web Forms, VB.NET, SQL Server y Bootstrap.
                </p>
            </div>

            <div class="col-md-6">
                <img src="Images/Consultorio.jpg" alt="Contáctenos" class="img-fluid rounded shadow-sm" style="max-width: 50%; height: auto;" />

            </div>
        </div>

        <hr class="my-5" />

        <div class="text-center text-muted small">
            Proyecto académico · Última actualización: <%= DateTime.Now.ToString("dd/MM/yyyy") %>
        </div>
    </div>
</asp:Content>
