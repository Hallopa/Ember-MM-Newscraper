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

Imports System
Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Windows.Forms
Imports System.Drawing
Imports NLog

Public Class ModulesManager

#Region "Fields"
    Shared logger As Logger = NLog.LogManager.GetCurrentClassLogger()

    Public Shared AssemblyList As New List(Of AssemblyListItem)
    Public Shared VersionList As New List(Of VersionItem)

    Public externalProcessorModules As New List(Of _externalGenericModuleClass)
    Public externalScrapersModules_Data_Movie As New List(Of _externalScraperModuleClass_Data_Movie)
    Public externalScrapersModules_Data_MovieSet As New List(Of _externalScraperModuleClass_Data_MovieSet)
    Public externalScrapersModules_Data_TV As New List(Of _externalScraperModuleClass_Data_TV)
    Public externalScrapersModules_Image_Movie As New List(Of _externalScraperModuleClass_Image_Movie)
    Public externalScrapersModules_Image_MovieSet As New List(Of _externalScraperModuleClass_Image_MovieSet)
    Public externalScrapersModules_Image_TV As New List(Of _externalScraperModuleClass_Image_TV)
    Public externalScrapersModules_Theme_Movie As New List(Of _externalScraperModuleClass_Theme_Movie)
    Public externalScrapersModules_Theme_TV As New List(Of _externalScraperModuleClass_Theme_TV)
    Public externalScrapersModules_Trailer_Movie As New List(Of _externalScraperModuleClass_Trailer_Movie)
    Public RuntimeObjects As New EmberRuntimeObjects

    'Singleton Instace for module manager .. allways use this one
    Private Shared Singleton As ModulesManager = Nothing

    Private moduleLocation As String = Path.Combine(Functions.AppPath, "Modules")

    Friend WithEvents bwLoadGenericModules As New System.ComponentModel.BackgroundWorker
    Friend WithEvents bwLoadScrapersModules_Movie As New System.ComponentModel.BackgroundWorker
    Friend WithEvents bwLoadScrapersModules_MovieSet As New System.ComponentModel.BackgroundWorker
    Friend WithEvents bwLoadScrapersModules_TV As New System.ComponentModel.BackgroundWorker

    Dim bwloadGenericModules_done As Boolean
    Dim bwloadScrapersModules_Movie_done As Boolean
    Dim bwloadScrapersModules_MovieSet_done As Boolean
    Dim bwloadScrapersModules_TV_done As Boolean

    Dim AssemblyList_Generic As New List(Of AssemblyListItem)
    Dim AssemblyList_Movie As New List(Of AssemblyListItem)
    Dim AssemblyList_MovieSet As New List(Of AssemblyListItem)
    Dim AssemblyList_TV As New List(Of AssemblyListItem)

#End Region 'Fields

#Region "Events"

    Public Event GenericEvent(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object))
    Event ScraperEvent_Movie(ByVal eType As Enums.ScraperEventType, ByVal Parameter As Object)
    Event ScraperEvent_MovieSet(ByVal eType As Enums.ScraperEventType, ByVal Parameter As Object)
    Event ScraperEvent_TV(ByVal eType As Enums.ScraperEventType, ByVal Parameter As Object)

#End Region 'Events

#Region "Properties"

    Public Shared ReadOnly Property Instance() As ModulesManager
        Get
            If (Singleton Is Nothing) Then
                Singleton = New ModulesManager()
            End If
            Return Singleton
        End Get
    End Property

    Public ReadOnly Property ModulesLoaded() As Boolean
        Get
            Return bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done
        End Get

    End Property
#End Region 'Properties

