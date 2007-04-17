Imports System
Imports System.Text
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()> Public Class TestFtpClient

#Region "Additional test attributes"
    '
    ' You can use the following additional attributes as you write your tests:
    '
    ' Use ClassInitialize to run code before running the first test in the class
    ' <ClassInitialize()> Public Shared Sub MyClassInitialize(ByVal testContext As TestContext)
    ' End Sub
    <ClassInitialize()> Public Shared Sub MyClassInitialize(ByVal testContext As TestContext)
        'Setup local test directory
        localdir = New IO.DirectoryInfo(My.Settings.LocalDir)
        'ensure it exists
        If localdir.Exists = False Then localdir.Create()

        'set remote dir and ensure it ends in \
        remoteDir = My.Settings.RemoteDir.TrimEnd("/"c) & "/"

        'binary jpg image to use for testing
        imageFile = New IO.FileInfo(localdir.FullName & "\" & My.Settings.ImageFile & ".jpg")
        'clear any existing file
        If imageFile.Exists Then imageFile.Delete()
        'write internal resource into it
        My.Resources.ColossusImage.Save(imageFile.FullName)
        'set remote target version
        imageFileTarget = remoteDir & imageFile.Name

        'text file to use for testing
        textFile = New IO.FileInfo(localdir.FullName & "\" & My.Settings.TextFile & ".txt")
        'clear any existing file
        If textFile.Exists Then textFile.Delete()
        'write internal resource into it
        My.Computer.FileSystem.WriteAllText(textFile.FullName, My.Resources.ColossusText, False)
        'set remote target filename
        textFileTarget = remoteDir & textFile.Name

        'ensure remote test directory exists
        Dim ftp As New Utilities.FTP.FTPclient(My.Settings.Host, My.Settings.Username, My.Settings.Password)
        If ftp.ftpDirectoryExists(remoteDir) = False Then
            'create directory
            ftp.FtpCreateDirectory(remoteDir)
        End If
    End Sub

    Private Shared remoteDir As String
    Private Shared localdir As IO.DirectoryInfo
    Private Shared imageFile As IO.FileInfo
    Private Shared textFile As IO.FileInfo

    Private Shared imageFileTarget As String
    Private Shared textFileTarget As String

    ' Use ClassCleanup to run code after all tests in a class have run
    ' <ClassCleanup()> Public Shared Sub MyClassCleanup()
    ' End Sub
    <ClassCleanup()> Public Shared Sub MyClassCleanup()
        'Clear out test directory
        If imageFile.Exists Then imageFile.Delete()
        If textFile.Exists Then textFile.Delete()

    End Sub

    ' Use TestInitialize to run code before running each test
    ' <TestInitialize()> Public Sub MyTestInitialize()
    ' End Sub
    '
    ' Use TestCleanup to run code after each test has run
    ' <TestCleanup()> Public Sub MyTestCleanup()
    ' End Sub
    '
#End Region

#Region "Test constructors"

    <TestMethod()> Public Sub Constructor1()
        Dim ftp As New Utilities.FTP.FTPclient()
    End Sub

    <TestMethod()> Public Sub Constructor2()
        Dim ftp As New Utilities.FTP.FTPclient(My.Settings.Host)
    End Sub

    <TestMethod()> Public Sub Constructor3()
        Dim ftp As New Utilities.FTP.FTPclient(My.Settings.Host, My.Settings.Username, My.Settings.Password)
    End Sub

    <TestMethod()> Public Sub Constructor4()
        Dim ftp As New Utilities.FTP.FTPclient(My.Settings.Host, My.Settings.Username, My.Settings.Password, True)
    End Sub

#End Region

#Region "Test logins"

    <TestMethod()> Public Sub LoginGood()
        Dim ftp As New Utilities.FTP.FTPclient(My.Settings.Host, My.Settings.Username, My.Settings.Password)
        'get directory string from root only
        Dim root As List(Of String) = ftp.ListDirectory("/")
    End Sub

    ''' <summary>
    ''' Test login with bad hostname
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod(), _
    ExpectedException(GetType(System.Net.WebException))> _
    Public Sub LoginBad1()
        Dim ftp As New Utilities.FTP.FTPclient("nonexistenthostname")
        Dim root As List(Of String) = ftp.ListDirectory("/")
    End Sub

    ''' <summary>
    ''' Test login with wrong password
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod(), _
    ExpectedException(GetType(System.Net.WebException))> _
    Public Sub LoginBad2()
        Dim ftp As New Utilities.FTP.FTPclient(My.Settings.Host, My.Settings.Username, "incorrectpassword")
        Dim root As List(Of String) = ftp.ListDirectory("/")
    End Sub

#End Region

#Region "Test downloads and uploads"

    ''' <summary>
    ''' Test upload of binary file
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod()> Public Sub UploadBinary()
        Dim ftp As New Utilities.FTP.FTPclient(My.Settings.Host, My.Settings.Username, My.Settings.Password)
        'Ensure target does not exist
        If ftp.FtpFileExists(imageFileTarget) Then ftp.FtpDelete(imageFileTarget)
        'Test upload
        ftp.Upload(imageFile, imageFileTarget)

        'Check it exists
        Assert.IsTrue(ftp.FtpFileExists(imageFileTarget), "Remote file not found after upload")

        'Compare filesizes
        Assert.IsTrue(ftp.GetFileSize(imageFileTarget) = imageFile.Length, "File sizes do not match - upload error?")

        'Clean up
        ftp.FtpDelete(imageFileTarget)
    End Sub

    <TestMethod()> Public Sub UploadText()
        Assert.Fail("Text mode not implemented at present")
    End Sub

    <TestMethod()> Public Sub DownloadBinary()
        Dim ftp As New Utilities.FTP.FTPclient(My.Settings.Host, My.Settings.Username, My.Settings.Password)
        'Ensure target does not exist
        If Not ftp.FtpFileExists(imageFileTarget) Then
            'upload target
            ftp.Upload(imageFile, imageFileTarget)
        End If

        'Check it exists before testing download
        Assert.IsTrue(ftp.FtpFileExists(imageFileTarget), "Remote file not found for download testing")

        'Download to a tempfile
        Dim tmpFile As String = My.Computer.FileSystem.GetTempFileName()

        Try
            ftp.Download(imageFileTarget, tmpFile, True)

            Dim tmp As New IO.FileInfo(tmpFile)
            Dim tmpSize As Long = tmp.Length

            'remove file
            tmp.Delete()

            'Compare filesizes
            Assert.IsTrue(ftp.GetFileSize(imageFileTarget) = tmpSize, "File sizes do not match - upload error?")

            'Clean up
            ftp.FtpDelete(imageFileTarget)

        Catch ex As Exception
            'ensure temp file removed
            If IO.File.Exists(tmpFile) Then IO.File.Delete(tmpFile)
            Throw
        End Try
    End Sub

    <TestMethod()> Public Sub DownloadText()
        Assert.Fail("Text mode not implemented at present")
    End Sub

#End Region

#Region "Test Misc FTP functions"

    ''' <summary>
    ''' Test creating and deleting remote directories 
    ''' </summary>
    ''' <remarks>
    ''' Requires write/delete access on FTP server
    ''' </remarks>
    <TestMethod()> Public Sub CreateAndDeleteDirectory()
        Dim ftp As New Utilities.FTP.FTPclient(My.Settings.Host, My.Settings.Username, My.Settings.Password)

        'Create a temporary subdirectory
        Dim SubDir As String = remoteDir & "TestDir\"

        'Create directory
        Assert.IsTrue(ftp.FtpCreateDirectory(SubDir), "Failed to create subdirectory")

        'Remove directory
        Assert.IsTrue(ftp.FtpDeleteDirectory(SubDir), "Failed to delete subdirectory")

    End Sub

    ''' <summary>
    ''' Test rename of file
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod()> Public Sub RenameFile()
        Dim ftp As New Utilities.FTP.FTPclient(My.Settings.Host, My.Settings.Username, My.Settings.Password)

        'upload image for rename test
        ftp.Upload(imageFile, imageFileTarget)

        'Name to change to
        Dim newName As String = remoteDir & "ImageRenamed.jpg"

        'Attempt rename
        Assert.IsTrue(ftp.FtpRename(imageFileTarget, newName), "Failed to rename file")
        'Double-checks
        Assert.IsTrue(ftp.FtpFileExists(newName), "New file not found")
        Assert.IsFalse(ftp.FtpFileExists(imageFileTarget), "Old file still exists")

        'clean up
        ftp.FtpDelete(newName)
        
    End Sub

#End Region

#Region "Recursion"

    ''' <summary>
    ''' Demonstrate and test recursion
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod()> _
    Public Sub RecursionTest()
        Dim ftp As New Utilities.FTP.FTPclient(My.Settings.Host, My.Settings.Username, My.Settings.Password)

        RecurseDirectories(ftp, "/")

    End Sub

    ''' <summary>
    ''' Recurse directory
    ''' </summary>
    ''' <param name="ftp"></param>
    ''' <param name="dir"></param>
    ''' <remarks></remarks>
    <CLSCompliant(False)> _
    Protected Sub RecurseDirectories(ByVal ftp As Utilities.FTP.FTPclient, ByVal dir As String)
        Dim list As Utilities.FTP.FTPdirectory = ftp.ListDirectoryDetail(dir)

        For Each fi As Utilities.FTP.FTPfileInfo In list.GetFiles
            Console.WriteLine("F: {0}", fi.FullName)
        Next
        For Each fi As Utilities.FTP.FTPfileInfo In list.GetDirectories
            Console.WriteLine("D: {0}", fi.FullName)
            RecurseDirectories(ftp, fi.FullName)
        Next

    End Sub
#End Region

#Region "performance tests: with/without KeepAlive"
    <TestMethod()> Public Sub SpeedTestKeepAliveFALSE()
        Const REPEAT As Integer = 100
        Dim ftp As New Utilities.FTP.FTPclient(My.Settings.Host, My.Settings.Username, My.Settings.Password, False)

        Dim sw As New System.Diagnostics.Stopwatch
        sw.Start()

        Dim target As String = My.Settings.RemoteDir & My.Settings.ImageFile & ".jpg"
        'upload and delete the JPG ten times
        For i As Integer = 1 To REPEAT
            ftp.Upload(imageFile, target)
            ftp.FtpDelete(target)
        Next
        sw.Stop()
        Console.WriteLine("{0} uploads took {1} with KeepAlive=False", REPEAT, sw.Elapsed.ToString)
    End Sub

    <TestMethod()> Public Sub SpeedTestKeepAliveTRUE()
        Const REPEAT As Integer = 100
        Dim ftp As New Utilities.FTP.FTPclient(My.Settings.Host, My.Settings.Username, My.Settings.Password, False)

        Dim sw As New System.Diagnostics.Stopwatch
        sw.Start()

        Dim target As String = My.Settings.RemoteDir & My.Settings.ImageFile & ".jpg"
        'upload and delete the JPG ten times
        For i As Integer = 1 To REPEAT
            ftp.Upload(imageFile, target)
            ftp.FtpDelete(target)
        Next
        sw.Stop()
        Console.WriteLine("{0} uploads took {1} with KeepAlive=True", REPEAT, sw.Elapsed.ToString)
    End Sub


#End Region
End Class
