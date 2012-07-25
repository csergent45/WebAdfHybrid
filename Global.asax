<%@ Application Language="VB" %>


<script runat="server">
        
    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the application is started
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the session is started
    End Sub

    Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires at the beginning of each request
    End Sub

    Sub Application_AuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires upon attempting to authenticate the use
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        Response.Write("<b>")
        Response.Write("Oops! Like an error has occured!!</b><br />")
        Response.Write("Duplicate data may still exist in the database or this application may be in test mode.</b><br />")
        Response.Write("Please contact the developer or MIS to be directed to the right person.")
        Server.ClearError()
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
        '// Code that runs when a session ends. 
        '// Note: The Session_End event is raised only when the sessionstate mode
        '// is set to InProc in the Web.config file. If session mode is set to StateServer 
        '// or SQLServer, the event is not raised.
        '// Close out session and quit application
        
        Dim contexts As New System.Collections.Generic.List(Of ESRI.ArcGIS.Server.IServerContext)
        Dim i As Integer
        For i = 0 To Session.Count - 1
            If TypeOf Session(i) Is ESRI.ArcGIS.Server.IServerContext Then
                contexts.Add(CType(Session(i), ESRI.ArcGIS.Server.IServerContext))
            Else
                If TypeOf Session(i) Is IDisposable Then
                    CType(Session(i), IDisposable).Dispose()
                End If
            End If
        Next i
        Dim context As ESRI.ArcGIS.Server.IServerContext
        For Each context In contexts
            context.RemoveAll()
            context.ReleaseContext()
        Next context
    End Sub

    Sub Session_Abandon(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs when a session is abandoned. 
        Dim contexts As New System.Collections.Generic.List(Of ESRI.ArcGIS.Server.IServerContext)
        Dim i As Integer
        For i = 0 To Session.Count - 1
            If TypeOf Session(i) Is ESRI.ArcGIS.Server.IServerContext Then
                contexts.Add(CType(Session(i), ESRI.ArcGIS.Server.IServerContext))
            Else
                If TypeOf Session(i) Is IDisposable Then
                    CType(Session(i), IDisposable).Dispose()
                End If
            End If
        Next i
        Dim context As ESRI.ArcGIS.Server.IServerContext
        For Each context In contexts
            context.RemoveAll()
            context.ReleaseContext()
        Next context
    End Sub

    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the application ends
    End Sub
       
</script>