#Region "Methods"

    Private Sub BuildVersionList()
        VersionList.Clear()
        VersionList.Add(New VersionItem With {.AssemblyFileName = "*EmberAPP", .Name = "Ember Application", .Version = My.Application.Info.Version.ToString()})
        VersionList.Add(New VersionItem With {.AssemblyFileName = "*EmberAPI", .Name = "Ember API", .Version = Functions.EmberAPIVersion()})
        For Each _externalScraperModule As _externalScraperModuleClass_Data_Movie In externalScrapersModules_Data_Movie
            VersionList.Add(New VersionItem With {.Name = _externalScraperModule.ProcessorModule.ModuleName, _
              .AssemblyFileName = _externalScraperModule.AssemblyFileName, _
              .Version = _externalScraperModule.ProcessorModule.ModuleVersion})
        Next
        For Each _externalScraperModule As _externalScraperModuleClass_Data_MovieSet In externalScrapersModules_Data_MovieSet
            VersionList.Add(New VersionItem With {.Name = _externalScraperModule.ProcessorModule.ModuleName, _
              .AssemblyFileName = _externalScraperModule.AssemblyFileName, _
              .Version = _externalScraperModule.ProcessorModule.ModuleVersion})
        Next
        For Each _externalScraperModule As _externalScraperModuleClass_Data_TV In externalScrapersModules_Data_TV
            VersionList.Add(New VersionItem With {.Name = _externalScraperModule.ProcessorModule.ModuleName, _
              .AssemblyFileName = _externalScraperModule.AssemblyFileName, _
              .Version = _externalScraperModule.ProcessorModule.ModuleVersion})
        Next
        For Each _externalScraperModule As _externalScraperModuleClass_Image_Movie In externalScrapersModules_Image_Movie
            VersionList.Add(New VersionItem With {.Name = _externalScraperModule.ProcessorModule.ModuleName, _
              .AssemblyFileName = _externalScraperModule.AssemblyFileName, _
              .Version = _externalScraperModule.ProcessorModule.ModuleVersion})
        Next
        For Each _externalScraperModule As _externalScraperModuleClass_Image_MovieSet In externalScrapersModules_Image_MovieSet
            VersionList.Add(New VersionItem With {.Name = _externalScraperModule.ProcessorModule.ModuleName, _
              .AssemblyFileName = _externalScraperModule.AssemblyFileName, _
              .Version = _externalScraperModule.ProcessorModule.ModuleVersion})
        Next
        For Each _externalScraperModule As _externalScraperModuleClass_Image_TV In externalScrapersModules_Image_TV
            VersionList.Add(New VersionItem With {.Name = _externalScraperModule.ProcessorModule.ModuleName, _
              .AssemblyFileName = _externalScraperModule.AssemblyFileName, _
              .Version = _externalScraperModule.ProcessorModule.ModuleVersion})
        Next
        For Each _externalScraperModule As _externalScraperModuleClass_Theme_Movie In externalScrapersModules_Theme_Movie
            VersionList.Add(New VersionItem With {.Name = _externalScraperModule.ProcessorModule.ModuleName, _
              .AssemblyFileName = _externalScraperModule.AssemblyFileName, _
              .Version = _externalScraperModule.ProcessorModule.ModuleVersion})
        Next
        For Each _externalTVThemeScraperModule As _externalScraperModuleClass_Theme_TV In externalScrapersModules_Theme_TV
            VersionList.Add(New VersionItem With {.Name = _externalTVThemeScraperModule.ProcessorModule.ModuleName, _
                    .AssemblyFileName = _externalTVThemeScraperModule.AssemblyFileName, _
                    .Version = _externalTVThemeScraperModule.ProcessorModule.ModuleVersion})
        Next
        For Each _externalScraperModule As _externalScraperModuleClass_Trailer_Movie In externalScrapersModules_Trailer_Movie
            VersionList.Add(New VersionItem With {.Name = _externalScraperModule.ProcessorModule.ModuleName, _
              .AssemblyFileName = _externalScraperModule.AssemblyFileName, _
              .Version = _externalScraperModule.ProcessorModule.ModuleVersion})
        Next
        For Each _externalModule As _externalGenericModuleClass In externalProcessorModules
            VersionList.Add(New VersionItem With {.Name = _externalModule.ProcessorModule.ModuleName, _
              .AssemblyFileName = _externalModule.AssemblyFileName, _
              .Version = _externalModule.ProcessorModule.ModuleVersion})
        Next
    End Sub

    Private Sub bwLoadGenericModules_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwLoadGenericModules.DoWork
        LoadGenericModules()
    End Sub

    Private Sub bwLoadGenericModules_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwLoadGenericModules.RunWorkerCompleted
        bwloadGenericModules_done = True
        If bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done Then
            CreateAssemblyList()
            BuildVersionList()
        End If
    End Sub

    Private Sub bwLoadScrapersModules_Movie_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwLoadScrapersModules_Movie.DoWork
        LoadScrapersModules_Movie()
    End Sub

    Private Sub bwLoadScrapersModules_Movie_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwLoadScrapersModules_Movie.RunWorkerCompleted
        bwloadScrapersModules_Movie_done = True
        If bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done Then
            CreateAssemblyList()
            BuildVersionList()
        End If
    End Sub

    Private Sub bwLoadScrapersModules_MovieSet_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwLoadScrapersModules_MovieSet.DoWork
        LoadScrapersModules_MovieSet()
    End Sub

    Private Sub bwLoadScrapersModules_MovieSet_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwLoadScrapersModules_MovieSet.RunWorkerCompleted
        bwloadScrapersModules_MovieSet_done = True
        If bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done Then
            CreateAssemblyList()
            BuildVersionList()
        End If
    End Sub

    Private Sub bwLoadScrapersModules_TV_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bwLoadScrapersModules_TV.DoWork
        LoadScrapersModules_TV()
    End Sub

    Private Sub bwLoadScrapersModules_TV_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bwLoadScrapersModules_TV.RunWorkerCompleted
        bwloadScrapersModules_TV_done = True
        If bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done Then
            CreateAssemblyList()
            BuildVersionList()
        End If
    End Sub

    Private Sub CreateAssemblyList()
        For Each assembly As AssemblyListItem In AssemblyList_Generic
            If String.IsNullOrEmpty(AssemblyList.FirstOrDefault(Function(x) x.AssemblyName = assembly.AssemblyName).AssemblyName) Then
                AssemblyList.Add(assembly)
            End If
        Next
        For Each assembly As AssemblyListItem In AssemblyList_Movie
            If String.IsNullOrEmpty(AssemblyList.FirstOrDefault(Function(x) x.AssemblyName = assembly.AssemblyName).AssemblyName) Then
                AssemblyList.Add(assembly)
            End If
        Next
        For Each assembly As AssemblyListItem In AssemblyList_MovieSet
            If String.IsNullOrEmpty(AssemblyList.FirstOrDefault(Function(x) x.AssemblyName = assembly.AssemblyName).AssemblyName) Then
                AssemblyList.Add(assembly)
            End If
        Next
        For Each assembly As AssemblyListItem In AssemblyList_TV
            If String.IsNullOrEmpty(AssemblyList.FirstOrDefault(Function(x) x.AssemblyName = assembly.AssemblyName).AssemblyName) Then
                AssemblyList.Add(assembly)
            End If
        Next
    End Sub

    Public Function GetMovieCollectionID(ByVal sIMDBID As String) As String
        Dim CollectionID As String = String.Empty

        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While

        If Not String.IsNullOrEmpty(sIMDBID) Then
            Dim ret As Interfaces.ModuleResult
            For Each _externalScraperModuleClass_Data As _externalScraperModuleClass_Data_MovieSet In externalScrapersModules_Data_MovieSet.Where(Function(e) e.ProcessorModule.ModuleName = "TMDB_Data")
                ret = _externalScraperModuleClass_Data.ProcessorModule.GetCollectionID(sIMDBID, CollectionID)
                If ret.breakChain Then Exit For
            Next
        End If
        Return CollectionID
    End Function

    Function GetMovieStudio(ByRef DBMovie As Database.DBElement) As List(Of String)
        Dim ret As Interfaces.ModuleResult
        Dim sStudio As New List(Of String)
        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While
        For Each _externalScraperModule As _externalScraperModuleClass_Data_Movie In externalScrapersModules_Data_Movie.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
            Try
                ret = _externalScraperModule.ProcessorModule.GetMovieStudio(DBMovie, sStudio)
            Catch ex As Exception
            End Try
            If ret.breakChain Then Exit For
        Next
        sStudio = sStudio.Distinct().ToList() 'remove double entries
        Return sStudio
    End Function

    Public Function GetMovieTMDBID(ByRef sIMDBID As String) As String
        Dim TMDBID As String = String.Empty

        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While

        If Not String.IsNullOrEmpty(sIMDBID) Then
            Dim ret As Interfaces.ModuleResult
            For Each _externalScraperModuleClass_Data As _externalScraperModuleClass_Data_Movie In externalScrapersModules_Data_Movie.Where(Function(e) e.ProcessorModule.ModuleName = "TMDB_Data")
                ret = _externalScraperModuleClass_Data.ProcessorModule.GetTMDBID(sIMDBID, TMDBID)
                If ret.breakChain Then Exit For
            Next
        End If
        Return TMDBID
    End Function

    Public Function GetTVLanguages() As clsXMLTVDBLanguages
        Dim ret As Interfaces.ModuleResult
        Dim Langs As New clsXMLTVDBLanguages
        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While
        For Each _externalScraperModule As _externalScraperModuleClass_Data_TV In externalScrapersModules_Data_TV.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
            Try
                ret = _externalScraperModule.ProcessorModule.GetLanguages(Langs)
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
            End Try
            If ret.breakChain Then Exit For
        Next
        Return Langs
    End Function

    Public Sub GetVersions()
        Dim dlgVersions As New dlgVersions
        Dim li As ListViewItem
        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While
        For Each v As VersionItem In VersionList
            li = dlgVersions.lstVersions.Items.Add(v.Name)
            li.SubItems.Add(v.Version)
        Next
        dlgVersions.ShowDialog()
    End Sub

    Public Sub Handler_ScraperEvent_Movie(ByVal eType As Enums.ScraperEventType, ByVal Parameter As Object)
        RaiseEvent ScraperEvent_Movie(eType, Parameter)
    End Sub

    Public Sub Handler_ScraperEvent_MovieSet(ByVal eType As Enums.ScraperEventType, ByVal Parameter As Object)
        RaiseEvent ScraperEvent_MovieSet(eType, Parameter)
    End Sub

    Public Sub Handler_ScraperEvent_TV(ByVal eType As Enums.ScraperEventType, ByVal Parameter As Object)
        RaiseEvent ScraperEvent_TV(eType, Parameter)
    End Sub

    Public Sub LoadAllModules()
        bwloadGenericModules_done = False
        bwloadScrapersModules_Movie_done = False
        bwloadScrapersModules_MovieSet_done = False
        bwloadScrapersModules_TV_done = False

        bwLoadGenericModules.RunWorkerAsync()
        bwLoadScrapersModules_Movie.RunWorkerAsync()
        bwLoadScrapersModules_MovieSet.RunWorkerAsync()
        bwLoadScrapersModules_TV.RunWorkerAsync()
    End Sub

    ''' <summary>
    ''' Load all Generic Modules and field in externalProcessorModules List
    ''' </summary>
    Public Sub LoadGenericModules(Optional ByVal modulefile As String = "*.dll")
        logger.Trace("loadModules started")
        If Directory.Exists(moduleLocation) Then
            'Assembly to load the file
            Dim assembly As System.Reflection.Assembly
            'For each .dll file in the module directory
            For Each file As String In System.IO.Directory.GetFiles(moduleLocation, modulefile)
                Try
                    'Load the assembly
                    assembly = System.Reflection.Assembly.LoadFile(file)
                    'Loop through each of the assemeblies type
                    For Each fileType As Type In assembly.GetTypes
                        Try
                            'Activate the located module
                            Dim t As Type = fileType.GetInterface("GenericModule")
                            If Not t Is Nothing Then
                                Dim ProcessorModule As Interfaces.GenericModule 'Object
                                ProcessorModule = CType(Activator.CreateInstance(fileType), Interfaces.GenericModule)
                                'Add the activated module to the arraylist
                                Dim _externalProcessorModule As New _externalGenericModuleClass
                                Dim filename As String = file
                                If String.IsNullOrEmpty(AssemblyList_Generic.FirstOrDefault(Function(x) x.AssemblyName = Path.GetFileNameWithoutExtension(filename)).AssemblyName) Then
                                    AssemblyList_Generic.Add(New AssemblyListItem With {.AssemblyName = Path.GetFileNameWithoutExtension(filename), .Assembly = assembly})
                                End If
                                _externalProcessorModule.ProcessorModule = ProcessorModule
                                _externalProcessorModule.AssemblyName = String.Concat(Path.GetFileNameWithoutExtension(file), ".", fileType.FullName)
                                _externalProcessorModule.AssemblyFileName = Path.GetFileName(file)
                                _externalProcessorModule.Type = ProcessorModule.ModuleType
                                externalProcessorModules.Add(_externalProcessorModule)
                                ProcessorModule.Init(_externalProcessorModule.AssemblyName, Path.GetFileNameWithoutExtension(file))
                                Dim found As Boolean = False
                                For Each i In Master.eSettings.EmberModules
                                    If i.AssemblyName = _externalProcessorModule.AssemblyName Then
                                        _externalProcessorModule.ProcessorModule.Enabled = i.GenericEnabled
                                        found = True
                                    End If
                                Next
                                If Not found AndAlso Path.GetFileNameWithoutExtension(file).Contains("generic.EmberCore") Then
                                    _externalProcessorModule.ProcessorModule.Enabled = True
                                    'SetModuleEnable(_externalProcessorModule.AssemblyName, True)
                                End If
                                AddHandler ProcessorModule.GenericEvent, AddressOf GenericRunCallBack
                                'ProcessorModule.Enabled = _externalProcessorModule.ProcessorModule.Enabled
                            End If
                        Catch ex As Exception
                            logger.Error(New StackFrame().GetMethod().Name, ex)
                        End Try
                    Next
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name, ex)
                End Try
            Next
            Dim c As Integer = 0
            For Each ext As _externalGenericModuleClass In externalProcessorModules.OrderBy(Function(x) x.ModuleOrder)
                ext.ModuleOrder = c
                c += 1
            Next

        End If
        logger.Trace("loadModules finished")

    End Sub

    ''' <summary>
    ''' Load all Scraper Modules and field in externalScrapersModules List
    ''' </summary>
    Public Sub LoadScrapersModules_Movie(Optional ByVal modulefile As String = "*.dll")
        logger.Trace("loadMovieScrapersModules started")
        Dim DataScraperAnyEnabled As Boolean = False
        Dim DataScraperFound As Boolean = False
        Dim ImageScraperAnyEnabled As Boolean = False
        Dim ImageScraperFound As Boolean = False
        Dim ThemeScraperAnyEnabled As Boolean = False
        Dim ThemeScraperFound As Boolean = False
        Dim TrailerScraperAnyEnabled As Boolean = False
        Dim TrailerScraperFound As Boolean = False

        If Directory.Exists(moduleLocation) Then
            'Assembly to load the file
            Dim assembly As System.Reflection.Assembly
            'For each .dll file in the module directory
            For Each file As String In System.IO.Directory.GetFiles(moduleLocation, modulefile)
                Try
                    assembly = System.Reflection.Assembly.LoadFile(file)
                    'Loop through each of the assemeblies type
                    For Each fileType As Type In assembly.GetTypes

                        'Activate the located module
                        Dim t1 As Type = fileType.GetInterface("ScraperModule_Data_Movie")
                        If Not t1 Is Nothing Then
                            Dim ProcessorModule As Interfaces.ScraperModule_Data_Movie
                            ProcessorModule = CType(Activator.CreateInstance(fileType), Interfaces.ScraperModule_Data_Movie)
                            'Add the activated module to the arraylist
                            Dim _externalScraperModule As New _externalScraperModuleClass_Data_Movie
                            Dim filename As String = file
                            If String.IsNullOrEmpty(AssemblyList_Movie.FirstOrDefault(Function(x) x.AssemblyName = Path.GetFileNameWithoutExtension(filename)).AssemblyName) Then
                                AssemblyList_Movie.Add(New AssemblyListItem With {.AssemblyName = Path.GetFileNameWithoutExtension(filename), .Assembly = assembly})
                            End If
                            _externalScraperModule.ProcessorModule = ProcessorModule
                            _externalScraperModule.AssemblyName = String.Concat(Path.GetFileNameWithoutExtension(file), ".", fileType.FullName)
                            _externalScraperModule.AssemblyFileName = Path.GetFileName(file)

                            externalScrapersModules_Data_Movie.Add(_externalScraperModule)
                            logger.Trace(String.Concat("Scraper Added: ", _externalScraperModule.AssemblyName, "_", _externalScraperModule.ContentType))
                            _externalScraperModule.ProcessorModule.Init(_externalScraperModule.AssemblyName)
                            For Each i As _XMLEmberModuleClass In Master.eSettings.EmberModules.Where(Function(x) x.AssemblyName = _externalScraperModule.AssemblyName AndAlso _
                                                                                                          x.ContentType = Enums.ContentType.Movie)
                                _externalScraperModule.ProcessorModule.ScraperEnabled = i.ModuleEnabled
                                DataScraperAnyEnabled = DataScraperAnyEnabled OrElse i.ModuleEnabled
                                _externalScraperModule.ModuleOrder = i.ModuleOrder
                                DataScraperFound = True
                            Next
                            If Not DataScraperFound Then
                                _externalScraperModule.ModuleOrder = 999
                            End If
                        Else
                            Dim t2 As Type = fileType.GetInterface("ScraperModule_Image_Movie")
                            If Not t2 Is Nothing Then
                                Dim ProcessorModule As Interfaces.ScraperModule_Image_Movie
                                ProcessorModule = CType(Activator.CreateInstance(fileType), Interfaces.ScraperModule_Image_Movie)
                                'Add the activated module to the arraylist
                                Dim _externalScraperModule As New _externalScraperModuleClass_Image_Movie
                                Dim filename As String = file
                                If String.IsNullOrEmpty(AssemblyList_Movie.FirstOrDefault(Function(x) x.AssemblyName = Path.GetFileNameWithoutExtension(filename)).AssemblyName) Then
                                    AssemblyList_Movie.Add(New AssemblyListItem With {.AssemblyName = Path.GetFileNameWithoutExtension(filename), .Assembly = assembly})
                                End If
                                _externalScraperModule.ProcessorModule = ProcessorModule
                                _externalScraperModule.AssemblyName = String.Concat(Path.GetFileNameWithoutExtension(file), ".", fileType.FullName)
                                _externalScraperModule.AssemblyFileName = Path.GetFileName(file)

                                externalScrapersModules_Image_Movie.Add(_externalScraperModule)
                                logger.Trace(String.Concat("Scraper Added: ", _externalScraperModule.AssemblyName, "_", _externalScraperModule.ContentType))
                                _externalScraperModule.ProcessorModule.Init(_externalScraperModule.AssemblyName)
                                For Each i As _XMLEmberModuleClass In Master.eSettings.EmberModules.Where(Function(x) x.AssemblyName = _externalScraperModule.AssemblyName AndAlso _
                                                                                                          x.ContentType = Enums.ContentType.Movie)
                                    _externalScraperModule.ProcessorModule.ScraperEnabled = i.ModuleEnabled
                                    ImageScraperAnyEnabled = ImageScraperAnyEnabled OrElse i.ModuleEnabled
                                    _externalScraperModule.ModuleOrder = i.ModuleOrder
                                    ImageScraperFound = True
                                Next
                                If Not ImageScraperFound Then
                                    _externalScraperModule.ModuleOrder = 999
                                End If
                            Else
                                Dim t3 As Type = fileType.GetInterface("ScraperModule_Trailer_Movie")
                                If Not t3 Is Nothing Then
                                    Dim ProcessorModule As Interfaces.ScraperModule_Trailer_Movie
                                    ProcessorModule = CType(Activator.CreateInstance(fileType), Interfaces.ScraperModule_Trailer_Movie)
                                    'Add the activated module to the arraylist
                                    Dim _externalScraperModule As New _externalScraperModuleClass_Trailer_Movie
                                    Dim filename As String = file
                                    If String.IsNullOrEmpty(AssemblyList_Movie.FirstOrDefault(Function(x) x.AssemblyName = Path.GetFileNameWithoutExtension(filename)).AssemblyName) Then
                                        AssemblyList_Movie.Add(New AssemblyListItem With {.AssemblyName = Path.GetFileNameWithoutExtension(filename), .Assembly = assembly})
                                    End If
                                    _externalScraperModule.ProcessorModule = ProcessorModule
                                    _externalScraperModule.AssemblyName = String.Concat(Path.GetFileNameWithoutExtension(file), ".", fileType.FullName)
                                    _externalScraperModule.AssemblyFileName = Path.GetFileName(file)

                                    externalScrapersModules_Trailer_Movie.Add(_externalScraperModule)
                                    logger.Trace(String.Concat("Scraper Added: ", _externalScraperModule.AssemblyName, "_", _externalScraperModule.ContentType))
                                    _externalScraperModule.ProcessorModule.Init(_externalScraperModule.AssemblyName)
                                    For Each i As _XMLEmberModuleClass In Master.eSettings.EmberModules.Where(Function(x) x.AssemblyName = _externalScraperModule.AssemblyName AndAlso _
                                                                                                          x.ContentType = Enums.ContentType.Movie)
                                        _externalScraperModule.ProcessorModule.ScraperEnabled = i.ModuleEnabled
                                        TrailerScraperAnyEnabled = TrailerScraperAnyEnabled OrElse i.ModuleEnabled
                                        _externalScraperModule.ModuleOrder = i.ModuleOrder
                                        TrailerScraperFound = True
                                    Next
                                    If Not TrailerScraperFound Then
                                        _externalScraperModule.ModuleOrder = 999
                                    End If
                                Else
                                    Dim t4 As Type = fileType.GetInterface("ScraperModule_Theme_Movie")
                                    If Not t4 Is Nothing Then
                                        Dim ProcessorModule As Interfaces.ScraperModule_Theme_Movie
                                        ProcessorModule = CType(Activator.CreateInstance(fileType), Interfaces.ScraperModule_Theme_Movie)
                                        'Add the activated module to the arraylist
                                        Dim _externalScraperModule As New _externalScraperModuleClass_Theme_Movie
                                        Dim filename As String = file
                                        If String.IsNullOrEmpty(AssemblyList_Movie.FirstOrDefault(Function(x) x.AssemblyName = Path.GetFileNameWithoutExtension(filename)).AssemblyName) Then
                                            AssemblyList_Movie.Add(New AssemblyListItem With {.AssemblyName = Path.GetFileNameWithoutExtension(filename), .Assembly = assembly})
                                        End If
                                        _externalScraperModule.ProcessorModule = ProcessorModule
                                        _externalScraperModule.AssemblyName = String.Concat(Path.GetFileNameWithoutExtension(file), ".", fileType.FullName)
                                        _externalScraperModule.AssemblyFileName = Path.GetFileName(file)

                                        externalScrapersModules_Theme_Movie.Add(_externalScraperModule)
                                        logger.Trace(String.Concat("Scraper Added: ", _externalScraperModule.AssemblyName, "_", _externalScraperModule.ContentType))
                                        _externalScraperModule.ProcessorModule.Init(_externalScraperModule.AssemblyName)
                                        For Each i As _XMLEmberModuleClass In Master.eSettings.EmberModules.Where(Function(x) x.AssemblyName = _externalScraperModule.AssemblyName AndAlso _
                                                                                                          x.ContentType = Enums.ContentType.Movie)
                                            _externalScraperModule.ProcessorModule.ScraperEnabled = i.ModuleEnabled
                                            ThemeScraperAnyEnabled = ThemeScraperAnyEnabled OrElse i.ModuleEnabled
                                            _externalScraperModule.ModuleOrder = i.ModuleOrder
                                            ThemeScraperFound = True
                                        Next
                                        If Not ThemeScraperFound Then
                                            _externalScraperModule.ModuleOrder = 999
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Next
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name, ex)
                End Try
            Next
            Dim c As Integer = 0
            For Each ext As _externalScraperModuleClass_Data_Movie In externalScrapersModules_Data_Movie.OrderBy(Function(x) x.ModuleOrder)
                ext.ModuleOrder = c
                c += 1
            Next
            c = 0
            For Each ext As _externalScraperModuleClass_Image_Movie In externalScrapersModules_Image_Movie.OrderBy(Function(x) x.ModuleOrder)
                ext.ModuleOrder = c
                c += 1
            Next
            c = 0
            For Each ext As _externalScraperModuleClass_Theme_Movie In externalScrapersModules_Theme_Movie.OrderBy(Function(x) x.ModuleOrder)
                ext.ModuleOrder = c
                c += 1
            Next
            c = 0
            For Each ext As _externalScraperModuleClass_Trailer_Movie In externalScrapersModules_Trailer_Movie.OrderBy(Function(x) x.ModuleOrder)
                ext.ModuleOrder = c
                c += 1
            Next
            If Not DataScraperAnyEnabled AndAlso Not DataScraperFound Then
                SetScraperEnable_Data_Movie("scraper.Data.TMDB.ScraperModule.TMDB_Data", True)
            End If
            If Not ImageScraperAnyEnabled AndAlso Not ImageScraperFound Then
                SetScraperEnable_Image_Movie("scraper.Image.FanartTV.ScraperModule.FanartTV_Image", True)
                SetScraperEnable_Image_Movie("scraper.Image.TMDB.ScraperModule.TMDB_Image", True)
            End If
            If Not ThemeScraperAnyEnabled AndAlso Not ThemeScraperFound Then
                SetScraperEnable_Theme_Movie("scraper.Theme.TelevisionTunes.ScraperModule.TelevisionTunes_Theme", True)
            End If
            If Not TrailerScraperAnyEnabled AndAlso Not TrailerScraperFound Then
                SetScraperEnable_Trailer_Movie("scraper.Trailer.TMDB.ScraperModule.TMDB_Trailer", True)
            End If
        End If
        logger.Trace("loadMovieScrapersModules finished")
    End Sub

    ''' <summary>
    ''' Load all Scraper Modules and field in externalScrapersModules List
    ''' </summary>
    Public Sub LoadScrapersModules_MovieSet(Optional ByVal modulefile As String = "*.dll")
        logger.Trace("loadMovieSetScrapersModules started")
        Dim DataScraperAnyEnabled As Boolean = False
        Dim DataScraperFound As Boolean = False
        Dim ImageScraperAnyEnabled As Boolean = False
        Dim ImageScraperFound As Boolean = False

        If Directory.Exists(moduleLocation) Then
            'Assembly to load the file
            Dim assembly As System.Reflection.Assembly
            'For each .dll file in the module directory
            For Each file As String In System.IO.Directory.GetFiles(moduleLocation, modulefile)
                Try
                    assembly = System.Reflection.Assembly.LoadFile(file)
                    'Loop through each of the assemeblies type
                    For Each fileType As Type In assembly.GetTypes

                        'Activate the located module
                        Dim t1 As Type = fileType.GetInterface("ScraperModule_Data_MovieSet")
                        If Not t1 Is Nothing Then
                            Dim ProcessorModule As Interfaces.ScraperModule_Data_MovieSet
                            ProcessorModule = CType(Activator.CreateInstance(fileType), Interfaces.ScraperModule_Data_MovieSet)
                            'Add the activated module to the arraylist
                            Dim _externalScraperModule As New _externalScraperModuleClass_Data_MovieSet
                            Dim filename As String = file
                            If String.IsNullOrEmpty(AssemblyList_MovieSet.FirstOrDefault(Function(x) x.AssemblyName = Path.GetFileNameWithoutExtension(filename)).AssemblyName) Then
                                AssemblyList_MovieSet.Add(New AssemblyListItem With {.AssemblyName = Path.GetFileNameWithoutExtension(filename), .Assembly = assembly})
                            End If
                            _externalScraperModule.ProcessorModule = ProcessorModule
                            _externalScraperModule.AssemblyName = String.Concat(Path.GetFileNameWithoutExtension(file), ".", fileType.FullName)
                            _externalScraperModule.AssemblyFileName = Path.GetFileName(file)

                            externalScrapersModules_Data_MovieSet.Add(_externalScraperModule)
                            logger.Trace(String.Concat("Scraper Added: ", _externalScraperModule.AssemblyName, "_", _externalScraperModule.ContentType))
                            _externalScraperModule.ProcessorModule.Init(_externalScraperModule.AssemblyName)
                            For Each i As _XMLEmberModuleClass In Master.eSettings.EmberModules.Where(Function(x) x.AssemblyName = _externalScraperModule.AssemblyName AndAlso _
                                                                                                          x.ContentType = Enums.ContentType.MovieSet)
                                _externalScraperModule.ProcessorModule.ScraperEnabled = i.ModuleEnabled
                                DataScraperAnyEnabled = DataScraperAnyEnabled OrElse i.ModuleEnabled
                                _externalScraperModule.ModuleOrder = i.ModuleOrder
                                DataScraperFound = True
                            Next
                            If Not DataScraperFound Then
                                _externalScraperModule.ModuleOrder = 999
                            End If
                        Else
                            Dim t2 As Type = fileType.GetInterface("ScraperModule_Image_MovieSet")
                            If Not t2 Is Nothing Then
                                Dim ProcessorModule As Interfaces.ScraperModule_Image_MovieSet
                                ProcessorModule = CType(Activator.CreateInstance(fileType), Interfaces.ScraperModule_Image_MovieSet)
                                'Add the activated module to the arraylist
                                Dim _externalScraperModule As New _externalScraperModuleClass_Image_MovieSet
                                Dim filename As String = file
                                If String.IsNullOrEmpty(AssemblyList_MovieSet.FirstOrDefault(Function(x) x.AssemblyName = Path.GetFileNameWithoutExtension(filename)).AssemblyName) Then
                                    AssemblyList_MovieSet.Add(New AssemblyListItem With {.AssemblyName = Path.GetFileNameWithoutExtension(filename), .Assembly = assembly})
                                End If
                                _externalScraperModule.ProcessorModule = ProcessorModule
                                _externalScraperModule.AssemblyName = String.Concat(Path.GetFileNameWithoutExtension(file), ".", fileType.FullName)
                                _externalScraperModule.AssemblyFileName = Path.GetFileName(file)

                                externalScrapersModules_Image_MovieSet.Add(_externalScraperModule)
                                logger.Trace(String.Concat("Scraper Added: ", _externalScraperModule.AssemblyName, "_", _externalScraperModule.ContentType))
                                _externalScraperModule.ProcessorModule.Init(_externalScraperModule.AssemblyName)
                                For Each i As _XMLEmberModuleClass In Master.eSettings.EmberModules.Where(Function(x) x.AssemblyName = _externalScraperModule.AssemblyName AndAlso _
                                                                                                          x.ContentType = Enums.ContentType.MovieSet)
                                    _externalScraperModule.ProcessorModule.ScraperEnabled = i.ModuleEnabled
                                    ImageScraperAnyEnabled = ImageScraperAnyEnabled OrElse i.ModuleEnabled
                                    _externalScraperModule.ModuleOrder = i.ModuleOrder
                                    ImageScraperFound = True
                                Next
                                If Not ImageScraperFound Then
                                    _externalScraperModule.ModuleOrder = 999
                                End If
                            End If
                        End If
                    Next
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name, ex)
                End Try
            Next
            Dim c As Integer = 0
            For Each ext As _externalScraperModuleClass_Data_MovieSet In externalScrapersModules_Data_MovieSet.OrderBy(Function(x) x.ModuleOrder)
                ext.ModuleOrder = c
                c += 1
            Next
            c = 0
            For Each ext As _externalScraperModuleClass_Image_MovieSet In externalScrapersModules_Image_MovieSet.OrderBy(Function(x) x.ModuleOrder)
                ext.ModuleOrder = c
                c += 1
            Next
            If Not DataScraperAnyEnabled AndAlso Not DataScraperFound Then
                SetScraperEnable_Data_MovieSet("scraper.Data.TMDB.ScraperModule.TMDB_Data", True)
            End If
            If Not ImageScraperAnyEnabled AndAlso Not ImageScraperFound Then
                SetScraperEnable_Image_MovieSet("scraper.Image.FanartTV.ScraperModule.FanartTV_Image", True)
                SetScraperEnable_Image_MovieSet("scraper.Image.TMDB.ScraperModule.TMDB_Image", True)
            End If
        End If
        logger.Trace("loadMovieScrapersModules finished")
    End Sub

    Public Sub LoadScrapersModules_TV()
        logger.Trace("loadTVScrapersModules started")
        Dim DataScraperAnyEnabled As Boolean = False
        Dim DataScraperFound As Boolean = False
        Dim ImageScraperAnyEnabled As Boolean = False
        Dim ImageScraperFound As Boolean = False
        Dim ThemeScraperAnyEnabled As Boolean = False
        Dim ThemeScraperFound As Boolean = False

        If Directory.Exists(moduleLocation) Then
            'Assembly to load the file
            Dim assembly As System.Reflection.Assembly
            'For each .dll file in the module directory
            For Each file As String In System.IO.Directory.GetFiles(moduleLocation, "*.dll")
                Try
                    assembly = System.Reflection.Assembly.LoadFile(file)
                    'Loop through each of the assemeblies type

                    For Each fileType As Type In assembly.GetTypes

                        'Activate the located module
                        Dim t1 As Type = fileType.GetInterface("ScraperModule_Data_TV")
                        If Not t1 Is Nothing Then
                            Dim ProcessorModule As Interfaces.ScraperModule_Data_TV
                            ProcessorModule = CType(Activator.CreateInstance(fileType), Interfaces.ScraperModule_Data_TV)
                            'Add the activated module to the arraylist
                            Dim _externalScraperModule As New _externalScraperModuleClass_Data_TV
                            Dim filename As String = file
                            If String.IsNullOrEmpty(AssemblyList_TV.FirstOrDefault(Function(x) x.AssemblyName = Path.GetFileNameWithoutExtension(filename)).AssemblyName) Then
                                AssemblyList_TV.Add(New AssemblyListItem With {.AssemblyName = Path.GetFileNameWithoutExtension(filename), .Assembly = assembly})
                            End If
                            _externalScraperModule.ProcessorModule = ProcessorModule
                            _externalScraperModule.AssemblyName = String.Concat(Path.GetFileNameWithoutExtension(file), ".", fileType.FullName)
                            _externalScraperModule.AssemblyFileName = Path.GetFileName(file)

                            externalScrapersModules_Data_TV.Add(_externalScraperModule)
                            logger.Trace(String.Concat("Scraper Added: ", _externalScraperModule.AssemblyName, "_", _externalScraperModule.ContentType))
                            _externalScraperModule.ProcessorModule.Init(_externalScraperModule.AssemblyName)
                            For Each i As _XMLEmberModuleClass In Master.eSettings.EmberModules.Where(Function(x) x.AssemblyName = _externalScraperModule.AssemblyName AndAlso _
                                                                                                          x.ContentType = Enums.ContentType.TV)
                                _externalScraperModule.ProcessorModule.ScraperEnabled = i.ModuleEnabled
                                DataScraperAnyEnabled = DataScraperAnyEnabled OrElse i.ModuleEnabled
                                _externalScraperModule.ModuleOrder = i.ModuleOrder
                                DataScraperFound = True
                            Next
                            If Not DataScraperFound Then
                                _externalScraperModule.ModuleOrder = 999
                            End If
                        Else
                            Dim t2 As Type = fileType.GetInterface("ScraperModule_Image_TV")
                            If Not t2 Is Nothing Then
                                Dim ProcessorModule As Interfaces.ScraperModule_Image_TV
                                ProcessorModule = CType(Activator.CreateInstance(fileType), Interfaces.ScraperModule_Image_TV)
                                'Add the activated module to the arraylist
                                Dim _externalScraperModule As New _externalScraperModuleClass_Image_TV
                                Dim filename As String = file
                                If String.IsNullOrEmpty(AssemblyList_TV.FirstOrDefault(Function(x) x.AssemblyName = Path.GetFileNameWithoutExtension(filename)).AssemblyName) Then
                                    AssemblyList_TV.Add(New AssemblyListItem With {.AssemblyName = Path.GetFileNameWithoutExtension(filename), .Assembly = assembly})
                                End If
                                _externalScraperModule.ProcessorModule = ProcessorModule
                                _externalScraperModule.AssemblyName = String.Concat(Path.GetFileNameWithoutExtension(file), ".", fileType.FullName)
                                _externalScraperModule.AssemblyFileName = Path.GetFileName(file)

                                externalScrapersModules_Image_TV.Add(_externalScraperModule)
                                logger.Trace(String.Concat("Scraper Added: ", _externalScraperModule.AssemblyName, "_", _externalScraperModule.ContentType))
                                _externalScraperModule.ProcessorModule.Init(_externalScraperModule.AssemblyName)
                                For Each i As _XMLEmberModuleClass In Master.eSettings.EmberModules.Where(Function(x) x.AssemblyName = _externalScraperModule.AssemblyName AndAlso _
                                                                                                          x.ContentType = Enums.ContentType.TV)
                                    _externalScraperModule.ProcessorModule.ScraperEnabled = i.ModuleEnabled
                                    ImageScraperAnyEnabled = ImageScraperAnyEnabled OrElse i.ModuleEnabled
                                    _externalScraperModule.ModuleOrder = i.ModuleOrder
                                    ImageScraperFound = True
                                Next
                                If Not ImageScraperFound Then
                                    _externalScraperModule.ModuleOrder = 999
                                End If
                            Else
                                Dim t3 As Type = fileType.GetInterface("ScraperModule_Theme_TV")
                                If Not t3 Is Nothing Then
                                    Dim ProcessorModule As Interfaces.ScraperModule_Theme_TV
                                    ProcessorModule = CType(Activator.CreateInstance(fileType), Interfaces.ScraperModule_Theme_TV)
                                    'Add the activated module to the arraylist
                                    Dim _externalTVScraperModule As New _externalScraperModuleClass_Theme_TV
                                    Dim filename As String = file
                                    If String.IsNullOrEmpty(AssemblyList_TV.FirstOrDefault(Function(x) x.AssemblyName = Path.GetFileNameWithoutExtension(filename)).AssemblyName) Then
                                        AssemblyList_TV.Add(New AssemblyListItem With {.AssemblyName = Path.GetFileNameWithoutExtension(filename), .Assembly = assembly})
                                    End If

                                    _externalTVScraperModule.ProcessorModule = ProcessorModule
                                    _externalTVScraperModule.AssemblyName = String.Concat(Path.GetFileNameWithoutExtension(file), ".", fileType.FullName)
                                    _externalTVScraperModule.AssemblyFileName = Path.GetFileName(file)
                                    externalScrapersModules_Theme_TV.Add(_externalTVScraperModule)
                                    _externalTVScraperModule.ProcessorModule.Init(_externalTVScraperModule.AssemblyName)
                                    For Each i As _XMLEmberModuleClass In Master.eSettings.EmberModules.Where(Function(x) x.AssemblyName = _externalTVScraperModule.AssemblyName)
                                        _externalTVScraperModule.ProcessorModule.ScraperEnabled = i.ModuleEnabled
                                        ThemeScraperAnyEnabled = ThemeScraperAnyEnabled OrElse i.ModuleEnabled
                                        _externalTVScraperModule.ModuleOrder = i.ModuleOrder
                                        ThemeScraperFound = True
                                    Next
                                    If Not ThemeScraperFound Then
                                        _externalTVScraperModule.ModuleOrder = 999
                                    End If
                                End If
                            End If
                        End If
                    Next
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name, ex)
                End Try
            Next
            Dim c As Integer = 0
            For Each ext As _externalScraperModuleClass_Data_TV In externalScrapersModules_Data_TV.OrderBy(Function(x) x.ModuleOrder)
                ext.ModuleOrder = c
                c += 1
            Next
            c = 0
            For Each ext As _externalScraperModuleClass_Image_TV In externalScrapersModules_Image_TV.OrderBy(Function(x) x.ModuleOrder)
                ext.ModuleOrder = c
                c += 1
            Next
            c = 0
            For Each ext As _externalScraperModuleClass_Theme_TV In externalScrapersModules_Theme_TV.OrderBy(Function(x) x.ModuleOrder)
                ext.ModuleOrder = c
                c += 1
            Next
            If Not DataScraperAnyEnabled AndAlso Not DataScraperFound Then
                SetScraperEnable_Data_TV("scraper.Data.TVDB.ScraperModule.TVDB_Data", True)
            End If
            If Not ImageScraperAnyEnabled AndAlso Not ImageScraperFound Then
                SetScraperEnable_Image_TV("scraper.Image.FanartTV.ScraperModule.FanartTV_Image", True)
                SetScraperEnable_Image_TV("scraper.Image.TMDB.ScraperModule.TMDB_Image", True)
                SetScraperEnable_Image_TV("scraper.Image.TVDB.ScraperModule.TVDB_Image", True)
            End If
            If Not ThemeScraperAnyEnabled AndAlso Not ThemeScraperFound Then
                SetScraperEnable_Theme_TV("scraper.TelevisionTunes.Theme.EmberTVScraperModule.TelevisionTunes_Theme", True)
            End If
        End If
        logger.Trace("loadTVScrapersModules finished")
    End Sub

    Function QueryScraperCapabilities_Image_Movie(ByVal externalScraperModule As _externalScraperModuleClass_Image_Movie, ByVal ScrapeModifier As Structures.ScrapeModifier) As Boolean
        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While

        If ScrapeModifier.MainBanner AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainBanner) Then Return True
        If ScrapeModifier.MainClearArt AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainClearArt) Then Return True
        If ScrapeModifier.MainClearLogo AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainClearLogo) Then Return True
        If ScrapeModifier.MainDiscArt AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainDiscArt) Then Return True
        If ScrapeModifier.MainExtrafanarts AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainFanart) Then Return True
        If ScrapeModifier.MainExtrathumbs AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainFanart) Then Return True
        If ScrapeModifier.MainFanart AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainFanart) Then Return True
        If ScrapeModifier.MainLandscape AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainLandscape) Then Return True
        If ScrapeModifier.MainPoster AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainPoster) Then Return True

        Return False
    End Function

    Function QueryScraperCapabilities_Image_Movie(ByVal externalScraperModule As _externalScraperModuleClass_Image_Movie, ByVal ImageType As Enums.ModifierType) As Boolean
        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While

        Select Case ImageType
            Case Enums.ModifierType.MainExtrafanarts
                Return externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainFanart)
            Case Enums.ModifierType.MainExtrathumbs
                Return externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainFanart)
            Case Else
                Return externalScraperModule.ProcessorModule.QueryScraperCapabilities(ImageType)
        End Select

        Return False
    End Function

    Function QueryScraperCapabilities_Image_MovieSet(ByVal externalScraperModule As _externalScraperModuleClass_Image_MovieSet, ByVal ScrapeModifier As Structures.ScrapeModifier) As Boolean
        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While

        If ScrapeModifier.MainBanner AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainBanner) Then Return True
        If ScrapeModifier.MainClearArt AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainClearArt) Then Return True
        If ScrapeModifier.MainClearLogo AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainClearLogo) Then Return True
        If ScrapeModifier.MainDiscArt AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainDiscArt) Then Return True
        If ScrapeModifier.MainFanart AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainFanart) Then Return True
        If ScrapeModifier.MainLandscape AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainLandscape) Then Return True
        If ScrapeModifier.MainPoster AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainPoster) Then Return True

        Return False
    End Function

    Function QueryScraperCapabilities_Image_MovieSet(ByVal externalScraperModule As _externalScraperModuleClass_Image_MovieSet, ByVal ImageType As Enums.ModifierType) As Boolean
        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While

        Return externalScraperModule.ProcessorModule.QueryScraperCapabilities(ImageType)

        Return False
    End Function

    Function QueryScraperCapabilities_Image_TV(ByVal externalScraperModule As _externalScraperModuleClass_Image_TV, ByVal ScrapeModifier As Structures.ScrapeModifier) As Boolean
        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While

        If ScrapeModifier.EpisodeFanart AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.EpisodeFanart) Then Return True
        If ScrapeModifier.EpisodePoster AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.EpisodePoster) Then Return True
        If ScrapeModifier.MainBanner AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainBanner) Then Return True
        If ScrapeModifier.MainCharacterArt AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainCharacterArt) Then Return True
        If ScrapeModifier.MainClearArt AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainClearArt) Then Return True
        If ScrapeModifier.MainClearLogo AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainClearLogo) Then Return True
        If ScrapeModifier.MainFanart AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainFanart) Then Return True
        If ScrapeModifier.MainLandscape AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainLandscape) Then Return True
        If ScrapeModifier.MainPoster AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainPoster) Then Return True
        If ScrapeModifier.SeasonBanner AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.SeasonBanner) Then Return True
        If ScrapeModifier.SeasonFanart AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.SeasonFanart) Then Return True
        If ScrapeModifier.SeasonLandscape AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.SeasonLandscape) Then Return True
        If ScrapeModifier.SeasonPoster AndAlso externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.SeasonPoster) Then Return True

        Return False
    End Function

    Function QueryScraperCapabilities_Image_TV(ByVal externalScraperModule As _externalScraperModuleClass_Image_TV, ByVal ImageType As Enums.ModifierType) As Boolean
        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While

        Select Case ImageType
            Case Enums.ModifierType.AllSeasonsBanner
                If externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainBanner) OrElse _
                    externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.SeasonBanner) Then Return True
            Case Enums.ModifierType.AllSeasonsFanart
                If externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainFanart) OrElse _
                    externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.SeasonFanart) Then Return True
            Case Enums.ModifierType.AllSeasonsLandscape
                If externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainLandscape) OrElse _
                    externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.SeasonLandscape) Then Return True
            Case Enums.ModifierType.AllSeasonsPoster
                If externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainPoster) OrElse _
                    externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.SeasonPoster) Then Return True
            Case Enums.ModifierType.EpisodeFanart
                If externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainFanart) OrElse _
                    externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.EpisodeFanart) Then Return True
            Case Enums.ModifierType.MainExtrafanarts
                Return externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainFanart)
            Case Enums.ModifierType.SeasonFanart
                If externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.MainFanart) OrElse _
                    externalScraperModule.ProcessorModule.QueryScraperCapabilities(Enums.ModifierType.SeasonFanart) Then Return True
            Case Else
                Return externalScraperModule.ProcessorModule.QueryScraperCapabilities(ImageType)
        End Select

        Return False
    End Function
    ''' <summary>
    ''' Calls all the generic modules of the supplied type (if one is defined), passing the supplied _params.
    ''' The module will do its task and return any expected results in the _refparams.
    ''' </summary>
    ''' <param name="mType">The <c>Enums.ModuleEventType</c> of module to execute.</param>
    ''' <param name="_params">Parameters to pass to the module</param>
    ''' <param name="_singleobjekt"><c>Object</c> representing the module's result (if relevant)</param>
    ''' <param name="RunOnlyOne">If <c>True</c>, allow only one module to perform the required task.</param>
    ''' <returns></returns>
    ''' <remarks>Note that if any module returns a result of breakChain, no further modules are processed</remarks>
    Public Function RunGeneric(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object), Optional ByVal _singleobjekt As Object = Nothing, Optional ByVal RunOnlyOne As Boolean = False, Optional ByRef DBElement As Database.DBElement = Nothing) As Boolean
        Dim ret As Interfaces.ModuleResult

        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While

        Try
            Dim modules As IEnumerable(Of _externalGenericModuleClass) = externalProcessorModules.Where(Function(e) e.ProcessorModule.ModuleType.Contains(mType) AndAlso e.ProcessorModule.Enabled)
            If (modules.Count() <= 0) Then
                logger.Warn("No generic modules defined <{0}>", mType.ToString)
            Else
                For Each _externalGenericModule As _externalGenericModuleClass In modules
                    Try
                        logger.Trace("Run generic module <{0}>", _externalGenericModule.ProcessorModule.ModuleName)
                        ret = _externalGenericModule.ProcessorModule.RunGeneric(mType, _params, _singleobjekt, DBElement)
                    Catch ex As Exception
                        logger.Error(New StackFrame().GetMethod().Name & Convert.ToChar(Windows.Forms.Keys.Tab) & "Error scraping movies images using <" & _externalGenericModule.ProcessorModule.ModuleName & ">", ex)
                    End Try
                    If ret.breakChain OrElse RunOnlyOne Then Exit For
                Next
            End If
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try

        Return ret.Cancelled
    End Function

    Public Sub SaveSettings()
        Dim tmpForXML As New List(Of _XMLEmberModuleClass)

        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While

        For Each _externalProcessorModule As _externalGenericModuleClass In externalProcessorModules
            Dim t As New _XMLEmberModuleClass
            t.AssemblyName = _externalProcessorModule.AssemblyName
            t.AssemblyFileName = _externalProcessorModule.AssemblyFileName
            t.GenericEnabled = _externalProcessorModule.ProcessorModule.Enabled
            t.ContentType = _externalProcessorModule.ContentType
            tmpForXML.Add(t)
        Next
        For Each _externalScraperModule As _externalScraperModuleClass_Data_Movie In externalScrapersModules_Data_Movie
            Dim t As New _XMLEmberModuleClass
            t.AssemblyName = _externalScraperModule.AssemblyName
            t.AssemblyFileName = _externalScraperModule.AssemblyFileName
            t.ModuleEnabled = _externalScraperModule.ProcessorModule.ScraperEnabled
            t.ModuleOrder = _externalScraperModule.ModuleOrder
            t.ContentType = _externalScraperModule.ContentType
            tmpForXML.Add(t)
        Next
        For Each _externalScraperModule As _externalScraperModuleClass_Data_MovieSet In externalScrapersModules_Data_MovieSet
            Dim t As New _XMLEmberModuleClass
            t.AssemblyName = _externalScraperModule.AssemblyName
            t.AssemblyFileName = _externalScraperModule.AssemblyFileName
            t.ModuleEnabled = _externalScraperModule.ProcessorModule.ScraperEnabled
            t.ModuleOrder = _externalScraperModule.ModuleOrder
            t.ContentType = _externalScraperModule.ContentType
            tmpForXML.Add(t)
        Next
        For Each _externalScraperModule As _externalScraperModuleClass_Data_TV In externalScrapersModules_Data_TV
            Dim t As New _XMLEmberModuleClass
            t.AssemblyName = _externalScraperModule.AssemblyName
            t.AssemblyFileName = _externalScraperModule.AssemblyFileName
            t.ModuleEnabled = _externalScraperModule.ProcessorModule.ScraperEnabled
            t.ModuleOrder = _externalScraperModule.ModuleOrder
            t.ContentType = _externalScraperModule.ContentType
            tmpForXML.Add(t)
        Next
        For Each _externalScraperModule As _externalScraperModuleClass_Image_Movie In externalScrapersModules_Image_Movie
            Dim t As New _XMLEmberModuleClass
            t.AssemblyName = _externalScraperModule.AssemblyName
            t.AssemblyFileName = _externalScraperModule.AssemblyFileName
            t.ModuleEnabled = _externalScraperModule.ProcessorModule.ScraperEnabled
            t.ModuleOrder = _externalScraperModule.ModuleOrder
            t.ContentType = _externalScraperModule.ContentType
            tmpForXML.Add(t)
        Next
        For Each _externalScraperModule As _externalScraperModuleClass_Image_MovieSet In externalScrapersModules_Image_MovieSet
            Dim t As New _XMLEmberModuleClass
            t.AssemblyName = _externalScraperModule.AssemblyName
            t.AssemblyFileName = _externalScraperModule.AssemblyFileName
            t.ModuleEnabled = _externalScraperModule.ProcessorModule.ScraperEnabled
            t.ModuleOrder = _externalScraperModule.ModuleOrder
            t.ContentType = _externalScraperModule.ContentType
            tmpForXML.Add(t)
        Next
        For Each _externalScraperModule As _externalScraperModuleClass_Image_TV In externalScrapersModules_Image_TV
            Dim t As New _XMLEmberModuleClass
            t.AssemblyName = _externalScraperModule.AssemblyName
            t.AssemblyFileName = _externalScraperModule.AssemblyFileName
            t.ModuleEnabled = _externalScraperModule.ProcessorModule.ScraperEnabled
            t.ModuleOrder = _externalScraperModule.ModuleOrder
            t.ContentType = _externalScraperModule.ContentType
            tmpForXML.Add(t)
        Next
        For Each _externalScraperModule As _externalScraperModuleClass_Theme_Movie In externalScrapersModules_Theme_Movie
            Dim t As New _XMLEmberModuleClass
            t.AssemblyName = _externalScraperModule.AssemblyName
            t.AssemblyFileName = _externalScraperModule.AssemblyFileName
            t.ModuleEnabled = _externalScraperModule.ProcessorModule.ScraperEnabled
            t.ModuleOrder = _externalScraperModule.ModuleOrder
            t.ContentType = _externalScraperModule.ContentType
            tmpForXML.Add(t)
        Next
        For Each _externalTVScraperModule As _externalScraperModuleClass_Theme_TV In externalScrapersModules_Theme_TV
            Dim t As New _XMLEmberModuleClass
            t.AssemblyName = _externalTVScraperModule.AssemblyName
            t.AssemblyFileName = _externalTVScraperModule.AssemblyFileName
            t.ModuleEnabled = _externalTVScraperModule.ProcessorModule.ScraperEnabled
            t.ModuleOrder = _externalTVScraperModule.ModuleOrder
            t.ContentType = _externalTVScraperModule.ContentType
            tmpForXML.Add(t)
        Next
        For Each _externalScraperModule As _externalScraperModuleClass_Trailer_Movie In externalScrapersModules_Trailer_Movie
            Dim t As New _XMLEmberModuleClass
            t.AssemblyName = _externalScraperModule.AssemblyName
            t.AssemblyFileName = _externalScraperModule.AssemblyFileName
            t.ModuleEnabled = _externalScraperModule.ProcessorModule.ScraperEnabled
            t.ModuleOrder = _externalScraperModule.ModuleOrder
            t.ContentType = _externalScraperModule.ContentType
            tmpForXML.Add(t)
        Next
        Master.eSettings.EmberModules = tmpForXML
        Master.eSettings.Save()
    End Sub

    ''' <summary>
    ''' Request that enabled movie scrapers perform their functions on the supplied movie
    ''' </summary>
    ''' <param name="DBElement">Movie to be scraped</param>
    ''' <param name="ScrapeType">What kind of scrape is being requested, such as whether user-validation is desired</param>
    ''' <param name="ScrapeOptions">What kind of data is being requested from the scrape</param>
    ''' <returns><c>True</c> if one of the scrapers was cancelled</returns>
    ''' <remarks>Note that if no movie scrapers are enabled, a silent warning is generated.</remarks>
    Public Function ScrapeData_Movie(ByRef DBElement As Database.DBElement, ByRef ScrapeModifier As Structures.ScrapeModifier, ByVal ScrapeType As Enums.ScrapeType, ByVal ScrapeOptions As Structures.ScrapeOptions, ByVal showMessage As Boolean) As Boolean
        logger.Trace(String.Format("[APIModules] [ScrapeData_Movie] [Start] {0}", DBElement.Filename))
        If DBElement.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_Movie(DBElement, showMessage) Then
            Dim modules As IEnumerable(Of _externalScraperModuleClass_Data_Movie) = externalScrapersModules_Data_Movie.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
            Dim ret As Interfaces.ModuleResult_Data_Movie
            Dim ScrapedList As New List(Of MediaContainers.Movie)

            While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
                Application.DoEvents()
            End While

            'clean DBMovie if the movie is to be changed. For this, all existing (incorrect) information must be deleted and the images triggers set to remove.
            If (ScrapeType = Enums.ScrapeType.SingleScrape OrElse ScrapeType = Enums.ScrapeType.SingleAuto) AndAlso ScrapeModifier.DoSearch Then
                DBElement.ImagesContainer = New MediaContainers.ImagesContainer
                DBElement.Movie = New MediaContainers.Movie

                Dim tmpTitle As String = String.Empty
                If FileUtils.Common.isVideoTS(DBElement.Filename) Then
                    tmpTitle = StringUtils.FilterName_Movie(Directory.GetParent(Directory.GetParent(DBElement.Filename).FullName).Name, False)
                ElseIf FileUtils.Common.isBDRip(DBElement.Filename) Then
                    tmpTitle = StringUtils.FilterName_Movie(Directory.GetParent(Directory.GetParent(Directory.GetParent(DBElement.Filename).FullName).FullName).Name, False)
                Else
                    tmpTitle = StringUtils.FilterName_Movie(If(DBElement.IsSingle, Directory.GetParent(DBElement.Filename).Name, Path.GetFileNameWithoutExtension(DBElement.Filename)))
                End If

                Dim tmpYear As String = String.Empty
                If FileUtils.Common.isVideoTS(DBElement.Filename) Then
                    tmpYear = StringUtils.GetYear(Directory.GetParent(Directory.GetParent(DBElement.Filename).FullName).Name)
                ElseIf FileUtils.Common.isBDRip(DBElement.Filename) Then
                    tmpYear = StringUtils.GetYear(Directory.GetParent(Directory.GetParent(Directory.GetParent(DBElement.Filename).FullName).FullName).Name)
                Else
                    If DBElement.Source.UseFolderName AndAlso DBElement.IsSingle Then
                        tmpYear = StringUtils.GetYear(Directory.GetParent(DBElement.Filename).Name)
                    Else
                        tmpYear = StringUtils.GetYear(Path.GetFileNameWithoutExtension(DBElement.Filename))
                    End If
                End If

                DBElement.Movie.Title = tmpTitle
                DBElement.Movie.Year = tmpYear
            End If

            'create a clone of DBMovie
            Dim oDBMovie As Database.DBElement = CType(DBElement.CloneDeep, Database.DBElement)

            If (modules.Count() <= 0) Then
                logger.Warn("[APIModules] [ScrapeData_Movie] [Abort] No scrapers enabled")
            Else
                For Each _externalScraperModule As _externalScraperModuleClass_Data_Movie In modules
                    logger.Trace(String.Format("[APIModules] [ScrapeData_Movie] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))
                    AddHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_Movie

                    ret = _externalScraperModule.ProcessorModule.Scraper_Movie(oDBMovie, ScrapeModifier, ScrapeType, ScrapeOptions)

                    If ret.Cancelled Then Return ret.Cancelled

                    If ret.Result IsNot Nothing Then
                        ScrapedList.Add(ret.Result)
                    End If
                    RemoveHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_Movie
                    If ret.breakChain Then Exit For
                Next

                If ScrapedList.Count = 0 Then
                    logger.Trace(String.Format("[APIModules] [ScrapeData_Movie] [Cancelled] [No Scraper Results] {0}", DBElement.Filename))
                    Return True 'Cancelled
                End If

                'Merge scraperresults considering global datascraper settings
                DBElement = NFO.MergeDataScraperResults_Movie(DBElement, ScrapedList, ScrapeType, ScrapeOptions)

                'create cache paths for Actor Thumbs
                DBElement.Movie.CreateCachePaths_ActorsThumbs()
            End If
            logger.Trace(String.Format("[APIModules] [ScrapeData_Movie] [Done] {0}", DBElement.Filename))
            Return ret.Cancelled
        Else
            logger.Trace(String.Format("[APIModules] [ScrapeData_Movie] [Abort] [Offline] {0}", DBElement.Filename))
            Return True 'Cancelled
        End If
    End Function
    ''' <summary>
    ''' Request that enabled movie scrapers perform their functions on the supplied movie
    ''' </summary>
    ''' <param name="DBElement">MovieSet to be scraped. Scraper will directly manipulate this structure</param>
    ''' <param name="ScrapeType">What kind of scrape is being requested, such as whether user-validation is desired</param>
    ''' <param name="ScrapeOptions">What kind of data is being requested from the scrape</param>
    ''' <returns><c>True</c> if one of the scrapers was cancelled</returns>
    ''' <remarks>Note that if no movie set scrapers are enabled, a silent warning is generated.</remarks>
    Public Function ScrapeData_MovieSet(ByRef DBElement As Database.DBElement, ByRef ScrapeModifier As Structures.ScrapeModifier, ByVal ScrapeType As Enums.ScrapeType, ByVal ScrapeOptions As Structures.ScrapeOptions, ByVal showMessage As Boolean) As Boolean
        logger.Trace(String.Format("[APIModules] [ScrapeData_MovieSet] [Start] {0}", DBElement.MovieSet.Title))
        'If DBMovieSet.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_MovieSet(DBMovieSet, showMessage) Then
        Dim modules As IEnumerable(Of _externalScraperModuleClass_Data_MovieSet) = externalScrapersModules_Data_MovieSet.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
        Dim ret As Interfaces.ModuleResult_Data_MovieSet
        Dim ScrapedList As New List(Of MediaContainers.MovieSet)

        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While

        'clean DBMovie if the movie is to be changed. For this, all existing (incorrect) information must be deleted and the images triggers set to remove.
        If (ScrapeType = Enums.ScrapeType.SingleScrape OrElse ScrapeType = Enums.ScrapeType.SingleAuto) AndAlso ScrapeModifier.DoSearch Then
            Dim tmpTitle As String = DBElement.MovieSet.Title

            DBElement.ImagesContainer = New MediaContainers.ImagesContainer
            DBElement.MovieSet = New MediaContainers.MovieSet

            DBElement.MovieSet.Title = tmpTitle
        End If

        'create a clone of DBMovieSet
        Dim oDBMovieSet As Database.DBElement = CType(DBElement.CloneDeep, Database.DBElement)

        If (modules.Count() <= 0) Then
            logger.Warn("[APIModules] [ScrapeData_MovieSet] [Abort] No scrapers enabled")
        Else
            For Each _externalScraperModule As _externalScraperModuleClass_Data_MovieSet In modules
                logger.Trace(String.Format("[APIModules] [ScrapeData_MovieSet] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))
                AddHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_MovieSet

                ret = _externalScraperModule.ProcessorModule.Scraper(oDBMovieSet, ScrapeModifier, ScrapeType, ScrapeOptions)

                If ret.Cancelled Then
                    logger.Trace(String.Format("[APIModules] [ScrapeData_MovieSet] [Cancelled] [No Scraper Results] {0}", DBElement.MovieSet.Title))
                    Return ret.Cancelled
                End If

                If ret.Result IsNot Nothing Then
                    ScrapedList.Add(ret.Result)
                End If
                RemoveHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_MovieSet
                If ret.breakChain Then Exit For
            Next

            If ScrapedList.Count = 0 Then
                logger.Trace(String.Format("[APIModules] [ScrapeData_MovieSet] [Cancelled] [No Scraper Results] {0}", DBElement.MovieSet.Title))
                Return True 'Cancelled
            End If

            'Merge scraperresults considering global datascraper settings
            DBElement = NFO.MergeDataScraperResults_MovieSet(DBElement, ScrapedList, ScrapeType, ScrapeOptions)
        End If
        logger.Trace(String.Format("[APIModules] [ScrapeData_MovieSet] [Done] {0}", DBElement.MovieSet.Title))
        Return ret.Cancelled
        'Else
        'Return True 'Cancelled
        'End If
    End Function

    Public Function ScrapeData_TVEpisode(ByRef DBElement As Database.DBElement, ByVal ScrapeOptions As Structures.ScrapeOptions, ByVal showMessage As Boolean) As Boolean
        logger.Trace(String.Format("[APIModules] [ScrapeData_TVEpisode] [Start] {0}", DBElement.Filename))
        If DBElement.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_TVShow(DBElement, showMessage) Then
            Dim modules As IEnumerable(Of _externalScraperModuleClass_Data_TV) = externalScrapersModules_Data_TV.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
            Dim ret As Interfaces.ModuleResult_Data_TVEpisode
            Dim ScrapedList As New List(Of MediaContainers.EpisodeDetails)

            While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
                Application.DoEvents()
            End While

            'create a clone of DBTV
            Dim oEpisode As Database.DBElement = CType(DBElement.CloneDeep, Database.DBElement)

            If (modules.Count() <= 0) Then
                logger.Warn("[APIModules] [ScrapeData_TVEpisode] [Abort] No scrapers enabled")
            Else
                For Each _externalScraperModule As _externalScraperModuleClass_Data_TV In modules
                    logger.Trace(String.Format("[APIModules] [ScrapeData_TVEpisode] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))
                    AddHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_TV

                    ret = _externalScraperModule.ProcessorModule.Scraper_TVEpisode(oEpisode, ScrapeOptions)

                    If ret.Cancelled Then Return ret.Cancelled

                    If ret.Result IsNot Nothing Then
                        ScrapedList.Add(ret.Result)
                    End If
                    RemoveHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_TV
                    If ret.breakChain Then Exit For
                Next

                If ScrapedList.Count = 0 Then
                    logger.Trace(String.Format("[APIModules] [ScrapeData_TVEpisode] [Cancelled] [No Scraper Results] {0}", DBElement.Filename))
                    Return True 'Cancelled
                End If

                'Merge scraperresults considering global datascraper settings
                DBElement = NFO.MergeDataScraperResults_TVEpisode_Single(DBElement, ScrapedList, ScrapeOptions)

                'create cache paths for Actor Thumbs
                DBElement.TVEpisode.CreateCachePaths_ActorsThumbs()
            End If
            Return ret.Cancelled
        Else
            Return True 'Cancelled
        End If
    End Function

    Public Function ScrapeData_TVSeason(ByRef DBElement As Database.DBElement, ByVal ScrapeOptions As Structures.ScrapeOptions, ByVal showMessage As Boolean) As Boolean
        logger.Trace(String.Format("[APIModules] [ScrapeData_TVSeason] [Start] {0}: Season {1}", DBElement.TVShow.Title, DBElement.TVSeason.Season))
        If DBElement.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_TVShow(DBElement, showMessage) Then
            Dim modules As IEnumerable(Of _externalScraperModuleClass_Data_TV) = externalScrapersModules_Data_TV.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
            Dim ret As Interfaces.ModuleResult_Data_TVSeason
            Dim ScrapedList As New List(Of MediaContainers.SeasonDetails)

            While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
                Application.DoEvents()
            End While

            'create a clone of DBTV
            Dim oSeason As Database.DBElement = CType(DBElement.CloneDeep, Database.DBElement)

            If (modules.Count() <= 0) Then
                logger.Warn("[APIModules] [ScrapeData_TVSeason] [Abort] No scrapers enabled")
            Else
                For Each _externalScraperModule As _externalScraperModuleClass_Data_TV In modules
                    logger.Trace(String.Format("[APIModules] [ScrapeData_TVSeason] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))
                    AddHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_TV

                    ret = _externalScraperModule.ProcessorModule.Scraper_TVSeason(oSeason, ScrapeOptions)

                    If ret.Cancelled Then Return ret.Cancelled

                    If ret.Result IsNot Nothing Then
                        ScrapedList.Add(ret.Result)
                    End If
                    RemoveHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_TV
                    If ret.breakChain Then Exit For
                Next

                If ScrapedList.Count = 0 Then
                    logger.Trace(String.Format("[APIModules] [ScrapeData_TVSeason] [Cancelled] [No Scraper Results] {0}: Season {1}", DBElement.TVShow.Title, DBElement.TVSeason.Season))
                    Return True 'Cancelled
                End If

                'Merge scraperresults considering global datascraper settings
                DBElement = NFO.MergeDataScraperResults_TVSeason(DBElement, ScrapedList, ScrapeOptions)
            End If
            Return ret.Cancelled
        Else
            Return True 'Cancelled
        End If
    End Function
    ''' <summary>
    ''' Request that enabled movie scrapers perform their functions on the supplied movie
    ''' </summary>
    ''' <param name="DBElement">Show to be scraped</param>
    ''' <param name="ScrapeType">What kind of scrape is being requested, such as whether user-validation is desired</param>
    ''' <param name="ScrapeOptions">What kind of data is being requested from the scrape</param>
    ''' <returns><c>True</c> if one of the scrapers was cancelled</returns>
    ''' <remarks>Note that if no movie scrapers are enabled, a silent warning is generated.</remarks>
    Public Function ScrapeData_TVShow(ByRef DBElement As Database.DBElement, ByRef ScrapeModifier As Structures.ScrapeModifier, ByVal ScrapeType As Enums.ScrapeType, ByVal ScrapeOptions As Structures.ScrapeOptions, ByVal showMessage As Boolean) As Boolean
        logger.Trace(String.Format("[APIModules] [ScrapeData_TVShow] [Start] {0}", DBElement.TVShow.Title))
        If DBElement.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_TVShow(DBElement, showMessage) Then
            Dim modules As IEnumerable(Of _externalScraperModuleClass_Data_TV) = externalScrapersModules_Data_TV.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
            Dim ret As Interfaces.ModuleResult_Data_TVShow
            Dim ScrapedList As New List(Of MediaContainers.TVShow)

            While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
                Application.DoEvents()
            End While

            'clean DBTV if the tv show is to be changed. For this, all existing (incorrect) information must be deleted and the images triggers set to remove.
            If (ScrapeType = Enums.ScrapeType.SingleScrape OrElse ScrapeType = Enums.ScrapeType.SingleAuto) AndAlso ScrapeModifier.DoSearch Then
                DBElement.ExtrafanartsPath = String.Empty
                DBElement.ImagesContainer = New MediaContainers.ImagesContainer
                DBElement.NfoPath = String.Empty
                DBElement.Seasons.Clear()
                DBElement.ThemePath = String.Empty
                DBElement.TVShow = New MediaContainers.TVShow

                Dim tmpTitle As String = StringUtils.FilterName_TVShow(FileUtils.Common.GetDirectory(DBElement.ShowPath), False)

                DBElement.TVShow.Title = tmpTitle

                For Each sEpisode As Database.DBElement In DBElement.Episodes
                    Dim iEpisode As Integer = sEpisode.TVEpisode.Episode
                    Dim iSeason As Integer = sEpisode.TVEpisode.Season
                    sEpisode.ImagesContainer = New MediaContainers.ImagesContainer
                    sEpisode.NfoPath = String.Empty
                    sEpisode.TVEpisode = New MediaContainers.EpisodeDetails With {.Episode = iEpisode, .Season = iSeason}
                Next
            End If

            'create a clone of DBTV
            Dim oShow As Database.DBElement = CType(DBElement.CloneDeep, Database.DBElement)

            If (modules.Count() <= 0) Then
                logger.Warn("[APIModules] [ScrapeData_TVShow] [Abort] No scrapers enabled")
            Else
                For Each _externalScraperModule As _externalScraperModuleClass_Data_TV In modules
                    logger.Trace(String.Format("[APIModules] [ScrapeData_TVShow] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))
                    AddHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_TV

                    ret = _externalScraperModule.ProcessorModule.Scraper_TVShow(oShow, ScrapeModifier, ScrapeType, ScrapeOptions)

                    If ret.Cancelled Then Return ret.Cancelled

                    If ret.Result IsNot Nothing Then
                        ScrapedList.Add(ret.Result)
                    End If
                    RemoveHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_TV
                    If ret.breakChain Then Exit For
                Next

                If ScrapedList.Count = 0 Then
                    logger.Trace(String.Format("[APIModules] [ScrapeData_TVShow] [Cancelled] [No Scraper Results] {0}", DBElement.TVShow.Title))
                    Return True 'Cancelled
                End If

                'Merge scraperresults considering global datascraper settings
                DBElement = NFO.MergeDataScraperResults_TV(DBElement, ScrapedList, ScrapeType, ScrapeOptions, ScrapeModifier.withEpisodes)

                'create cache paths for Actor Thumbs
                DBElement.TVShow.CreateCachePaths_ActorsThumbs()
                If ScrapeModifier.withEpisodes Then
                    For Each tEpisode As Database.DBElement In DBElement.Episodes
                        tEpisode.TVEpisode.CreateCachePaths_ActorsThumbs()
                    Next
                End If
            End If
            Return ret.Cancelled
        Else
            Return True 'Cancelled
        End If
    End Function
    ''' <summary>
    ''' Request that enabled movie image scrapers perform their functions on the supplied movie
    ''' </summary>
    ''' <param name="DBElement">Movie to be scraped. Scraper will directly manipulate this structure</param>
    ''' <param name="ImagesContainer">Container of images that the scraper should add to</param>
    ''' <returns><c>True</c> if one of the scrapers was cancelled</returns>
    ''' <remarks>Note that if no movie scrapers are enabled, a silent warning is generated.</remarks>
    Public Function ScrapeImage_Movie(ByRef DBElement As Database.DBElement, ByRef ImagesContainer As MediaContainers.SearchResultsContainer, ByVal ScrapeModifier As Structures.ScrapeModifier, ByVal showMessage As Boolean) As Boolean
        logger.Trace(String.Format("[APIModules] [ScrapeImage_Movie] [Start] {0}", DBElement.Filename))
        If DBElement.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_Movie(DBElement, showMessage) Then
            Dim modules As IEnumerable(Of _externalScraperModuleClass_Image_Movie) = externalScrapersModules_Image_Movie.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
            Dim ret As Interfaces.ModuleResult

            While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
                Application.DoEvents()
            End While

            If (modules.Count() <= 0) Then
                logger.Warn("[APIModules] [ScrapeImage_Movie] [Abort] No scrapers enabled")
            Else
                For Each _externalScraperModule As _externalScraperModuleClass_Image_Movie In modules
                    logger.Trace(String.Format("[APIModules] [ScrapeImage_Movie] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))
                    If QueryScraperCapabilities_Image_Movie(_externalScraperModule, ScrapeModifier) Then
                        AddHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_Movie
                        Dim aContainer As New MediaContainers.SearchResultsContainer
                        ret = _externalScraperModule.ProcessorModule.Scraper(DBElement, aContainer, ScrapeModifier)
                        If aContainer IsNot Nothing Then
                            ImagesContainer.MainBanners.AddRange(aContainer.MainBanners)
                            ImagesContainer.MainCharacterArts.AddRange(aContainer.MainCharacterArts)
                            ImagesContainer.MainClearArts.AddRange(aContainer.MainClearArts)
                            ImagesContainer.MainClearLogos.AddRange(aContainer.MainClearLogos)
                            ImagesContainer.MainDiscArts.AddRange(aContainer.MainDiscArts)
                            ImagesContainer.MainFanarts.AddRange(aContainer.MainFanarts)
                            ImagesContainer.MainLandscapes.AddRange(aContainer.MainLandscapes)
                            ImagesContainer.MainPosters.AddRange(aContainer.MainPosters)
                        End If
                        RemoveHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_Movie
                        If ret.breakChain Then Exit For
                    End If
                Next

                'sorting
                ImagesContainer.Sort(DBElement)

                'create cache paths
                ImagesContainer.CreateCachePaths(DBElement)
            End If

            Return ret.Cancelled
        Else
            Return True 'Cancelled
        End If
    End Function
    ''' <summary>
    ''' Request that enabled movieset image scrapers perform their functions on the supplied movie
    ''' </summary>
    ''' <param name="DBElement">Movieset to be scraped. Scraper will directly manipulate this structure</param>
    ''' <param name="ImagesContainer">Container of images that the scraper should add to</param>
    ''' <returns><c>True</c> if one of the scrapers was cancelled</returns>
    ''' <remarks>Note that if no movie scrapers are enabled, a silent warning is generated.</remarks>
    Public Function ScrapeImage_MovieSet(ByRef DBElement As Database.DBElement, ByRef ImagesContainer As MediaContainers.SearchResultsContainer, ByVal ScrapeModifier As Structures.ScrapeModifier) As Boolean
        logger.Trace(String.Format("[APIModules] [ScrapeImage_MovieSet] [Start] {0}", DBElement.MovieSet.Title))
        Dim modules As IEnumerable(Of _externalScraperModuleClass_Image_MovieSet) = externalScrapersModules_Image_MovieSet.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
        Dim ret As Interfaces.ModuleResult

        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While

        If (modules.Count() <= 0) Then
            logger.Warn("[APIModules] [ScrapeImage_MovieSet] [Abort] No scrapers enabled")
        Else
            For Each _externalScraperModule As _externalScraperModuleClass_Image_MovieSet In modules
                logger.Trace(String.Format("[APIModules] [ScrapeImage_MovieSet] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))
                If QueryScraperCapabilities_Image_MovieSet(_externalScraperModule, ScrapeModifier) Then
                    AddHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_MovieSet
                    Dim aContainer As New MediaContainers.SearchResultsContainer
                    ret = _externalScraperModule.ProcessorModule.Scraper(DBElement, aContainer, ScrapeModifier)
                    If aContainer IsNot Nothing Then
                        ImagesContainer.MainBanners.AddRange(aContainer.MainBanners)
                        ImagesContainer.MainCharacterArts.AddRange(aContainer.MainCharacterArts)
                        ImagesContainer.MainClearArts.AddRange(aContainer.MainClearArts)
                        ImagesContainer.MainClearLogos.AddRange(aContainer.MainClearLogos)
                        ImagesContainer.MainDiscArts.AddRange(aContainer.MainDiscArts)
                        ImagesContainer.MainFanarts.AddRange(aContainer.MainFanarts)
                        ImagesContainer.MainLandscapes.AddRange(aContainer.MainLandscapes)
                        ImagesContainer.MainPosters.AddRange(aContainer.MainPosters)
                    End If
                    RemoveHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_MovieSet
                    If ret.breakChain Then Exit For
                End If
            Next

            'sorting
            ImagesContainer.Sort(DBElement)

            'create cache paths
            ImagesContainer.CreateCachePaths(DBElement)
        End If

        Return ret.Cancelled
    End Function
    ''' <summary>
    ''' Request that enabled tv image scrapers perform their functions on the supplied movie
    ''' </summary>
    ''' <param name="DBElement">TV Show to be scraped. Scraper will directly manipulate this structure</param>
    ''' <param name="ScrapeModifier">What kind of image is being scraped (poster, fanart, etc)</param>
    ''' <param name="ImagesContainer">Container of images that the scraper should add to</param>
    ''' <returns><c>True</c> if one of the scrapers was cancelled</returns>
    ''' <remarks>Note that if no movie scrapers are enabled, a silent warning is generated.</remarks>
    Public Function ScrapeImage_TV(ByRef DBElement As Database.DBElement, ByRef ImagesContainer As MediaContainers.SearchResultsContainer, ByVal ScrapeModifier As Structures.ScrapeModifier, ByVal showMessage As Boolean) As Boolean
        logger.Trace(String.Format("[APIModules] [ScrapeImage_TV] [Start] {0}", DBElement.TVShow.Title))
        If DBElement.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_TVShow(DBElement, showMessage) Then
            Dim modules As IEnumerable(Of _externalScraperModuleClass_Image_TV) = externalScrapersModules_Image_TV.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
            Dim ret As Interfaces.ModuleResult

            While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
                Application.DoEvents()
            End While

            'workaround to get MainFanarts for AllSeasonsFanarts, EpisodeFanarts and SeasonFanarts,
            'also get MainBanners, MainLandscapes and MainPosters for AllSeasonsBanners, AllSeasonsLandscapes and AllSeasonsPosters
            If ScrapeModifier.AllSeasonsBanner Then
                ScrapeModifier.MainBanner = True
                ScrapeModifier.SeasonBanner = True
            End If
            If ScrapeModifier.AllSeasonsFanart Then
                ScrapeModifier.MainFanart = True
                ScrapeModifier.SeasonFanart = True
            End If
            If ScrapeModifier.AllSeasonsLandscape Then
                ScrapeModifier.MainLandscape = True
                ScrapeModifier.SeasonLandscape = True
            End If
            If ScrapeModifier.AllSeasonsPoster Then
                ScrapeModifier.MainPoster = True
                ScrapeModifier.SeasonPoster = True
            End If
            If ScrapeModifier.EpisodeFanart Then
                ScrapeModifier.MainFanart = True
            End If
            If ScrapeModifier.MainExtrafanarts Then
                ScrapeModifier.MainFanart = True
            End If
            If ScrapeModifier.MainExtrathumbs Then
                ScrapeModifier.MainFanart = True
            End If
            If ScrapeModifier.SeasonFanart Then
                ScrapeModifier.MainFanart = True
            End If

            If (modules.Count() <= 0) Then
                logger.Warn("[APIModules] [ScrapeImage_TV] [Abort] No scrapers enabled")
            Else
                For Each _externalScraperModule As _externalScraperModuleClass_Image_TV In modules
                    logger.Trace(String.Format("[APIModules] [ScrapeImage_TV] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))
                    If QueryScraperCapabilities_Image_TV(_externalScraperModule, ScrapeModifier) Then
                        AddHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_TV
                        Dim aContainer As New MediaContainers.SearchResultsContainer
                        ret = _externalScraperModule.ProcessorModule.Scraper(DBElement, aContainer, ScrapeModifier)
                        If aContainer IsNot Nothing Then
                            ImagesContainer.EpisodeFanarts.AddRange(aContainer.EpisodeFanarts)
                            ImagesContainer.EpisodePosters.AddRange(aContainer.EpisodePosters)
                            ImagesContainer.SeasonBanners.AddRange(aContainer.SeasonBanners)
                            ImagesContainer.SeasonFanarts.AddRange(aContainer.SeasonFanarts)
                            ImagesContainer.SeasonLandscapes.AddRange(aContainer.SeasonLandscapes)
                            ImagesContainer.SeasonPosters.AddRange(aContainer.SeasonPosters)
                            ImagesContainer.MainBanners.AddRange(aContainer.MainBanners)
                            ImagesContainer.MainCharacterArts.AddRange(aContainer.MainCharacterArts)
                            ImagesContainer.MainClearArts.AddRange(aContainer.MainClearArts)
                            ImagesContainer.MainClearLogos.AddRange(aContainer.MainClearLogos)
                            ImagesContainer.MainFanarts.AddRange(aContainer.MainFanarts)
                            ImagesContainer.MainLandscapes.AddRange(aContainer.MainLandscapes)
                            ImagesContainer.MainPosters.AddRange(aContainer.MainPosters)
                        End If
                        RemoveHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_TV
                        If ret.breakChain Then Exit For
                    End If
                Next

                'sorting
                ImagesContainer.Sort(DBElement)

                'create cache paths
                ImagesContainer.CreateCachePaths(DBElement)
            End If

            Return ret.Cancelled
        Else
            Return True 'Cancelled
        End If
    End Function
    ''' <summary>
    ''' Request that enabled movie theme scrapers perform their functions on the supplied movie
    ''' </summary>
    ''' <param name="DBElement">Movie to be scraped. Scraper will directly manipulate this structure</param>
    ''' <param name="URLList">List of Themes objects that the scraper will append to. Note that only the URL is returned, 
    ''' not the full content of the trailer</param>
    ''' <returns><c>True</c> if one of the scrapers was cancelled</returns>
    ''' <remarks></remarks>
    Public Function ScrapeTheme_Movie(ByRef DBElement As Database.DBElement, ByRef URLList As List(Of Themes)) As Boolean
        logger.Trace(String.Format("[APIModules] [ScrapeTheme_Movie] [Start] {0}", DBElement.Filename))
        Dim modules As IEnumerable(Of _externalScraperModuleClass_Theme_Movie) = externalScrapersModules_Theme_Movie.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
        Dim ret As Interfaces.ModuleResult
        Dim aList As List(Of Themes)

        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While

        If (modules.Count() <= 0) Then
            logger.Warn("[APIModules] [ScrapeTheme_Movie] [Abort] No scrapers enabled")
        Else
            For Each _externalScraperModule As _externalScraperModuleClass_Theme_Movie In modules
                logger.Trace(String.Format("[APIModules] [ScrapeTheme_Movie] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))
                AddHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_Movie
                aList = New List(Of Themes)
                ret = _externalScraperModule.ProcessorModule.Scraper(DBElement, aList)
                If aList IsNot Nothing AndAlso aList.Count > 0 Then
                    For Each aIm In aList
                        URLList.Add(aIm)
                    Next
                End If
                RemoveHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_Movie
                If ret.breakChain Then Exit For
            Next
        End If
        Return ret.Cancelled
    End Function
    ''' <summary>
    ''' Request that enabled movie trailer scrapers perform their functions on the supplied movie
    ''' </summary>
    ''' <param name="DBElement">Movie to be scraped. Scraper will directly manipulate this structure</param>
    ''' <param name="Type">NOT ACTUALLY USED!</param>
    ''' <param name="TrailerList">List of Trailer objects that the scraper will append to. Note that only the URL is returned, 
    ''' not the full content of the trailer</param>
    ''' <returns><c>True</c> if one of the scrapers was cancelled</returns>
    ''' <remarks></remarks>
    Public Function ScrapeTrailer_Movie(ByRef DBElement As Database.DBElement, ByVal Type As Enums.ModifierType, ByRef TrailerList As List(Of MediaContainers.Trailer)) As Boolean
        logger.Trace(String.Format("[APIModules] [ScrapeTrailer_Movie] [Start] {0}", DBElement.Filename))
        Dim modules As IEnumerable(Of _externalScraperModuleClass_Trailer_Movie) = externalScrapersModules_Trailer_Movie.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
        Dim ret As Interfaces.ModuleResult

        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While

        If (modules.Count() <= 0) Then
            logger.Warn("[APIModules] [ScrapeTrailer_Movie] [Abort] No scrapers enabled")
        Else
            For Each _externalScraperModule As _externalScraperModuleClass_Trailer_Movie In modules
                logger.Trace(String.Format("[APIModules] [ScrapeTrailer_Movie] [Using] {0}", _externalScraperModule.ProcessorModule.ModuleName))
                AddHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_Movie
                Dim aList As New List(Of MediaContainers.Trailer)
                ret = _externalScraperModule.ProcessorModule.Scraper(DBElement, Type, aList)
                If aList IsNot Nothing Then
                    TrailerList.AddRange(aList)
                End If
                RemoveHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_Movie
                If ret.breakChain Then Exit For
            Next
        End If
        Return ret.Cancelled
    End Function

    Function ScraperWithCapabilityAnyEnabled_Image_Movie(ByVal ImageType As Enums.ModifierType) As Boolean
        Dim ret As Boolean = False
        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While
        For Each _externalScraperModule As _externalScraperModuleClass_Image_Movie In externalScrapersModules_Image_Movie.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
            Try
                ret = QueryScraperCapabilities_Image_Movie(_externalScraperModule, ImageType)
                If ret Then Exit For
            Catch ex As Exception
            End Try
        Next
        Return ret
    End Function

    Function ScraperWithCapabilityAnyEnabled_Image_MovieSet(ByVal ImageType As Enums.ModifierType) As Boolean
        Dim ret As Boolean = False
        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While
        For Each _externalScraperModule As _externalScraperModuleClass_Image_MovieSet In externalScrapersModules_Image_MovieSet.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
            Try
                ret = QueryScraperCapabilities_Image_MovieSet(_externalScraperModule, ImageType)
                If ret Then Exit For
            Catch ex As Exception
            End Try
        Next
        Return ret
    End Function

    Function ScraperWithCapabilityAnyEnabled_Image_TV(ByVal ImageType As Enums.ModifierType) As Boolean
        Dim ret As Boolean = False
        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While
        For Each _externalScraperModule As _externalScraperModuleClass_Image_TV In externalScrapersModules_Image_TV.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
            Try
                ret = QueryScraperCapabilities_Image_TV(_externalScraperModule, ImageType)
                If ret Then Exit For
            Catch ex As Exception
            End Try
        Next
        Return ret
    End Function

    Function ScraperWithCapabilityAnyEnabled_Theme_Movie(ByVal cap As Enums.ModifierType) As Boolean
        Dim ret As Boolean = False
        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While
        For Each _externalScraperModule As _externalScraperModuleClass_Theme_Movie In externalScrapersModules_Theme_Movie.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
            Try
                ret = True 'if a theme scraper is enabled we can exit.
                Exit For
            Catch ex As Exception
            End Try
        Next
        Return ret
    End Function

    Function ScraperWithCapabilityAnyEnabled_Theme_TV(ByVal cap As Enums.ModifierType) As Boolean
        Dim ret As Boolean = False
        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While
        For Each _externalScraperModule As _externalScraperModuleClass_Theme_TV In externalScrapersModules_Theme_TV.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
            Try
                ret = True 'if a theme scraper is enabled we can exit.
                Exit For
            Catch ex As Exception
            End Try
        Next
        Return ret
    End Function

    Function ScraperWithCapabilityAnyEnabled_Trailer_Movie(ByVal cap As Enums.ModifierType) As Boolean
        Dim ret As Boolean = False
        While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
            Application.DoEvents()
        End While
        For Each _externalScraperModule As _externalScraperModuleClass_Trailer_Movie In externalScrapersModules_Trailer_Movie.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
            Try
                ret = True 'if a trailer scraper is enabled we can exit.
                Exit For
            Catch ex As Exception
            End Try
        Next
        Return ret
    End Function

    ''' <summary>
    ''' Sets the enabled flag of the module identified by <paramref name="ModuleAssembly"/> to the value of <paramref name="value"/>
    ''' </summary>
    ''' <param name="ModuleAssembly"><c>String</c> representing the assembly name of the module</param>
    ''' <param name="value"><c>Boolean</c> value to set the enabled flag to</param>
    ''' <remarks></remarks>
    Public Sub SetModuleEnable_Generic(ByVal ModuleAssembly As String, ByVal value As Boolean)
        If (String.IsNullOrEmpty(ModuleAssembly)) Then
            logger.Error("Invalid ModuleAssembly")
            Return
        End If

        Dim modules As IEnumerable(Of _externalGenericModuleClass) = externalProcessorModules.Where(Function(p) p.AssemblyName = ModuleAssembly)
        If (modules.Count < 0) Then
            logger.Warn("No modules of type <{0}> were found", ModuleAssembly)
        Else
            For Each _externalProcessorModule As _externalGenericModuleClass In modules
                Try
                    _externalProcessorModule.ProcessorModule.Enabled = value
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name & Convert.ToChar(Windows.Forms.Keys.Tab) & "Could not set module <" & ModuleAssembly & "> to enabled status <" & value & ">", ex)
                End Try
            Next
        End If
    End Sub

    Public Sub SetScraperEnable_Data_Movie(ByVal ModuleAssembly As String, ByVal value As Boolean)
        If (String.IsNullOrEmpty(ModuleAssembly)) Then
            logger.Error("Invalid ModuleAssembly")
            Return
        End If

        Dim modules As IEnumerable(Of _externalScraperModuleClass_Data_Movie) = externalScrapersModules_Data_Movie.Where(Function(p) p.AssemblyName = ModuleAssembly)
        If (modules.Count < 0) Then
            logger.Warn("No modules of type <{0}> were found", ModuleAssembly)
        Else
            For Each _externalScraperModule As _externalScraperModuleClass_Data_Movie In modules
                Try
                    _externalScraperModule.ProcessorModule.ScraperEnabled = value
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name & Convert.ToChar(Windows.Forms.Keys.Tab) & "Could not set module <" & ModuleAssembly & "> to enabled status <" & value & ">", ex)
                End Try
            Next
        End If
    End Sub

    Public Sub SetScraperEnable_Data_MovieSet(ByVal ModuleAssembly As String, ByVal value As Boolean)
        If (String.IsNullOrEmpty(ModuleAssembly)) Then
            logger.Error("Invalid ModuleAssembly")
            Return
        End If

        Dim modules As IEnumerable(Of _externalScraperModuleClass_Data_MovieSet) = externalScrapersModules_Data_MovieSet.Where(Function(p) p.AssemblyName = ModuleAssembly)
        If (modules.Count < 0) Then
            logger.Warn("No modules of type <{0}> were found", ModuleAssembly)
        Else
            For Each _externalScraperModule As _externalScraperModuleClass_Data_MovieSet In modules
                Try
                    _externalScraperModule.ProcessorModule.ScraperEnabled = value
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name & Convert.ToChar(Windows.Forms.Keys.Tab) & "Could not set module <" & ModuleAssembly & "> to enabled status <" & value & ">", ex)
                End Try
            Next
        End If
    End Sub

    Public Sub SetScraperEnable_Data_TV(ByVal ModuleAssembly As String, ByVal value As Boolean)
        If (String.IsNullOrEmpty(ModuleAssembly)) Then
            logger.Error("Invalid ModuleAssembly")
            Return
        End If

        Dim modules As IEnumerable(Of _externalScraperModuleClass_Data_TV) = externalScrapersModules_Data_TV.Where(Function(p) p.AssemblyName = ModuleAssembly)
        If (modules.Count < 0) Then
            logger.Warn("No modules of type <{0}> were found", ModuleAssembly)
        Else
            For Each _externalScraperModule As _externalScraperModuleClass_Data_TV In modules
                Try
                    _externalScraperModule.ProcessorModule.ScraperEnabled = value
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name & Convert.ToChar(Windows.Forms.Keys.Tab) & "Could not set module <" & ModuleAssembly & "> to enabled status <" & value & ">", ex)
                End Try
            Next
        End If
    End Sub

    Public Sub SetScraperEnable_Image_Movie(ByVal ModuleAssembly As String, ByVal value As Boolean)
        If (String.IsNullOrEmpty(ModuleAssembly)) Then
            logger.Error("Invalid ModuleAssembly")
            Return
        End If

        Dim modules As IEnumerable(Of _externalScraperModuleClass_Image_Movie) = externalScrapersModules_Image_Movie.Where(Function(p) p.AssemblyName = ModuleAssembly)
        If (modules.Count < 0) Then
            logger.Warn("No modules of type <{0}> were found", ModuleAssembly)
        Else
            For Each _externalScraperModule As _externalScraperModuleClass_Image_Movie In modules
                Try
                    _externalScraperModule.ProcessorModule.ScraperEnabled = value
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name & Convert.ToChar(Windows.Forms.Keys.Tab) & "Could not set module <" & ModuleAssembly & "> to enabled status <" & value & ">", ex)
                End Try
            Next
        End If
    End Sub

    Public Sub SetScraperEnable_Image_MovieSet(ByVal ModuleAssembly As String, ByVal value As Boolean)
        If (String.IsNullOrEmpty(ModuleAssembly)) Then
            logger.Error("Invalid ModuleAssembly")
            Return
        End If

        Dim modules As IEnumerable(Of _externalScraperModuleClass_Image_MovieSet) = externalScrapersModules_Image_MovieSet.Where(Function(p) p.AssemblyName = ModuleAssembly)
        If (modules.Count < 0) Then
            logger.Warn("No modules of type <{0}> were found", ModuleAssembly)
        Else
            For Each _externalScraperModule As _externalScraperModuleClass_Image_MovieSet In modules
                Try
                    _externalScraperModule.ProcessorModule.ScraperEnabled = value
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name & Convert.ToChar(Windows.Forms.Keys.Tab) & "Could not set module <" & ModuleAssembly & "> to enabled status <" & value & ">", ex)
                End Try
            Next
        End If
    End Sub

    Public Sub SetScraperEnable_Image_TV(ByVal ModuleAssembly As String, ByVal value As Boolean)
        If (String.IsNullOrEmpty(ModuleAssembly)) Then
            logger.Error("Invalid ModuleAssembly")
            Return
        End If

        Dim modules As IEnumerable(Of _externalScraperModuleClass_Image_TV) = externalScrapersModules_Image_TV.Where(Function(p) p.AssemblyName = ModuleAssembly)
        If (modules.Count < 0) Then
            logger.Warn("No modules of type <{0}> were found", ModuleAssembly)
        Else
            For Each _externalScraperModule As _externalScraperModuleClass_Image_TV In externalScrapersModules_Image_TV.Where(Function(p) p.AssemblyName = ModuleAssembly)
                Try
                    _externalScraperModule.ProcessorModule.ScraperEnabled = value
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name, ex)
                End Try
            Next
        End If
    End Sub

    ''' <summary>
    ''' Sets the enabled flag of the module identified by <paramref name="ModuleAssembly"/> to the value of <paramref name="value"/>
    ''' </summary>
    ''' <param name="ModuleAssembly"><c>String</c> representing the assembly name of the module</param>
    ''' <param name="value"><c>Boolean</c> value to set the enabled flag to</param>
    ''' <remarks></remarks>

    Public Sub SetScraperEnable_Theme_Movie(ByVal ModuleAssembly As String, ByVal value As Boolean)
        If (String.IsNullOrEmpty(ModuleAssembly)) Then
            logger.Error("Invalid ModuleAssembly")
            Return
        End If

        Dim modules As IEnumerable(Of _externalScraperModuleClass_Theme_Movie) = externalScrapersModules_Theme_Movie.Where(Function(p) p.AssemblyName = ModuleAssembly)
        If (modules.Count < 0) Then
            logger.Warn("No modules of type <{0}> were found", ModuleAssembly)
        Else
            For Each _externalScraperModule As _externalScraperModuleClass_Theme_Movie In modules
                Try
                    _externalScraperModule.ProcessorModule.ScraperEnabled = value
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name & Convert.ToChar(Windows.Forms.Keys.Tab) & "Could not set module <" & ModuleAssembly & "> to enabled status <" & value & ">", ex)
                End Try
            Next
        End If
    End Sub

    Public Sub SetScraperEnable_Theme_TV(ByVal ModuleAssembly As String, ByVal value As Boolean)
        For Each _externalScraperModule As _externalScraperModuleClass_Theme_TV In externalScrapersModules_Theme_TV.Where(Function(p) p.AssemblyName = ModuleAssembly)
            Try
                _externalScraperModule.ProcessorModule.ScraperEnabled = value
            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
            End Try
        Next
    End Sub
    ''' <summary>
    ''' Sets the enabled flag of the module identified by <paramref name="ModuleAssembly"/> to the value of <paramref name="value"/>
    ''' </summary>
    ''' <param name="ModuleAssembly"><c>String</c> representing the assembly name of the module</param>
    ''' <param name="value"><c>Boolean</c> value to set the enabled flag to</param>
    ''' <remarks></remarks>

    Public Sub SetScraperEnable_Trailer_Movie(ByVal ModuleAssembly As String, ByVal value As Boolean)
        If (String.IsNullOrEmpty(ModuleAssembly)) Then
            logger.Error("Invalid ModuleAssembly")
            Return
        End If

        Dim modules As IEnumerable(Of _externalScraperModuleClass_Trailer_Movie) = externalScrapersModules_Trailer_Movie.Where(Function(p) p.AssemblyName = ModuleAssembly)
        If (modules.Count < 0) Then
            logger.Warn("No modules of type <{0}> were found", ModuleAssembly)
        Else
            For Each _externalScraperModule As _externalScraperModuleClass_Trailer_Movie In modules
                Try
                    _externalScraperModule.ProcessorModule.ScraperEnabled = value
                Catch ex As Exception
                    logger.Error(New StackFrame().GetMethod().Name & Convert.ToChar(Windows.Forms.Keys.Tab) & "Could not set module <" & ModuleAssembly & "> to enabled status <" & value & ">", ex)
                End Try
            Next
        End If
    End Sub

    Function ChangeEpisode(ByVal ShowID As Integer, ByVal TVDBID As String, ByVal Lang As String) As MediaContainers.EpisodeDetails
        'Dim testDBTV As Database.DBElement = Master.currTV
        'If testDBTV.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_TVEpisode(testDBTV, True) Then
        '    Dim ret As Interfaces.ModuleResult
        '    Dim epDetails As New MediaContainers.EpisodeDetails
        '    While Not (bwloadGenericModules_done AndAlso bwloadScrapersModules_Movie_done AndAlso bwloadScrapersModules_MovieSet_done AndAlso bwloadScrapersModules_TV_done)
        '        Application.DoEvents()
        '    End While
        '    For Each _externalScraperModule As _externalScraperModuleClass_Data_TV In externalScrapersModules_Data_TV.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)
        '        Try
        '            'ret = _externalScraperModule.ProcessorModule.ChangeEpisode(ShowID, TVDBID, Lang, epDetails)
        '        Catch ex As Exception
        '        End Try
        '        If ret.breakChain Then Exit For
        '    Next
        '    Return epDetails
        'Else
        Return Nothing 'Cancelled
        'End If
    End Function

    Private Sub GenericRunCallBack(ByVal mType As Enums.ModuleEventType, ByRef _params As List(Of Object))
        RaiseEvent GenericEvent(mType, _params)
    End Sub

