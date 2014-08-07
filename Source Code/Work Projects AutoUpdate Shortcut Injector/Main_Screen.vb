Imports System.IO
Imports System.Threading
Imports System.ComponentModel
Imports System.Text
Imports System.Security.Cryptography



Public Class Main_Screen

    Private busyworking As Boolean = False

    Private lastinputline As String = ""
    Private inputlines As Long = 0
    Private highestPercentageReached As Integer = 0
    Private inputlinesprecount As Long = 0
    Private pretestdone As Boolean = False
    Private primary_PercentComplete As Integer = 0
    Private percentComplete As Integer

    Private SelectedIndex As Integer = 0

    Private backupdirectory As String = ""
    Private savedirectory As String = ""

    Private AlertMessage As String = ""




    Private Sub Error_Handler(ByVal ex As Exception, Optional ByVal identifier_msg As String = "")
        Try
            If ex.Message.IndexOf("Thread was being aborted") < 0 Then
                Dim Display_Message1 As New Display_Message()
                Display_Message1.Message_Textbox.Text = "The Application encountered the following problem: " & vbCrLf & identifier_msg & ": " & ex.Message.ToString

                Display_Message1.Timer1.Interval = 1000
                Display_Message1.ShowDialog()
                Dim dir As System.IO.DirectoryInfo = New System.IO.DirectoryInfo((Application.StartupPath & "\").Replace("\\", "\") & "Error Logs")
                If dir.Exists = False Then
                    dir.Create()
                End If
                dir = Nothing
                Dim filewriter As System.IO.StreamWriter = New System.IO.StreamWriter((Application.StartupPath & "\").Replace("\\", "\") & "Error Logs\" & Format(Now(), "yyyyMMdd") & "_Error_Log.txt", True)
                filewriter.WriteLine("#" & Format(Now(), "dd/MM/yyyy hh:mm:ss tt") & " - " & identifier_msg & ": " & ex.ToString)
                filewriter.WriteLine("")
                filewriter.Flush()
                filewriter.Close()
                filewriter = Nothing
            End If
            ex = Nothing
            identifier_msg = Nothing
        Catch exc As Exception
            MsgBox("An error occurred in the application's error handling routine. The application will try to recover from this serious error.", MsgBoxStyle.Critical, "Critical Error Encountered")
        End Try
    End Sub




   



    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim result As DialogResult
        result = FolderBrowserDialog1.ShowDialog
        If result = Windows.Forms.DialogResult.OK Then
            TextBox1.Text = FolderBrowserDialog1.SelectedPath
        End If
    End Sub


    


    Private Sub cancelAsyncButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cancelAsyncButton.Click

        ' Cancel the asynchronous operation.
        Me.BackgroundWorker1.CancelAsync()

        ' Disable the Cancel button.
        cancelAsyncButton.Enabled = False
        sender = Nothing
        e = Nothing
    End Sub 'cancelAsyncButton_Click

    Private Sub PreCount_Function(ByVal worker As BackgroundWorker)
        Try
            inputlinesprecount = 0
            inputlines = 0
            Dim dinfo As DirectoryInfo
            dinfo = New DirectoryInfo(TextBox1.Text)
            'Dim backupfolder As String = (Application.StartupPath & "\").Replace("\\", "\") & "WP7D Backup " & Format(Now, "yyyyMMddHHmmss")
            'backupdirectory = backupfolder
            'If My.Computer.FileSystem.DirectoryExists(backupfolder) = False Then
            '    My.Computer.FileSystem.CreateDirectory(backupfolder)
            'End If

            For Each finfo As DirectoryInfo In dinfo.GetDirectories
                'If My.Computer.FileSystem.FileExists((finfo.FullName & "\Build.txt").Replace("\\", "\")) Then
                '    Dim mfinfo As FileInfo = New FileInfo((finfo.FullName & "\Build.txt").Replace("\\", "\"))
                '    mfinfo.CopyTo((backupfolder & "\" & finfo.Name & " - Build.txt").Replace("\\", "\"))
                '    lastinputline = "Backed up: " & mfinfo.Name
                'Else
                '    AlertMessage = AlertMessage & "Missing Build.txt File: " & finfo.Name & vbCrLf
                'End If
                inputlinesprecount = inputlinesprecount + 1
                inputlines = inputlines + 1
                worker.ReportProgress(0)
                finfo = Nothing
            Next

            'If inputlinesprecount < 1 Then
            '    My.Computer.FileSystem.DeleteDirectory(backupfolder, FileIO.DeleteDirectoryOption.DeleteAllContents)
            'End If

        Catch ex As Exception
            Error_Handler(ex, "PreCount_Function")
        End Try
    End Sub

    Private Sub startAsyncButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles startAsyncButton.Click
        Try
            If busyworking = False Then
                If My.Computer.FileSystem.DirectoryExists(TextBox1.Text) Then


                    busyworking = True


                    inputlines = 0
                    lastinputline = ""
                    highestPercentageReached = 0
                    inputlinesprecount = 0

                    backupdirectory = ""
                    savedirectory = ""
                    pretestdone = False

                    TextBox1.Enabled = False
                    Button1.Enabled = False
                    startAsyncButton.Enabled = False
                    cancelAsyncButton.Enabled = True
                    ' Start the asynchronous operation.
                    AlertMessage = ""

                    BackgroundWorker1.RunWorkerAsync(TextBox1.Text)
                Else
                    MsgBox("Please ensure that you select an existing directory to process", MsgBoxStyle.Information, "Invalid Directory Selected")
                End If
            End If
        Catch ex As Exception
            Error_Handler(ex, "StartWorker")
        End Try
    End Sub

    ' This event handler is where the actual work is done.
    Private Sub backgroundWorker1_DoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs) Handles BackgroundWorker1.DoWork

        ' Get the BackgroundWorker object that raised this event.
        Dim worker As BackgroundWorker = CType(sender, BackgroundWorker)

        ' Assign the result of the computation
        ' to the Result property of the DoWorkEventArgs
        ' object. This is will be available to the 
        ' RunWorkerCompleted eventhandler.
        e.Result = MainWorkerFunction(worker, e)
        sender = Nothing
        e = Nothing
        worker.Dispose()
        worker = Nothing
    End Sub 'backgroundWorker1_DoWork

    ' This event handler deals with the results of the
    ' background operation.
    Private Sub backgroundWorker1_RunWorkerCompleted(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        busyworking = False


        ' First, handle the case where an exception was thrown.
        If Not (e.Error Is Nothing) Then
            Error_Handler(e.Error, "backgroundWorker1_RunWorkerCompleted")
        ElseIf e.Cancelled Then
            ' Next, handle the case where the user canceled the 
            ' operation.
            ' Note that due to a race condition in 
            ' the DoWork event handler, the Cancelled
            ' flag may not have been set, even though
            ' CancelAsync was called.
            Me.ToolStripStatusLabel1.Text = "Operation Cancelled" & "   (" & inputlines & " of " & inputlinesprecount & ")"
            Me.ProgressBar1.Value = 0

        Else
            ' Finally, handle the case where the operation succeeded.
            Me.ToolStripStatusLabel1.Text = "Operation Completed" & "   (" & inputlines & " of " & inputlinesprecount & ")"
            Me.ProgressBar1.Value = 100
            If AlertMessage.Length > 0 Then
                'MsgBox("The following alerts were raised during the operation. If you wish to save these alerts, press Ctrl+C and paste it into NotePad." & vbCrLf & vbCrLf & "********************" & vbCrLf & vbCrLf & AlertMessage, MsgBoxStyle.Information, "Raised Alerts")
                MsgBox(AlertMessage & " copies of AutoUpdate.ico were distributed", MsgBoxStyle.Information, "Copies Distributed")
            End If
        End If

        TextBox1.Enabled = True
        Button1.Enabled = True
        startAsyncButton.Enabled = True
        cancelAsyncButton.Enabled = False

        sender = Nothing
        e = Nothing


    End Sub 'backgroundWorker1_RunWorkerCompleted

    Private Sub backgroundWorker1_ProgressChanged(ByVal sender As Object, ByVal e As ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged


        Me.ProgressBar1.Value = e.ProgressPercentage
        'If lastinputline.StartsWith("Operation Completed") Then
        'Me.ToolStripStatusLabel1.Text = lastinputline
        'Else
        Me.ToolStripStatusLabel1.Text = lastinputline & "   (" & inputlines & " of " & inputlinesprecount & ")"
        'End If


        sender = Nothing
        e = Nothing
    End Sub

    Function MainWorkerFunction(ByVal worker As BackgroundWorker, ByVal e As DoWorkEventArgs) As Boolean
        Dim result As Boolean = False
        Try
            If Me.pretestdone = False Then
                primary_PercentComplete = 0
                worker.ReportProgress(0)
                PreCount_Function(worker)
                Me.pretestdone = True
            End If

            If worker.CancellationPending Then
                e.Cancel = True
                Return False
            End If

            primary_PercentComplete = 0
            worker.ReportProgress(0)

            inputlines = 0
            lastinputline = ""


            Dim dinfo As DirectoryInfo
            dinfo = New DirectoryInfo(TextBox1.Text)

            If dinfo.Exists Then
                For Each subdir As DirectoryInfo In dinfo.GetDirectories
                    lastinputline = "Processing: " & subdir.Name
                    ' Report progress as a percentage of the total task.
                    percentComplete = 0
                    If inputlinesprecount > 0 Then
                        percentComplete = CSng(inputlines) / CSng(inputlinesprecount) * 100
                    Else
                        percentComplete = 100
                    End If
                    primary_PercentComplete = percentComplete
                    If percentComplete > 100 Then
                        percentComplete = 100
                    End If
                    If percentComplete = 100 Then
                        lastinputline = "Operation Completed"
                    End If
                    If percentComplete > highestPercentageReached Then
                        highestPercentageReached = percentComplete
                        worker.ReportProgress(percentComplete)
                    End If
                    If worker.CancellationPending Then
                        e.Cancel = True
                        Exit For
                        Return False
                    End If


                    Dim sourcecodedir As String = ""
                    sourcecodedir = subdir.Name.Substring(0, subdir.Name.LastIndexOf(" - "))

                    If My.Computer.FileSystem.DirectoryExists((subdir.FullName & "\Source Code\" & sourcecodedir & "\Required Files\").Replace("\\", "\")) Then
                        If My.Computer.FileSystem.FileExists((subdir.FullName & "\Source Code\" & sourcecodedir & "\Required Files\AutoUpdate.exe").Replace("\\", "\")) Then
                            If My.Computer.FileSystem.FileExists((subdir.FullName & "\Source Code\" & sourcecodedir & " Installer\Images\AutoUpdate.ico").Replace("\\", "\")) = False Then
                                If My.Computer.FileSystem.FileExists((Application.StartupPath & "\AutoUpdate.ico").Replace("\\", "\")) = True Then
                                    My.Computer.FileSystem.CopyFile((Application.StartupPath & "\AutoUpdate.ico").Replace("\\", "\"), (subdir.FullName & "\Source Code\" & sourcecodedir & " Installer\Images\AutoUpdate.ico").Replace("\\", "\"))
                                    If AlertMessage = "" Then
                                        AlertMessage = "0"
                                    End If
                                    AlertMessage = (Integer.Parse(AlertMessage) + 1).ToString
                                    Try
                                        If My.Computer.FileSystem.FileExists((subdir.FullName & "\Source Code\" & sourcecodedir & " Installer\" & sourcecodedir & " Installer.aip").Replace("\\", "\")) = True Then
                                            My.Computer.FileSystem.CopyFile((subdir.FullName & "\Source Code\" & sourcecodedir & " Installer\" & sourcecodedir & " Installer.aip").Replace("\\", "\"), (subdir.FullName & "\Source Code\" & sourcecodedir & " Installer\" & sourcecodedir & " Installer (Backup " & Format(Now, "yyyyMMddHHmmss") & ").aip").Replace("\\", "\"))
                                            Dim reader As StreamReader = My.Computer.FileSystem.OpenTextFileReader((subdir.FullName & "\Source Code\" & sourcecodedir & " Installer\" & sourcecodedir & " Installer.aip").Replace("\\", "\"), System.Text.Encoding.ASCII)
                                            Dim writer As StreamWriter = My.Computer.FileSystem.OpenTextFileWriter((subdir.FullName & "\Source Code\" & sourcecodedir & " Installer\" & sourcecodedir & " Installer_XTEMPX_.aip").Replace("\\", "\"), False, System.Text.Encoding.ASCII)
                                            Try
                                                While reader.Peek <> -1
                                                    Dim lineread As String = reader.ReadLine
                                                    If lineread <> "    <ROW Shortcut=""" & sourcecodedir.Replace("-", "").Replace("!", "").Replace(" ", "_") & "_AutoUpdate"" Directory_=""SHORTCUTDIR"" Name=""AutoUp~1|" & sourcecodedir & " AutoUpdate"" Component_=""Application_Loader.exe"" Target=""[TARGETDIR]AutoUpdate.exe"" Arguments=""force"" Description=""AutoUpdate"" Hotkey=""0"" Icon_=""TARGETDIR_AutoUpdate.exe"" IconIndex=""0"" ShowCmd=""1"" WkDir=""""/>" And lineread <> "    <ROW Name=""TARGETDIR_AutoUpdate.exe"" SourcePath=""Images\AutoUpdate.ico"" Index=""0""/>" And lineread <> "    <ROW Name=""SystemFolder_msiexec.exe"" SourcePath=""&lt;uninstall.ico&gt;"" Index=""0""/>" And lineread <> "    <ROW Shortcut=""" & sourcecodedir.Replace("-", "").Replace("!", "").Replace(" ", "_") & "_Uninstall"" Directory_=""SHORTCUTDIR"" Name=""Uninst~1|" & sourcecodedir & " Uninstall"" Component_=""Application_Loader.exe"" Target=""[SystemFolder]msiexec.exe"" Arguments=""/x [ProductCode]"" Description="""" Hotkey=""0"" Icon_=""SystemFolder_msiexec.exe"" IconIndex=""0"" ShowCmd=""1"" WkDir=""""/>" And lineread <> "    <ROW Shortcut=""" & sourcecodedir.Replace("-", "").Replace("!", "").Replace(" ", "_") & "_Uninstaller"" Directory_=""SHORTCUTDIR"" Name=""Uninst~1|" & sourcecodedir & " Uninstaller"" Component_=""Application_Loader.exe"" Target=""[SystemFolder]msiexec.exe"" Arguments=""/x [ProductCode]"" Description="""" Hotkey=""0"" Icon_=""SystemFolder_msiexec.exe"" IconIndex=""0"" ShowCmd=""1"" WkDir=""""/>" And lineread <> "    <ROW Shortcut=""Uninstall_" & sourcecodedir.Replace("-", "").Replace("!", "").Replace(" ", "_") & """ Directory_=""SHORTCUTDIR"" Name=""Uninst~1|Uninstall " & sourcecodedir & """ Component_=""Application_Loader.exe"" Target=""[SystemFolder]msiexec.exe"" Arguments=""/x [ProductCode]"" Description="""" Hotkey=""0"" Icon_=""SystemFolder_msiexec.exe"" IconIndex=""0"" ShowCmd=""1"" WkDir=""""/>" Then
                                                        writer.WriteLine(lineread)
                                                    End If
                                                    If lineread.Trim = "<COMPONENT cid=""caphyon.advinst.msicomp.MsiShortsComponent"">" Then
                                                        writer.WriteLine("    <ROW Shortcut=""" & sourcecodedir.Replace("-", "").Replace("!", "").Replace(" ", "_") & "_AutoUpdate"" Directory_=""SHORTCUTDIR"" Name=""AutoUp~1|" & sourcecodedir & " AutoUpdate"" Component_=""Application_Loader.exe"" Target=""[TARGETDIR]AutoUpdate.exe"" Arguments=""force"" Description=""AutoUpdate"" Hotkey=""0"" Icon_=""TARGETDIR_AutoUpdate.exe"" IconIndex=""0"" ShowCmd=""1"" WkDir=""""/>")
                                                        writer.WriteLine("    <ROW Shortcut=""" & sourcecodedir.Replace("-", "").Replace("!", "").Replace(" ", "_") & "_Uninstall"" Directory_=""SHORTCUTDIR"" Name=""Uninst~1|" & sourcecodedir & " Uninstall"" Component_=""Application_Loader.exe"" Target=""[SystemFolder]msiexec.exe"" Arguments=""/x [ProductCode]"" Description="""" Hotkey=""0"" Icon_=""SystemFolder_msiexec.exe"" IconIndex=""0"" ShowCmd=""1"" WkDir=""""/>")
                                                    End If
                                                    If lineread.Trim = "<COMPONENT cid=""caphyon.advinst.msicomp.MsiIconsComponent"">" Then
                                                        writer.WriteLine("    <ROW Name=""TARGETDIR_AutoUpdate.exe"" SourcePath=""Images\AutoUpdate.ico"" Index=""0""/>")
                                                        writer.WriteLine("    <ROW Name=""SystemFolder_msiexec.exe"" SourcePath=""&lt;uninstall.ico&gt;"" Index=""0""/>")
                                                    End If
                                                End While
                                            Catch ex As Exception
                                                Error_Handler(ex, "Injecting command into Installer file")
                                            End Try
                                            writer.Close()
                                            writer = Nothing
                                            reader.Close()
                                            reader = Nothing






                                            My.Computer.FileSystem.MoveFile((subdir.FullName & "\Source Code\" & sourcecodedir & " Installer\" & sourcecodedir & " Installer_XTEMPX_.aip").Replace("\\", "\"), (subdir.FullName & "\Source Code\" & sourcecodedir & " Installer\" & sourcecodedir & " Installer.aip").Replace("\\", "\"), True)
                                        End If
                                    Catch ex As Exception
                                        Error_Handler(ex, "Injecting command into Installer file")
                                    End Try
                                End If

                            End If
                        End If
                    End If



                
                    inputlines = inputlines + 1
                    lastinputline = "Processed: " & (subdir.Name & "\Build.txt").Replace("\\", "\")
                    ' Report progress as a percentage of the total task.
                    percentComplete = 0
                    If inputlinesprecount > 0 Then
                        percentComplete = CSng(inputlines) / CSng(inputlinesprecount) * 100
                    Else
                        percentComplete = 100
                    End If
                    primary_PercentComplete = percentComplete
                    If percentComplete > 100 Then
                        percentComplete = 100
                    End If
                    If percentComplete = 100 Then
                        lastinputline = "Operation Completed"
                    End If
                    If percentComplete > highestPercentageReached Then
                        highestPercentageReached = percentComplete
                        worker.ReportProgress(percentComplete)
                    End If
                    subdir = Nothing
                    If worker.CancellationPending Then
                        e.Cancel = True
                        Exit For
                        dinfo = Nothing
                        Return False
                    End If
                Next
            End If
            dinfo = Nothing




        Catch ex As Exception
            Error_Handler(ex, "MainWorkerFunction")
        End Try
        worker.Dispose()
        worker = Nothing
        e = Nothing
        Return result

    End Function

    Private Sub Form1_Close(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            Me.ToolStripStatusLabel1.Text = "Application Closing"
            SaveSettings()
        Catch ex As Exception
            Error_Handler(ex, "Application Close")
        End Try
    End Sub

    Private Sub LoadSettings()
        Try
            Dim configfile As String = (Application.StartupPath & "\config.sav").Replace("\\", "\")
            If My.Computer.FileSystem.FileExists(configfile) Then
                Dim reader As StreamReader = New StreamReader(configfile)
                Dim lineread As String
                Dim variablevalue As String
                While reader.Peek <> -1
                    lineread = reader.ReadLine
                    If lineread.IndexOf("=") <> -1 Then

                        variablevalue = lineread.Remove(0, lineread.IndexOf("=") + 1)

                        If lineread.StartsWith("ImageFolder=") Then
                            Dim dinfo As DirectoryInfo = New DirectoryInfo(variablevalue)
                            If dinfo.Exists Then
                                FolderBrowserDialog1.SelectedPath = variablevalue
                                TextBox1.Text = variablevalue
                            End If
                            dinfo = Nothing
                        End If

                        'If lineread.StartsWith("SetVariable=") Then
                        '    ComboBox1.SelectedIndex = variablevalue
                        'End If

                        'If lineread.StartsWith("PixelValue=") Then
                        '    NumericUpDown2.Value = variablevalue
                        'End If
                    
                    End If
                End While
                reader.Close()
                reader = Nothing
            End If
        Catch ex As Exception
            Error_Handler(ex, "Load Settings")
        End Try
    End Sub

    Private Sub SaveSettings()
        Try
            Dim configfile As String = (Application.StartupPath & "\config.sav").Replace("\\", "\")

            Dim writer As StreamWriter = New StreamWriter(configfile, False)

            If TextBox1.Text.Length > 0 Then
                Dim dinfo As DirectoryInfo = New DirectoryInfo(TextBox1.Text)
                If dinfo.Exists Then
                    writer.WriteLine("ImageFolder=" & TextBox1.Text)
                End If
                dinfo = Nothing
            End If
            'If ComboBox1.SelectedIndex <> -1 Then
            '    writer.WriteLine("SetVariable=" & ComboBox1.SelectedIndex)
            'End If

            'writer.WriteLine("PixelValue=" & NumericUpDown2.Value)

            writer.Flush()
            writer.Close()
            writer = Nothing

        Catch ex As Exception
            Error_Handler(ex, "Save Settings")
        End Try
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            ComboBox1.SelectedIndex = 0
            Me.Text = My.Application.Info.ProductName & " " & Format(My.Application.Info.Version.Major, "0000") & Format(My.Application.Info.Version.Minor, "00") & Format(My.Application.Info.Version.Build, "00") & "." & Format(My.Application.Info.Version.Revision, "00") & ""
            LoadSettings()
            Me.ToolStripStatusLabel1.Text = "Application Loaded"
        Catch ex As Exception
            Error_Handler(ex, "Application Load")
        End Try

    End Sub



    Private Sub ComboBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox1.SelectedIndexChanged
        SelectedIndex = ComboBox1.SelectedIndex
    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        Try
            Me.ToolStripStatusLabel1.Text = "About displayed"
            AboutBox1.ShowDialog()
        Catch ex As Exception
            Error_Handler(ex, "Display About Screen")
        End Try
    End Sub

    Private Sub HelpToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HelpToolStripMenuItem.Click
        Try
            Me.ToolStripStatusLabel1.Text = "Help displayed"
            HelpBox1.ShowDialog()
        Catch ex As Exception
            Error_Handler(ex, "Display Help Screen")
        End Try
    End Sub



    Private Sub TextBox1_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles TextBox1.DragDrop
        Try
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                Dim MyFiles() As String
                Dim i As Integer

                ' Assign the files to an array.
                MyFiles = e.Data.GetData(DataFormats.FileDrop)
                ' Loop through the array and add the files to the list.
                'For i = 0 To MyFiles.Length - 1
                If MyFiles.Length > 0 Then
                    Dim finfo As DirectoryInfo = New DirectoryInfo(MyFiles(0))
                    If finfo.Exists = True Then
                        TextBox1.Text = (MyFiles(0))
                        FolderBrowserDialog1.SelectedPath = (MyFiles(0))
                    End If
                End If
                'Next
            End If
        Catch ex As Exception
            Error_Handler(ex)
        End Try
    End Sub

    Private Sub TextBox1_DragEnter(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles TextBox1.DragEnter
        Try
            If e.Data.GetDataPresent(DataFormats.FileDrop) Then
                e.Effect = DragDropEffects.All
            End If
        Catch ex As Exception
            Error_Handler(ex)
        End Try
    End Sub
End Class
