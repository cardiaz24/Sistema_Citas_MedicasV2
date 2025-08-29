Imports System.ComponentModel.DataAnnotations

Public Class Horario
    Public Property Id As Integer

    Private _doctorId As Integer
    Public Property DoctorId As Integer
        Get
            Return _doctorId
        End Get
        Set(value As Integer)
            If value <= 0 Then
                Throw New ArgumentException("DoctorId debe ser mayor a 0")
            End If
            _doctorId = value
        End Set
    End Property

    Private _fecha As DateTime
    Public Property Fecha As DateTime
        Get
            Return _fecha
        End Get
        Set(value As DateTime)
            If value <= DateTime.Now Then
                Throw New ArgumentException("La fecha debe ser futura")
            End If
            _fecha = value
        End Set
    End Property

    Public Property Disponible As Boolean

    ' Constructores
    Public Sub New()
        Disponible = True
    End Sub

    Public Sub New(doctorId As Integer, fecha As DateTime)
        Me.DoctorId = doctorId
        Me.Fecha = fecha
        Me.Disponible = True
    End Sub

    ' Métodos de utilidad
    Public Function EstaDisponible() As Boolean
        Return Disponible AndAlso Fecha > DateTime.Now
    End Function

    Public Function EsMismoDia(otherDate As DateTime) As Boolean
        Return Fecha.Date = otherDate.Date
    End Function

    Public Overrides Function ToString() As String
        Return $"Horario [Id: {Id}, DoctorId: {DoctorId}, Fecha: {Fecha:g}, Disponible: {Disponible}]"
    End Function
End Class