#End Region 'Methods

#Region "Nested Types"

    Structure AssemblyListItem

#Region "Fields"

        Public Assembly As System.Reflection.Assembly
        Public AssemblyName As String

#End Region 'Fields

    End Structure

    Structure VersionItem

#Region "Fields"

        Public AssemblyFileName As String
        Public Name As String
        Public NeedUpdate As Boolean
        Public Version As String

#End Region 'Fields

    End Structure

    Class EmberRuntimeObjects

#Region "Fields"

        Private _ContextMenuMovieList As ContextMenuStrip
        Private _ContextMenuMovieSetList As ContextMenuStrip
        Private _ContextMenuTVEpisodeList As ContextMenuStrip
        Private _ContextMenuTVSeasonList As ContextMenuStrip
        Private _ContextMenuTVShowList As ContextMenuStrip
        Private _FilterMovies As String
        Private _FilterMoviesSearch As String
        Private _FilterMoviesType As String
        Private _FilterShows As String
        Private _FilterShowsSearch As String
        Private _FilterShowsType As String
        Private _ListMovieSets As String
        Private _ListMovies As String
        Private _ListShows As String
        Private _LoadMedia As LoadMedia
        Private _MainMenu As MenuStrip
        Private _MainTabControl As TabControl
        Private _MainToolStrip As ToolStrip
        Private _MediaListMovieSets As DataGridView
        Private _MediaListMovies As DataGridView
        Private _MediaListTVEpisodes As DataGridView
        Private _MediaListTVSeasons As DataGridView
        Private _MediaListTVShows As DataGridView
        Private _MediaTabSelected As Structures.MainTabType
        Private _OpenImageViewer As OpenImageViewer
        Private _TrayMenu As ContextMenuStrip


