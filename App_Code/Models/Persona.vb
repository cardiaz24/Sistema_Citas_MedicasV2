Imports System.ComponentModel.DataAnnotations
Imports System.Text.RegularExpressions

Public Class Persona
    Public Property Id As Integer

    Private _nombre As String
    Public Property Nombre As String
        Get
            Return _nombre
        End Get
        Set(value As String)
            If String.IsNullOrWhiteSpace(value) Then
                Throw New ArgumentException("El nombre no puede estar vacío")
            End If
            If value.Length > 100 Then
                Throw New ArgumentException("El nombre no puede exceder 100 caracteres")
            End If
            _nombre = value.Trim()
        End Set
    End Property

    Private _apellidos As String
    Public Property Apellidos As String
        Get
            Return _apellidos
        End Get
        Set(value As String)
            If String.IsNullOrWhiteSpace(value) Then
                Throw New ArgumentException("Los apellidos no pueden estar vacíos")
            End If
            If value.Length > 100 Then
                Throw New ArgumentException("Los apellidos no pueden exceder 100 caracteres")
            End If
            _apellidos = value.Trim()
        End Set
    End Property

    Private _email As String
    Public Property Email As String
        Get
            Return _email
        End Get
        Set(value As String)
            If String.IsNullOrWhiteSpace(value) Then
                Throw New ArgumentException("El email no puede estar vacío")
            End If

            Dim emailAttribute As New EmailAddressAttribute()
            If Not emailAttribute.IsValid(value) Then
                Throw New ArgumentException("El formato del email no es válido")
            End If

            If value.Length > 100 Then
                Throw New ArgumentException("El email no puede exceder 100 caracteres")
            End If

            _email = value.Trim().ToLower()
        End Set
    End Property

    Private _password As String
    Public Property Password As String
        Get
            Return _password
        End Get
        Set(value As String)
            If String.IsNullOrWhiteSpace(value) Then
                Throw New ArgumentException("La contraseña no puede estar vacía")
            End If
            If value.Length < 6 Then
                Throw New ArgumentException("La contraseña debe tener al menos 6 caracteres")
            End If
            If value.Length > 100 Then
                Throw New ArgumentException("La contraseña no puede exceder 100 caracteres")
            End If
            _password = value
        End Set
    End Property

    Public Property FechaCreacion As DateTime = DateTime.Now

    ' Constructores
    Public Sub New()
    End Sub

    Public Sub New(nombre As String, apellidos As String, email As String, password As String)
        Me.Nombre = nombre
        Me.Apellidos = apellidos
        Me.Email = email
        Me.Password = password
        Me.FechaCreacion = DateTime.Now
    End Sub

    ' Método de validación
    Public Overridable Function Validar() As Boolean
        Try
            ' Las validaciones se ejecutan al asignar los valores
            ' Solo verificamos que las propiedades requeridas tengan valor
            Return Not String.IsNullOrEmpty(Nombre) AndAlso
                   Not String.IsNullOrEmpty(Apellidos) AndAlso
                   Not String.IsNullOrEmpty(Email) AndAlso
                   Not String.IsNullOrEmpty(Password)
        Catch
            Return False
        End Try
    End Function

    ' Método para hashear la contraseña 
    Public Sub HashPassword()
        If Not String.IsNullOrEmpty(_password) Then
            Using sha256 As Security.Cryptography.SHA256 = Security.Cryptography.SHA256.Create()
                Dim bytes As Byte() = System.Text.Encoding.UTF8.GetBytes(_password)
                Dim hash As Byte() = sha256.ComputeHash(bytes)
                _password = Convert.ToBase64String(hash)
            End Using
        End If
    End Sub

    ' Método para verificar contraseña 
    Public Function VerifyPassword(inputPassword As String) As Boolean
        Using sha256 As Security.Cryptography.SHA256 = Security.Cryptography.SHA256.Create()
            Dim bytes As Byte() = System.Text.Encoding.UTF8.GetBytes(inputPassword)
            Dim hash As Byte() = sha256.ComputeHash(bytes)
            Dim inputHash As String = Convert.ToBase64String(hash)
            Return _password = inputHash
        End Using
    End Function

    Public Overrides Function Equals(obj As Object) As Boolean
        Dim persona = TryCast(obj, Persona)
        If persona Is Nothing Then Return False

        Return Id = persona.Id AndAlso
               Nombre = persona.Nombre AndAlso
               Apellidos = persona.Apellidos AndAlso
               Email = persona.Email AndAlso
               Password = persona.Password AndAlso
               FechaCreacion = persona.FechaCreacion
    End Function

    Public Overrides Function GetHashCode() As Integer
        Dim hash As New HashCode()
        hash.Add(Id)
        hash.Add(Nombre)
        hash.Add(Apellidos)
        hash.Add(Email)
        hash.Add(Password)
        hash.Add(FechaCreacion)
        Return hash.ToHashCode()
    End Function

    ' Método ToString para debugging
    Public Overrides Function ToString() As String
        Return $"Persona [Id: {Id}, Nombre: {Nombre}, Apellidos: {Apellidos}, Email: {Email}]"
    End Function

    ' Método de clonación
    Public Function Clone() As Persona
        Return New Persona() With {
            .Id = Me.Id,
            .Nombre = Me.Nombre,
            .Apellidos = Me.Apellidos,
            .Email = Me.Email,
            .Password = Me.Password,
            .FechaCreacion = Me.FechaCreacion
        }
    End Function
End Class