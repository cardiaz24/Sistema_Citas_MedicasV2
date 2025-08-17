Imports System
Imports System.Collections.Generic
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient


Public Class DatabaseHelper
    Private ReadOnly connectionString As String = ConfigurationManager.ConnectionStrings("ConexionSt").ConnectionString

    Public Sub New()
        EnsureErrorLogTableExists() ' Asegúrate de que la tabla exista al crear una instancia.
    End Sub

    ' Método para obtener la conexión
    Public Function GetConnection() As SqlConnection
        Dim conn As New SqlConnection(connectionString)
        Try
            conn.Open()
        Catch ex As SqlException
            Throw New Exception("Error al abrir la conexión a la base de datos: " & ex.Message)
        End Try
        Return conn
    End Function

    ' Método para ejecutar un comando SQL (INSERT, UPDATE, DELETE)
    Public Sub ExecuteNonQuery(query As String, 
                               Optional parameters As List(Of SqlParameter) = Nothing, 
                               Optional isStoredProcedure As Boolean = False)
        If String.IsNullOrWhiteSpace(query) Then
            Throw New ArgumentException("La consulta no puede estar vacía.")
        End If

        Using conn As SqlConnection = GetConnection()
            Using cmd As New SqlCommand(query, conn)
                If parameters IsNot Nothing Then
                    cmd.Parameters.AddRange(parameters.ToArray())
                End If
                If isStoredProcedure Then
                    cmd.CommandType = CommandType.StoredProcedure
                End If
                Try
                    cmd.ExecuteNonQuery()
                Catch ex As Exception
                    LogError(ex, query) ' Registrar el error en caso de excepción
                    Throw New Exception("Error al ejecutar el comando: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub

    ' Método para ejecutar una consulta SQL y devolver un DataTable
    Public Function ExecuteQuery(query As String, Optional parameters As List(Of SqlParameter) = Nothing, Optional isStoredProcedure As Boolean = False) As DataTable
        If String.IsNullOrWhiteSpace(query) Then
            Throw New ArgumentException("La consulta no puede estar vacía.")
        End If

        Dim dt As New DataTable()
        Using conn As SqlConnection = GetConnection()
            Using cmd As New SqlCommand(query, conn)
                If parameters IsNot Nothing Then
                    cmd.Parameters.AddRange(parameters.ToArray())
                End If
                If isStoredProcedure Then
                    cmd.CommandType = CommandType.StoredProcedure
                End If
                Try
                    Using adapter As New SqlDataAdapter(cmd)
                        adapter.Fill(dt)
                    End Using
                Catch ex As Exception
                    LogError(ex, query) ' Registrar el error en caso de excepción
                    Throw New Exception("Error al ejecutar la consulta: " & ex.Message)
                End Try
            End Using
        End Using
        Return dt
    End Function

    ' Método para asegurar que la tabla ErrorLog existe
    Private Sub EnsureErrorLogTableExists()
        Dim query As String = "
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ErrorLog')
            BEGIN
                CREATE TABLE ErrorLog (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    ErrorMessage NVARCHAR(4000),
                    ErrorSeverity INT,
                    ErrorState INT,
                    ErrorProcedure NVARCHAR(200),
                    ErrorLine INT,
                    ErrorDateTime DATETIME DEFAULT GETDATE()
                )
            END"

        ExecuteNonQuery(query)
    End Sub

    Private Sub LogErrorToFile(message As String)
        Dim path As String = "C:\Logs\error_log.txt"
        IO.File.AppendAllText(path, $"{DateTime.Now}: {message}{Environment.NewLine}")
    End Sub


    ' Método para registrar errores en una tabla de auditoría.
    Private Sub LogError(ex As Exception, Optional query As String = "")
        Dim fullMessage As String = $"Message: {ex.Message}" & Environment.NewLine &
                                    $"StackTrace: {ex.StackTrace}" & Environment.NewLine &
                                    $"InnerException: {If(ex.InnerException IsNot Nothing, ex.InnerException.Message, "N/A")}" & Environment.NewLine &
                                    $"Query: {query}"

        ' Escapar comillas simples para evitar errores de SQL
        Dim errorMessage As String = fullMessage.Replace("'", "''")
        Dim severity As Integer = 16
        Dim state As Integer = 1
        Dim procedureName As String = If(ex.TargetSite IsNot Nothing, ex.TargetSite.Name, DBNull.Value.ToString())
        Dim lineNumber As Integer = 0

        ' Intentar extraer el número de línea si está disponible en el stack trace
        If ex.StackTrace IsNot Nothing AndAlso ex.StackTrace.Contains(":line ") Then
            Dim partes = ex.StackTrace.Split(":line ")
            Dim ultimaParte = partes(partes.Length - 1) ' toma la última ocurrencia
            Integer.TryParse(ultimaParte.Split(" "c)(0), lineNumber)
        End If
        Dim logQuery As String = "
        INSERT INTO ErrorLog (ErrorMessage, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorDateTime) 
        VALUES (@ErrorMessage, @ErrorSeverity, @ErrorState, @ErrorProcedure, @ErrorLine, GETDATE())"

        Dim parameters As New List(Of SqlParameter) From {
            New SqlParameter("@ErrorMessage", errorMessage),
            New SqlParameter("@ErrorSeverity", severity),
            New SqlParameter("@ErrorState", state),
            New SqlParameter("@ErrorProcedure", procedureName),
            New SqlParameter("@ErrorLine", lineNumber)
        }

        Try
            ExecuteNonQuery(logQuery, parameters)
        Catch logEx As Exception
            ' Fallback: guardar en archivo si falla el log a la base de datos
            LogErrorToFile(fullMessage)
        End Try

    End Sub

    Public Function CreateParameter(name As String, value As Object) As SqlParameter
        Return New SqlParameter(name, If(value IsNot Nothing, value, DBNull.Value))
    End Function

End Class
