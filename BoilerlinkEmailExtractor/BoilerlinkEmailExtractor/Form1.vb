Public Class Form1
    Dim input As String
    Dim phrase As String
    Dim Occurrences As Integer
    Dim charIndex As Integer
    Dim prevId As String
    Dim emailIndex As Integer = 0
    Dim lastClipboard As String = ""

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If TextBox1.Text.StartsWith("https://boilerlink.purdue.edu/organization/") Then
            If TextBox1.Text.EndsWith("/") Then
                WebBrowser1.Navigate(TextBox1.Text & "roster/manage")
            Else
                WebBrowser1.Navigate(TextBox1.Text & "/roster/manage")
            End If
            MsgBox("Great! Please log in to your Boilerlink account. Your login will not be saved.")
        Else
            MsgBox("The link entered is not a valid Purdue Boilerlink club page.")
        End If
    End Sub

    Private Sub TextBox1_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles TextBox1.KeyDown
        If e.KeyCode = Keys.Enter Then
            If TextBox1.Text.StartsWith("https://boilerlink.purdue.edu/organization/") Then
                If TextBox1.Text.EndsWith("/") Then
                    WebBrowser1.Navigate(TextBox1.Text & "roster/manage")
                Else
                    WebBrowser1.Navigate(TextBox1.Text & "/roster/manage")
                End If
                MsgBox("Great! Please log in to your Boilerlink account. Your login will not be saved.")
            Else
                MsgBox("The link entered is not a valid Purdue Boilerlink club page.")
            End If
        End If
    End Sub

    Private Sub WebBrowser1_DocumentCompleted(ByVal sender As Object, ByVal e As System.Windows.Forms.WebBrowserDocumentCompletedEventArgs) Handles WebBrowser1.DocumentCompleted
        ProgressBar1.Value = 0
        RichTextBox1.Clear()
        RichTextBox1.Text = WebBrowser1.DocumentText
        If RichTextBox1.Text.Contains("<TD class=""checkbox""><SPAN><INPUT") Then
            Button3.Enabled = True
        End If
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Button3.Enabled = False
        input = RichTextBox1.Text
        phrase = "<TD class=""checkbox""><SPAN><INPUT"
        Occurrences = (input.Length - input.Replace(phrase, String.Empty).Length) / phrase.Length
        charIndex = 0
        prevId = ""

        MsgBox("We found " & Occurrences & " member emails on this page.")

        For i As Integer = 0 To (Occurrences - 1) Step 1
            charIndex = input.IndexOf(phrase, charIndex)
            Dim rawContent As String = input.Substring(charIndex, 200)
            'check for duplicate IDs
            If rawContent <> prevId Then
                'parse for value to get ID
                Dim startPos As Integer = rawContent.IndexOf("value=")
                startPos = startPos + 7 'move cursor past "value="
                'We have a 7 digit ID to trim
                Dim id As String = rawContent.Substring(startPos, 7)
                'Add items to the list
                ListBox1.Items.Add(id)
                lblNumEmails.Text = "Member Emails: " & ListBox1.Items.Count
                prevId = rawContent
            End If
            charIndex = charIndex + 1
        Next
        ProgressBar1.Maximum = Occurrences

        processEmails.Start()

    End Sub

    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked Then
            chkClp.Start()
        Else
            chkClp.Stop()
        End If
    End Sub

    Private Sub processEmails_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles processEmails.Tick
        Try
            If emailIndex > (ListBox1.Items.Count - 1) Then
                ProgressBar1.Value = Occurrences
                processEmails.Stop()
                MsgBox("Emails acquired successfully! Remember to acquire emails from all pages.")
            Else
                Dim currentID As String = ListBox1.Items.Item(emailIndex)
                WebBrowser2.Navigate("https://boilerlink.purdue.edu/users/membercard/" & currentID)
                processEmails.Stop()
                ProgressBar1.Value = emailIndex
            End If

        Catch
        End Try
    End Sub

    Private Sub WebBrowser2_DocumentCompleted(ByVal sender As Object, ByVal e As System.Windows.Forms.WebBrowserDocumentCompletedEventArgs) Handles WebBrowser2.DocumentCompleted
        Dim memberEmail As String = WebBrowser2.Document.Body.InnerHtml
        Dim emailStart As Integer = memberEmail.IndexOf("mailto:") + 7
        Dim emailEnd As Integer = memberEmail.IndexOf("""", emailStart + 1)
        memberEmail = memberEmail.Substring(emailStart, (emailEnd - emailStart))
        ListBox1.Items.Item(emailIndex) = memberEmail

        emailIndex = emailIndex + 1
        processEmails.Start()
    End Sub

    Private Sub chkClp_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkClp.Tick
        If My.Computer.Clipboard.ContainsText Then
            Dim currentClp As String = My.Computer.Clipboard.GetText
            If currentClp <> lastClipboard Then
                If currentClp.StartsWith("mailto:") Then
                    ListBox1.Items.Add(currentClp.Substring(7))
                    My.Computer.Audio.PlaySystemSound(System.Media.SystemSounds.Asterisk)
                    lastClipboard = currentClp
                End If
            End If
        End If
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        SaveFileDialog1.ShowDialog()
        If SaveFileDialog1.FileName <> "" Then
            Try
                Dim content As String = ""
                For Each item In ListBox1.Items
                    content = content & ", " & item
                Next
                content = content.Substring(2)
                My.Computer.FileSystem.WriteAllText(SaveFileDialog1.FileName, content, False, System.Text.Encoding.Default)
                MsgBox("Filed saved!")
            Catch ex As Exception
                MsgBox("Failed to save file. Try another directory.")
            End Try
        End If
    End Sub
End Class
