﻿' ################################################################################
' #                             EMBER MEDIA MANAGER                              #
' ################################################################################
' ################################################################################
' # This file is part of Ember Media Manager.                                    #
' #                                                                              #
' # Ember Media Manager is free software: you can redistribute it and/or modify  #
' # it under the terms of the GNU General Public License as published by         #
' # the Free Software Foundation, either version 3 of the License, or            #
' # (at your option) any later version.                                          #
' #                                                                              #
' # Ember Media Manager is distributed in the hope that it will be useful,       #
' # but WITHOUT ANY WARRANTY; without even the implied warranty of               #
' # MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the                #
' # GNU General Public License for more details.                                 #
' #                                                                              #
' # You should have received a copy of the GNU General Public License            #
' # along with Ember Media Manager.  If not, see <http://www.gnu.org/licenses/>. #
' ################################################################################

Imports EmberAPI
Imports NLog
Imports System.Diagnostics

Public Class dlgDeleteConfirm

#Region "Fields"
    Shared logger As Logger = NLog.LogManager.GetCurrentClassLogger()

    Private PropogatingDown As Boolean = False
    Private PropogatingUp As Boolean = False
    Private _deltype As Enums.DelType

#End Region 'Fields

#Region "Methods"

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()
        Left = Master.AppPos.Left + (Master.AppPos.Width - Width) \ 2
        Top = Master.AppPos.Top + (Master.AppPos.Height - Height) \ 2
        StartPosition = FormStartPosition.Manual
    End Sub

    Public Overloads Function ShowDialog(ByVal ItemsToDelete As Dictionary(Of Long, Long), ByVal DelType As Enums.DelType) As System.Windows.Forms.DialogResult
        _deltype = DelType
        Populate_FileList(ItemsToDelete)
        Return MyBase.ShowDialog
    End Function

    Private Sub AddFileNode(ByVal ParentNode As TreeNode, ByVal item As IO.FileInfo)
        Try
            Dim NewNode As TreeNode = ParentNode.Nodes.Add(item.FullName, item.Name)
            NewNode.Tag = item.FullName
            NewNode.ImageKey = "FILE"
            NewNode.SelectedImageKey = "FILE"
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
            Throw
        End Try
    End Sub

    Private Sub AddFolderNode(ByVal ParentNode As TreeNode, ByVal dir As IO.DirectoryInfo)
        Try
            Dim NewNode As TreeNode = ParentNode.Nodes.Add(dir.FullName, dir.Name)
            NewNode.Tag = dir.FullName
            NewNode.ImageKey = "FOLDER"
            NewNode.SelectedImageKey = "FOLDER"

            If Not Master.SourcesList.Contains(dir.FullName) Then
                'populate all the sub-folders in the folder
                For Each item As IO.DirectoryInfo In dir.GetDirectories
                    AddFolderNode(NewNode, item)
                Next
            End If

            'populate all the files in the folder
            For Each item As IO.FileInfo In dir.GetFiles()
                AddFileNode(NewNode, item)
            Next
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
            Throw
        End Try
    End Sub

    Private Sub btnToggleAllFiles_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnToggleAllFiles.Click
        ToggleAllNodes()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        DialogResult = System.Windows.Forms.DialogResult.Cancel
        Close()
    End Sub

    Private Function DeleteSelectedItems() As Boolean
        Dim result As Boolean = True
        Dim tPair As New KeyValuePair(Of Long, Long)
        Try
            With tvFiles
                If .Nodes.Count = 0 Then Return False

                Using SQLtransaction As SQLite.SQLiteTransaction = Master.DB.MyVideosDBConn.BeginTransaction() 'Only on Batch Mode
                    For Each ItemParentNode As TreeNode In .Nodes
                        Select Case _deltype
                            Case Enums.DelType.Movies
                                Master.DB.DeleteMovieFromDB(Convert.ToInt64(ItemParentNode.Tag), True)
                            Case Enums.DelType.Shows
                                Master.DB.DeleteTVShowFromDB(Convert.ToInt64(ItemParentNode.Tag), True)
                            Case Enums.DelType.Seasons
                                tPair = DirectCast(ItemParentNode.Tag, KeyValuePair(Of Long, Long))
                            Case Enums.DelType.Episodes
                                Master.DB.DeleteTVEpFromDB(Convert.ToInt64(ItemParentNode.Tag), False, True, True)
                        End Select

                        If ItemParentNode.Nodes.Count > 0 Then
                            For Each node As TreeNode In ItemParentNode.Nodes
                                If node.Checked Then
                                    Select Case node.ImageKey
                                        Case "FOLDER"
                                            Dim oDir As New IO.DirectoryInfo(node.Tag.ToString)
                                            If oDir.Exists Then
                                                oDir.Delete(True)
                                                If _deltype = Enums.DelType.Seasons Then Master.DB.DeleteTVSeasonFromDB(tPair.Value, Convert.ToInt32(tPair.Key), True)
                                                Exit For
                                            End If

                                        Case "FILE"
                                            Dim oFile As New IO.FileInfo(node.Tag.ToString)
                                            If oFile.Exists Then
                                                oFile.Delete()
                                                If _deltype = Enums.DelType.Seasons AndAlso Master.eSettings.FileSystemValidExts.Contains(IO.Path.GetExtension(node.Tag.ToString)) Then Master.DB.DeleteTVEpFromDBByPath(node.Tag.ToString, True, True)
                                            End If
                                    End Select

                                End If
                            Next

                        End If

                    Next
                    If _deltype = Enums.DelType.Seasons OrElse _deltype = Enums.DelType.Episodes Then Master.DB.DeleteEmptyTVSeasonsFromDB(True)
                    SQLtransaction.Commit()
                End Using
            End With
            Return result
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
    End Function

    Private Sub dlgDeleteConfirm_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        SetUp()
    End Sub

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        If DeleteSelectedItems() Then
            DialogResult = System.Windows.Forms.DialogResult.OK
        Else
            DialogResult = System.Windows.Forms.DialogResult.Cancel
        End If
        Close()
    End Sub

    Private Sub Populate_FileList(ByVal ItemsToDelete As Dictionary(Of Long, Long))
        Dim hadError As Boolean = False
        Dim ePath As String = String.Empty
        Dim fDeleter As New FileUtils.Delete
        Dim ItemsList As New List(Of IO.FileSystemInfo)
        Dim ItemParentNode As New TreeNode

        Try
            With tvFiles

                Select Case _deltype
                    Case Enums.DelType.Movies
                        For Each MovieId As Long In ItemsToDelete.Keys
                            hadError = False

                            Dim mMovie As Database.DBElement = Master.DB.LoadMovieFromDB(MovieId)

                            ItemParentNode = .Nodes.Add(mMovie.ID.ToString, mMovie.ListTitle)
                            ItemParentNode.ImageKey = "MOVIE"
                            ItemParentNode.SelectedImageKey = "MOVIE"
                            ItemParentNode.Tag = mMovie.ID

                            'get the associated files
                            ItemsList = fDeleter.GetItemsToDelete(False, mMovie)

                            For Each fileItem As IO.FileSystemInfo In ItemsList
                                If Not ItemParentNode.Nodes.ContainsKey(fileItem.FullName) Then
                                    If TypeOf fileItem Is IO.DirectoryInfo Then
                                        Try
                                            AddFolderNode(ItemParentNode, DirectCast(fileItem, IO.DirectoryInfo))
                                        Catch
                                            hadError = True
                                            Exit For
                                        End Try
                                    Else
                                        Try
                                            AddFileNode(ItemParentNode, DirectCast(fileItem, IO.FileInfo))
                                        Catch
                                            hadError = True
                                            Exit For
                                        End Try
                                    End If
                                End If
                            Next

                            If hadError Then .Nodes.Remove(ItemParentNode)
                        Next
                    Case Enums.DelType.Shows
                        For Each ShowID As Long In ItemsToDelete.Keys
                            hadError = False

                            Dim tShow As Database.DBElement = Master.DB.LoadTVShowFromDB(ShowID, False, False)

                            ItemParentNode = .Nodes.Add(ShowID.ToString, tShow.TVShow.Title)
                            ItemParentNode.ImageKey = "MOVIE"
                            ItemParentNode.SelectedImageKey = "MOVIE"
                            ItemParentNode.Tag = tShow.ID

                            Try
                                AddFolderNode(ItemParentNode, New IO.DirectoryInfo(tShow.ShowPath))
                            Catch
                                .Nodes.Remove(ItemParentNode)
                            End Try
                        Next
                    Case Enums.DelType.Seasons


                        Using SQLDelCommand As SQLite.SQLiteCommand = Master.DB.MyVideosDBConn.CreateCommand()
                            For Each Season As KeyValuePair(Of Long, Long) In ItemsToDelete
                                hadError = False

                                Dim tSeason As Database.DBElement = Master.DB.LoadTVSeasonFromDB(Season.Value, Convert.ToInt32(Season.Key), True)

                                ItemParentNode = .Nodes.Add(Season.Key.ToString, String.Format("{0} - {1}", tSeason.TVShow.Title, tSeason.TVSeason.Season))
                                ItemParentNode.ImageKey = "MOVIE"
                                ItemParentNode.SelectedImageKey = "MOVIE"
                                ItemParentNode.Tag = Season

                                SQLDelCommand.CommandText = String.Concat("SELECT idEpisode, idFile FROM episode WHERE idShow = ", Season.Value, " AND Season = ", Season.Key, ";")
                                Using SQLDelReader As SQLite.SQLiteDataReader = SQLDelCommand.ExecuteReader
                                    While SQLDelReader.Read
                                        Using SQLCommand As SQLite.SQLiteCommand = Master.DB.MyVideosDBConn.CreateCommand()
                                            SQLCommand.CommandText = String.Concat("SELECT strFilename FROM files WHERE idFile = ", SQLDelReader("idFile"), ";")
                                            Using SQLReader As SQLite.SQLiteDataReader = SQLCommand.ExecuteReader
                                                If SQLReader.HasRows Then
                                                    SQLReader.Read()
                                                    If Functions.IsSeasonDirectory(IO.Directory.GetParent(SQLReader("strFilename").ToString).FullName) Then
                                                        Try
                                                            AddFolderNode(ItemParentNode, New IO.DirectoryInfo(IO.Directory.GetParent(SQLReader("strFilename").ToString).FullName))
                                                            ePath = IO.Path.Combine(IO.Directory.GetParent(SQLReader("strFilename").ToString).Parent.FullName, String.Format("season{0}.tbn", Season.Key.ToString.PadLeft(2, Convert.ToChar("0"))))
                                                            If IO.File.Exists(ePath) Then AddFileNode(ItemParentNode, New IO.FileInfo(ePath))
                                                            ePath = IO.Path.Combine(IO.Directory.GetParent(SQLReader("strFilename").ToString).Parent.FullName, String.Format("season{0}.tbn", Season.Key.ToString))
                                                            If IO.File.Exists(ePath) Then AddFileNode(ItemParentNode, New IO.FileInfo(ePath))
                                                            ePath = IO.Path.Combine(IO.Directory.GetParent(SQLReader("strFilename").ToString).Parent.FullName, String.Format("season{0}.jpg", Season.Key.ToString.PadLeft(2, Convert.ToChar("0"))))
                                                            If IO.File.Exists(ePath) Then AddFileNode(ItemParentNode, New IO.FileInfo(ePath))
                                                            ePath = IO.Path.Combine(IO.Directory.GetParent(SQLReader("strFilename").ToString).Parent.FullName, String.Format("season{0}.jpg", Season.Key.ToString))
                                                            If IO.File.Exists(ePath) Then AddFileNode(ItemParentNode, New IO.FileInfo(ePath))
                                                        Catch
                                                            .Nodes.Remove(ItemParentNode)
                                                        End Try
                                                        Exit While
                                                    Else
                                                        Try
                                                            ePath = IO.Path.Combine(IO.Directory.GetParent(SQLReader("strFilename").ToString).FullName, IO.Path.GetFileNameWithoutExtension(SQLReader("strFilename").ToString))
                                                            AddFileNode(ItemParentNode, New IO.FileInfo(SQLReader("strFilename").ToString))
                                                            If IO.File.Exists(String.Concat(ePath, ".nfo")) Then AddFileNode(ItemParentNode, New IO.FileInfo(String.Concat(ePath, ".nfo")))
                                                            If IO.File.Exists(String.Concat(ePath, ".tbn")) Then AddFileNode(ItemParentNode, New IO.FileInfo(String.Concat(ePath, ".tbn")))
                                                            If IO.File.Exists(String.Concat(ePath, ".jpg")) Then AddFileNode(ItemParentNode, New IO.FileInfo(String.Concat(ePath, ".jpg")))
                                                            If IO.File.Exists(String.Concat(ePath, "-fanart.jpg")) Then AddFileNode(ItemParentNode, New IO.FileInfo(String.Concat(ePath, "-fanart.jpg")))
                                                            If IO.File.Exists(String.Concat(ePath, ".fanart.jpg")) Then AddFileNode(ItemParentNode, New IO.FileInfo(String.Concat(ePath, ".fanart.jpg")))
                                                            ePath = IO.Path.Combine(IO.Directory.GetParent(SQLReader("strFilename").ToString).FullName, String.Format("season{0}.tbn", Season.Key.ToString.PadLeft(2, Convert.ToChar("0"))))
                                                            If IO.File.Exists(ePath) Then AddFileNode(ItemParentNode, New IO.FileInfo(ePath))
                                                            ePath = IO.Path.Combine(IO.Directory.GetParent(SQLReader("strFilename").ToString).FullName, String.Format("season{0}.tbn", Season.Key.ToString))
                                                            If IO.File.Exists(ePath) Then AddFileNode(ItemParentNode, New IO.FileInfo(ePath))
                                                            ePath = IO.Path.Combine(IO.Directory.GetParent(SQLReader("strFilename").ToString).FullName, String.Format("season{0}.jpg", Season.Key.ToString.PadLeft(2, Convert.ToChar("0"))))
                                                            If IO.File.Exists(ePath) Then AddFileNode(ItemParentNode, New IO.FileInfo(ePath))
                                                            ePath = IO.Path.Combine(IO.Directory.GetParent(SQLReader("strFilename").ToString).FullName, String.Format("season{0}.jpg", Season.Key.ToString))
                                                            If IO.File.Exists(ePath) Then AddFileNode(ItemParentNode, New IO.FileInfo(ePath))
                                                        Catch
                                                            .Nodes.Remove(ItemParentNode)
                                                            Exit While
                                                        End Try
                                                    End If
                                                End If
                                            End Using
                                        End Using
                                    End While
                                End Using
                            Next
                        End Using
                    Case Enums.DelType.Episodes


                        Using SQLCommand As SQLite.SQLiteCommand = Master.DB.MyVideosDBConn.CreateCommand()
                            For Each Ep As Long In ItemsToDelete.Keys
                                hadError = False

                                Dim tEpisode As Database.DBElement = Master.DB.LoadTVEpisodeFromDB(Ep, True)

                                ItemParentNode = .Nodes.Add(Ep.ToString, tEpisode.TVEpisode.Title)
                                ItemParentNode.ImageKey = "MOVIE"
                                ItemParentNode.SelectedImageKey = "MOVIE"
                                ItemParentNode.Tag = Ep

                                SQLCommand.CommandText = String.Concat("SELECT strFilename FROM files WHERE idFile = ", Ep, ";")
                                Using SQLReader As SQLite.SQLiteDataReader = SQLCommand.ExecuteReader
                                    If SQLReader.HasRows Then
                                        SQLReader.Read()
                                        Try
                                            ePath = IO.Path.Combine(IO.Directory.GetParent(SQLReader("strFilename").ToString).FullName, IO.Path.GetFileNameWithoutExtension(SQLReader("strFilename").ToString))
                                            AddFileNode(ItemParentNode, New IO.FileInfo(SQLReader("strFilename").ToString))
                                            If IO.File.Exists(String.Concat(ePath, ".nfo")) Then AddFileNode(ItemParentNode, New IO.FileInfo(String.Concat(ePath, ".nfo")))
                                            If IO.File.Exists(String.Concat(ePath, ".tbn")) Then AddFileNode(ItemParentNode, New IO.FileInfo(String.Concat(ePath, ".tbn")))
                                            If IO.File.Exists(String.Concat(ePath, ".jpg")) Then AddFileNode(ItemParentNode, New IO.FileInfo(String.Concat(ePath, ".jpg")))
                                            If IO.File.Exists(String.Concat(ePath, "-fanart.jpg")) Then AddFileNode(ItemParentNode, New IO.FileInfo(String.Concat(ePath, "-fanart.jpg")))
                                            If IO.File.Exists(String.Concat(ePath, ".fanart.jpg")) Then AddFileNode(ItemParentNode, New IO.FileInfo(String.Concat(ePath, ".fanart.jpg")))
                                        Catch
                                            .Nodes.Remove(ItemParentNode)
                                        End Try
                                    End If
                                End Using
                            Next
                        End Using

                End Select

                'check all the nodes
                For Each node As TreeNode In .Nodes
                    node.Checked = True
                    node.Expand()
                Next

            End With
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
    End Sub

    Private Sub SetUp()
        Text = Master.eLang.GetString(714, "Confirm Items To Be Deleted")
        btnToggleAllFiles.Text = Master.eLang.GetString(715, "Toggle All Files")

        OK_Button.Text = Master.eLang.GetString(179, "OK")
        Cancel_Button.Text = Master.eLang.GetString(167, "Cancel")
    End Sub

    Private Sub ToggleAllNodes()
        Try
            Dim Checked As Nullable(Of Boolean)
            With tvFiles
                If .Nodes.Count = 0 Then Return
                For Each node As TreeNode In .Nodes
                    If Not Checked.HasValue Then
                        'this is the first node of this type, set toggle status based on this
                        Checked = Not node.Checked
                    End If
                    node.Checked = Checked.Value
                Next
            End With
        Catch
            'swallow this - not a critical function
        End Try
    End Sub

    Private Sub tvwFiles_AfterCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles tvFiles.AfterCheck
        Try
            If e.Node.Parent Is Nothing Then
                'this is a movie node
                If PropogatingUp Then Return

                'check/uncheck all children
                PropogatingDown = True
                For Each node As TreeNode In e.Node.Nodes
                    node.Checked = e.Node.Checked
                Next
                PropogatingDown = False
            Else
                'this is a file/folder node
                If e.Node.Checked Then
                    If Not PropogatingUp Then
                        PropogatingDown = True
                        For Each node As TreeNode In e.Node.Nodes
                            node.Checked = True
                        Next
                        PropogatingDown = False
                    End If

                    'if all children are checked then check root node
                    For Each node As TreeNode In e.Node.Parent.Nodes
                        If Not node.Checked Then Return
                    Next
                    PropogatingUp = True
                    e.Node.Parent.Checked = True
                    PropogatingUp = False
                Else
                    If Not PropogatingUp Then
                        'uncheck any children
                        PropogatingDown = True
                        For Each node As TreeNode In e.Node.Nodes
                            node.Checked = False
                        Next
                        PropogatingDown = False
                    End If

                    'make sure parent is no longer checked
                    PropogatingUp = True
                    e.Node.Parent.Checked = False
                    PropogatingUp = False
                End If
            End If
        Catch
            'swallow this - not a critical function
        End Try
    End Sub

    Private Sub tvwFiles_AfterSelect(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles tvFiles.AfterSelect
        Try
            Select Case e.Node.ImageKey
                Case "MOVIE"
                    lblNodeSelected.Text = CType(e.Node.Tag, Database.DBElement).ListTitle
                Case "RECORD"
                    lblNodeSelected.Text = CType(e.Node.Tag, Database.DBElement).ListTitle
                Case "FOLDER"
                    lblNodeSelected.Text = e.Node.Tag.ToString
                Case "FILE"
                    lblNodeSelected.Text = e.Node.Tag.ToString
            End Select
        Catch ex As Exception
            lblNodeSelected.Text = String.Empty
        End Try
    End Sub

#End Region 'Methods

End Class