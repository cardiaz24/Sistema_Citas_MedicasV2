<%@ Page Title="Inicio" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.vb" Inherits="Sistema_Citas_MedicasV2._Default" %>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="pb-5">

        <!-- Hero con imagen de fondo -->
        <section class="text-white text-center p-5 mb-5" style="background-image:url('Images/fondo.jpg'); background-size: cover; background-position: center;">
            <div class="bg-dark bg-opacity-75 p-4 rounded">
                <h1 class="display-5 fw-bold">Bienvenido al Sistema de Citas Médicas</h1>
                <p class="lead mb-4">
                    Administra tus citas, horarios y pacientes desde un solo lugar.
                </p>
                <a href="Login.aspx" class="btn btn-primary btn-lg px-4">Iniciar Sesión</a>
            </div>
        </section>

        <!-- Mini dashboard informativo -->
        <section class="container">
         <div class="row text-center mt-5">
    <!-- Pacientes -->
    <div class="col-md-4">
        <div class="card shadow-sm p-3">
            <img src="/Images/Paciente.jpg" alt="Pacientes" class="img-fluid mb-3" style="height: 300px;" />
            <h5 class="text-primary">Pacientes</h5>
            <p>Registro, gestión y seguimiento de pacientes activos.</p>
            <span class="text-muted small">Acceso restringido</span>
        </div>
    </div>

    <!-- Doctores -->
    <div class="col-md-4">
        <div class="card shadow-sm p-3">
            <img src="/Images/Doctores.jpg" alt="Doctores" class="img-fluid mb-3" style="height: 300px;" />
            <h5 class="text-success">Doctores</h5>
            <p>Gestión de perfiles médicos y horarios de atención.</p>
            <span class="text-muted small">Acceso restringido</span>
        </div>
    </div>

    <!-- Administración -->
    <div class="col-md-4">
        <div class="card shadow-sm p-3">
            <img src="/Images/Administracion.jpg" alt="Administración" class="img-fluid mb-3" style="height: 300px;" />
          
            <h5 class="text-danger">Administración</h5>
            <p>Panel exclusivo para gestionar el sistema completo.</p>
            <span class="text-muted small">Solo administradores</span>
        </div>
    </div>
</div>
        </section>

        <!-- Acerca del sistema -->
        <section class="container mt-5">
            <div class="card shadow border-0">
                <div class="card-body">
                    <h4 class="card-title text-muted text-uppercase mb-3">Sobre el sistema</h4>
                    <p>
                        Este sistema fue diseñado para facilitar la gestión de citas médicas en centros de salud. 
                        Con módulos para pacientes, doctores y administradores, permite un control integral de horarios, usuarios y reportes.
                    </p>
                    <ul class="small text-muted">
                        <li>Diseñado con ASP.NET Web Forms y Bootstrap 5</li>
                        <li>Soporte para múltiples roles de usuario</li>
                        <li>Optimizado para pantallas móviles</li>
                    </ul>
                </div>
                <div class="card-footer text-end small text-muted">
                    Última actualización: <%= DateTime.Now.ToString("dd/MM/yyyy") %>
                </div>
            </div>
        </section>

    </main>
</asp:Content>