#End Region 'Fields

#Region "Delegates"

        Delegate Sub LoadMedia(ByVal Scan As Structures.ScanOrClean, ByVal SourceID As Long)

        'all runtime object including Function (delegate) that need to be exposed to Modules
        Delegate Sub OpenImageViewer(ByVal _Image As Image)

#End Region 'Delegates

#Region "Properties"

        Public Property ListMovies() As String
            Get
                Return If(_ListMovies IsNot Nothing, _ListMovies, "movielist")
            End Get
            Set(ByVal value As String)
                _ListMovies = value
            End Set
        End Property

        Public Property ListMovieSets() As String
            Get
                Return If(_ListMovieSets IsNot Nothing, _ListMovieSets, "setslist")
            End Get
            Set(ByVal value As String)
                _ListMovieSets = value
            End Set
        End Property

        Public Property ListShows() As String
            Get
                Return If(_ListShows IsNot Nothing, _ListShows, "tvshowlist")
            End Get
            Set(ByVal value As String)
                _ListShows = value
            End Set
        End Property

        Public Property FilterMovies() As String
            Get
                Return _FilterMovies
            End Get
            Set(ByVal value As String)
                _FilterMovies = value
            End Set
        End Property

        Public Property FilterMoviesSearch() As String
            Get
                Return _FilterMoviesSearch
            End Get
            Set(ByVal value As String)
                _FilterMoviesSearch = value
            End Set
        End Property

        Public Property FilterMoviesType() As String
            Get
                Return _FilterMoviesType
            End Get
            Set(ByVal value As String)
                _FilterMoviesType = value
            End Set
        End Property
        Public Property FilterShows() As String
            Get
                Return _FilterShows
            End Get
            Set(ByVal value As String)
                _FilterShows = value
            End Set
        End Property

        Public Property FilterShowsSearch() As String
            Get
                Return _FilterShowsSearch
            End Get
            Set(ByVal value As String)
                _FilterShowsSearch = value
            End Set
        End Property

        Public Property FilterShowsType() As String
            Get
                Return _FilterShowsType
            End Get
            Set(ByVal value As String)
                _FilterShowsType = value
            End Set
        End Property

        Public Property MediaTabSelected() As Structures.MainTabType
            Get
                Return _MediaTabSelected
            End Get
            Set(ByVal value As Structures.MainTabType)
                _MediaTabSelected = value
            End Set
        End Property

        Public Property MainToolStrip() As ToolStrip
            Get
                Return _MainToolStrip
            End Get
            Set(ByVal value As ToolStrip)
                _MainToolStrip = value
            End Set
        End Property

        Public Property MediaListMovies() As DataGridView
            Get
                Return _MediaListMovies
            End Get
            Set(ByVal value As DataGridView)
                _MediaListMovies = value
            End Set
        End Property

        Public Property MediaListMovieSets() As DataGridView
            Get
                Return _MediaListMovieSets
            End Get
            Set(ByVal value As DataGridView)
                _MediaListMovieSets = value
            End Set
        End Property

        Public Property MediaListTVEpisodes() As DataGridView
            Get
                Return _MediaListTVEpisodes
            End Get
            Set(ByVal value As DataGridView)
                _MediaListTVEpisodes = value
            End Set
        End Property

        Public Property MediaListTVSeasons() As DataGridView
            Get
                Return _MediaListTVSeasons
            End Get
            Set(ByVal value As DataGridView)
                _MediaListTVSeasons = value
            End Set
        End Property

        Public Property MediaListTVShows() As DataGridView
            Get
                Return _MediaListTVShows
            End Get
            Set(ByVal value As DataGridView)
                _MediaListTVShows = value
            End Set
        End Property

        Public Property ContextMenuMovieList() As ContextMenuStrip
            Get
                Return _ContextMenuMovieList
            End Get
            Set(ByVal value As ContextMenuStrip)
                _ContextMenuMovieList = value
            End Set
        End Property

        Public Property ContextMenuMovieSetList() As ContextMenuStrip
            Get
                Return _ContextMenuMovieSetList
            End Get
            Set(ByVal value As ContextMenuStrip)
                _ContextMenuMovieSetList = value
            End Set
        End Property

        Public Property ContextMenuTVEpisodeList() As ContextMenuStrip
            Get
                Return _ContextMenuTVEpisodeList
            End Get
            Set(ByVal value As ContextMenuStrip)
                _ContextMenuTVEpisodeList = value
            End Set
        End Property

        Public Property ContextMenuTVSeasonList() As ContextMenuStrip
            Get
                Return _ContextMenuTVSeasonList
            End Get
            Set(ByVal value As ContextMenuStrip)
                _ContextMenuTVSeasonList = value
            End Set
        End Property

        Public Property ContextMenuTVShowList() As ContextMenuStrip
            Get
                Return _ContextMenuTVShowList
            End Get
            Set(ByVal value As ContextMenuStrip)
                _ContextMenuTVShowList = value
            End Set
        End Property

        Public Property MainMenu() As MenuStrip
            Get
                Return _MainMenu
            End Get
            Set(ByVal value As MenuStrip)
                _MainMenu = value
            End Set
        End Property

        Public Property TrayMenu() As ContextMenuStrip
            Get
                Return _TrayMenu
            End Get
            Set(ByVal value As ContextMenuStrip)
                _TrayMenu = value
            End Set
        End Property

        Public Property MainTabControl() As TabControl
            Get
                Return _MainTabControl
            End Get
            Set(ByVal value As TabControl)
                _MainTabControl = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub DelegateLoadMedia(ByRef lm As LoadMedia)
            'Setup from EmberAPP
            _LoadMedia = lm
        End Sub

        Public Sub DelegateOpenImageViewer(ByRef IV As OpenImageViewer)
            _OpenImageViewer = IV
        End Sub

        Public Sub InvokeLoadMedia(ByVal Scan As Structures.ScanOrClean, Optional ByVal SourceID As Long = -1)
            'Invoked from Modules
            _LoadMedia.Invoke(Scan, SourceID)
        End Sub

        Public Sub InvokeOpenImageViewer(ByRef _image As Image)
            _OpenImageViewer.Invoke(_image)
        End Sub

