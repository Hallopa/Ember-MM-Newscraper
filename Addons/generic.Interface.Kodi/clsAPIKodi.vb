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

Imports NLog
Imports EmberAPI
Imports XBMCRPC
Imports System.IO
Imports generic.Interface.Kodi.KodiInterface

Namespace Kodi

    Public Class APIKodi

#Region "Fields"

        Shared logger As Logger = LogManager.GetCurrentClassLogger()
        'current selected host, Kodi Host type already declared in EmberAPI (XML serialization) -> no MySettings declaration needed here
        Private _currenthost As New KodiInterface.Host
        'current selected client
        Private _kodi As Client
        'helper object, needed for communication client (notification, eventhandler support)
        Private platformServices As IPlatformServices = New PlatformServices
        'Private NotificationsEnabled As Boolean


#End Region 'Fields

#Region "Events"

#End Region 'Events

#Region "Methods"
        ''' <summary>
        ''' Initialize Communication Client for ONE Kodi Host
        ''' </summary>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' </remarks>
        Public Sub New(ByVal host As KodiInterface.Host)
            _currenthost = Nothing
            _currenthost = host
            Init()
        End Sub
        ''' <summary>
        ''' Initialize API class (host)
        ''' </summary>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' </remarks>
        Friend Sub Init()
            'dispose old client before initalizing new ones (just to be safe) 
            If _kodi IsNot Nothing Then
                _kodi.Dispose()
            End If
            'now initialize new client object
            _kodi = New Client(platformServices, _currenthost.Address, _currenthost.Port, _currenthost.Username, _currenthost.Password)
            'Listen to Kodi Events
            'AddHandler _kodi.VideoLibrary.OnScanFinished, AddressOf VideoLibrary_OnScanFinished
            'AddHandler _kodi.VideoLibrary.OnCleanFinished, AddressOf VideoLibrary_OnCleanFinished
            '_kodi.StartNotificationListener()
        End Sub
        ''' <summary>
        ''' Get all movies from Kodi host
        ''' </summary>
        ''' <returns>list of kodi movies, Nothing: error</returns>
        ''' <remarks></remarks>
        Private Async Function GetAllMovies() As Task(Of VideoLibrary.GetMoviesResponse)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] GetAllMovies: No host initialized! Abort!")
                Return Nothing
            End If

            Try
                Dim response = Await _kodi.VideoLibrary.GetMovies(Video.Fields.Movie.AllFields).ConfigureAwait(False)
                Return response
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Async Function GetAllMovieSets() As Task(Of VideoLibrary.GetMovieSetsResponse)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] GetAllMovieSets: No host initialized! Abort!")
                Return Nothing
            End If

            Try
                Dim response = Await _kodi.VideoLibrary.GetMovieSets(Video.Fields.MovieSet.AllFields).ConfigureAwait(False)
                Return response
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return Nothing
            End Try
        End Function

        Private Async Function GetAllTVSeasons(ByVal ShowID As Integer) As Task(Of VideoLibrary.GetSeasonsResponse)
            If _kodi Is Nothing Then
                logger.Warn("[APIKodi] GetAllMovieSets: No host initialized! Abort!")
                Return Nothing
            End If

            Try
                Dim response = Await _kodi.VideoLibrary.GetSeasons(ShowID, Video.Fields.Season.AllFields).ConfigureAwait(False)
                Return response
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Get all tvshows from Kodi host
        ''' </summary>
        ''' <returns>list of kodi tv shows, Nothing: error</returns>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' Notice: No exception handling here because this function is called/nested in other functions and an exception must not be consumed (meaning a disconnect host would not be recognized at once)
        ''' </remarks>
        Private Async Function GetAllTVShows() As Task(Of VideoLibrary.GetTVShowsResponse)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] GetAllTVShows: No host initialized! Abort!")
                Return Nothing
            End If

            Try
                Dim response = Await _kodi.VideoLibrary.GetTVShows(Video.Fields.TVShow.AllFields).ConfigureAwait(False)
                Return response
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Get full details of a Movie by ID
        ''' </summary>
        ''' <param name="iKodiID"></param>
        ''' <returns></returns>
        Private Async Function GetFullDetailsByID_Movie(ByVal iKodiID As Integer) As Task(Of Video.Details.Movie)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] GetFullDetailsByID_Movie: No host initialized! Abort!")
                Return Nothing
            End If

            If Not iKodiID = -1 Then
                Try
                    Dim KodiElement As VideoLibrary.GetMovieDetailsResponse = Await _kodi.VideoLibrary.GetMovieDetails(iKodiID, Video.Fields.Movie.AllFields).ConfigureAwait(False)
                    If KodiElement IsNot Nothing AndAlso KodiElement.moviedetails IsNot Nothing Then Return KodiElement.moviedetails
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name, ex)
                    Return Nothing
                End Try
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' Get full details of a MovieSet by ID
        ''' </summary>
        ''' <param name="iKodiID"></param>
        ''' <returns></returns>
        Private Async Function GetFullDetailsByID_MovieSet(ByVal iKodiID As Integer) As Task(Of Video.Details.MovieSet)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] GetFullDetailsByID_MovieSet: No host initialized! Abort!")
                Return Nothing
            End If

            If Not iKodiID = -1 Then
                Try
                    Dim KodiElement As VideoLibrary.GetMovieSetDetailsResponse = Await _kodi.VideoLibrary.GetMovieSetDetails(iKodiID, Video.Fields.MovieSet.AllFields).ConfigureAwait(False)
                    If KodiElement IsNot Nothing AndAlso KodiElement.setdetails IsNot Nothing Then Return KodiElement.setdetails
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name, ex)
                    Return Nothing
                End Try
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' Get full details of a MovieSet by ID
        ''' </summary>
        ''' <param name="iKodiID"></param>
        ''' <returns></returns>
        Private Async Function GetFullDetailsByID_TVEpisode(ByVal iKodiID As Integer) As Task(Of Video.Details.Episode)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] GetFullDetailsByID_TVEpisode: No host initialized! Abort!")
                Return Nothing
            End If

            If Not iKodiID = -1 Then
                Try
                    Dim KodiElement As VideoLibrary.GetEpisodeDetailsResponse = Await _kodi.VideoLibrary.GetEpisodeDetails(iKodiID, Video.Fields.Episode.AllFields).ConfigureAwait(False)
                    If KodiElement IsNot Nothing AndAlso KodiElement.episodedetails IsNot Nothing Then Return KodiElement.episodedetails
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name, ex)
                    Return Nothing
                End Try
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' Get full details of a MovieSet by ID
        ''' </summary>
        ''' <param name="iKodiID"></param>
        ''' <returns></returns>
        Private Async Function GetFullDetailsByID_TVSeason(ByVal iKodiID As Integer) As Task(Of Video.Details.Season)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] GetFullDetailsByID_TVSeason: No host initialized! Abort!")
                Return Nothing
            End If

            If Not iKodiID = -1 Then
                Try
                    Dim KodiElement As VideoLibrary.GetSeasonDetailsResponse = Await _kodi.VideoLibrary.GetSeasonDetails(iKodiID, Video.Fields.Season.AllFields).ConfigureAwait(False)
                    If KodiElement IsNot Nothing AndAlso KodiElement.seasondetails IsNot Nothing Then Return KodiElement.seasondetails
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name, ex)
                    Return Nothing
                End Try
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' Get full details of a MovieSet by ID
        ''' </summary>
        ''' <param name="iKodiID"></param>
        ''' <returns></returns>
        Private Async Function GetFullDetailsByID_TVShow(ByVal iKodiID As Integer) As Task(Of Video.Details.TVShow)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] GetFullDetailsByID_TVShow: No host initialized! Abort!")
                Return Nothing
            End If

            If Not iKodiID = -1 Then
                Try
                    Dim KodiElement As VideoLibrary.GetTVShowDetailsResponse = Await _kodi.VideoLibrary.GetTVShowDetails(iKodiID, Video.Fields.TVShow.AllFields).ConfigureAwait(False)
                    If KodiElement IsNot Nothing AndAlso KodiElement.tvshowdetails IsNot Nothing Then Return KodiElement.tvshowdetails
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name, ex)
                    Return Nothing
                End Try
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' Get JSONRPC version of host
        ''' </summary>
        ''' <param name="kHost">specific host to query</param>
        ''' <remarks>
        ''' 2015/06/29 Cocotus - First implementation
        ''' </remarks>
        Public Shared Function GetHostJSONVersion(ByVal kHost As KodiInterface.Host) As String
            Try
                Dim _APIKodi As New Kodi.APIKodi(kHost)
                Return _APIKodi.GetHostJSONVersion.Result
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return String.Empty
            End Try
        End Function
        ''' <summary>
        ''' Get JSON RPC version of host
        ''' </summary>
        ''' <returns>string which contains exact JSONRPC version, Nothing: Empty string</returns>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' </remarks>
        Private Async Function GetHostJSONVersion() As Task(Of String)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] GetHostJSONVersion: No host initialized! Abort!")
                Return Nothing
            End If

            Try
                Dim response = Await _kodi.JSONRPC.Version.ConfigureAwait(False)
                Dim codename As String = ""
                'see codename table here: http://kodi.wiki/view/JSON-RPC_API
                Select Case response.version.major.ToString & response.version.minor
                    Case "2"
                        codename = "Dharma "
                    Case "4"
                        codename = "Eden "
                    Case "60"
                        codename = "Frodo "
                    Case "614"
                        codename = "Gotham "
                    Case "621"
                        codename = "Helix "
                    Case "625"
                        codename = "Isengard "
                End Select
                Return codename & response.version.major.ToString & "." & response.version.minor
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return ""
            End Try
        End Function
        ''' <summary>
        ''' Search movie ID in Kodi database
        ''' </summary>
        ''' <param name="tDBElement"></param>
        ''' <returns></returns>
        Private Async Function GetMediaID(ByVal tDBElement As Database.DBElement) As Task(Of Integer)
            Dim KodiID As Integer = -1

            Select Case tDBElement.ContentType
                Case Enums.ContentType.Movie
                    Dim KodiMovie As Video.Details.Movie = Await SearchMovie(tDBElement).ConfigureAwait(False)
                    If KodiMovie IsNot Nothing Then Return KodiMovie.movieid
                Case Enums.ContentType.MovieSet
                    Dim KodiMovieset As Video.Details.MovieSet = Await SearchMovieSet(tDBElement).ConfigureAwait(False)
                    If KodiMovieset IsNot Nothing Then Return KodiMovieset.setid
                Case Enums.ContentType.TVEpisode
                    Dim KodiEpsiode As Video.Details.Episode = Await SearchTVEpisode(tDBElement).ConfigureAwait(False)
                    If Not KodiEpsiode Is Nothing Then Return KodiEpsiode.episodeid
                Case Enums.ContentType.TVSeason
                    Dim KodiSeason As Video.Details.Season = Await SearchTVSeason(tDBElement).ConfigureAwait(False)
                    If Not KodiSeason Is Nothing Then Return KodiSeason.seasonid
                Case Enums.ContentType.TVShow
                    Dim KodiTVShow As Video.Details.TVShow = Await SearchTVShow(tDBElement).ConfigureAwait(False)
                    If Not KodiTVShow Is Nothing Then Return KodiTVShow.tvshowid
            End Select

            Return -1
        End Function

        Private Function GetPlayCount() As Boolean
            Return True
        End Function

        Public Shared Function GetPathAndFilename(ByVal tDBElement As Database.DBElement, Optional ByVal tForcedContentType As Enums.ContentType = Enums.ContentType.None) As PathAndFilename
            Dim tPathAndFilename As New PathAndFilename With {.strFilename = String.Empty, .strPath = String.Empty}
            Dim tContentType As Enums.ContentType = If(tForcedContentType = Enums.ContentType.None, tDBElement.ContentType, tForcedContentType)

            Select Case tContentType
                Case Enums.ContentType.Movie, Enums.ContentType.TVEpisode
                    If FileUtils.Common.isVideoTS(tDBElement.Filename) Then
                        'Kodi needs the VIDEO_TS folder path
                        tPathAndFilename.strFilename = Path.GetFileName(tDBElement.Filename)
                        tPathAndFilename.strPath = Directory.GetParent(tDBElement.Filename).FullName
                    ElseIf FileUtils.Common.isBDRip(tDBElement.Filename) Then
                        'Kodi needs the BDMV folder path and index.bdmv as filename
                        Dim lFi As New List(Of FileInfo)
                        Dim di As DirectoryInfo = New DirectoryInfo(Directory.GetParent(Directory.GetParent(tDBElement.Filename).FullName).FullName)
                        lFi.AddRange(di.GetFiles)
                        For Each tFile In lFi
                            If tFile.Name.ToLower = "index.bdmv" Then
                                tPathAndFilename.strFilename = tFile.Name
                                Exit For
                            End If
                        Next
                        tPathAndFilename.strPath = Directory.GetParent(Directory.GetParent(tDBElement.Filename).FullName).FullName
                    Else
                        tPathAndFilename.strFilename = Path.GetFileName(tDBElement.Filename)
                        tPathAndFilename.strPath = Directory.GetParent(tDBElement.Filename).FullName
                    End If
                Case Enums.ContentType.TVShow
                    tPathAndFilename.strPath = tDBElement.ShowPath
            End Select

            If tPathAndFilename.strPath.Contains(Path.DirectorySeparatorChar) AndAlso Not tPathAndFilename.strPath.EndsWith(Path.DirectorySeparatorChar) Then
                tPathAndFilename.strPath = String.Concat(tPathAndFilename.strPath, Path.DirectorySeparatorChar)
            ElseIf tPathAndFilename.strPath.Contains(Path.AltDirectorySeparatorChar) AndAlso Not tPathAndFilename.strPath.EndsWith(Path.AltDirectorySeparatorChar) Then
                tPathAndFilename.strPath = String.Concat(tPathAndFilename.strPath, Path.AltDirectorySeparatorChar)
            End If

            Return tPathAndFilename
        End Function
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="LocalPath"></param>
        ''' <returns></returns>
        ''' <remarks>ATTENTION: It's not allowed to use "Remotepath.ToLower" (Kodi can't find UNC sources with wrong case)</remarks>
        Private Function GetRemotePath(ByVal strLocalPath As String) As String
            Dim strRemotePath As String = String.Empty
            Dim bRemoteIsUNC As Boolean = False

            For Each Source In _currenthost.Sources
                Dim tLocalSource As String = String.Empty
                'add a directory separator at the end of the path to distinguish between
                'D:\Movies
                'D:\Movies Shared
                '(needed for "LocalPath.ToLower.StartsWith(tLocalSource)"
                If Source.LocalPath.Contains(Path.DirectorySeparatorChar) Then
                    tLocalSource = If(Source.LocalPath.EndsWith(Path.DirectorySeparatorChar), Source.LocalPath, String.Concat(Source.LocalPath, Path.DirectorySeparatorChar)).Trim
                ElseIf Source.LocalPath.Contains(Path.AltDirectorySeparatorChar) Then
                    tLocalSource = If(Source.LocalPath.EndsWith(Path.AltDirectorySeparatorChar), Source.LocalPath, String.Concat(Source.LocalPath, Path.AltDirectorySeparatorChar)).Trim
                End If
                If strLocalPath.ToLower.StartsWith(tLocalSource.ToLower) Then
                    Dim tRemoteSource As String = String.Empty
                    If Source.RemotePath.Contains(Path.DirectorySeparatorChar) Then
                        tRemoteSource = If(Source.RemotePath.EndsWith(Path.DirectorySeparatorChar), Source.RemotePath, String.Concat(Source.RemotePath, Path.DirectorySeparatorChar)).Trim
                    ElseIf Source.RemotePath.Contains(Path.AltDirectorySeparatorChar) Then
                        tRemoteSource = If(Source.RemotePath.EndsWith(Path.AltDirectorySeparatorChar), Source.RemotePath, String.Concat(Source.RemotePath, Path.AltDirectorySeparatorChar)).Trim
                        bRemoteIsUNC = True
                    End If
                    strRemotePath = strLocalPath.ToLower.Replace(tLocalSource.ToLower, tRemoteSource)
                    If bRemoteIsUNC Then
                        strRemotePath = strRemotePath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    Else
                        strRemotePath = strRemotePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                    End If
                    Exit For
                End If
            Next

            If String.IsNullOrEmpty(strRemotePath) Then logger.Error(String.Format("[APIKodi] [{0}] GetRemotePath: ""{1}"" | Source not mapped!", _currenthost.Label, strLocalPath))

            Return strRemotePath
        End Function
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="LocalPath"></param>
        ''' <returns></returns>
        ''' <remarks>ATTENTION: It's not allowed to use "Remotepath.ToLower" (Kodi can't find UNC sources with wrong case)</remarks>
        Private Function GetRemotePath_MovieSet(ByVal strLocalPath As String) As String
            Dim HostPath As String = _currenthost.MovieSetArtworksPath
            Dim RemotePath As String = String.Empty
            Dim RemoteIsUNC As Boolean = False

            For Each Source In Master.eSettings.GetMovieSetsArtworkPaths()
                Dim tLocalSource As String = String.Empty
                'add a directory separator at the end of the path to distinguish between
                'D:\MovieSetsArtwork
                'D:\MovieSetsArtwork Shared
                '(needed for "LocalPath.ToLower.StartsWith(tLocalSource)"
                If Source.Contains(Path.DirectorySeparatorChar) Then
                    tLocalSource = If(Source.EndsWith(Path.DirectorySeparatorChar), Source, String.Concat(Source, Path.DirectorySeparatorChar)).Trim
                ElseIf Source.Contains(Path.AltDirectorySeparatorChar) Then
                    tLocalSource = If(Source.EndsWith(Path.AltDirectorySeparatorChar), Source, String.Concat(Source, Path.AltDirectorySeparatorChar)).Trim
                End If
                If strLocalPath.ToLower.StartsWith(tLocalSource.ToLower) Then
                    Dim tRemoteSource As String = String.Empty
                    If HostPath.Contains(Path.DirectorySeparatorChar) Then
                        tRemoteSource = If(HostPath.EndsWith(Path.DirectorySeparatorChar), HostPath, String.Concat(HostPath, Path.DirectorySeparatorChar)).Trim
                    ElseIf HostPath.Contains(Path.AltDirectorySeparatorChar) Then
                        tRemoteSource = If(HostPath.EndsWith(Path.AltDirectorySeparatorChar), HostPath, String.Concat(HostPath, Path.AltDirectorySeparatorChar)).Trim
                        RemoteIsUNC = True
                    End If
                    RemotePath = strLocalPath.Replace(tLocalSource, tRemoteSource)
                    If RemoteIsUNC Then
                        RemotePath = RemotePath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    Else
                        RemotePath = RemotePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                    End If
                    Exit For
                End If
            Next

            Return RemotePath
        End Function
        ''' <summary>
        ''' Get all video sources configured in host
        ''' </summary>
        ''' <param name="kHost">specific host to query</param>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' Called from dlgHost.vb when user hits "Populate" button to get host sources
        ''' </remarks>
        Public Shared Function GetSources(ByVal kHost As KodiInterface.Host) As List(Of List.Items.SourcesItem)
            Dim listSources As New List(Of List.Items.SourcesItem)
            Try
                Dim _APIKodi As New Kodi.APIKodi(kHost)
                listSources = _APIKodi.GetSources(Files.Media.video).Result
                Return listSources
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
            End Try
            Return listSources
        End Function
        ''' <summary>
        ''' Get all sources configured in Kodi host
        ''' </summary>
        ''' <param name="mediaType">type of source (default: video)</param>
        ''' <returns>list of sources</returns>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' 2015/07/05 Cocotus - Added multipath support, i.e nfs://192.168.2.200/Media_1/Movie/nfs://192.168.2.200/Media_2/Movie/
        ''' </remarks>
        Private Async Function GetSources(mediaType As Files.Media) As Task(Of List(Of List.Items.SourcesItem))
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] GetSources: No host initialized! Abort!")
                Return Nothing
            End If

            Try
                Dim response = Await _kodi.Files.GetSources(mediaType).ConfigureAwait(False)
                Dim tmplist = response.sources.ToList

                'type multipath sources contain multiple paths
                Dim lstremotesources As New List(Of List.Items.SourcesItem)
                Dim paths As New List(Of String)
                Const MultiPath As String = "multipath://"
                For Each remotesource In tmplist
                    Dim newsource As New List.Items.SourcesItem
                    If remotesource.file.StartsWith(MultiPath) Then
                        logger.Warn("[APIKodi] GetSources: " & _currenthost.Label & ": " & remotesource.file & " - Multipath format, try to split...")
                        'remove "multipath://" from path and split on "/"
                        'i.e multipath://nfs%3a%2f%2f192.168.2.200%2fMedia_1%2fMovie%2f/nfs%3a%2f%2f192.168.2.200%2fMedia_2%2fMovie%2f/
                        For Each path As String In remotesource.file.Remove(0, MultiPath.Length).Split("/"c)
                            If Not String.IsNullOrEmpty(path) Then
                                newsource = New List.Items.SourcesItem
                                'URL decode each item
                                newsource.file = Web.HttpUtility.UrlDecode(path)
                                newsource.label = remotesource.label
                                lstremotesources.Add(newsource)
                            End If
                        Next
                    Else
                        logger.Warn("[APIKodi] GetSources: " & _currenthost.Label & ": """ & remotesource.file, """")
                        newsource.file = remotesource.file
                        newsource.label = remotesource.label
                        lstremotesources.Add(newsource)
                    End If
                Next
                Return lstremotesources
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return Nothing
            End Try
        End Function

        Private Async Function GetTextures(ByVal tDBElement As Database.DBElement) As Task(Of Textures.GetTexturesResponse)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] GetTextures: No host initialized! Abort!")
                Return Nothing
            End If

            Dim lImagesToRemove As New List(Of String)

            With tDBElement.ImagesContainer
                If .Banner.LocalFilePathSpecified Then lImagesToRemove.Add(.Banner.LocalFilePath)
                If .CharacterArt.LocalFilePathSpecified Then lImagesToRemove.Add(.CharacterArt.LocalFilePath)
                If .ClearArt.LocalFilePathSpecified Then lImagesToRemove.Add(.ClearArt.LocalFilePath)
                If .ClearLogo.LocalFilePathSpecified Then lImagesToRemove.Add(.ClearLogo.LocalFilePath)
                If .DiscArt.LocalFilePathSpecified Then lImagesToRemove.Add(.DiscArt.LocalFilePath)
                If .Fanart.LocalFilePathSpecified Then lImagesToRemove.Add(.Fanart.LocalFilePath)
                If .Landscape.LocalFilePathSpecified Then lImagesToRemove.Add(.Landscape.LocalFilePath)
                If .Poster.LocalFilePathSpecified Then lImagesToRemove.Add(.Poster.LocalFilePath)
            End With

            Select Case tDBElement.ContentType
                Case Enums.ContentType.Movie
                    For Each tActor As MediaContainers.Person In tDBElement.Movie.Actors.Where(Function(f) f.LocalFilePathSpecified)
                        lImagesToRemove.Add(tActor.LocalFilePath)
                    Next
                Case Enums.ContentType.TVEpisode
                    For Each tActor As MediaContainers.Person In tDBElement.TVEpisode.Actors.Where(Function(f) f.LocalFilePathSpecified)
                        lImagesToRemove.Add(tActor.LocalFilePath)
                    Next
                Case Enums.ContentType.TVShow
                    For Each tActor As MediaContainers.Person In tDBElement.TVShow.Actors.Where(Function(f) f.LocalFilePathSpecified)
                        lImagesToRemove.Add(tActor.LocalFilePath)
                    Next
            End Select

            If lImagesToRemove.Count > 0 Then
                Try
                    Dim filter As New List.Filter.TexturesOr With {.or = New List(Of Object)}
                    For Each tURL As String In lImagesToRemove
                        Dim filterRule As New List.Filter.Rule.Textures
                        filterRule.field = List.Filter.Fields.Textures.url
                        filterRule.Operator = List.Filter.Operators.Is
                        filterRule.value = If(tDBElement.ContentType = Enums.ContentType.MovieSet, GetRemotePath_MovieSet(tURL), GetRemotePath(tURL))
                        filter.or.Add(filterRule)
                    Next

                    'TODO: JSON is limited to 20k characters. We have to split the filter if there are to many actor thumbs
                    Dim response As Textures.GetTexturesResponse = Await _kodi.Textures.GetTextures(filter, Textures.Fields.Texture.AllFields)
                    Return response
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name, ex)
                    Return Nothing
                End Try
            End If
            Return Nothing
        End Function

        Private Async Function SearchMovie(ByVal tDBElement As Database.DBElement) As Task(Of Video.Details.Movie)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] SearchMovie: No host initialized! Abort!")
                Return Nothing
            End If

            Dim kMovies As VideoLibrary.GetMoviesResponse

            Dim tPathAndFilename As PathAndFilename = GetPathAndFilename(tDBElement)
            Dim strFilename As String = tPathAndFilename.strFilename
            Dim strRemotePath As String = GetRemotePath(tPathAndFilename.strPath)

            If Not String.IsNullOrEmpty(strRemotePath) Then
                Try
                    Dim filter As New List.Filter.MoviesAnd With {.and = New List(Of Object)}
                    Dim filterRule_Path As New List.Filter.Rule.Movies
                    filterRule_Path.field = List.Filter.Fields.Movies.path
                    filterRule_Path.Operator = List.Filter.Operators.Is
                    filterRule_Path.value = strRemotePath
                    filter.and.Add(filterRule_Path)
                    Dim filterRule_Filename As New List.Filter.Rule.Movies
                    filterRule_Filename.field = List.Filter.Fields.Movies.filename
                    filterRule_Filename.Operator = List.Filter.Operators.Is
                    filterRule_Filename.value = strFilename
                    filter.and.Add(filterRule_Filename)

                    kMovies = Await _kodi.VideoLibrary.GetMovies(filter).ConfigureAwait(False)
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name, ex)
                    Return Nothing
                End Try
            Else
                logger.Error(String.Format("[APIKodi] [{0}] SearchMovie: ""{1}"" | Source not mapped!", _currenthost.Label, tDBElement.Source.Path))
                Return Nothing
            End If

            If kMovies IsNot Nothing Then
                If kMovies.movies IsNot Nothing Then
                    If kMovies.movies.Count = 1 Then
                        logger.Trace(String.Format("[APIKodi] [{0}] SearchMovie: ""{1}"" | OK, found in host database! [ID:{2}]", _currenthost.Label, tDBElement.Filename, kMovies.movies.Item(0).movieid))
                        Return kMovies.movies.Item(0)
                    ElseIf kMovies.movies.Count > 1 Then
                        logger.Warn(String.Format("[APIKodi] [{0}] SearchMovie: ""{1}"" | MORE THAN ONE movie found in host database!", _currenthost.Label, tDBElement.Filename))
                        Return Nothing
                    Else
                        logger.Warn(String.Format("[APIKodi] [{0}] SearchMovie: ""{1}"" | NOT found in host database!", _currenthost.Label, tDBElement.Filename))
                        Return Nothing
                    End If
                Else
                    logger.Warn(String.Format("[APIKodi] [{0}] SearchMovie: ""{1}"" | NOT found in host database!", _currenthost.Label, tDBElement.Filename))
                    Return Nothing
                End If
            Else
                logger.Error(String.Format("[APIKodi] [{0}] SearchMovie: ""{1}"" | No connection to Host!", _currenthost.Label, tDBElement.Filename))
                Return Nothing
            End If
        End Function

        Private Async Function SearchMovieSet(ByVal tDBElement As Database.DBElement) As Task(Of Video.Details.MovieSet)
            'get a list of all moviesets saved in Kodi DB
            Dim kMovieSets As VideoLibrary.GetMovieSetsResponse = Await GetAllMovieSets().ConfigureAwait(False)

            If kMovieSets IsNot Nothing Then
                If kMovieSets.sets IsNot Nothing Then

                    'compare by movieset title
                    For Each tMovieSet In kMovieSets.sets
                        If tMovieSet.title.ToLower = tDBElement.MovieSet.Title.ToLower Then
                            logger.Trace(String.Format("[APIKodi] [{0}] SearchMovieSetByDetails: ""{1}"" | OK, found in host database! [ID:{2}]", _currenthost.Label, tDBElement.MovieSet.Title, tMovieSet.setid))
                            Return tMovieSet
                        End If
                    Next

                    'compare by movies inside movieset
                    For Each tMovie In tDBElement.MovieList
                        logger.Trace(String.Format("[APIKodi] [{0}] SearchMovieSetByDetails: ""{1}"" | NOT found in database, trying to find the movieset by movies...", _currenthost.Label, tDBElement.MovieSet.Title))
                        'search movie ID in Kodi DB
                        Dim MovieID As Integer = -1
                        Dim KodiMovie = Await SearchMovie(tMovie).ConfigureAwait(False)
                        If KodiMovie IsNot Nothing Then
                            For Each tMovieSet In kMovieSets.sets
                                If tMovieSet.setid = KodiMovie.setid Then
                                    logger.Trace(String.Format("[APIKodi] [{0}] SearchMovieSetByDetails: ""{1}"" | OK, found in host database by movie ""{2}""! [ID:{3}]", _currenthost.Label, tDBElement.MovieSet.Title, KodiMovie.title, tMovieSet.setid))
                                    Return tMovieSet
                                End If
                            Next
                        End If
                    Next

                    logger.Warn(String.Format("[APIKodi] [{0}] SearchMovieSetByDetails: ""{1}"" | NOT found in host database!", _currenthost.Label, tDBElement.MovieSet.Title))
                    Return Nothing
                Else
                    logger.Warn(String.Format("[APIKodi] [{0}] SearchMovieSetByDetails: ""{1}"" | NOT found in host database!", _currenthost.Label, tDBElement.MovieSet.Title))
                    Return Nothing
                End If
            Else
                logger.Error(String.Format("[APIKodi] [{0}] SearchMovieSetByDetails: ""{1}"" | No connection to Host!", _currenthost.Label, tDBElement.MovieSet.Title))
                Return Nothing
            End If
        End Function

        Private Async Function SearchTVEpisode(ByVal tDBElement As Database.DBElement) As Task(Of Video.Details.Episode)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] SearchTVEpisode: No host initialized! Abort!")
                Return Nothing
            End If

            Dim KodiTVShow = Await SearchTVShow(tDBElement).ConfigureAwait(False)
            Dim ShowID As Integer = -1
            If KodiTVShow IsNot Nothing Then
                ShowID = KodiTVShow.tvshowid
            End If
            If ShowID = -1 Then
                logger.Warn(String.Format("[APIKodi] [{0}] SearchTVEpisode: ""{1}"" | TV Show NOT found in host database!", _currenthost.Label, tDBElement.ShowPath))
                Return Nothing
            End If

            Dim kTVEpisodes As VideoLibrary.GetEpisodesResponse

            Dim tPathAndFilename As PathAndFilename = GetPathAndFilename(tDBElement)
            Dim strFilename As String = tPathAndFilename.strFilename
            Dim strRemotePath As String = GetRemotePath(tPathAndFilename.strPath)

            If Not String.IsNullOrEmpty(strRemotePath) Then
                Try
                    Dim filter As New List.Filter.EpisodesAnd With {.and = New List(Of Object)}
                    Dim filterRule_Path As New List.Filter.Rule.Episodes
                    filterRule_Path.field = List.Filter.Fields.Episodes.path
                    filterRule_Path.Operator = List.Filter.Operators.Is
                    filterRule_Path.value = strRemotePath
                    filter.and.Add(filterRule_Path)
                    Dim filterRule_Filename As New List.Filter.Rule.Episodes
                    filterRule_Filename.field = List.Filter.Fields.Episodes.filename
                    filterRule_Filename.Operator = List.Filter.Operators.Is
                    filterRule_Filename.value = Path.GetFileName(strFilename)
                    filter.and.Add(filterRule_Filename)

                    kTVEpisodes = Await _kodi.VideoLibrary.GetEpisodes(filter, ShowID, tDBElement.TVEpisode.Season, Video.Fields.Episode.AllFields).ConfigureAwait(False)
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name, ex)
                    Return Nothing
                End Try
            Else
                logger.Error(String.Format("[APIKodi] [{0}] SearchTVEpisode: ""{1}"" | Source not mapped!", _currenthost.Label, tDBElement.Source.Path))
                Return Nothing
            End If

            If kTVEpisodes IsNot Nothing Then
                If kTVEpisodes.episodes IsNot Nothing Then
                    If kTVEpisodes.episodes.Count = 1 Then
                        logger.Trace(String.Format("[APIKodi] [{0}] SearchTVEpisode: ""{1}"" | OK, found in host database! [ID:{2}]", _currenthost.Label, tDBElement.Filename, kTVEpisodes.episodes.Item(0).episodeid))
                        Return kTVEpisodes.episodes.Item(0)
                    ElseIf kTVEpisodes.episodes.Count > 1 Then
                        'try to filter MultiEpisode files
                        Dim sEpisode = kTVEpisodes.episodes.Where(Function(f) f.episode = tDBElement.TVEpisode.Episode)
                        If sEpisode.Count = 1 Then
                            Return sEpisode(0)
                        ElseIf sEpisode.Count > 1 Then
                            logger.Warn(String.Format("[APIKodi] [{0}] SearchTVEpisode: ""{1}"" | MORE THAN ONE episode found in host database!", _currenthost.Label, tDBElement.Filename))
                            Return Nothing
                        Else
                            logger.Warn(String.Format("[APIKodi] [{0}] SearchTVEpisode: ""{1}"" | NOT found in host database!", _currenthost.Label, tDBElement.Filename))
                            Return Nothing
                        End If
                    Else
                        logger.Warn(String.Format("[APIKodi] [{0}] SearchTVEpisode: ""{1}"" | NOT found in host database!", _currenthost.Label, tDBElement.Filename))
                        Return Nothing
                    End If
                Else
                    logger.Warn(String.Format("[APIKodi] [{0}] SearchTVEpisode: ""{1}"" | NOT found in host database!", _currenthost.Label, tDBElement.Filename))
                    Return Nothing
                End If
            Else
                logger.Error(String.Format("[APIKodi] [{0}] SearchTVEpisode: ""{1}"" | No connection to Host!", _currenthost.Label, tDBElement.Filename))
                Return Nothing
            End If
        End Function

        Private Async Function SearchTVSeason(ByVal tDBElement As Database.DBElement) As Task(Of Video.Details.Season)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] SearchTVSeason: No host initialized! Abort!")
                Return Nothing
            End If

            Dim KodiTVShow = Await SearchTVShow(tDBElement).ConfigureAwait(False)
            Dim ShowID As Integer = -1
            If KodiTVShow IsNot Nothing Then
                ShowID = KodiTVShow.tvshowid
            End If
            If ShowID = -1 Then
                logger.Warn(String.Format("[APIKodi] [{0}] SearchTVSeason: ""{1}: Season {2}"" | NOT found in host database!", _currenthost.Label, tDBElement.ShowPath, tDBElement.TVSeason.Season))
                Return Nothing
            End If

            'get a list of all seasons saved in Kodi DB by ShowID
            Dim kTVSeasons As VideoLibrary.GetSeasonsResponse = Await GetAllTVSeasons(ShowID).ConfigureAwait(False)

            If kTVSeasons IsNot Nothing Then
                If kTVSeasons.seasons IsNot Nothing Then
                    Dim result = kTVSeasons.seasons.FirstOrDefault(Function(f) f.season = If(tDBElement.TVSeason.Season = 999, -1, tDBElement.TVSeason.Season))
                    If result IsNot Nothing Then
                        logger.Trace(String.Format("[APIKodi] [{0}] SearchTVSeason: ""{1}: Season {2}"" | OK, found in host database! [ID:{3}]", _currenthost.Label, tDBElement.ShowPath, tDBElement.TVSeason.Season, result.seasonid))
                        Return result
                    Else
                        logger.Warn(String.Format("[APIKodi] [{0}] SearchTVSeason: ""{1}: Season {2}"" | NOT found in host database!", _currenthost.Label, tDBElement.ShowPath, tDBElement.TVSeason.Season))
                        Return Nothing
                    End If
                Else
                    logger.Warn(String.Format("[APIKodi] [{0}] SearchTVSeason: ""{1}: Season {2}"" | NOT found in host database!", _currenthost.Label, tDBElement.ShowPath, tDBElement.TVSeason.Season))
                    Return Nothing
                End If
            Else
                logger.Error(String.Format("[APIKodi] [{0}] SearchTVSeason: ""{1}: Season {2}"" | No connection to Host!", _currenthost.Label, tDBElement.ShowPath, tDBElement.TVSeason.Season))
                Return Nothing
            End If
        End Function

        Private Async Function SearchTVShow(ByVal tDBElement As Database.DBElement) As Task(Of Video.Details.TVShow)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] SearchTVShow: No host initialized! Abort!")
                Return Nothing
            End If

            Dim kTVShows As VideoLibrary.GetTVShowsResponse

            Dim tPathAndFilename As PathAndFilename = GetPathAndFilename(tDBElement, Enums.ContentType.TVShow)
            Dim strRemotePath As String = GetRemotePath(tPathAndFilename.strPath)

            If Not String.IsNullOrEmpty(strRemotePath) Then
                Try
                    Dim filter As New List.Filter.TVShowsOr With {.or = New List(Of Object)}
                    Dim filterRule As New List.Filter.Rule.TVShows
                    filterRule.field = List.Filter.Fields.TVShows.path
                    filterRule.Operator = List.Filter.Operators.Is
                    filterRule.value = strRemotePath
                    filter.or.Add(filterRule)

                    kTVShows = Await _kodi.VideoLibrary.GetTVShows(filter).ConfigureAwait(False)

                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name, ex)
                    Return Nothing
                End Try
            Else
                logger.Error(String.Format("[APIKodi] [{0}] SearchTVShow: ""{1}"" | Source not mapped!", _currenthost.Label, tDBElement.Source.Path))
                Return Nothing
            End If

            If kTVShows IsNot Nothing Then
                If kTVShows.tvshows IsNot Nothing Then
                    If kTVShows.tvshows.Count = 1 Then
                        logger.Trace(String.Format("[APIKodi] [{0}] SearchTVShow: ""{1}"" | OK, found in host database! [ID:{2}]", _currenthost.Label, tDBElement.ShowPath, kTVShows.tvshows.Item(0).tvshowid))
                        Return kTVShows.tvshows.Item(0)
                    ElseIf kTVShows.tvshows.Count > 1 Then
                        logger.Warn(String.Format("[APIKodi] [{0}] SearchTVShow: ""{1}"" | MORE THAN ONE tv show found in host database!", _currenthost.Label, tDBElement.ShowPath))
                        Return Nothing
                    Else
                        logger.Warn(String.Format("[APIKodi] [{0}] SearchTVShow: ""{1}"" | NOT found in host database!", _currenthost.Label, tDBElement.ShowPath))
                        Return Nothing
                    End If
                Else
                    logger.Warn(String.Format("[APIKodi] [{0}] SearchTVShow: ""{1}"" | NOT found in host database!", _currenthost.Label, tDBElement.ShowPath))
                    Return Nothing
                End If
            Else
                logger.Error(String.Format("[APIKodi] [{0}] SearchTVShow: ""{1}"" | No connection to Host!", _currenthost.Label, tDBElement.ShowPath))
                Return Nothing
            End If
        End Function

        Public Async Function TestConnectionToHost() As Task(Of Boolean)
            Try
                Dim Response = Await _kodi.JSONRPC.Ping
                Return True
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                logger.Error(String.Format("[APIKodi] [{0}] TestConnectionToHost | No connection to Host!", _currenthost.Label))
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Update movie details at Kodi
        ''' </summary>
        ''' <param name="EmbermovieID">ID of specific movie (EmberDB)</param>
        ''' <param name="SendHostNotification">Send notification to host</param>
        ''' <returns>true=Update successfull, false=error or movie not found in KodiDB</returns>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation, main code by DanCooper
        ''' updates all movie fields which are filled/set in Ember (also paths of images)
        ''' at the moment the movie to update on host is identified by searching and comparing filename of movie(special handling for DVDs/Blurays), meaning there might be problems when filename is appearing more than once in movie library
        ''' </remarks>
        Public Async Function UpdateInfo_Movie(ByVal lngMovieID As Long, ByVal blnSendHostNotification As Boolean, ByVal blnSyncPlayCount As Boolean, ByVal GenericSubEvent As IProgress(Of GenericSubEventCallBackAsync), ByVal GenericMainEvent As IProgress(Of GenericEventCallBackAsync)) As Task(Of Boolean)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] UpdateMovieInfo: No host initialized! Abort!")
                Return False
            End If

            Dim bNeedSave As Boolean = False
            Dim bIsNew As Boolean = False
            Dim uMovie As Database.DBElement = Master.DB.LoadMovieFromDB(lngMovieID)

            Try
                logger.Trace(String.Format("[APIKodi] [{0}] UpdateMovieInfo: ""{1}"" | Start syncing process...", _currenthost.Label, uMovie.Movie.Title))

                'search Movie ID in Kodi DB
                Dim KodiElement As Video.Details.Movie = Await GetFullDetailsByID_Movie(Await GetMediaID(uMovie))

                If KodiElement Is Nothing Then
                    logger.Trace(String.Format("[APIKodi] [{0}] UpdateMovieInfo: ""{1}"" | NOT found in database, scan directory on host...", _currenthost.Label, uMovie.Movie.Title))
                    Await VideoLibrary_ScanPath(uMovie).ConfigureAwait(False)
                    While Await IsScanningVideo()
                        Threading.Thread.Sleep(1000)
                    End While
                    KodiElement = Await GetFullDetailsByID_Movie(Await GetMediaID(uMovie))
                    If KodiElement IsNot Nothing Then bIsNew = True
                End If

                If KodiElement IsNot Nothing Then
                    'check if we have to retrieve the PlayCount from Kodi
                    If blnSyncPlayCount AndAlso Not uMovie.Movie.PlayCount = KodiElement.playcount Then
                        uMovie.Movie.PlayCount = KodiElement.playcount
                        uMovie.Movie.LastPlayed = KodiElement.lastplayed
                        bNeedSave = True
                    End If

                    'string or string.empty
                    Dim mDateAdded As String = If(uMovie.Movie.DateAddedSpecified, uMovie.Movie.DateAdded, Nothing)
                    Dim mImdbnumber As String = uMovie.Movie.ID
                    Dim mLastPlayed As String = If(uMovie.Movie.LastPlayedSpecified, uMovie.Movie.LastPlayed, Nothing)
                    Dim mMPAA As String = uMovie.Movie.MPAA
                    Dim mOriginalTitle As String = uMovie.Movie.OriginalTitle
                    Dim mOutline As String = uMovie.Movie.Outline
                    Dim mPlot As String = uMovie.Movie.Plot
                    Dim mSet As String = If(uMovie.Movie.Sets.Count > 0, uMovie.Movie.Sets.Item(0).Title, String.Empty)
                    Dim mSortTitle As String = uMovie.Movie.SortTitle
                    Dim mTagline As String = uMovie.Movie.Tagline
                    Dim mTitle As String = uMovie.Movie.Title
                    Dim mTrailer As String = If(Not String.IsNullOrEmpty(uMovie.Trailer.LocalFilePath), GetRemotePath(uMovie.Trailer.LocalFilePath), If(uMovie.Movie.TrailerSpecified, uMovie.Movie.Trailer, String.Empty))
                    If mTrailer Is Nothing Then mTrailer = String.Empty

                    'digit grouping symbol for Votes count
                    Dim mVotes As String = If(Not String.IsNullOrEmpty(uMovie.Movie.Votes), uMovie.Movie.Votes, Nothing)
                    If Master.eSettings.GeneralDigitGrpSymbolVotes Then
                        If uMovie.Movie.VotesSpecified Then
                            Dim vote As String = Double.Parse(uMovie.Movie.Votes, Globalization.CultureInfo.InvariantCulture).ToString("N0", Globalization.CultureInfo.CurrentCulture)
                            If vote IsNot Nothing Then
                                mVotes = vote
                            End If
                        End If
                    End If

                    'integer or 0
                    Dim mPlaycount As Integer = If(uMovie.Movie.PlayCountSpecified, uMovie.Movie.PlayCount, 0)
                    Dim mRating As Double = If(uMovie.Movie.RatingSpecified, CType(Double.Parse(uMovie.Movie.Rating, Globalization.CultureInfo.InvariantCulture).ToString("N1", Globalization.CultureInfo.CurrentCulture), Double), 0)
                    Dim mRuntime As Integer = 0
                    If uMovie.Movie.RuntimeSpecified AndAlso Integer.TryParse(uMovie.Movie.Runtime, 0) Then
                        mRuntime = CType(uMovie.Movie.Runtime, Integer) * 60 'API requires runtime in seconds
                    End If
                    Dim mTop250 As Integer = If(uMovie.Movie.Top250Specified, CType(uMovie.Movie.Top250, Integer), 0)
                    Dim mYear As Integer = If(uMovie.Movie.YearSpecified, CType(uMovie.Movie.Year, Integer), 0)

                    'arrays
                    'Countries
                    Dim mCountryList As List(Of String) = If(uMovie.Movie.CountriesSpecified, uMovie.Movie.Countries, New List(Of String))

                    'Directors
                    Dim mDirectorList As List(Of String) = If(uMovie.Movie.DirectorsSpecified, uMovie.Movie.Directors, New List(Of String))

                    'Genres
                    Dim mGenreList As List(Of String) = If(uMovie.Movie.GenresSpecified, uMovie.Movie.Genres, New List(Of String))

                    'Studios
                    Dim mStudioList As List(Of String) = If(uMovie.Movie.StudiosSpecified, uMovie.Movie.Studios, New List(Of String))

                    'Tags
                    Dim mTagList As List(Of String) = If(uMovie.Movie.TagsSpecified, uMovie.Movie.Tags, New List(Of String))

                    'Writers (Credits)
                    Dim mWriterList As List(Of String) = If(uMovie.Movie.CreditsSpecified, uMovie.Movie.Credits, New List(Of String))


                    'string or null/nothing
                    Dim mBanner As String = If(uMovie.ImagesContainer.Banner.LocalFilePathSpecified,
                                                  GetRemotePath(uMovie.ImagesContainer.Banner.LocalFilePath), Nothing)
                    Dim mClearArt As String = If(uMovie.ImagesContainer.ClearArt.LocalFilePathSpecified,
                                                  GetRemotePath(uMovie.ImagesContainer.ClearArt.LocalFilePath), Nothing)
                    Dim mClearLogo As String = If(uMovie.ImagesContainer.ClearLogo.LocalFilePathSpecified,
                                                  GetRemotePath(uMovie.ImagesContainer.ClearLogo.LocalFilePath), Nothing)
                    Dim mDiscArt As String = If(uMovie.ImagesContainer.DiscArt.LocalFilePathSpecified,
                                                  GetRemotePath(uMovie.ImagesContainer.DiscArt.LocalFilePath), Nothing)
                    Dim mFanart As String = If(uMovie.ImagesContainer.Fanart.LocalFilePathSpecified,
                                                 GetRemotePath(uMovie.ImagesContainer.Fanart.LocalFilePath), Nothing)
                    Dim mLandscape As String = If(uMovie.ImagesContainer.Landscape.LocalFilePathSpecified,
                                                  GetRemotePath(uMovie.ImagesContainer.Landscape.LocalFilePath), Nothing)
                    Dim mPoster As String = If(uMovie.ImagesContainer.Poster.LocalFilePathSpecified,
                                                  GetRemotePath(uMovie.ImagesContainer.Poster.LocalFilePath), Nothing)

                    'all image paths will be set in artwork object
                    Dim artwork As New Media.Artwork.Set
                    artwork.banner = mBanner
                    artwork.clearart = mClearArt
                    artwork.clearlogo = mClearLogo
                    artwork.discart = mDiscArt
                    artwork.fanart = mFanart
                    artwork.landscape = mLandscape
                    artwork.poster = mPoster
                    'artwork.thumb = mPoster ' not supported in Ember?!

                    Dim response = Await _kodi.VideoLibrary.SetMovieDetails(KodiElement.movieid,
                                                                        title:=mTitle,
                                                                        playcount:=mPlaycount,
                                                                        runtime:=mRuntime,
                                                                        director:=mDirectorList,
                                                                        studio:=mStudioList,
                                                                        year:=mYear,
                                                                        plot:=mPlot,
                                                                        genre:=mGenreList,
                                                                        rating:=mRating,
                                                                        mpaa:=mMPAA,
                                                                        imdbnumber:=mImdbnumber,
                                                                        votes:=mVotes,
                                                                        lastplayed:=mLastPlayed,
                                                                        originaltitle:=mOriginalTitle,
                                                                        trailer:=mTrailer,
                                                                        tagline:=mTagline,
                                                                        plotoutline:=mOutline,
                                                                        writer:=mWriterList,
                                                                        country:=mCountryList,
                                                                        top250:=mTop250,
                                                                        sorttitle:=mSortTitle,
                                                                        set:=mSet,
                                                                        tag:=mTagList,
                                                                        art:=artwork).ConfigureAwait(False)
                    'not supported right now in Ember
                    'showlink:=mshowlink, _     
                    'thumbnail:=mposter, _
                    'fanart:=mFanart, _
                    'resume:=mresume, _
                    ' dateadded:=mdateAdded, _

                    If response.Contains("error") Then
                        logger.Error(String.Format("[APIKodi] [{0}] UpdateMovieInfo: {1}", _currenthost.Label, response))
                        Return False
                    Else
                        'Remove old textures (cache)
                        Await RemoveTextures(uMovie)

                        'Send message to Kodi?
                        If blnSendHostNotification = True Then
                            Await SendMessage("Ember Media Manager", If(bIsNew, Master.eLang.GetString(881, "Added"), Master.eLang.GetString(1408, "Updated")) & ": " & uMovie.Movie.Title).ConfigureAwait(False)
                        End If
                        If bNeedSave Then
                            logger.Trace(String.Format("[APIKodi] [{0}] UpdateMovieInfo: ""{1}"" | Save Playcount from host", _currenthost.Label, uMovie.Movie.Title))
                            Master.DB.SaveMovieToDB(uMovie, False, False, True, False)
                            GenericSubEvent.Report(New GenericSubEventCallBackAsync With {
                                                   .tGenericEventCallBackAsync = New GenericEventCallBackAsync With
                                                   {.tEventType = Enums.ModuleEventType.AfterEdit_Movie, .tParams = New List(Of Object)(New Object() {uMovie.ID})},
                                                   .tProgress = GenericMainEvent})
                        End If
                        logger.Trace(String.Format("[APIKodi] [{0}] UpdateMovieInfo: ""{1}"" | {2} on host", _currenthost.Label, uMovie.Movie.Title, If(bIsNew, "Added", "Updated")))
                        Return True
                    End If
                Else
                    logger.Error(String.Format("[APIKodi] [{0}] UpdateMovieInfo: ""{1}"" | NOT found on host! Abort!", _currenthost.Label, uMovie.Movie.Title))
                    Return False
                End If

            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Update movieset details at Kodi
        ''' </summary>
        ''' <param name="EmbermoviesetID">ID of specific movieset (EmberDB)</param>
        ''' <param name="SendHostNotification">Send notification to host</param>
        ''' <returns>true=Update successfull, false=error or movieset not found in KodiDB</returns>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation, main code by DanCooper
        ''' updates all movieset fields which are filled/set in Ember (also paths of images)
        ''' </remarks>
        Public Async Function UpdateInfo_MovieSet(ByVal lngMovieSetID As Long, ByVal blnSendHostNotification As Boolean) As Task(Of Boolean)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] UpdateMovieSetInfo: No host initialized! Abort!")
                Return False
            End If

            Dim bIsNew As Boolean = False
            Dim uMovieset As Database.DBElement = Master.DB.LoadMovieSetFromDB(lngMovieSetID)

            Try
                logger.Trace(String.Format("[APIKodi] [{0}] UpdateMovieSetInfo: ""{1}"" | Start syncing process...", _currenthost.Label, uMovieset.MovieSet.Title))

                'search MovieSet ID in Kodi DB
                Dim KodiElement As Video.Details.MovieSet = Await GetFullDetailsByID_MovieSet(Await GetMediaID(uMovieset))

                If KodiElement Is Nothing Then
                    logger.Error(String.Format("[APIKodi] [{0}] UpdateMovieSetInfo: ""{1}"" | NOT found on host! Abort!", _currenthost.Label, uMovieset.MovieSet.Title))
                    Return False
                    'what to do in this case?
                    'Await VideoLibrary_ScanPath(uMovieset).ConfigureAwait(False)
                    'Threading.Thread.Sleep(2000) 'TODO better solution for this?!
                    'KodiElement = Await GetFullDetailsByID_MovieSet(Await GetMediaID(uMovieset))
                    'If KodiElement IsNot Nothing Then bIsNew = True
                End If

                If KodiElement IsNot Nothing Then
                    'string or string.empty
                    Dim mTitle As String = uMovieset.MovieSet.Title

                    'string or null/nothing
                    Dim mBanner As String = If(uMovieset.ImagesContainer.Banner.LocalFilePathSpecified,
                                                  GetRemotePath_MovieSet(uMovieset.ImagesContainer.Banner.LocalFilePath), Nothing)
                    Dim mClearArt As String = If(uMovieset.ImagesContainer.ClearArt.LocalFilePathSpecified,
                                                  GetRemotePath_MovieSet(uMovieset.ImagesContainer.ClearArt.LocalFilePath), Nothing)
                    Dim mClearLogo As String = If(uMovieset.ImagesContainer.ClearLogo.LocalFilePathSpecified,
                                                  GetRemotePath_MovieSet(uMovieset.ImagesContainer.ClearLogo.LocalFilePath), Nothing)
                    Dim mDiscArt As String = If(uMovieset.ImagesContainer.DiscArt.LocalFilePathSpecified,
                                                  GetRemotePath_MovieSet(uMovieset.ImagesContainer.DiscArt.LocalFilePath), Nothing)
                    Dim mFanart As String = If(uMovieset.ImagesContainer.Fanart.LocalFilePathSpecified,
                                                 GetRemotePath_MovieSet(uMovieset.ImagesContainer.Fanart.LocalFilePath), Nothing)
                    Dim mLandscape As String = If(uMovieset.ImagesContainer.Landscape.LocalFilePathSpecified,
                                                  GetRemotePath_MovieSet(uMovieset.ImagesContainer.Landscape.LocalFilePath), Nothing)
                    Dim mPoster As String = If(uMovieset.ImagesContainer.Poster.LocalFilePathSpecified,
                                                  GetRemotePath_MovieSet(uMovieset.ImagesContainer.Poster.LocalFilePath), Nothing)

                    'all image paths will be set in artwork object
                    Dim artwork As New Media.Artwork.Set
                    artwork.banner = mBanner
                    artwork.clearart = mClearArt
                    artwork.clearlogo = mClearLogo
                    artwork.discart = mDiscArt
                    artwork.fanart = mFanart
                    artwork.landscape = mLandscape
                    artwork.poster = mPoster

                    Dim response = Await _kodi.VideoLibrary.SetMovieSetDetails(KodiElement.setid,
                                                                        title:=mTitle,
                                                                        art:=artwork).ConfigureAwait(False)


                    If response.Contains("error") Then
                        logger.Error(String.Format("[APIKodi] [{0}] UpdateMovieSetInfo: {1}", _currenthost.Label, response))
                        Return False
                    Else
                        'Remove old textures (cache)
                        Await RemoveTextures(uMovieset)

                        'Send message to Kodi?
                        If blnSendHostNotification = True Then
                            Await SendMessage("Ember Media Manager", If(bIsNew, Master.eLang.GetString(881, "Added"), Master.eLang.GetString(1408, "Updated")) & ": " & uMovieset.MovieSet.Title).ConfigureAwait(False)
                        End If
                        logger.Trace(String.Format("[APIKodi] [{0}] UpdateMovieSetInfo: ""{1}"" | {2} on host", _currenthost.Label, uMovieset.MovieSet.Title, If(bIsNew, "Added", "Updated")))
                        Return True
                    End If
                Else
                    logger.Error(String.Format("[APIKodi] [{0}] UpdateMovieSetInfo: ""{1}"" | NOT found on host! Abort!", _currenthost.Label, uMovieset.MovieSet.Title))
                    Return False
                End If

            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Update episode details at Kodi
        ''' </summary>
        ''' <param name="EmberepisodeID">ID of specific episode (EmberDB)</param>
        ''' <param name="SendHostNotification">Send notification to host</param>
        ''' <returns>true=Update successfull, false=error or episode not found in KodiDB</returns>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' updates all episode fields (also pathes of images)
        ''' at the moment episode on host is identified by searching and comparing filename of episode
        ''' </remarks>
        Public Async Function UpdateInfo_TVEpisode(ByVal lngTVEpisodeID As Long, ByVal blnSendHostNotification As Boolean, ByVal blnSyncPlayCount As Boolean, ByVal GenericSubEvent As IProgress(Of GenericSubEventCallBackAsync), ByVal GenericMainEvent As IProgress(Of GenericEventCallBackAsync)) As Task(Of Boolean)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] UpdateTVEpisodeInfo: No host initialized! Abort!")
                Return False
            End If

            Dim bNeedSave As Boolean = False
            Dim bIsNew As Boolean = False
            Dim uEpisode As Database.DBElement = Master.DB.LoadTVEpisodeFromDB(lngTVEpisodeID, True)

            Try
                logger.Trace(String.Format("[APIKodi] [{0}] UpdateTVEpisodeInfo: ""{1}"" | Start syncing process...", _currenthost.Label, uEpisode.TVEpisode.Title))

                'search TV Episode ID in Kodi DB
                Dim KodiElement As Video.Details.Episode = Await GetFullDetailsByID_TVEpisode(Await GetMediaID(uEpisode))

                'scan episode path
                If KodiElement Is Nothing Then
                    logger.Trace(String.Format("[APIKodi] [{0}] UpdateTVEpisodeInfo: ""{1}"" | NOT found in database, scan directory on host...", _currenthost.Label, uEpisode.TVEpisode.Title))
                    Await VideoLibrary_ScanPath(uEpisode).ConfigureAwait(False)
                    While Await IsScanningVideo()
                        Threading.Thread.Sleep(1000)
                    End While
                    KodiElement = Await GetFullDetailsByID_TVEpisode(Await GetMediaID(uEpisode))
                    If KodiElement IsNot Nothing Then bIsNew = True
                End If

                'scan tv show path path
                If KodiElement Is Nothing Then
                    logger.Trace(String.Format("[APIKodi] [{0}] UpdateTVEpisodeInfo: ""{1}"" | NOT found in database, scan directory on host...", _currenthost.Label, uEpisode.TVEpisode.Title))
                    Await VideoLibrary_ScanPath(uEpisode, True).ConfigureAwait(False)
                    While Await IsScanningVideo()
                        Threading.Thread.Sleep(1000)
                    End While
                    KodiElement = Await GetFullDetailsByID_TVEpisode(Await GetMediaID(uEpisode))
                    If KodiElement IsNot Nothing Then bIsNew = True
                End If

                If KodiElement IsNot Nothing Then
                    'check if we have to retrieve the PlayCount from Kodi
                    If blnSyncPlayCount AndAlso Not uEpisode.TVEpisode.Playcount = KodiElement.playcount Then
                        uEpisode.TVEpisode.Playcount = KodiElement.playcount
                        uEpisode.TVEpisode.LastPlayed = KodiElement.lastplayed
                        bNeedSave = True
                    End If

                    'string or string.empty
                    Dim mDateAdded As String = uEpisode.TVEpisode.DateAdded
                    Dim mLastPlayed As String = uEpisode.TVEpisode.LastPlayed
                    Dim mPlot As String = uEpisode.TVEpisode.Plot
                    Dim mTitle As String = uEpisode.TVEpisode.Title

                    'digit grouping symbol for Votes count
                    Dim mVotes As String = If(Not String.IsNullOrEmpty(uEpisode.TVEpisode.Votes), uEpisode.TVEpisode.Votes, Nothing)
                    If Master.eSettings.GeneralDigitGrpSymbolVotes Then
                        If uEpisode.TVEpisode.VotesSpecified Then
                            Dim vote As String = Double.Parse(uEpisode.TVEpisode.Votes, Globalization.CultureInfo.InvariantCulture).ToString("N0", Globalization.CultureInfo.CurrentCulture)
                            If vote IsNot Nothing Then
                                mVotes = vote
                            End If
                        End If
                    End If

                    'integer or 0
                    Dim mPlaycount As Integer = If(uEpisode.TVEpisode.PlaycountSpecified, CType(uEpisode.TVEpisode.Playcount, Integer), 0)
                    Dim mRating As Double = If(uEpisode.TVEpisode.RatingSpecified, CType(Double.Parse(uEpisode.TVEpisode.Rating, Globalization.CultureInfo.InvariantCulture).ToString("N1", Globalization.CultureInfo.CurrentCulture), Double), 0)
                    Dim mRuntime As Integer = 0
                    If uEpisode.TVEpisode.RuntimeSpecified AndAlso Integer.TryParse(uEpisode.TVEpisode.Runtime, 0) Then
                        mRuntime = CType(uEpisode.TVEpisode.Runtime, Integer) * 60 'API requires runtime in seconds
                    End If

                    'arrays
                    'Directors
                    Dim mDirectorList As List(Of String) = If(uEpisode.TVEpisode.DirectorsSpecified, uEpisode.TVEpisode.Directors, New List(Of String))

                    'Writers (Credits)
                    Dim mWriterList As List(Of String) = If(uEpisode.TVEpisode.CreditsSpecified, uEpisode.TVEpisode.Credits, New List(Of String))

                    'string or null/nothing
                    Dim mFanart As String = If(uEpisode.ImagesContainer.Fanart.LocalFilePathSpecified,
                                                 GetRemotePath(uEpisode.ImagesContainer.Fanart.LocalFilePath), Nothing)
                    Dim mPoster As String = If(uEpisode.ImagesContainer.Poster.LocalFilePathSpecified,
                                                  GetRemotePath(uEpisode.ImagesContainer.Poster.LocalFilePath), Nothing)

                    'all image paths will be set in artwork object
                    Dim artwork As New Media.Artwork.Set
                    artwork.fanart = mFanart
                    artwork.thumb = mPoster

                    Dim response = Await _kodi.VideoLibrary.SetEpisodeDetails(KodiElement.episodeid,
                                                                        title:=mTitle,
                                                                        playcount:=mPlaycount,
                                                                        runtime:=mRuntime,
                                                                        director:=mDirectorList,
                                                                        plot:=mPlot,
                                                                        rating:=mRating,
                                                                        votes:=mVotes,
                                                                        lastplayed:=mLastPlayed,
                                                                        writer:=mWriterList,
                                                                        art:=artwork).ConfigureAwait(False)
                    'not supported right now in Ember
                    'originaltitle:=moriginaltitle, _    
                    'firstaired:=mfirstaired, _    
                    'productioncode:=mproductioncode, _     
                    'thumbnail:=mposter, _
                    'fanart:=mFanart, _
                    'resume:=mresume, _


                    If response.Contains("error") Then
                        logger.Error(String.Format("[APIKodi] [{0}] UpdateTVEpisodeInfo: {1}", _currenthost.Label, response))
                        Return False
                    Else
                        'Remove old textures (cache)
                        Await RemoveTextures(uEpisode)

                        'Send message to Kodi?
                        If blnSendHostNotification = True Then
                            Await SendMessage("Ember Media Manager", If(bIsNew, Master.eLang.GetString(881, "Added"), Master.eLang.GetString(1408, "Updated")) & ": " & uEpisode.TVShow.Title & ": " & uEpisode.TVEpisode.Title).ConfigureAwait(False)
                        End If
                        If bNeedSave Then
                            logger.Trace(String.Format("[APIKodi] [{0}] UpdateTVEpisodeInfo: ""{1}"" | Save Playcount from host", _currenthost.Label, uEpisode.TVEpisode.Title))
                            Master.DB.SaveTVEpisodeToDB(uEpisode, False, False, True, False, False)
                            GenericSubEvent.Report(New GenericSubEventCallBackAsync With {
                                                   .tGenericEventCallBackAsync = New GenericEventCallBackAsync With
                                                   {.tEventType = Enums.ModuleEventType.AfterEdit_TVEpisode, .tParams = New List(Of Object)(New Object() {uEpisode.ID})},
                                                   .tProgress = GenericMainEvent})
                        End If
                        logger.Trace(String.Format("[APIKodi] [{0}] UpdateTVEpisodeInfo: ""{1}"" | {2} on host", _currenthost.Label, uEpisode.TVEpisode.Title, If(bIsNew, "Added", "Updated")))
                        Return True
                    End If
                Else
                    logger.Error(String.Format("[APIKodi] [{0}] UpdateTVEpisodeInfo: ""{1}"" | NOT found on host! Abort!", _currenthost.Label, uEpisode.TVEpisode.Title))
                    Return False
                End If
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Update season details at Kodi
        ''' </summary>
        ''' <param name="EmberseasonID">ID of specific season (EmberDB)</param>
        ''' <param name="SendHostNotification">Send notification to host</param>
        ''' <returns>true=Update successfull, false=error or movieset not found in KodiDB</returns>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation, main code by DanCooper
        ''' updates all movieset fields which are filled/set in Ember (also paths of images)
        ''' </remarks>
        Public Async Function UpdateInfo_TVSeason(ByVal lngTVSeasonID As Long, ByVal SendHostNotification As Boolean) As Task(Of Boolean)
            If _kodi Is Nothing Then
                logger.Warn("[APIKodi] UpdateTVSeasonInfo: No host initialized! Abort!")
                Return False
            End If

            Dim bIsNew As Boolean = False
            Dim uSeason As Database.DBElement = Master.DB.LoadTVSeasonFromDB(lngTVSeasonID, True)

            Try
                logger.Trace(String.Format("[APIKodi] [{0}] UpdateTVSeasonInfo: ""{1}: Season {2}"" | Start syncing process...", _currenthost.Label, uSeason.ShowPath, uSeason.TVSeason.Season))

                'search Movie ID in Kodi DB
                Dim KodiElement As Video.Details.Season = Await GetFullDetailsByID_TVSeason(Await GetMediaID(uSeason))

                If KodiElement Is Nothing Then
                    logger.Trace(String.Format("[APIKodi] [{0}] UpdateTVSeasonInfo: ""{1}: Season {2}"" | NOT found in database, scan directory on host...", _currenthost.Label, uSeason.ShowPath, uSeason.TVSeason.Season))
                    Await VideoLibrary_ScanPath(uSeason).ConfigureAwait(False)
                    While Await IsScanningVideo()
                        Threading.Thread.Sleep(1000)
                    End While
                    KodiElement = Await GetFullDetailsByID_TVSeason(Await GetMediaID(uSeason))
                    If KodiElement IsNot Nothing Then bIsNew = True
                End If

                If KodiElement IsNot Nothing Then
                    'string or null/nothing
                    Dim mBanner As String = If(uSeason.ImagesContainer.Banner.LocalFilePathSpecified,
                                                  GetRemotePath(uSeason.ImagesContainer.Banner.LocalFilePath), Nothing)
                    Dim mFanart As String = If(uSeason.ImagesContainer.Fanart.LocalFilePathSpecified,
                                                 GetRemotePath(uSeason.ImagesContainer.Fanart.LocalFilePath), Nothing)
                    Dim mLandscape As String = If(uSeason.ImagesContainer.Landscape.LocalFilePathSpecified,
                                                  GetRemotePath(uSeason.ImagesContainer.Landscape.LocalFilePath), Nothing)
                    Dim mPoster As String = If(uSeason.ImagesContainer.Poster.LocalFilePathSpecified,
                                                  GetRemotePath(uSeason.ImagesContainer.Poster.LocalFilePath), Nothing)

                    'all image paths will be set in artwork object
                    Dim artwork As New Media.Artwork.Set
                    artwork.banner = mBanner
                    artwork.fanart = mFanart
                    artwork.landscape = mLandscape
                    artwork.poster = mPoster

                    Dim response = Await _kodi.VideoLibrary.SetSeasonDetails(KodiElement.seasonid,
                                                                             art:=artwork).ConfigureAwait(False)

                    If response.Contains("error") Then
                        logger.Error(String.Format("[APIKodi] [{0}] UpdateTVSeasonInfo: {1}", _currenthost.Label, response))
                        Return False
                    Else
                        'Remove old textures (cache)
                        Await RemoveTextures(uSeason)

                        'Send message to Kodi?
                        If SendHostNotification = True Then
                            Await SendMessage("Ember Media Manager", If(bIsNew, Master.eLang.GetString(881, "Added"), Master.eLang.GetString(1408, "Updated")) & ": " & uSeason.TVShow.Title & ": Season " & uSeason.TVSeason.Season).ConfigureAwait(False)
                        End If
                        logger.Trace(String.Format("[APIKodi] [{0}] UpdateTVSeasonInfo: ""{1}: Season {2}"" | {3} on host", _currenthost.Label, uSeason.ShowPath, uSeason.TVSeason.Season, If(bIsNew, "Added", "Updated")))
                        Return True
                    End If
                Else
                    logger.Error(String.Format("[APIKodi] [{0}] UpdateTVSeasonInfo: ""{1}: Season {2}"" | NOT found on host! Abort!", _currenthost.Label, uSeason.ShowPath, uSeason.TVSeason.Season))
                    Return False
                End If

            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Update TVShow details at Kodi
        ''' </summary>
        ''' <param name="EmbershowID">ID of specific tvshow (EmberDB)</param>
        ''' <param name="SendHostNotification">Send notification to host</param>
        ''' <returns>true=Update successfull, false=error or show not found in KodiDB</returns>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' updates all TVShow fields (also paths of images)
        ''' at the moment TVShow on host is identified by searching and comparing path of TVShow
        ''' </remarks>
        Public Async Function UpdateInfo_TVShow(ByVal lngTVShowID As Long, ByVal blnSendHostNotification As Boolean) As Task(Of Boolean)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] UpdateTVShowInfo: No host initialized! Abort!")
                Return False
            End If

            Dim bIsNew As Boolean = False
            Dim uTVShow As Database.DBElement = Master.DB.LoadTVShowFromDB(lngTVShowID, False, False)

            Try
                logger.Trace(String.Format("[APIKodi] [{0}] UpdateTVShowInfo: ""{1}"" | Start syncing process...", _currenthost.Label, uTVShow.TVShow.Title))

                'search Movie ID in Kodi DB
                Dim KodiElement As Video.Details.TVShow = Await GetFullDetailsByID_TVShow(Await GetMediaID(uTVShow))

                If KodiElement Is Nothing Then
                    logger.Trace(String.Format("[APIKodi] [{0}] UpdateTVShowInfo: ""{1}"" | NOT found in database, scan directory on host...", _currenthost.Label, uTVShow.TVShow.Title))
                    Await VideoLibrary_ScanPath(uTVShow).ConfigureAwait(False)
                    While Await IsScanningVideo()
                        Threading.Thread.Sleep(1000)
                    End While
                    KodiElement = Await GetFullDetailsByID_TVShow(Await GetMediaID(uTVShow))
                    If KodiElement IsNot Nothing Then bIsNew = True
                End If

                If KodiElement IsNot Nothing Then

                    'TODO missing:
                    ' Dim mPlaycount As String = If(uTVShow.TVShow.PlayCountSpecified, uTVShow.TVShow.PlayCount, "0")
                    ' Dim mLastPlayed As String = If(uEpisode.TVEp.LastPlayedSpecified, Web.HttpUtility.JavaScriptStringEncode(uEpisode.TVEp.LastPlayed, True), "null")

                    'string or string.empty
                    Dim mEpisodeGuide As String = uTVShow.TVShow.EpisodeGuide.URL
                    Dim mImdbnumber As String = uTVShow.TVShow.TVDB
                    Dim mMPAA As String = uTVShow.TVShow.MPAA
                    Dim mOriginalTitle As String = uTVShow.TVShow.OriginalTitle
                    Dim mPlot As String = uTVShow.TVShow.Plot
                    Dim mPremiered As String = uTVShow.TVShow.Premiered
                    Dim mSortTitle As String = uTVShow.TVShow.SortTitle
                    Dim mTitle As String = uTVShow.TVShow.Title

                    'digit grouping symbol for Votes count
                    Dim mVotes As String = If(Not String.IsNullOrEmpty(uTVShow.TVShow.Votes), uTVShow.TVShow.Votes, Nothing)
                    If Master.eSettings.GeneralDigitGrpSymbolVotes Then
                        If uTVShow.TVShow.VotesSpecified Then
                            Dim vote As String = Double.Parse(uTVShow.TVShow.Votes, Globalization.CultureInfo.InvariantCulture).ToString("N0", Globalization.CultureInfo.CurrentCulture)
                            If vote IsNot Nothing Then
                                mVotes = vote
                            End If
                        End If
                    End If

                    'integer or 0
                    Dim mRating As Double = If(uTVShow.TVShow.RatingSpecified, CType(Double.Parse(uTVShow.TVShow.Rating, Globalization.CultureInfo.InvariantCulture).ToString("N1", Globalization.CultureInfo.CurrentCulture), Double), 0)
                    Dim mRuntime As Integer = If(uTVShow.TVShow.RuntimeSpecified, CType(uTVShow.TVShow.Runtime, Integer), 0)

                    'arrays
                    'Genres
                    Dim mGenreList As List(Of String) = If(uTVShow.TVShow.GenresSpecified, uTVShow.TVShow.Genres, New List(Of String))

                    'Studios
                    Dim mStudioList As List(Of String) = If(uTVShow.TVShow.StudiosSpecified, uTVShow.TVShow.Studios, New List(Of String))

                    'Tags
                    Dim mTagList As List(Of String) = If(uTVShow.TVShow.Tags.Count > 0, uTVShow.TVShow.Tags, New List(Of String))

                    'string or null/nothing
                    Dim mBanner As String = If(uTVShow.ImagesContainer.Banner.LocalFilePathSpecified,
                                                  GetRemotePath(uTVShow.ImagesContainer.Banner.LocalFilePath), Nothing)
                    Dim mCharacterArt As String = If(uTVShow.ImagesContainer.CharacterArt.LocalFilePathSpecified,
                                               GetRemotePath(uTVShow.ImagesContainer.CharacterArt.LocalFilePath), Nothing)
                    Dim mClearArt As String = If(uTVShow.ImagesContainer.ClearArt.LocalFilePathSpecified,
                                                GetRemotePath(uTVShow.ImagesContainer.ClearArt.LocalFilePath), Nothing)
                    Dim mClearLogo As String = If(uTVShow.ImagesContainer.ClearLogo.LocalFilePathSpecified,
                                                GetRemotePath(uTVShow.ImagesContainer.ClearLogo.LocalFilePath), Nothing)
                    Dim mFanart As String = If(uTVShow.ImagesContainer.Fanart.LocalFilePathSpecified,
                                               GetRemotePath(uTVShow.ImagesContainer.Fanart.LocalFilePath), Nothing)
                    Dim mLandscape As String = If(uTVShow.ImagesContainer.Landscape.LocalFilePathSpecified,
                                              GetRemotePath(uTVShow.ImagesContainer.Landscape.LocalFilePath), Nothing)
                    Dim mPoster As String = If(uTVShow.ImagesContainer.Poster.LocalFilePathSpecified,
                                                 GetRemotePath(uTVShow.ImagesContainer.Poster.LocalFilePath), Nothing)

                    'all image paths will be set in artwork object
                    Dim artwork As New Media.Artwork.Set
                    artwork.banner = mBanner
                    artwork.characterart = mCharacterArt
                    artwork.clearart = mClearArt
                    artwork.clearlogo = mClearLogo
                    artwork.fanart = mFanart
                    artwork.landscape = mLandscape
                    artwork.poster = mPoster

                    Dim response = Await _kodi.VideoLibrary.SetTVShowDetails(KodiElement.tvshowid,
                                                                        title:=mTitle,
                                                                        studio:=mStudioList,
                                                                        plot:=mPlot,
                                                                        genre:=mGenreList,
                                                                        rating:=mRating,
                                                                        mpaa:=mMPAA,
                                                                        imdbnumber:=mImdbnumber,
                                                                        premiered:=mPremiered,
                                                                        votes:=mVotes,
                                                                        originaltitle:=mOriginalTitle,
                                                                        sorttitle:=mSortTitle,
                                                                        episodeguide:=mEpisodeGuide,
                                                                        tag:=mTagList,
                                                                        art:=artwork).ConfigureAwait(False)
                    'not supported right now in Ember
                    'thumbnail:=mposter, _
                    'fanart:=mFanart, _
                    'resume:=mresume, _
                    'playcount:=mplaycount, _
                    'lastplayed:=mlastplayed, _

                    If response.Contains("error") Then
                        logger.Error(String.Format("[APIKodi] [{0}] UpdateTVShowInfo: {1}", _currenthost.Label, response))
                        Return False
                    Else
                        'Remove old textures (cache)
                        Await RemoveTextures(uTVShow)

                        'Send message to Kodi?
                        If blnSendHostNotification = True Then
                            Await SendMessage("Ember Media Manager", If(bIsNew, Master.eLang.GetString(881, "Added"), Master.eLang.GetString(1408, "Updated")) & ": " & uTVShow.TVShow.Title).ConfigureAwait(False)
                        End If
                        logger.Trace(String.Format("[APIKodi] [{0}] UpdateTVShowInfo: ""{1}"" | {2} on host", _currenthost.Label, uTVShow.TVShow.Title, If(bIsNew, "Added", "Updated")))
                        Return True
                    End If
                Else
                    logger.Error(String.Format("[APIKodi] [{0}] UpdateTVShowInfo: ""{1}"" | NOT found on host! Abort!", _currenthost.Label, uTVShow.TVShow.Title))
                    Return False
                End If
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return False
            End Try
        End Function
        ''' <summary>
        ''' Clean video library of host
        ''' </summary>
        ''' <returns>string with status message, if failed: Nothing</returns>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' </remarks>
        Public Async Function VideoLibrary_Clean() As Task(Of String)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] CleanVideoLibrary: No host initialized! Abort!")
                Return Nothing
            End If

            Try
                Dim response As String = String.Empty
                response = Await _kodi.VideoLibrary.Clean.ConfigureAwait(False)
                logger.Trace("[APIKodi] CleanVideoLibrary: " & _currenthost.Label)
                Return response
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Triggered as soon as cleaning of video library is finished
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>just an example for eventhandler</remarks>
        Private Sub VideoLibrary_OnCleanFinished(ByVal sender As String, ByVal data As Object)
            'Finished cleaning of video library
            ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.Notification, New List(Of Object)(New Object() {"info", 1, Master.eLang.GetString(1422, "Kodi Interface"), _currenthost.Label & " | " & Master.eLang.GetString(1450, "Cleaning Video Library...") & " OK!", New Bitmap(My.Resources.logo)}))
        End Sub

        ''' <summary>
        ''' Triggered as soon as video library scan (whole database not specific folder!) is finished
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>just an example for eventhandler</remarks>
        Private Sub VideoLibrary_OnScanFinished(ByVal sender As String, ByVal data As Object)
            'Finished updating video library
            ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.Notification, New List(Of Object)(New Object() {"info", 1, Master.eLang.GetString(1422, "Kodi Interface"), _currenthost.Label & " | " & Master.eLang.GetString(1448, "Updating Video Library...") & " OK!", New Bitmap(My.Resources.logo)}))
        End Sub
        ''' <summary>
        ''' Scan video library of Kodi host
        ''' </summary>
        ''' <returns>string with status message, if failed: Nothing</returns>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' </remarks>
        Public Async Function VideoLibrary_Scan() As Task(Of String)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] CleanVideoLibrary: No host initialized! Abort!")
                Return Nothing
            End If

            Try
                Dim response As String = String.Empty
                response = Await _kodi.VideoLibrary.Scan.ConfigureAwait(False)
                logger.Trace("[APIKodi] ScanVideoLibrary: " & _currenthost.Label)
                Return response
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Scan specific directory for new content
        ''' </summary>
        ''' <param name="EmbervideofileID">ID of specific videoitem (EmberDB)</param>
        ''' <param name="EmbervideofileID">type of videoitem (EmberDB), at the moment following is supported: movie, tvshow, episode</param>
        ''' <returns>true=Update successfull, false=error</returns>
        ''' Notice: No exception handling here because this function is called/nested in other functions and an exception must not be consumed (meaning a disconnect host would not be recognized at once)
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' </remarks>
        Public Async Function VideoLibrary_ScanPath(ByVal tDBElement As Database.DBElement, Optional ByVal bUseShowPath As Boolean = False) As Task(Of Boolean)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] ScanVideoPath: No host initialized! Abort!")
                Return Nothing
            End If

            Dim strLocalPath As String = String.Empty

            Select Case tDBElement.ContentType
                Case Enums.ContentType.Movie
                    If FileUtils.Common.isBDRip(tDBElement.Filename) Then
                        'filename must point to m2ts file! 
                        'Ember-Filepath i.e.  E:\Media_1\Movie\Horror\Europa Report\BDMV\STREAM\00000.m2ts
                        'for adding new Bluray rips scan the root folder of movie, i.e: E:\Media_1\Movie\Horror\Europa Report\
                        strLocalPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(tDBElement.Filename).FullName).FullName).FullName
                    ElseIf FileUtils.Common.isVideoTS(tDBElement.Filename) Then
                        'filename must point to IFO file!
                        'Ember-Filepath i.e.  E:\Media_1\Movie\Action\Crow\VIDEO_TS\VIDEO_TS.IFO
                        'for adding new DVDs scan the root folder of movie, i.e:  E:\Media_1\Movie\Action\Crow\
                        strLocalPath = Directory.GetParent(Directory.GetParent(tDBElement.Filename).FullName).FullName
                    Else
                        If Path.GetFileNameWithoutExtension(tDBElement.Filename).ToLower = "video_ts" Then
                            strLocalPath = Directory.GetParent(Directory.GetParent(tDBElement.Filename).FullName).FullName
                        Else
                            strLocalPath = Directory.GetParent(tDBElement.Filename).FullName
                        End If
                    End If
                Case Enums.ContentType.TVSeason, Enums.ContentType.TVShow
                    If FileUtils.Common.isBDRip(tDBElement.ShowPath) Then
                        'needs some testing?!
                        strLocalPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(tDBElement.ShowPath).FullName).FullName).FullName
                    ElseIf FileUtils.Common.isVideoTS(tDBElement.ShowPath) Then
                        'needs some testing?!
                        strLocalPath = Directory.GetParent(Directory.GetParent(tDBElement.ShowPath).FullName).FullName
                    Else
                        strLocalPath = tDBElement.ShowPath
                    End If
                Case Enums.ContentType.TVEpisode
                    If Not bUseShowPath Then
                        If FileUtils.Common.isBDRip(tDBElement.Filename) Then
                            'needs some testing?!
                            strLocalPath = Directory.GetParent(Directory.GetParent(Directory.GetParent(tDBElement.Filename).FullName).FullName).FullName
                        ElseIf FileUtils.Common.isVideoTS(tDBElement.Filename) Then
                            'needs some testing?!
                            strLocalPath = Directory.GetParent(Directory.GetParent(tDBElement.Filename).FullName).FullName
                        Else
                            strLocalPath = Directory.GetParent(tDBElement.Filename).FullName
                        End If
                    Else
                        strLocalPath = tDBElement.ShowPath
                    End If
                Case Else
                    logger.Warn(String.Format("[APIKodi] [{0}] ScanVideoPath: No videotype specified! Abort!", _currenthost.Label))
                    Return False
            End Select

            Dim strRemotePath As String = GetRemotePath(strLocalPath)
            If strRemotePath Is Nothing Then
                Return False
            End If
            Dim strResponse = Await _kodi.VideoLibrary.Scan(strRemotePath).ConfigureAwait(False)
            If strResponse.ToLower.Contains("error") Then
                logger.Trace(String.Format("[APIKodi] [{0}] ScanVideoPath: ""{1}"" | {2}", _currenthost.Label, strRemotePath, strResponse))
                Return False
            Else
                logger.Trace(String.Format("[APIKodi] [{0}] ScanVideoPath: ""{1}"" | Start scanning process...", _currenthost.Label, strRemotePath))
                Return True
            End If
        End Function
        ''' <summary>
        ''' Send message to Kodi which is displayed as notification
        ''' </summary>
        ''' <param name="title">title of notification in Kodi</param>
        ''' <param name="message">message to display</param>
        ''' <returns>string with displayed message, if failed: Nothing</returns>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' </remarks>
        Public Async Function SendMessage(ByVal strTitle As String, ByVal strMessage As String) As Task(Of String)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] SendMessage: No host initialized! Abort!")
                Return Nothing
            End If

            Try
                Dim response = Await _kodi.GUI.ShowNotification(strTitle, strMessage, 2500).ConfigureAwait(False)
                Return response
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Remove a cached image by Texture ID
        ''' </summary>
        ''' <param name="ID">ID of Texture</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Async Function RemoveTexture(ByVal intID As Integer) As Task(Of String)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] SendMessage: No host initialized! Abort!")
                Return Nothing
            End If

            Try
                Dim response = Await _kodi.Textures.RemoveTexture(intID)
                Return response
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Removes all cached images by a given path
        ''' </summary>
        ''' <param name="LocalPath"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Async Function RemoveTextures(ByVal tDBElement As Database.DBElement) As Task(Of Boolean)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] SendMessage: No host initialized! Abort!")
                Return False
            End If

            Try
                Dim TexturesResponce = Await GetTextures(tDBElement)
                If TexturesResponce IsNot Nothing Then
                    For Each tTexture In TexturesResponce.textures
                        Await RemoveTexture(tTexture.textureid)
                    Next
                Else
                    Return False
                End If
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return False
            End Try
            Return True
        End Function
        ''' <summary>
        ''' Scan video library of Kodi host
        ''' </summary>
        ''' <returns>string with status message, if failed: Nothing</returns>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' </remarks>
        Public Async Function IsScanningVideo() As Task(Of Boolean)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] IsScanningLibrary: No host initialized! Abort!")
                Return False
            End If

            Try
                Dim response As XBMC.GetInfoBooleansResponse = Await _kodi.XBMC.GetInfoBooleans(New List(Of String)(New String() {"Library.IsScanningVideo"}))
                Return response.IsScanningVideo
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return False
            End Try
        End Function

#Region "Helper functions/methods"

#End Region 'Helper functions/methods

#Region "Unused code"

        ''' <summary>
        ''' Retrieve ID of KODI playlist by playlist name
        ''' </summary>
        ''' <returns>ID of playlist, -1 if error occurs</returns>
        ''' <remarks>
        ''' 2015/11/29 Cocotus - First implementation
        ''' Not used in moment but left here to pick up later when TeamKodi has fixed playlist API...
        ''' ITS A PITA! At the moment there's no API method to retrieve the playlistID directly using the playlistName
        ''' Basically I check if the wanted playlist "directory" exists, if thats the case I use the items stored in the list to query against all other playlists avalaible on Kodi.
        ''' If the contents matches then I assume its correct playlist, and return the ID of playlist
        ''' This is no way efficient, but right now there's no other solution to get playlistID by playlistname
        ''' </remarks>
        Public Async Function GetPlaylistID(ByVal PlaylistName As String) As Task(Of Integer)
            If _kodi Is Nothing Then
                logger.Error("[APIKodi] GetPlaylistID: No host initialized! Abort!")
                Return Nothing
            End If
            Try
                Dim response = Await _kodi.Files.GetDirectory("special://videoplaylists", Files.Media.files)
                logger.Trace("[APIKodi] GetPlaylistID: " & _currenthost.Label)
                If Not response Is Nothing Then
                    For Each currentplaylist In response.files
                        'file property contains path (and name of playlist), i.e. "special://profile/playlists/video/test.m3u"
                        If Not currentplaylist Is Nothing AndAlso currentplaylist.file.Contains(PlaylistName) Then
                            logger.Trace("[APIKodi] Playlist with name: " & PlaylistName & " found!")
                            'get all items of the wanted playlist
                            Dim wantedplaylistcontent = Await _kodi.Files.GetDirectory(currentplaylist.file, Files.Media.files, XBMCRPC.List.Fields.Files.AllFields)
                            'now get all playlistsIDs avalaible using playlist-API
                            Dim lstallplaylists = Await _kodi.Playlist.GetPlaylists
                            'next loop through each playlist and check if its the one we want by comparing it's content with the content of our wanted playlist
                            For Each tmpplaylist In lstallplaylists
                                If Not tmpplaylist Is Nothing Then
                                    'get all items of playlist
                                    Dim tmpplaylistcontent = Await _kodi.Playlist.GetItems(tmpplaylist.playlistid)
                                    'check if current looped playlist has same count as our wanted playlist (we use this to reduce the number of playlists we need to loop through)
                                    If Not tmpplaylistcontent Is Nothing AndAlso Not tmpplaylistcontent.items Is Nothing AndAlso tmpplaylistcontent.items.Count = wantedplaylistcontent.files.Count Then
                                        'if the both playlists don't have any items then I don't bother and return this list as the correct one
                                        Dim IsIdenticalPlaylist = True

                                        '... if there are items in playlist then check if each item in wanted list is also existing in current looped list -> if thats the case then it's the correct list!
                                        For Each tmpplaylistitem In tmpplaylistcontent.items
                                            IsIdenticalPlaylist = False
                                            For Each wantedplaylistitem In wantedplaylistcontent.files
                                                If wantedplaylistitem.file = tmpplaylistitem.AsVideoDetailsFile.file Then
                                                    IsIdenticalPlaylist = True
                                                    Exit For
                                                End If
                                            Next
                                            If IsIdenticalPlaylist = False Then
                                                Exit For
                                            End If
                                        Next
                                        'check if the current examined playlist is the wanted playlist, if so return ID
                                        If IsIdenticalPlaylist = True Then
                                            logger.Trace("[APIKodi] Playlist with name: " & PlaylistName & " has ID: " & tmpplaylist.playlistid)
                                            Return tmpplaylist.playlistid
                                        End If
                                    End If
                                End If
                            Next
                        End If
                    Next
                Else
                    logger.Error("[APIKodi] GetPlaylistID: Error during retrieving playlists! Abort!")
                End If
                Return -1

            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return -1
            End Try
        End Function

        ''' <summary>
        ''' Check host connection
        ''' </summary>
        ''' <returns>true: host is online, false:offline</returns>
        ''' <remarks>
        ''' 2015/06/30 Cocotus - First implementation
        ''' </remarks>
        Public Async Function IsValidConnection() As Task(Of Boolean)
            Try
                Dim response = Await _kodi.JSONRPC.Ping.ConfigureAwait(False)
                If response.Length = 0 Then
                    Return False
                End If
                If response(0).ToString = "" Then
                    ' Dim t = _kodi.StartNotificationListener()
                    't.ContinueWith(t2 => { NotificationsEnabled = !t2.IsFaulted; });
                    Return True
                Else
                    'Dim t = _kodi.StartNotificationListener()
                    't.ContinueWith(t2 => { NotificationsEnabled = !t2.IsFaulted; });
                    Return True
                End If
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return False
            End Try
        End Function
        ''' <summary>
        ''' UnMute Kodi host
        ''' </summary>
        ''' <returns>true: success, false: error</returns>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' </remarks>
        Public Async Function UnMute() As Task(Of Boolean)
            Try
                Return Await _kodi.Application.SetMute(False).ConfigureAwait(False)
                'Await Refresh()
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Mute Kodi host
        ''' </summary>
        ''' <returns>true: success, false: error</returns>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' </remarks>
        Public Async Function Mute() As Task(Of Boolean)
            Try
                Return Await _kodi.Application.SetMute(True).ConfigureAwait(False)
                'Await Refresh()
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Set volume of Kodi host
        ''' </summary>
        ''' <returns>integer: volume level, Nothing: error</returns>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' </remarks>
        Public Async Function SetVolume(ByVal volume As Integer) As Task(Of Integer)
            Try
                Return Await _kodi.Application.SetVolume(volume).ConfigureAwait(False)
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Get JSON structure of Kodi host
        ''' </summary>
        ''' <returns>string: JSON structure, Nothing: error</returns>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' </remarks>
        Public Async Function GetJSONHost() As Task(Of String)
            Try
                Dim response = Await _kodi.JSONRPC.Introspect().ConfigureAwait(False)
                Return response.ToString
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' Get basic Kodi host information
        ''' </summary>
        ''' <returns>XBMCRPC.Application.Property.Value: object which contains specific host information, Nothing: error</returns>
        ''' <remarks>
        ''' 2015/06/27 Cocotus - First implementation
        ''' </remarks>
        Public Async Function GetHostInformation() As Task(Of Application.Property.Value)
            Try
                Dim response = Await _kodi.Application.GetProperties(Application.GetProperties_properties.AllFields()).ConfigureAwait(False)
                Return response
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
                Return Nothing
            End Try
        End Function

        ' Dim ret2 = Await xbmc.VideoLibrary.GetTVShows(TVShow.AllFields())
        ' Dim ret3 = Await xbmc.VideoLibrary.SetMovieDetails(3, playcount:=10)
        ' Dim ret4 = Await xbmc.Files.PrepareDownload(ret4.movies(0).thumbnail)
        ' Dim ret5 = Await xbmc.Files.GetDirectory(ret5b.files(0).file, Media.files, Files.AllFields())
        ' Dim ret6 = Await xbmc.Files.GetDirectory("C:\Archiv\Serien1\How I met your Mother\Staffel 3", Media.video)
        ' Dim ret7 = Await xbmc.Files.GetDirectory("C:\Archiv\HD2", Media.video, Files.AllFields())
        ' Dim ret8 = Await xbmc.Files.GetDirectory("C:\Users\steve_000\Music\Amazon MP3\die ärzte\auch", Media.music, Files.AllFields())
        ' Dim ret9 = Await xbmc.Playlist.GetItems(0, properties:=All.AllFields())
        ' Dim ret10 = Await xbmc.Playlist.GetPlaylists()
        ' Dim ret11 = Await xbmc.Player.GetActivePlayers()

        'Public Async Function Refresh() As Task
        '    Dim props = Await _kodi.Application.GetProperties(New GetProperties_properties() From { _
        '        Name.muted, _
        '        Name.volume _
        '    })

        '    SetProperty(_volumeLevel, props.volume, "VolumeLevel")
        '    VolumeEnabled = Not props.muted
        'End Function

#End Region 'Unused code

#End Region 'Methods

#Region "Nested Types"

        Structure PathAndFilename
            Dim strPath As String
            Dim strFilename As String
        End Structure

        Structure MySettings
            ''' <summary>
            ''' Username for Kodi webservice. Optional. Default is kodi for Kodi hosts ( xbmc for XBMC hosts )
            ''' </summary>
            ''' <returns>Username for Kodi webservice</returns>
            ''' <history>
            ''' 9/13/2015 Cocotus created
            ''' </history>
            Dim Username As String
            ''' <summary>
            ''' Password for Kodi webservice. Optional. As configured in Kodi / XBMC Setup
            ''' </summary>
            ''' <returns>Password for Kodi webservice</returns>
            ''' <history>
            ''' 9/13/2015 Cocotus created
            ''' </history>
            Dim Password As String
            ''' <summary>
            ''' IP address or DNS host name of Kodi / XBMC media player
            ''' </summary>
            ''' <returns>Address of Kodi webservice</returns>
            ''' <history>
            ''' 9/13/2015 Cocotus created
            ''' </history>
            Dim HostIP As String
            ''' <summary>
            ''' Kodi webport.Typically 80 or 8080. As configured in Kodi / XBMC Setup
            ''' </summary>
            ''' <returns>Kodi webport</returns>
            ''' <history>
            ''' 9/13/2015 Cocotus created
            ''' </history>
            Dim WebPort As Integer
        End Structure

#End Region 'Nested Types

    End Class

#Region "Client JSON Communication helper (needed for listening to notification events in Kodi)"
    Friend Class PlatformServices
        Implements IPlatformServices
        'following class platformservices is needed for listening to notification events in Kodi
        'found and converted to vb.net from https://github.com/DerPate2010/Xbmc2ndScr

#Region "Fields"

        Private _socketfactory As ISocketFactory

#End Region

#Region "Constructors"

        Public Sub New()
            SocketFactoryCreate = New SocketFactory()
        End Sub

#End Region 'Constructors

#Region "Methods"

        Public Async Function GetRequestStream(request As Net.WebRequest) As Task(Of Stream)
            Try
                Return Await request.GetRequestStreamAsync().ConfigureAwait(False)
            Catch ex As Exception
                Throw ex
            End Try
        End Function

        Public Async Function GetResponse(request As Net.WebRequest) As Task(Of Net.WebResponse)
            Try
                Return Await request.GetResponseAsync().ConfigureAwait(False)
            Catch ex As Exception
                Throw ex
            End Try
        End Function

#End Region 'Methods

#Region "Properties"

        Public ReadOnly Property SocketFactory As ISocketFactory Implements IPlatformServices.SocketFactory
            Get
                Return _socketfactory
            End Get
        End Property

        Public Property SocketFactoryCreate As ISocketFactory
            Get
                Return _socketfactory
            End Get
            Private Set(value As ISocketFactory)
                _socketfactory = value
            End Set
        End Property

#End Region 'Properties

    End Class

    Friend Class SocketFactory
        Implements ISocketFactory

#Region "Fields"

#End Region 'Fields

#Region "Constructors"

#End Region 'Constructors

#Region "Methods"

        Public Function GetSocket() As ISocket Implements ISocketFactory.GetSocket
            Return New DummySocket()
        End Function

        Public Async Function ResolveHostname(hostname As String) As Task(Of String()) Implements ISocketFactory.ResolveHostname
            Return Await ResolveHostname(hostname).ConfigureAwait(False)
        End Function

#End Region 'Methods

#Region "Properties"

#End Region 'Properties

    End Class

    Friend Class DummySocket
        Implements ISocket

#Region "Fields"

        Private _socket As Net.Sockets.Socket

#End Region 'Fields

#Region "Constructors"

#End Region 'Constructors

#Region "Methods"

        Public Sub Dispose() Implements IDisposable.Dispose
        End Sub

        Public Async Function ConnectAsync(hostName As String, port As Integer) As Task Implements ISocket.ConnectAsync
            _socket = New Net.Sockets.Socket(Net.Sockets.AddressFamily.InterNetwork, Net.Sockets.SocketType.Stream, Net.Sockets.ProtocolType.Tcp)
            _socket.Connect(hostName, port)
        End Function

        Public Function GetInputStream() As Stream Implements ISocket.GetInputStream
            Return New Net.Sockets.NetworkStream(_socket)
        End Function

#End Region 'Methods

#Region "Properties"

#End Region 'Properties

    End Class

#End Region 'Client JSON Communication helper (needed for listening to notification events in Kodi)

End Namespace
