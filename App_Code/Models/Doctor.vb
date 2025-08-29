Imports System.ComponentModel.DataAnnotations

Public Class Doctor
    Inherits Persona

    Private _especialidad As String
    Public Property Especialidad As String
        Get
            Return _especialidad
        End Get
        Set(value As String)
            If String.IsNullOrEmpty(value) Then
                Throw New ArgumentException("La especialidad no puede estar vacía")
            End If
            If value.Length > 100 Then
                Throw New ArgumentException("La especialidad no puede exceder 100 caracteres")
            End If
            _especialidad = value
        End Set
    End Property

    Public Property Activo As Boolean

    ' Constructores
    Public Sub New()
        MyBase.New()
        Activo = True
    End Sub

    Public Sub New(nombre As String, apellidos As String, email As String, password As String,
                   especialidad As String)
        MyBase.New(nombre, apellidos, email, password)
        Me.Especialidad = especialidad
        Me.Activo = True
    End Sub

    ' Métodos específicos de Doctor
    Public Function EstaDisponible() As Boolean
        Return Activo
    End Function

    Public Overrides Function ToString() As String
        Return $"Doctor: {MyBase.ToString()}, Especialidad: {Especialidad}, Activo: {Activo}"
    End Function
End Class