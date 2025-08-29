Imports System
Imports System.ComponentModel.DataAnnotations

Public Class Administrador
    Inherits Persona

    Public Property FechaRegistro As DateTime

    Private _nivelAcceso As String = "General"
    Public Property NivelAcceso As String
        Get
            Return _nivelAcceso
        End Get
        Set(value As String)
            If String.IsNullOrEmpty(value) Then
                _nivelAcceso = "General"
            Else
                Dim nivelesValidos As String() = {"General", "Avanzado", "Super"}
                If Array.Exists(nivelesValidos, Function(n) n.Equals(value, StringComparison.OrdinalIgnoreCase)) Then
                    _nivelAcceso = value
                Else
                    Throw New ArgumentException("Nivel de acceso no válido")
                End If
            End If
        End Set
    End Property

    Public Property Activo As Boolean

    ' Constructores
    Public Sub New()
        MyBase.New()
        FechaRegistro = DateTime.Now
        Activo = True
    End Sub

    Public Sub New(nombre As String, apellidos As String, email As String, password As String, nivelAcceso As String)
        MyBase.New(nombre, apellidos, email, password)
        Me.NivelAcceso = nivelAcceso
        FechaRegistro = DateTime.Now
        Activo = True
    End Sub

    ' Métodos de utilidad
    Public Function PuedeGestionarUsuarios() As Boolean
        Return Me.NivelAcceso.Equals("Avanzado", StringComparison.OrdinalIgnoreCase) OrElse
               Me.NivelAcceso.Equals("Super", StringComparison.OrdinalIgnoreCase)
    End Function

    Public Function PuedeGestionarSistemaCompleto() As Boolean
        Return Me.NivelAcceso.Equals("Super", StringComparison.OrdinalIgnoreCase)
    End Function

    ' Override de ToString para debugging
    Public Overrides Function ToString() As String
        Return $"Admin: {MyBase.ToString()}, Nivel: {NivelAcceso}, Activo: {Activo}"
    End Function
End Class