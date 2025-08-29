Imports System.ComponentModel.DataAnnotations

Public Class Paciente
    Inherits Persona

    Private _direccion As String
    Public Property Direccion As String
        Get
            Return _direccion
        End Get
        Set(value As String)
            If value IsNot Nothing AndAlso value.Length > 200 Then
                Throw New ArgumentException("La dirección no puede exceder 200 caracteres")
            End If
            _direccion = value
        End Set
    End Property

    Private _telefono As String
    Public Property Telefono As String
        Get
            Return _telefono
        End Get
        Set(value As String)
            If String.IsNullOrEmpty(value) Then
                Throw New ArgumentException("El teléfono no puede estar vacío")
            End If
            If value.Length > 20 Then
                Throw New ArgumentException("El teléfono no puede exceder 20 caracteres")
            End If
            ' Validar formato de teléfono
            If Not Regex.IsMatch(value, "^[\d\s\-\+\(\)]+$") Then
                Throw New ArgumentException("El formato del teléfono no es válido")
            End If
            _telefono = value
        End Set
    End Property

    Public Property FechaRegistro As DateTime

    ' Constructores
    Public Sub New()
        MyBase.New()
        FechaRegistro = DateTime.Now
    End Sub

    Public Sub New(nombre As String, apellidos As String, email As String, password As String,
                   direccion As String, telefono As String)
        MyBase.New(nombre, apellidos, email, password)
        Me.Direccion = direccion
        Me.Telefono = telefono
        Me.FechaRegistro = DateTime.Now
    End Sub


    Public Function TieneCitasPendientes() As Boolean

        Return False
    End Function

    Public Overrides Function ToString() As String
        Return $"Paciente: {MyBase.ToString()}, Tel: {Telefono}"
    End Function

End Class