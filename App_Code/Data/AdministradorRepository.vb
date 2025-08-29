Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports System.Security.Cryptography
Imports System.Text

Public Class AdministradorRepository
    Private ReadOnly db As New DatabaseHelper()

    Public Function GetAll() As DataTable
        Try
            Dim sql As String =
                "SELECT p.Id, p.Nombre, p.Apellidos, p.Email, a.FechaRegistro, a.NivelAcceso, a.Activo
                 FROM Personas p
                 INNER JOIN Administradores a ON p.Id = a.PersonaId"

            Return db.ExecuteQuery(sql)
        Catch ex As Exception
            ' Log the error (deberías implementar un sistema de logging)
            Throw New Exception("Error al obtener administradores: " & ex.Message)
        End Try
    End Function

    Public Function Insert(admin As Administrador) As Boolean
        Using conn As SqlConnection = db.GetConnection()
            conn.Open()
            Using tx As SqlTransaction = conn.BeginTransaction()
                Try
                    Dim hashedPassword = HashPassword(admin.Password)

                    ' Personas
                    Dim personaSql As String =
                    "INSERT INTO Personas (Nombre, Apellidos, Email, Password, FechaCreacion)
                     OUTPUT INSERTED.Id
                     VALUES (@Nombre, @Apellidos, @Email, @Password, GETDATE())"

                    Dim personaParams = New List(Of SqlParameter) From {
                    db.CreateParameter("@Nombre", admin.Nombre),
                    db.CreateParameter("@Apellidos", admin.Apellidos),
                    db.CreateParameter("@Email", admin.Email),
                    db.CreateParameter("@Password", hashedPassword)
                }

                    Dim personaId As Integer = Convert.ToInt32(db.ExecuteScalar(tx, personaSql, personaParams))

                    ' Administradores
                    Dim adminSql As String =
                    "INSERT INTO Administradores (PersonaId, FechaRegistro, NivelAcceso, Activo)
                     VALUES (@PersonaId, GETDATE(), @NivelAcceso, @Activo)"

                    Dim adminParams = New List(Of SqlParameter) From {
                    db.CreateParameter("@PersonaId", personaId),
                    db.CreateParameter("@NivelAcceso", admin.NivelAcceso),
                    db.CreateParameter("@Activo", admin.Activo)
                }
                    db.ExecuteNonQuery(tx, adminSql, adminParams)

                    ' Rol
                    Dim rolSql As String =
                    "INSERT INTO PersonaRoles (PersonaId, RolId, Activo)
                     VALUES (@PersonaId, (SELECT Id FROM Roles WHERE Nombre='Administrador'), 1)"
                    Dim rolParams = New List(Of SqlParameter) From {
                    db.CreateParameter("@PersonaId", personaId)
                }
                    db.ExecuteNonQuery(tx, rolSql, rolParams)

                    tx.Commit()
                    Return True
                Catch
                    tx.Rollback()
                    Throw
                End Try
            End Using
        End Using
    End Function

    'Función para hashear contraseñas
    Private Function HashPassword(password As String) As String
        Using sha256 As SHA256 = SHA256.Create()
            Dim bytes = Encoding.UTF8.GetBytes(password)
            Dim hash = sha256.ComputeHash(bytes)
            Return Convert.ToBase64String(hash)
        End Using
    End Function
End Class