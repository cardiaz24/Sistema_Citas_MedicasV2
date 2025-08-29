Imports System.ComponentModel.DataAnnotations

Public Class Cita
    Public Property Id As Integer

    Private _pacienteId As Integer
    Public Property PacienteId As Integer
        Get
            Return _pacienteId
        End Get
        Set(value As Integer)
            If value <= 0 Then
                Throw New ArgumentException("PacienteId debe ser mayor a 0")
            End If
            _pacienteId = value
        End Set
    End Property

    Private _horarioId As Integer
    Public Property HorarioId As Integer
        Get
            Return _horarioId
        End Get
        Set(value As Integer)
            If value <= 0 Then
                Throw New ArgumentException("HorarioId debe ser mayor a 0")
            End If
            _horarioId = value
        End Set
    End Property

    Private _estado As String = "Pendiente"
    Public Property Estado As String
        Get
            Return _estado
        End Get
        Set(value As String)
            Dim estadosValidos As String() = {"Pendiente", "Confirmada", "Completada", "Cancelada", "NoPresentado"}
            If Array.Exists(estadosValidos, Function(e) e.Equals(value, StringComparison.OrdinalIgnoreCase)) Then
                _estado = value
            Else
                Throw New ArgumentException("Estado de cita no válido")
            End If
        End Set
    End Property

    Public Property FechaCreacion As DateTime = DateTime.Now

    ' Constructores
    Public Sub New()
    End Sub

    Public Sub New(pacienteId As Integer, horarioId As Integer)
        Me.PacienteId = pacienteId
        Me.HorarioId = horarioId
        Me.Estado = "Pendiente"
        Me.FechaCreacion = DateTime.Now
    End Sub

    ' Métodos de utilidad
    Public Function PuedeCancelarse() As Boolean
        Return Estado = "Pendiente" OrElse Estado = "Confirmada"
    End Function

    Public Function EsPasada() As Boolean
        ' Esta lógica debería verificar la fecha del horario asociado
        Return False ' Placeholder
    End Function

    Public Overrides Function ToString() As String
        Return $"Cita [Id: {Id}, PacienteId: {PacienteId}, HorarioId: {HorarioId}, Estado: {Estado}]"
    End Function
End Class