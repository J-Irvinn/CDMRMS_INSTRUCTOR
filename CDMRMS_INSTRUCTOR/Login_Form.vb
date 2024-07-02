﻿Imports MetroFramework.Controls
Imports MySql.Data.MySqlClient
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions


Public Class CDMRMS_Instructor_Login


    ' FORM LOAD - START
    Private Sub CDMRMS_Instructor_Login_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        DatabaseConnection()
        Registration_Panel.Hide()

        Me.Icon = My.Resources.CdMRMS

    End Sub

    Private Sub CDMRMS_Instructor_Login_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        RememberMe()
    End Sub


    ' FORM LOAD - END


    ' DATABASE CONNECTION - START
    Private Shared ConnectionString As String = "server=localhost; port=3306; database=cdmregistrarmanagementsystem; uid=root; password=;"
    Private Shared connection As New MySqlConnection(ConnectionString)

    Private Sub DatabaseConnection()

        Try
            connection.Open()

        Catch ex As Exception
            MsgBox("Error: " & ex.Message, MessageBoxIcon.Error)
        Finally
            connection.Close()

        End Try

    End Sub

    Public Shared Function GetConnection() As MySqlConnection

        Return connection

    End Function
    ' DATABASE CONNECTION - END


    ' PASSWORD HASHING - START
    Private Function HashPassword(password As String) As String

        ' Create a new instance of SHA256
        Using SHA256 As SHA256 = SHA256.Create()
            ' ComputeHash - returns byte array
            Dim bytes As Byte() = SHA256.ComputeHash(Encoding.UTF8.GetBytes(password))
            ' Convert byte array to string
            Dim builder As New StringBuilder()

            For i As Integer = 0 To bytes.Length - 1
                builder.Append(bytes(i).ToString("x2"))
            Next

            Return builder.ToString()

        End Using

    End Function
    ' PASSWORD HASHING - END


    ' REGISTRATION - START
    Private Sub Register_Btn_Click(sender As Object, e As EventArgs) Handles Register_Btn.Click


        ' Variable declaration for registration
        Dim firstName As String = FN_Input.Text.Trim
        Dim middleName As String = MN_Input.Text.Trim
        Dim lastName As String = LN_Input.Text.Trim
        Dim instructorID As String = InstructorID_Input.Text.Trim
        Dim gender As String
        Dim email As String = Email_Input.Text.Trim
        Dim contact As String = Contact_Input.Text.Trim
        Dim birthday As Date = Birthdate_Picker.Value.Date
        Dim password As String = Password_Input.Text.Trim
        Dim password2 As String = Password2_Input.Text.Trim

        ' Check if all needed information are fill-out
        If String.IsNullOrEmpty(firstName) And String.IsNullOrEmpty(lastName) And String.IsNullOrEmpty(instructorID) And String.IsNullOrEmpty(contact) And String.IsNullOrEmpty(email) And String.IsNullOrEmpty(password) And String.IsNullOrEmpty(password2) Then
            MsgBox("Please enter all needed information", MessageBoxIcon.Warning)

        ElseIf String.IsNullOrEmpty(firstName) Or String.IsNullOrEmpty(lastName) Or String.IsNullOrEmpty(instructorID) Or String.IsNullOrEmpty(contact) Or String.IsNullOrEmpty(email) Or String.IsNullOrEmpty(password) Or String.IsNullOrEmpty(password2) Then
            MsgBox("Please fill all missing information", MessageBoxIcon.Warning)

        ElseIf ValidateInstructorID(instructorID) Then

            If ValidateEmailAddress(email) Then

                If ValidatePhilippinePhoneNumber(contact) Then

                    If password <> password2 Then
                        MsgBox("Password does'nt match.", MessageBoxIcon.Information)

                    Else
                        If String.IsNullOrEmpty(middleName) Then
                            middleName = "N/A"

                            If Male_RadioBtn.Checked Then
                                gender = Male_RadioBtn.Text
                                If Not IsDataExists(instructorID) Then
                                    InsertRegistrationData(firstName, middleName, lastName, instructorID, gender, email, contact, birthday, password)

                                Else
                                    MsgBox("Data already exists in the database.")
                                End If

                            ElseIf Female_RadioBtn.Checked Then
                                gender = Female_RadioBtn.Text
                                If Not IsDataExists(instructorID) Then
                                    InsertRegistrationData(firstName, middleName, lastName, instructorID, gender, email, contact, birthday, password)

                                Else
                                    MsgBox("Data already exists in the database.")
                                End If

                            ElseIf Not Male_RadioBtn.Checked Or Not Female_RadioBtn.Checked Then
                                MsgBox("Please select your Gender.", MessageBoxIcon.Warning)
                            End If

                        Else
                            If Male_RadioBtn.Checked Then
                                gender = Male_RadioBtn.Text
                                If Not IsDataExists(instructorID) Then
                                    InsertRegistrationData(firstName, middleName, lastName, instructorID, gender, email, contact, birthday, password)

                                Else
                                    MsgBox("Data already exists in the database.")
                                End If

                            ElseIf Female_RadioBtn.Checked Then
                                gender = Female_RadioBtn.Text
                                If Not IsDataExists(instructorID) Then
                                    InsertRegistrationData(firstName, middleName, lastName, instructorID, gender, email, contact, birthday, password)

                                Else
                                    MsgBox("Data already exists in the database.")
                                End If

                            ElseIf Not Male_RadioBtn.Checked Or Not Female_RadioBtn.Checked Then
                                MsgBox("Please select your Gender.", MessageBoxIcon.Warning)
                            End If
                        End If
                    End If
                Else
                    MsgBox("Invalid Phone number")

                End If
            Else
                MsgBox("Invalid Email address")

            End If
        Else
            MsgBox("Invalid Instructor ID")

        End If

    End Sub

    ' Only accept number and dash
    Private Sub InstructorID_Input_KeyPress(sender As Object, e As KeyPressEventArgs) Handles InstructorID_Input.KeyPress

        If e.KeyChar = "-"c AndAlso DirectCast(sender, MetroTextBox).Text.Contains("-") Then
            e.Handled = True

        End If

    End Sub

    ' Validate instructor ID number format
    Private Function ValidateInstructorID(instructorID As String) As String

        Dim pattern As String = "^CDM-\d{3,4}$"

        Dim regex As New Regex(pattern)
        Return regex.IsMatch(instructorID)

    End Function

    ' Validate Email address format
    Private Function ValidateEmailAddress(email As String) As String

        Dim pattern As String = "^\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b$"

        Dim regex As New Regex(pattern)
        Return regex.IsMatch(email)

    End Function

    ' Validate Philippine cellphone number format
    Private Function ValidatePhilippinePhoneNumber(contact As String) As String

        Dim pattern As String = "^(09\d{2})[- ]?(\d{7})$"

        Dim regex As New Regex(pattern)
        Return regex.IsMatch(contact)

    End Function

    ' Check if the data is already existing in database
    Private Function IsDataExists(instructorID As String) As Boolean

        Dim query As String = "SELECT COUNT(*) FROM `instructors` WHERE instructorID = @instructorID"
        Dim command As New MySqlCommand(query, connection)
        command.Parameters.AddWithValue("@instructorID", instructorID)

        connection.Open()
        Dim count As String = CInt(command.ExecuteScalar())
        connection.Close()

        Return count > 0

    End Function

    ' Insertion of validated Data to database
    Private Sub InsertRegistrationData(firstName As String, middleName As String, lastName As String, instructorID As String, gender As String, email As String, contact As String, birthday As Date, password As String)

        Dim hashedPassword As String = HashPassword(password)

        ' Insert Data to database
        Dim query As String = "INSERT INTO `instructors`(`firstname`, `middlename`, `lastname`, `instructorid`, `gender`, `email`, `contact#`, `birthday`, `password`) VALUES (@firstname, @middlename, @lastname, @instructorid, @gender, @email, @contact, @birthday, @password) "

        Try
            Using connection As New MySqlConnection(ConnectionString)
                Using command As New MySqlCommand(query, connection)

                    ' Store all user Data to Database
                    command.Parameters.AddWithValue("@firstname", firstName)
                    command.Parameters.AddWithValue("@middlename", middleName)
                    command.Parameters.AddWithValue("@lastname", lastName)
                    command.Parameters.AddWithValue("@instructorid", instructorID)
                    command.Parameters.AddWithValue("@gender", gender)
                    command.Parameters.AddWithValue("@email", email)
                    command.Parameters.AddWithValue("@contact", contact)
                    command.Parameters.AddWithValue("@birthday", birthday)
                    command.Parameters.AddWithValue("@password", hashedPassword)


                    connection.Open()
                    Dim count As Integer = Convert.ToInt32(command.ExecuteScalar())
                    MsgBox("Register Successfully!.", MessageBoxIcon.Information)
                    Login_Panel.Show()
                    Registration_Panel.Hide()


                    ' Clear all registrtion input fields
                    FN_Input.Clear()
                    MN_Input.Clear()
                    LN_Input.Clear()
                    InstructorID_Input.Clear()
                    If Male_RadioBtn.Checked Then
                        Male_RadioBtn.Checked = False
                    ElseIf Female_RadioBtn.Checked Then
                        Female_RadioBtn.Checked = False
                    End If
                    Email_Input.Clear()
                    Contact_Input.Clear()
                    Password_Input.Clear()
                    Password2_Input.Clear()

                End Using
            End Using

        Catch ex As Exception

            MsgBox("Error: " & ex.Message)

        End Try

    End Sub



    Private Sub Register_Link_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles Register_Link.LinkClicked

        Login_Panel.Hide()
        Registration_Panel.Show()

    End Sub
    ' REGISTRATION - END


    ' LOGIN - START

    Private Sub Login_Btn_Click(sender As Object, e As EventArgs) Handles Login_Btn.Click

        'LoginInstructorID_Input.Text = "CDM-1111"
        'LoginEmail_Input.Text = "johnirvingeanga@gmail.com"
        'LoginPassword_Input.Text = "01"

        Dim instructorID As String = LoginInstructorID_Input.Text.Trim
        Dim email As String = LoginEmail_Input.Text.Trim
        Dim password As String = LoginPassword_Input.Text.Trim

        If String.IsNullOrEmpty(instructorID) And String.IsNullOrEmpty(email) And String.IsNullOrEmpty(password) Then
            MessageBox.Show("All fields are mandatory. Please provide the necessary information to proceed.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)

        ElseIf String.IsNullOrEmpty(instructorID) Or String.IsNullOrEmpty(email) Or String.IsNullOrEmpty(password) Then
            MessageBox.Show("There are missing entries in the form. Please check and complete all fields.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)

        Else
            Dim isValidLogin As Boolean = ValidateLogin(instructorID, email, password)

            If isValidLogin Then
                Dim choice As DialogResult = MessageBox.Show("Login Successful.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information)


                If choice = DialogResult.OK Then
                    LoginInstructorID_Input.Clear()
                    LoginEmail_Input.Clear()
                    LoginPassword_Input.Clear()
                End If

                ' Stay Signed In 
                If StaySignedIn.Checked Then
                    My.Settings.StaySignedIn = True
                    My.Settings.InstructorID = LoginInstructorID_Input.Text
                    My.Settings.InstructorID = instructorID
                Else
                    My.Settings.StaySignedIn = False

                End If
                My.Settings.Save()


                ' Send data to main form
                Dim valueToPass As String = instructorID
                Dim main As New Instructor_Main With {
                    .PassedValue = valueToPass
                    }


                main.Show()
                Me.Hide()

            Else

                MessageBox.Show("The username or password you entered is incorrect. Please check your credentials and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

            End If
        End If

    End Sub

    ' Only accept number and dash
    Private Sub LoginInstructorID_Input_KeyPress(sender As Object, e As KeyPressEventArgs) Handles LoginInstructorID_Input.KeyPress

        If e.KeyChar = "-"c AndAlso DirectCast(sender, MetroTextBox).Text.Contains("-") Then
            e.Handled = True

        End If
    End Sub

    ' Validate login information
    Private Function ValidateLogin(instructorID As String, email As String, password As String) As String

        Dim hashedPassword As String = HashPassword(password)
        Dim isAuthenticated As Boolean = False

        Dim query As String = "SELECT * FROM `instructors` WHERE instructorid = @instructorID AND email = @email AND password = @password"
        Using connection As New MySqlConnection(ConnectionString)
            Using command As New MySqlCommand(query, connection)

                command.Parameters.AddWithValue("@instructorID", instructorID)
                command.Parameters.AddWithValue("@email", email)
                command.Parameters.AddWithValue("@password", hashedPassword)

                connection.Open()
                Dim count As Integer = Convert.ToInt32(command.ExecuteScalar())
                isAuthenticated = count > 0

            End Using
        End Using

        Return isAuthenticated

    End Function

    Private Sub RememberMe()
        If My.Settings.StaySignedIn Then
            Instructor_Main.Show()

        End If

    End Sub

    Private Sub Login_Link_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles Login_Link.LinkClicked

        Registration_Panel.Hide()
        Login_Panel.Show()

    End Sub

    ' LOGIN - END


    ' TOOLTIPS FOR LOGIN AND REGISTRATION INPUTS - START
    Private Sub RegToolTip_Label1_MouseHover(sender As Object, e As EventArgs) Handles RegToolTip_Label1.MouseHover
        Tooltip.ToolTipTitle = "Instructor's ID Number"
        Tooltip.SetToolTip(RegToolTip_Label1, "" & vbCrLf & " * It only accept 'CDM', Dash (1 only), And Numbers. " & vbCrLf & "   Sample Input: CDM-XXXX" & vbCrLf & vbCrLf & " * Other Characters Are Invalid." & vbCrLf & " ")
    End Sub

    Private Sub RegToolTip_Label2_MouseHover(sender As Object, e As EventArgs) Handles RegToolTip_Label2.MouseHover
        Tooltip.ToolTipTitle = "Email Address"
        Tooltip.SetToolTip(RegToolTip_Label2, "" & vbCrLf & " * Enter your email address in the format: " & vbCrLf & "   name@example.com" & vbCrLf & " ")
    End Sub

    Private Sub RegToolTip_Label3_MouseHover(sender As Object, e As EventArgs) Handles RegToolTip_Label3.MouseHover
        Tooltip.ToolTipTitle = "Contact Number"
        Tooltip.SetToolTip(RegToolTip_Label3, " " & vbCrLf & " * Please enter a valid contact number.   " & vbCrLf & "   (e.g., 09XXXXXXXXX)." & vbCrLf & " ")
    End Sub

    Private Sub RegToolTip_Label4_MouseHover(sender As Object, e As EventArgs) Handles RegToolTip_Label4.MouseHover
        Tooltip.ToolTipTitle = "Middle Name"
        Tooltip.SetToolTip(RegToolTip_Label4, " " & vbCrLf & " * Leave it blank if not applicable." & vbCrLf & " ")
    End Sub


    ' TOOLTIPS FOR LOGIN AND REGISTRATION INPUTS - END


End Class
