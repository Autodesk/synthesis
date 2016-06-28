Imports Inventor


Public Class Form1

    Private mApp As Inventor.Application

    'Member variables to keep the RefKey data
    Private oRefKey(1) As Byte
    Private oContextData(1) As Byte


    Public Sub New(ByVal oApp As Inventor.Application)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mApp = oApp

    End Sub


    'Get reference key and context data for a given face of a given given document
    'Document can be either a Part or an Assembly
    Public Sub GetFaceReferenceKey(ByVal oDoc As Document, ByVal oFace As Face, ByRef oRefKey() As Byte, ByRef oContextData() As Byte)

        ' Create a key context (required to obtain ref keys for BRep entities)
        Dim KeyContext As Long = oDoc.ReferenceKeyManager.CreateKeyContext()

        ' Get a reference key for the face
        oFace.GetReferenceKey(oRefKey, KeyContext)

        ' Save KeyContext as a byte array for future use
        oDoc.ReferenceKeyManager.SaveContextToArray(KeyContext, oContextData)

    End Sub


    'Retrieve a Face from its Reference Key and Context Data
    Public Function GetFaceFromReferenceKey(ByVal oDoc As Document, ByVal oRefKey() As Byte, ByVal oContextData() As Byte) As Face

        Try

            ' Retrieve ContextKey from byte array
            Dim oKeyContext As Long = oDoc.ReferenceKeyManager.LoadContextFromArray(oContextData)

            ' Bind reference key to the Face object
            Dim MatchType As Object
            Dim obj As Object = oDoc.ReferenceKeyManager.BindKeyToObject(oRefKey, oKeyContext, MatchType)

            If (TypeOf (obj) Is Face) Then
                Return obj
            End If

            Return Nothing

        Catch ex As Exception

            Return Nothing

        End Try

    End Function


    Public Function GetRefKeyFromSelection() As Boolean

        If (mApp.ActiveDocument Is Nothing) Then
            MsgBox("A document needs to be active")
            Return False
        End If

        If (mApp.ActiveDocument.SelectSet.Count = 1) Then

            If (TypeOf (mApp.ActiveDocument.SelectSet(1)) Is Face) Then

                'Get selected face
                Dim oFace As Face = mApp.ActiveDocument.SelectSet(1)

                GetFaceReferenceKey(mApp.ActiveDocument, oFace, oRefKey, oContextData)
                Return True

            End If

        End If

        MsgBox("A single Face needs to be selected")
        Return False

    End Function


    Public Sub GetObjectFromRefKey()

        If (mApp.ActiveDocument Is Nothing) Then
            MsgBox("A document needs to be active")
            Return
        End If

        Dim oFace As Face = GetFaceFromReferenceKey(mApp.ActiveDocument, oRefKey, oContextData)

        If (Not oFace Is Nothing) Then
            mApp.ActiveDocument.SelectSet.Select(oFace)
        End If

    End Sub

    'Save a reference key data on the disk
    Public Sub SaveRefKey()

        Dim sfd As New System.Windows.Forms.SaveFileDialog

        sfd.InitialDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location
        sfd.FileName = "FaceRefkey"

        sfd.Filter = "RefKeyData (*.ref) |*.ref;"

        If sfd.ShowDialog() = Windows.Forms.DialogResult.OK Then

            Dim fs As System.IO.FileStream = System.IO.File.Create(sfd.FileName)
            Dim bw As System.IO.BinaryWriter = New System.IO.BinaryWriter(fs)

            bw.Write(oContextData.Length)
            bw.Write(oContextData)

            bw.Write(oRefKey.Length)
            bw.Write(oRefKey)

            bw.Close()
            fs.Close()

        End If

    End Sub

    'Load a reference key data from the disk
    Public Function LoadRefKey() As Boolean

        Dim ofd As New System.Windows.Forms.OpenFileDialog

        ofd.InitialDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location
        ofd.Filter = "RefKeyData (*.ref) |*.ref;"

        If ofd.ShowDialog() = Windows.Forms.DialogResult.OK Then

            Dim fs As System.IO.FileStream = System.IO.File.OpenRead(ofd.FileName)
            Dim br As System.IO.BinaryReader = New System.IO.BinaryReader(fs)

            Dim count As Integer

            count = br.ReadInt32()
            oContextData = br.ReadBytes(count)

            count = br.ReadInt32()
            oRefKey = br.ReadBytes(count)

            br.Close()
            fs.Close()

            Return True

        End If

        Return False

    End Function


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        If (GetRefKeyFromSelection() = True) Then

            SaveRefKey()

        End If

    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click

        If (LoadRefKey() = True) Then

            GetObjectFromRefKey()

        End If

    End Sub


End Class