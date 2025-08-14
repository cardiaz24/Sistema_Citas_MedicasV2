<%@ Page Title="Contacto" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.vb" Inherits="Sistema_Citas_MedicasV2.Contact" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-5">
        <h2 class="mb-4">Contáctanos</h2>

        <div class="row">
            <div class="col-md-6">
                <p class="lead">¿Tienes dudas o sugerencias sobre el sistema? ¡Estamos para ayudarte!</p>

                <address>
                    <strong>Universidad Central</strong><br />
                    Facultad de Ingeniería<br />
                    Escuela de Ingeniería Informática<br />
                    San José, Costa Rica<br />
                </address>

                <p>
                    <strong>Teléfono:</strong> +506 22222222<br />
                    <strong>Email soporte:</strong> <a href="mailto:cardiaz24@hotmail.com">cardiaz24@hotmail.com</a>
                </p>

                <p>
                    <strong>Desarrollado por:</strong><br />
                    Estudiantes de Ingeniería Informática<br />
                    Proyecto Final – Año 2025
                </p>
            </div>

            <div class="col-md-6">
                <img src="Images/Contacto.jpg" alt="Contáctenos" class="img-fluid rounded shadow-sm" style="max-width: 100%; height: auto;" />
            </div>
        </div>
    </div>
</asp:Content>