#End Region 'Methods

    End Class

    Class _externalGenericModuleClass

#Region "Fields"

        Public AssemblyFileName As String

        'Public Enabled As Boolean
        Public AssemblyName As String
        Public ModuleOrder As Integer 'TODO: not important at this point.. for 1.5
        Public ProcessorModule As Interfaces.GenericModule 'Object
        Public Type As List(Of Enums.ModuleEventType)
        Public ContentType As Enums.ContentType = Enums.ContentType.Generic

#End Region 'Fields

    End Class

    Class _externalScraperModuleClass_Data_Movie

#Region "Fields"

        Public AssemblyFileName As String
        Public AssemblyName As String
        Public ProcessorModule As Interfaces.ScraperModule_Data_Movie 'Object
        Public ModuleOrder As Integer
        Public ContentType As Enums.ContentType = Enums.ContentType.Movie

#End Region 'Fields

    End Class

    Class _externalScraperModuleClass_Data_MovieSet

#Region "Fields"

        Public AssemblyFileName As String
        Public AssemblyName As String
        Public ProcessorModule As Interfaces.ScraperModule_Data_MovieSet 'Object
        Public ModuleOrder As Integer
        Public ContentType As Enums.ContentType = Enums.ContentType.MovieSet

#End Region 'Fields

    End Class

    Class _externalScraperModuleClass_Data_TV

#Region "Fields"

        Public AssemblyFileName As String
        Public AssemblyName As String
        Public ProcessorModule As Interfaces.ScraperModule_Data_TV 'Object
        Public ModuleOrder As Integer
        Public ContentType As Enums.ContentType = Enums.ContentType.TV

#End Region 'Fields

    End Class

    Class _externalScraperModuleClass_Image_Movie

#Region "Fields"

        Public AssemblyFileName As String
        Public AssemblyName As String
        Public ProcessorModule As Interfaces.ScraperModule_Image_Movie  'Object
        Public ModuleOrder As Integer
        Public ContentType As Enums.ContentType = Enums.ContentType.Movie

#End Region 'Fields

    End Class

    Class _externalScraperModuleClass_Image_MovieSet

#Region "Fields"

        Public AssemblyFileName As String
        Public AssemblyName As String
        Public ProcessorModule As Interfaces.ScraperModule_Image_MovieSet  'Object
        Public ModuleOrder As Integer
        Public ContentType As Enums.ContentType = Enums.ContentType.MovieSet

#End Region 'Fields

    End Class

    Class _externalScraperModuleClass_Image_TV

#Region "Fields"

        Public AssemblyFileName As String
        Public AssemblyName As String
        Public ProcessorModule As Interfaces.ScraperModule_Image_TV  'Object
        Public ModuleOrder As Integer
        Public ContentType As Enums.ContentType = Enums.ContentType.TV

#End Region 'Fields

    End Class

    Class _externalScraperModuleClass_Theme_Movie

#Region "Fields"

        Public AssemblyFileName As String
        Public AssemblyName As String
        Public ProcessorModule As Interfaces.ScraperModule_Theme_Movie     'Object
        Public ModuleOrder As Integer
        Public ContentType As Enums.ContentType = Enums.ContentType.Movie

#End Region 'Fields

    End Class

    Class _externalScraperModuleClass_Theme_TV

#Region "Fields"

        Public AssemblyFileName As String
        Public AssemblyName As String
        Public ProcessorModule As Interfaces.ScraperModule_Theme_TV  'Object
        Public ModuleOrder As Integer
        Public ContentType As Enums.ContentType = Enums.ContentType.TV

#End Region 'Fields

    End Class

    Class _externalScraperModuleClass_Trailer_Movie

#Region "Fields"

        Public AssemblyFileName As String
        Public AssemblyName As String
        Public ProcessorModule As Interfaces.ScraperModule_Trailer_Movie     'Object
        Public ModuleOrder As Integer
        Public ContentType As Enums.ContentType = Enums.ContentType.Movie

#End Region 'Fields

    End Class

    <XmlRoot("EmberModule")> _
    Class _XMLEmberModuleClass

#Region "Fields"

        Public AssemblyFileName As String
        Public AssemblyName As String
        Public ContentType As Enums.ContentType
        Public GenericEnabled As Boolean
        Public PostScraperEnabled As Boolean    'only for TV
        Public PostScraperOrder As Integer      'only for TV
        Public ModuleEnabled As Boolean
        Public ModuleOrder As Integer

#End Region 'Fields

    End Class

#End Region 'Nested Types

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class