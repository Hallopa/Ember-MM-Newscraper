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

Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml
Imports System.Xml.Serialization
Imports NLog
Imports System.Windows.Forms

Public Class NFO

#Region "Fields"
    Shared logger As Logger = NLog.LogManager.GetCurrentClassLogger()
#End Region

#Region "Methods"
    ''' <summary>
    ''' Returns the "merged" result of each data scraper results
    ''' </summary>
    ''' <param name="DBMovie">Movie to be scraped</param>
    ''' <param name="ScrapedList"><c>List(Of MediaContainers.Movie)</c> which contains unfiltered results of each data scraper</param>
    ''' <returns>The scrape result of movie (after applying various global scraper settings here)</returns>
    ''' <remarks>
    ''' This is used to determine the result of data scraping by going through all scraperesults of every data scraper and applying global data scraper settings here!
    ''' 
    ''' 2014/09/01 Cocotus - First implementation: Moved all global lock settings in various data scrapers to this function, only apply them once and not in every data scraper module! Should be more maintainable!
    ''' </remarks>
    Public Shared Function MergeDataScraperResults_Movie(ByVal DBMovie As Database.DBElement, ByVal ScrapedList As List(Of MediaContainers.Movie), ByVal ScrapeType As Enums.ScrapeType, ByVal ScrapeOptions As Structures.ScrapeOptions) As Database.DBElement

        'protects the first scraped result against overwriting
        Dim new_Actors As Boolean = False
        Dim new_Certification As Boolean = False
        Dim new_CollectionID As Boolean = False
        Dim new_Collections As Boolean = False
        Dim new_Countries As Boolean = False
        Dim new_Credits As Boolean = False
        Dim new_Directors As Boolean = False
        Dim new_Genres As Boolean = False
        Dim new_MPAA As Boolean = False
        Dim new_OriginalTitle As Boolean = False
        Dim new_Outline As Boolean = False
        Dim new_Plot As Boolean = False
        Dim new_Rating As Boolean = False
        Dim new_ReleaseDate As Boolean = False
        Dim new_Runtime As Boolean = False
        Dim new_Studio As Boolean = False
        Dim new_Tagline As Boolean = False
        Dim new_Title As Boolean = False
        Dim new_Top250 As Boolean = False
        Dim new_Trailer As Boolean = False
        Dim new_Year As Boolean = False

        'If "Use Preview Datascraperresults" option is enabled, a preview window which displays all datascraperresults will be opened before showing the Edit Movie page!
        If (ScrapeType = Enums.ScrapeType.SingleScrape OrElse ScrapeType = Enums.ScrapeType.SingleField) AndAlso Master.eSettings.MovieScraperUseDetailView AndAlso ScrapedList.Count > 0 Then
            PreviewDataScraperResults(ScrapedList)
        End If

        For Each scrapedmovie In ScrapedList

            'IDs
            If scrapedmovie.IMDBIDSpecified Then
                DBMovie.Movie.IMDBID = scrapedmovie.IMDBID
                DBMovie.Movie.ID = scrapedmovie.ID
            End If
            If scrapedmovie.TMDBIDSpecified Then
                DBMovie.Movie.TMDBID = scrapedmovie.TMDBID
            End If

            'Actors
            If (Not DBMovie.Movie.ActorsSpecified OrElse Not Master.eSettings.MovieLockActors) AndAlso ScrapeOptions.bMainActors AndAlso _
                scrapedmovie.ActorsSpecified AndAlso Master.eSettings.MovieScraperCast AndAlso Not new_Actors Then

                If Master.eSettings.MovieScraperCastWithImgOnly Then
                    For i = scrapedmovie.Actors.Count - 1 To 0 Step -1
                        If String.IsNullOrEmpty(scrapedmovie.Actors(i).URLOriginal) Then
                            scrapedmovie.Actors.RemoveAt(i)
                        End If
                    Next
                End If

                If Master.eSettings.MovieScraperCastLimit > 0 AndAlso scrapedmovie.Actors.Count > Master.eSettings.MovieScraperCastLimit Then
                    scrapedmovie.Actors.RemoveRange(Master.eSettings.MovieScraperCastLimit, scrapedmovie.Actors.Count - Master.eSettings.MovieScraperCastLimit)
                End If

                DBMovie.Movie.Actors = scrapedmovie.Actors
                'added check if there's any actors left to add, if not then try with results of following scraper...
                If scrapedmovie.ActorsSpecified Then
                    new_Actors = True
                    'add numbers for ordering
                    Dim iOrder As Integer = 0
                    For Each actor In scrapedmovie.Actors
                        actor.Order = iOrder
                        iOrder += 1
                    Next
                End If

            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperCast AndAlso Not Master.eSettings.MovieLockActors Then
                DBMovie.Movie.Actors.Clear()
            End If

            'Certification
            If (Not DBMovie.Movie.CertificationsSpecified OrElse Not Master.eSettings.MovieLockCert) AndAlso ScrapeOptions.bMainCertifications AndAlso _
                scrapedmovie.CertificationsSpecified AndAlso Master.eSettings.MovieScraperCert AndAlso Not new_Certification Then
                If Master.eSettings.MovieScraperCertLang = Master.eLang.All Then
                    DBMovie.Movie.Certifications.Clear()
                    DBMovie.Movie.Certifications.AddRange(scrapedmovie.Certifications)
                    new_Certification = True
                Else
                    For Each tCert In scrapedmovie.Certifications
                        If tCert.StartsWith(APIXML.CertLanguagesXML.Language.FirstOrDefault(Function(l) l.abbreviation = Master.eSettings.MovieScraperCertLang).name) Then
                            DBMovie.Movie.Certifications.Clear()
                            DBMovie.Movie.Certifications.Add(tCert)
                            new_Certification = True
                            Exit For
                        End If
                    Next
                End If
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperCert AndAlso Not Master.eSettings.MovieLockCert Then
                DBMovie.Movie.Certifications.Clear()
            End If

            'Credits
            If (Not DBMovie.Movie.CreditsSpecified OrElse Not Master.eSettings.MovieLockCredits) AndAlso _
                scrapedmovie.CreditsSpecified AndAlso Master.eSettings.MovieScraperCredits AndAlso Not new_Credits Then
                DBMovie.Movie.Credits.Clear()
                DBMovie.Movie.Credits.AddRange(scrapedmovie.Credits)
                new_Credits = True
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperCredits AndAlso Not Master.eSettings.MovieLockCredits Then
                DBMovie.Movie.Credits.Clear()
            End If

            'Collection ID
            If (Not DBMovie.Movie.TMDBColIDSpecified OrElse Not Master.eSettings.MovieLockCollectionID) AndAlso ScrapeOptions.bMainCollectionID AndAlso _
                scrapedmovie.TMDBColIDSpecified AndAlso Master.eSettings.MovieScraperCollectionID AndAlso Not new_CollectionID Then
                DBMovie.Movie.TMDBColID = scrapedmovie.TMDBColID
                new_CollectionID = True
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperCollectionID AndAlso Not Master.eSettings.MovieLockCollectionID Then
                DBMovie.Movie.TMDBColID = String.Empty
            End If

            'Collections
            If (DBMovie.Movie.Sets.Count = 0 OrElse Not Master.eSettings.MovieLockCollections) AndAlso _
                scrapedmovie.Sets.Count > 0 AndAlso Master.eSettings.MovieScraperCollectionsAuto AndAlso Not new_Collections Then
                DBMovie.Movie.Sets.Clear()
                For Each movieset In scrapedmovie.Sets
                    If Not String.IsNullOrEmpty(movieset.Title) Then
                        For Each sett As AdvancedSettingsSetting In clsAdvancedSettings.GetAllSettings.Where(Function(y) y.Name.StartsWith("MovieSetTitleRenamer:"))
                            movieset.Title = movieset.Title.Replace(sett.Name.Substring(21), sett.Value)
                        Next
                    End If
                Next
                DBMovie.Movie.Sets.AddRange(scrapedmovie.Sets)
                new_Collections = True
            End If

            'Countries
            If (Not DBMovie.Movie.CountriesSpecified OrElse Not Master.eSettings.MovieLockCountry) AndAlso ScrapeOptions.bMainCountries AndAlso _
                scrapedmovie.CountriesSpecified AndAlso Master.eSettings.MovieScraperCountry AndAlso Not new_Countries Then
                DBMovie.Movie.Countries.Clear()
                DBMovie.Movie.Countries.AddRange(scrapedmovie.Countries)
                new_Countries = True
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperCountry AndAlso Not Master.eSettings.MovieLockCountry Then
                DBMovie.Movie.Countries.Clear()
            End If

            'Directors
            If (Not DBMovie.Movie.DirectorsSpecified OrElse Not Master.eSettings.MovieLockDirector) AndAlso ScrapeOptions.bMainDirectors AndAlso _
                scrapedmovie.DirectorsSpecified AndAlso Master.eSettings.MovieScraperDirector AndAlso Not new_Directors Then
                DBMovie.Movie.Directors.Clear()
                DBMovie.Movie.Directors.AddRange(scrapedmovie.Directors)
                new_Directors = True
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperDirector AndAlso Not Master.eSettings.MovieLockDirector Then
                DBMovie.Movie.Directors.Clear()
            End If

            'Genres
            If (Not DBMovie.Movie.GenresSpecified OrElse Not Master.eSettings.MovieLockGenre) AndAlso ScrapeOptions.bMainGenres AndAlso
                scrapedmovie.GenresSpecified AndAlso Master.eSettings.MovieScraperGenre AndAlso Not new_Genres Then

                Dim tGenre As List(Of String) = StringUtils.GenreFilter(scrapedmovie.Genres)

                If Master.eSettings.MovieScraperGenreLimit > 0 AndAlso Master.eSettings.MovieScraperGenreLimit < tGenre.Count AndAlso tGenre.Count > 0 Then
                    tGenre.RemoveRange(Master.eSettings.MovieScraperGenreLimit, tGenre.Count - Master.eSettings.MovieScraperGenreLimit)
                End If
                DBMovie.Movie.Genres.Clear()
                DBMovie.Movie.Genres.AddRange(tGenre)
                new_Genres = True
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperGenre AndAlso Not Master.eSettings.MovieLockGenre Then
                DBMovie.Movie.Genres.Clear()
            End If

            'MPAA
            If (Not DBMovie.Movie.MPAASpecified OrElse Not Master.eSettings.MovieLockMPAA) AndAlso ScrapeOptions.bMainMPAA AndAlso _
                scrapedmovie.MPAASpecified AndAlso Master.eSettings.MovieScraperMPAA AndAlso Not new_MPAA Then
                DBMovie.Movie.MPAA = scrapedmovie.MPAA
                new_MPAA = True
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperMPAA AndAlso Not Master.eSettings.MovieLockMPAA Then
                DBMovie.Movie.MPAA = String.Empty
            End If

            'Originaltitle
            If (Not DBMovie.Movie.OriginalTitleSpecified OrElse Not Master.eSettings.MovieLockOriginalTitle) AndAlso ScrapeOptions.bMainOriginalTitle AndAlso _
                scrapedmovie.OriginalTitleSpecified AndAlso Master.eSettings.MovieScraperOriginalTitle AndAlso Not new_OriginalTitle Then
                DBMovie.Movie.OriginalTitle = scrapedmovie.OriginalTitle
                new_OriginalTitle = True
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperOriginalTitle AndAlso Not Master.eSettings.MovieLockOriginalTitle Then
                DBMovie.Movie.OriginalTitle = String.Empty
            End If

            'Outline
            If (Not DBMovie.Movie.OutlineSpecified OrElse Not Master.eSettings.MovieLockOutline) AndAlso ScrapeOptions.bMainOutline AndAlso _
                scrapedmovie.OutlineSpecified AndAlso Master.eSettings.MovieScraperOutline AndAlso Not new_Outline Then
                DBMovie.Movie.Outline = scrapedmovie.Outline
                new_Outline = True
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperOutline AndAlso Not Master.eSettings.MovieLockOutline Then
                DBMovie.Movie.Outline = String.Empty
            End If
            'check if brackets should be removed...
            If Master.eSettings.MovieScraperCleanPlotOutline Then
                DBMovie.Movie.Outline = StringUtils.RemoveBrackets(DBMovie.Movie.Outline)
            End If

            'Plot
            If (Not DBMovie.Movie.PlotSpecified OrElse Not Master.eSettings.MovieLockPlot) AndAlso ScrapeOptions.bMainPlot AndAlso _
                scrapedmovie.PlotSpecified AndAlso Master.eSettings.MovieScraperPlot AndAlso Not new_Plot Then
                DBMovie.Movie.Plot = scrapedmovie.Plot
                new_Plot = True
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperPlot AndAlso Not Master.eSettings.MovieLockPlot Then
                DBMovie.Movie.Plot = String.Empty
            End If
            'check if brackets should be removed...
            If Master.eSettings.MovieScraperCleanPlotOutline Then
                DBMovie.Movie.Plot = StringUtils.RemoveBrackets(DBMovie.Movie.Plot)
            End If

            'Rating
            If (Not DBMovie.Movie.RatingSpecified OrElse Not Master.eSettings.MovieLockRating) AndAlso ScrapeOptions.bMainRating AndAlso _
                scrapedmovie.RatingSpecified AndAlso Master.eSettings.MovieScraperRating AndAlso Not new_Rating Then
                DBMovie.Movie.Rating = scrapedmovie.Rating
                DBMovie.Movie.Votes = Regex.Replace(scrapedmovie.Votes, "\D", String.Empty)
                new_Rating = True
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperRating AndAlso Not Master.eSettings.MovieLockRating Then
                DBMovie.Movie.Rating = String.Empty
                DBMovie.Movie.Votes = String.Empty
            End If

            'ReleaseDate
            If (Not DBMovie.Movie.ReleaseDateSpecified OrElse Not Master.eSettings.MovieLockReleaseDate) AndAlso ScrapeOptions.bMainRelease AndAlso _
                scrapedmovie.ReleaseDateSpecified AndAlso Master.eSettings.MovieScraperRelease AndAlso Not new_ReleaseDate Then
                If Master.eSettings.MovieScraperReleaseFormat = False Then
                    Dim formatteddate As Date
                    If DateTime.TryParseExact(scrapedmovie.ReleaseDate, "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentUICulture, Globalization.DateTimeStyles.None, formatteddate) Then
                        DBMovie.Movie.ReleaseDate = Strings.FormatDateTime(formatteddate, Microsoft.VisualBasic.DateFormat.ShortDate).ToString
                    Else
                        DBMovie.Movie.ReleaseDate = scrapedmovie.ReleaseDate
                    End If
                Else
                    DBMovie.Movie.ReleaseDate = scrapedmovie.ReleaseDate
                End If
                new_ReleaseDate = True
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperRelease AndAlso Not Master.eSettings.MovieLockReleaseDate Then
                DBMovie.Movie.ReleaseDate = String.Empty
            End If

            'Studios
            If (Not DBMovie.Movie.StudiosSpecified OrElse Not Master.eSettings.MovieLockStudio) AndAlso ScrapeOptions.bMainStudios AndAlso _
                scrapedmovie.StudiosSpecified AndAlso Master.eSettings.MovieScraperStudio AndAlso Not new_Studio Then
                DBMovie.Movie.Studios.Clear()

                Dim _studios As New List(Of String)
                _studios.AddRange(scrapedmovie.Studios)

                If Master.eSettings.MovieScraperStudioWithImgOnly Then
                    For i = _studios.Count - 1 To 0 Step -1
                        If APIXML.dStudios.ContainsKey(_studios.Item(i).ToLower) = False Then
                            _studios.RemoveAt(i)
                        End If
                    Next
                End If

                If Master.eSettings.MovieScraperStudioLimit > 0 AndAlso Master.eSettings.MovieScraperStudioLimit < _studios.Count AndAlso _studios.Count > 0 Then
                    _studios.RemoveRange(Master.eSettings.MovieScraperStudioLimit, _studios.Count - Master.eSettings.MovieScraperStudioLimit)
                End If


                DBMovie.Movie.Studios.AddRange(_studios)
                'added check if there's any studios left to add, if not then try with results of following scraper...
                If _studios.Count > 0 Then
                    new_Studio = True
                End If


            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperStudio AndAlso Not Master.eSettings.MovieLockStudio Then
                DBMovie.Movie.Studios.Clear()
            End If

            'Tagline
            If (Not DBMovie.Movie.TaglineSpecified OrElse Not Master.eSettings.MovieLockTagline) AndAlso ScrapeOptions.bMainTagline AndAlso _
                scrapedmovie.TaglineSpecified AndAlso Master.eSettings.MovieScraperTagline AndAlso Not new_Tagline Then
                DBMovie.Movie.Tagline = scrapedmovie.Tagline
                new_Tagline = True
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperTagline AndAlso Not Master.eSettings.MovieLockTagline Then
                DBMovie.Movie.Tagline = String.Empty
            End If

            'Title
            If (Not DBMovie.Movie.TitleSpecified OrElse Not Master.eSettings.MovieLockTitle) AndAlso ScrapeOptions.bMainTitle AndAlso _
                scrapedmovie.TitleSpecified AndAlso Master.eSettings.MovieScraperTitle AndAlso Not new_Title Then
                DBMovie.Movie.Title = scrapedmovie.Title
                new_Title = True
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperTitle AndAlso Not Master.eSettings.MovieLockTitle Then
                DBMovie.Movie.Title = String.Empty
            End If

            'Top250
            If (Not DBMovie.Movie.Top250Specified OrElse Not Master.eSettings.MovieLockTop250) AndAlso ScrapeOptions.bMainTop250 AndAlso _
                scrapedmovie.Top250Specified AndAlso Master.eSettings.MovieScraperTop250 AndAlso Not new_Top250 Then
                DBMovie.Movie.Top250 = scrapedmovie.Top250
                new_Top250 = True
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperTop250 AndAlso Not Master.eSettings.MovieLockTop250 Then
                DBMovie.Movie.Top250 = String.Empty
            End If

            'Trailer
            If (Not DBMovie.Movie.TrailerSpecified OrElse Not Master.eSettings.MovieLockTrailer) AndAlso ScrapeOptions.bMainTrailer AndAlso _
                scrapedmovie.TrailerSpecified AndAlso Master.eSettings.MovieScraperTrailer AndAlso Not new_Trailer Then
                If Master.eSettings.MovieScraperXBMCTrailerFormat AndAlso YouTube.UrlUtils.IsYouTubeURL(scrapedmovie.Trailer) Then
                    DBMovie.Movie.Trailer = String.Concat("plugin://plugin.video.youtube/?action=play_video&videoid=", YouTube.UrlUtils.GetVideoID(scrapedmovie.Trailer))
                Else
                    DBMovie.Movie.Trailer = scrapedmovie.Trailer
                End If
                new_Trailer = True
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperTrailer AndAlso Not Master.eSettings.MovieLockTrailer Then
                DBMovie.Movie.Trailer = String.Empty
            End If

            'Year
            If (Not DBMovie.Movie.YearSpecified OrElse Not Master.eSettings.MovieLockYear) AndAlso ScrapeOptions.bMainYear AndAlso _
                scrapedmovie.YearSpecified AndAlso Master.eSettings.MovieScraperYear AndAlso Not new_Year Then
                DBMovie.Movie.Year = scrapedmovie.Year
                new_Year = True
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperYear AndAlso Not Master.eSettings.MovieLockYear Then
                DBMovie.Movie.Year = String.Empty
            End If

            'Runtime
            If (Not DBMovie.Movie.RuntimeSpecified OrElse Not Master.eSettings.MovieLockRuntime) AndAlso ScrapeOptions.bMainRuntime AndAlso _
                scrapedmovie.RuntimeSpecified AndAlso Master.eSettings.MovieScraperRuntime AndAlso Not new_Runtime Then
                DBMovie.Movie.Runtime = scrapedmovie.Runtime
                new_Runtime = True
            ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperRuntime AndAlso Not Master.eSettings.MovieLockRuntime Then
                DBMovie.Movie.Runtime = String.Empty
            End If

        Next

        'Certification for MPAA
        If DBMovie.Movie.CertificationsSpecified AndAlso Master.eSettings.MovieScraperCertForMPAA AndAlso _
            (Not Master.eSettings.MovieScraperCertForMPAAFallback AndAlso (Not DBMovie.Movie.MPAASpecified OrElse Not Master.eSettings.MovieLockMPAA) OrElse _
             Not new_MPAA AndAlso (Not DBMovie.Movie.MPAASpecified OrElse Not Master.eSettings.MovieLockMPAA)) Then

            Dim tmpstring As String = String.Empty
            tmpstring = If(Master.eSettings.MovieScraperCertLang = "us", StringUtils.USACertToMPAA(String.Join(" / ", DBMovie.Movie.Certifications.ToArray)), If(Master.eSettings.MovieScraperCertOnlyValue, DBMovie.Movie.Certification.Split(Convert.ToChar(":"))(1), DBMovie.Movie.Certification))
            'only update DBMovie if scraped result is not empty/nothing!
            If Not String.IsNullOrEmpty(tmpstring) Then
                DBMovie.Movie.MPAA = tmpstring
            End If
        End If

        'MPAA value if MPAA is not available
        If Not DBMovie.Movie.MPAASpecified AndAlso Not String.IsNullOrEmpty(Master.eSettings.MovieScraperMPAANotRated) Then
            DBMovie.Movie.MPAA = Master.eSettings.MovieScraperMPAANotRated
        End If

        'Plot for Outline
        If ((Not DBMovie.Movie.OutlineSpecified OrElse Not Master.eSettings.MovieLockOutline) AndAlso Master.eSettings.MovieScraperPlotForOutline AndAlso Not Master.eSettings.MovieScraperPlotForOutlineIfEmpty) OrElse _
            (Not DBMovie.Movie.OutlineSpecified AndAlso Master.eSettings.MovieScraperPlotForOutline AndAlso Master.eSettings.MovieScraperPlotForOutlineIfEmpty) Then
            DBMovie.Movie.Outline = StringUtils.ShortenOutline(DBMovie.Movie.Plot, Master.eSettings.MovieScraperOutlineLimit)
        End If

        'set ListTitle at the end of merging
        If DBMovie.Movie.TitleSpecified Then
            Dim tTitle As String = StringUtils.SortTokens_Movie(DBMovie.Movie.Title)
            If Master.eSettings.MovieDisplayYear AndAlso Not String.IsNullOrEmpty(DBMovie.Movie.Year) Then
                DBMovie.ListTitle = String.Format("{0} ({1})", tTitle, DBMovie.Movie.Year)
            Else
                DBMovie.ListTitle = tTitle
            End If
        Else
            If FileUtils.Common.isVideoTS(DBMovie.Filename) Then
                DBMovie.ListTitle = StringUtils.FilterName_Movie(Directory.GetParent(Directory.GetParent(DBMovie.Filename).FullName).Name)
            ElseIf FileUtils.Common.isBDRip(DBMovie.Filename) Then
                DBMovie.ListTitle = StringUtils.FilterName_Movie(Directory.GetParent(Directory.GetParent(Directory.GetParent(DBMovie.Filename).FullName).FullName).Name)
            Else
                If DBMovie.Source.UseFolderName AndAlso DBMovie.IsSingle Then
                    DBMovie.ListTitle = StringUtils.FilterName_Movie(Directory.GetParent(DBMovie.Filename).Name)
                Else
                    DBMovie.ListTitle = StringUtils.FilterName_Movie(Path.GetFileNameWithoutExtension(DBMovie.Filename))
                End If
            End If
        End If

        Return DBMovie
    End Function

    Public Shared Function MergeDataScraperResults_MovieSet(ByVal DBMovieSet As Database.DBElement, ByVal ScrapedList As List(Of MediaContainers.MovieSet), ByVal ScrapeType As Enums.ScrapeType, ByVal ScrapeOptions As Structures.ScrapeOptions) As Database.DBElement

        'protects the first scraped result against overwriting
        Dim new_Plot As Boolean = False
        Dim new_Title As Boolean = False

        For Each scrapedmovieset In ScrapedList

            'IDs
            If scrapedmovieset.TMDBSpecified Then
                DBMovieSet.MovieSet.TMDB = scrapedmovieset.TMDB
            End If

            'Plot
            If (Not DBMovieSet.MovieSet.PlotSpecified OrElse Not Master.eSettings.MovieSetLockPlot) AndAlso ScrapeOptions.bMainPlot AndAlso _
                scrapedmovieset.PlotSpecified AndAlso Master.eSettings.MovieSetScraperPlot AndAlso Not new_Plot Then
                DBMovieSet.MovieSet.Plot = scrapedmovieset.Plot
                new_Plot = True
                'ElseIf Master.eSettings.MovieSetScraperCleanFields AndAlso Not Master.eSettings.MovieSetScraperPlot AndAlso Not Master.eSettings.MovieSetLockPlot Then
                '    DBMovieSet.MovieSet.Plot = String.Empty
            End If

            'Title
            If (Not DBMovieSet.MovieSet.TitleSpecified OrElse Not Master.eSettings.MovieSetLockTitle) AndAlso ScrapeOptions.bMainTitle AndAlso _
                 scrapedmovieset.TitleSpecified AndAlso Master.eSettings.MovieSetScraperTitle AndAlso Not new_Title Then
                DBMovieSet.MovieSet.Title = scrapedmovieset.Title
                new_Title = True
                'ElseIf Master.eSettings.MovieSetScraperCleanFields AndAlso Not Master.eSettings.MovieSetScraperTitle AndAlso Not Master.eSettings.MovieSetLockTitle Then
                '    DBMovieSet.MovieSet.Title = String.Empty
            End If
        Next

        'set Title
        For Each sett As AdvancedSettingsSetting In clsAdvancedSettings.GetAllSettings.Where(Function(y) y.Name.StartsWith("MovieSetTitleRenamer:"))
            DBMovieSet.MovieSet.Title = DBMovieSet.MovieSet.Title.Replace(sett.Name.Substring(21), sett.Value)
        Next

        'set ListTitle at the end of merging
        If DBMovieSet.MovieSet.TitleSpecified Then
            Dim tTitle As String = StringUtils.SortTokens_MovieSet(DBMovieSet.MovieSet.Title)
            DBMovieSet.ListTitle = tTitle
        Else
            'If FileUtils.Common.isVideoTS(DBMovie.Filename) Then
            '    DBMovie.ListTitle = StringUtils.FilterName_Movie(Directory.GetParent(Directory.GetParent(DBMovie.Filename).FullName).Name)
            'ElseIf FileUtils.Common.isBDRip(DBMovie.Filename) Then
            '    DBMovie.ListTitle = StringUtils.FilterName_Movie(Directory.GetParent(Directory.GetParent(Directory.GetParent(DBMovie.Filename).FullName).FullName).Name)
            'Else
            '    If DBMovie.UseFolder AndAlso DBMovie.IsSingle Then
            '        DBMovie.ListTitle = StringUtils.FilterName_Movie(Directory.GetParent(DBMovie.Filename).Name)
            '    Else
            '        DBMovie.ListTitle = StringUtils.FilterName_Movie(Path.GetFileNameWithoutExtension(DBMovie.Filename))
            '    End If
            'End If
        End If

        Return DBMovieSet
    End Function
    ''' <summary>
    ''' Returns the "merged" result of each data scraper results
    ''' </summary>
    ''' <param name="DBTV">TV Show to be scraped</param>
    ''' <param name="ScrapedList"><c>List(Of MediaContainers.TVShow)</c> which contains unfiltered results of each data scraper</param>
    ''' <returns>The scrape result of movie (after applying various global scraper settings here)</returns>
    ''' <remarks>
    ''' This is used to determine the result of data scraping by going through all scraperesults of every data scraper and applying global data scraper settings here!
    ''' 
    ''' 2014/09/01 Cocotus - First implementation: Moved all global lock settings in various data scrapers to this function, only apply them once and not in every data scraper module! Should be more maintainable!
    ''' </remarks>
    Public Shared Function MergeDataScraperResults_TV(ByVal DBTV As Database.DBElement, ByVal ScrapedList As List(Of MediaContainers.TVShow), ByVal ScrapeType As Enums.ScrapeType, ByVal ScrapeOptions As Structures.ScrapeOptions, ByVal withEpisodes As Boolean) As Database.DBElement

        'protects the first scraped result against overwriting
        Dim new_Actors As Boolean = False
        Dim new_Certification As Boolean = False
        Dim new_Creators As Boolean = False
        Dim new_Collections As Boolean = False
        Dim new_ShowCountries As Boolean = False
        Dim new_Credits As Boolean = False
        Dim new_Directors As Boolean = False
        Dim new_Genres As Boolean = False
        Dim new_MPAA As Boolean = False
        Dim new_Outline As Boolean = False
        Dim new_Plot As Boolean = False
        Dim new_Rating As Boolean = False
        Dim new_Premiered As Boolean = False
        Dim new_Runtime As Boolean = False
        Dim new_Status As Boolean = False
        Dim new_Studio As Boolean = False
        Dim new_Tagline As Boolean = False
        Dim new_Title As Boolean = False
        Dim new_OriginalTitle As Boolean = False
        Dim new_Trailer As Boolean = False

        Dim KnownEpisodesIndex As New List(Of KnownEpisode)
        Dim KnownSeasonsIndex As New List(Of Integer)

        ''If "Use Preview Datascraperresults" option is enabled, a preview window which displays all datascraperresults will be opened before showing the Edit Movie page!
        'If (ScrapeType = Enums.ScrapeType_Movie_MovieSet_TV.SingleScrape OrElse ScrapeType = Enums.ScrapeType_Movie_MovieSet_TV.SingleField) AndAlso Master.eSettings.MovieScraperUseDetailView AndAlso ScrapedList.Count > 0 Then
        '    PreviewDataScraperResults(ScrapedList)
        'End If

        For Each scrapedshow In ScrapedList

            'IDs
            If scrapedshow.TVDBSpecified Then
                DBTV.TVShow.TVDB = scrapedshow.TVDB
            End If
            If scrapedshow.IMDBSpecified Then
                DBTV.TVShow.IMDB = scrapedshow.IMDB
            End If
            If scrapedshow.TMDBSpecified Then
                DBTV.TVShow.TMDB = scrapedshow.TMDB
            End If

            'Actors
            If (Not DBTV.TVShow.ActorsSpecified OrElse Not Master.eSettings.TVLockShowActors) AndAlso ScrapeOptions.bMainActors AndAlso _
                scrapedshow.ActorsSpecified AndAlso Master.eSettings.TVScraperShowActors AndAlso Not new_Actors Then

                'If Master.eSettings.MovieScraperCastWithImgOnly Then
                '    For i = scrapedmovie.Actors.Count - 1 To 0 Step -1
                '        If String.IsNullOrEmpty(scrapedmovie.Actors(i).ThumbURL) Then
                '            scrapedmovie.Actors.RemoveAt(i)
                '        End If
                '    Next
                'End If

                'If Master.eSettings.MovieScraperCastLimit > 0 AndAlso scrapedmovie.Actors.Count > Master.eSettings.MovieScraperCastLimit Then
                '    scrapedmovie.Actors.RemoveRange(Master.eSettings.MovieScraperCastLimit, scrapedmovie.Actors.Count - Master.eSettings.MovieScraperCastLimit)
                'End If

                DBTV.TVShow.Actors = scrapedshow.Actors
                'added check if there's any actors left to add, if not then try with results of following scraper...
                If scrapedshow.ActorsSpecified Then
                    new_Actors = True
                    'add numbers for ordering
                    Dim iOrder As Integer = 0
                    For Each actor In scrapedshow.Actors
                        actor.Order = iOrder
                        iOrder += 1
                    Next
                End If

            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperShowActors AndAlso Not Master.eSettings.TVLockShowActors Then
                DBTV.TVShow.Actors.Clear()
            End If

            'Certification
            If (Not DBTV.TVShow.CertificationsSpecified OrElse Not Master.eSettings.TVLockShowCert) AndAlso ScrapeOptions.bMainCertifications AndAlso _
                scrapedshow.CertificationsSpecified AndAlso Master.eSettings.TVScraperShowCert AndAlso Not new_Certification Then
                If Master.eSettings.TVScraperShowCertLang = Master.eLang.All Then
                    DBTV.TVShow.Certifications.Clear()
                    DBTV.TVShow.Certifications.AddRange(scrapedshow.Certifications)
                    new_Certification = True
                Else
                    For Each tCert In scrapedshow.Certifications
                        If tCert.StartsWith(APIXML.CertLanguagesXML.Language.FirstOrDefault(Function(l) l.abbreviation = Master.eSettings.TVScraperShowCertLang).name) Then
                            DBTV.TVShow.Certifications.Clear()
                            DBTV.TVShow.Certifications.Add(tCert)
                            new_Certification = True
                            Exit For
                        End If
                    Next
                End If
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperShowCert AndAlso Not Master.eSettings.TVLockShowCert Then
                DBTV.TVShow.Certifications.Clear()
            End If

            'Creators
            If (Not DBTV.TVShow.CreatorsSpecified OrElse Not Master.eSettings.TVLockShowCreators) AndAlso ScrapeOptions.bMainCreators AndAlso _
                scrapedshow.CreatorsSpecified AndAlso Master.eSettings.TVScraperShowCreators AndAlso Not new_Creators Then
                DBTV.TVShow.Creators = scrapedshow.Creators
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperShowCreators AndAlso Not Master.eSettings.TVLockShowCreators Then
                DBTV.TVShow.Creators.Clear()
            End If

            'Countries
            If (Not DBTV.TVShow.CountriesSpecified OrElse Not Master.eSettings.TVLockShowCountry) AndAlso ScrapeOptions.bMainCountries AndAlso _
                scrapedshow.CountriesSpecified AndAlso Master.eSettings.TVScraperShowCountry AndAlso Not new_ShowCountries Then
                DBTV.TVShow.Countries.Clear()
                DBTV.TVShow.Countries.AddRange(scrapedshow.Countries)
                new_ShowCountries = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperShowCountry AndAlso Not Master.eSettings.TVLockShowCountry Then
                DBTV.TVShow.Countries.Clear()
            End If

            'Genres
            If (Not DBTV.TVShow.GenresSpecified OrElse Not Master.eSettings.TVLockShowGenre) AndAlso ScrapeOptions.bMainGenres AndAlso
                scrapedshow.GenresSpecified AndAlso Master.eSettings.TVScraperShowGenre AndAlso Not new_Genres Then

                Dim tGenre As List(Of String) = StringUtils.GenreFilter(scrapedshow.Genres)

                'If Master.eSettings.TVScraperShowGenreLimit > 0 AndAlso Master.eSettings.TVScraperShowGenreLimit < _genres.Count AndAlso _genres.Count > 0 Then
                '    _genres.RemoveRange(Master.eSettings.TVScraperShowGenreLimit, _genres.Count - Master.eSettings.TVScraperShowGenreLimit)
                'End If
                DBTV.TVShow.Genres.Clear()
                DBTV.TVShow.Genres.AddRange(tGenre)
                new_Genres = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperShowGenre AndAlso Not Master.eSettings.TVLockShowGenre Then
                DBTV.TVShow.Genres.Clear()
            End If

            'MPAA
            If (Not DBTV.TVShow.MPAASpecified OrElse Not Master.eSettings.TVLockShowMPAA) AndAlso ScrapeOptions.bMainMPAA AndAlso _
              scrapedshow.MPAASpecified AndAlso Master.eSettings.TVScraperShowMPAA AndAlso Not new_MPAA Then
                DBTV.TVShow.MPAA = scrapedshow.MPAA
                new_MPAA = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperShowMPAA AndAlso Not Master.eSettings.TVLockShowMPAA Then
                DBTV.TVShow.MPAA = String.Empty
            End If

            'Originaltitle
            If (Not DBTV.TVShow.OriginalTitleSpecified OrElse Not Master.eSettings.TVLockShowOriginalTitle) AndAlso ScrapeOptions.bMainOriginalTitle AndAlso _
                scrapedshow.OriginalTitleSpecified AndAlso Master.eSettings.TVScraperShowOriginalTitle AndAlso Not new_OriginalTitle Then
                DBTV.TVShow.OriginalTitle = scrapedshow.OriginalTitle
                new_OriginalTitle = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperShowOriginalTitle AndAlso Not Master.eSettings.TVLockShowOriginalTitle Then
                DBTV.TVShow.OriginalTitle = String.Empty
            End If

            'Plot
            If (Not DBTV.TVShow.PlotSpecified OrElse Not Master.eSettings.TVLockShowPlot) AndAlso ScrapeOptions.bMainPlot AndAlso _
                 scrapedshow.PlotSpecified AndAlso Master.eSettings.TVScraperShowPlot AndAlso Not new_Plot Then
                DBTV.TVShow.Plot = scrapedshow.Plot
                new_Plot = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperShowPlot AndAlso Not Master.eSettings.TVLockShowPlot Then
                DBTV.TVShow.Plot = String.Empty
            End If

            'Premiered
            If (Not DBTV.TVShow.PremieredSpecified OrElse Not Master.eSettings.TVLockShowPremiered) AndAlso ScrapeOptions.bMainPremiered AndAlso _
                scrapedshow.PremieredSpecified AndAlso Master.eSettings.TVScraperShowPremiered AndAlso Not new_Premiered Then
                If Master.eSettings.MovieScraperReleaseFormat = False Then
                    Dim formatteddate As Date
                    If DateTime.TryParseExact(scrapedshow.Premiered, "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentUICulture, Globalization.DateTimeStyles.None, formatteddate) Then
                        DBTV.TVShow.Premiered = Strings.FormatDateTime(formatteddate, Microsoft.VisualBasic.DateFormat.ShortDate).ToString
                    Else
                        DBTV.TVShow.Premiered = scrapedshow.Premiered
                    End If
                Else
                    DBTV.TVShow.Premiered = scrapedshow.Premiered
                End If
                new_Premiered = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperShowPremiered AndAlso Not Master.eSettings.TVLockShowPremiered Then
                DBTV.TVShow.Premiered = String.Empty
            End If

            'Rating
            If (Not DBTV.TVShow.RatingSpecified OrElse DBTV.TVShow.Rating = "0" OrElse Not Master.eSettings.TVLockShowRating) AndAlso ScrapeOptions.bMainRating AndAlso _
                scrapedshow.RatingSpecified AndAlso Not scrapedshow.Rating = "0" AndAlso Master.eSettings.TVScraperShowRating AndAlso Not new_Rating Then
                DBTV.TVShow.Rating = scrapedshow.Rating
                DBTV.TVShow.Votes = Regex.Replace(scrapedshow.Votes, "\D", String.Empty)
                new_Rating = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperShowRating AndAlso Not Master.eSettings.TVLockShowRating Then
                DBTV.TVShow.Rating = String.Empty
                DBTV.TVShow.Votes = String.Empty
            End If

            'Runtime
            If (Not DBTV.TVShow.RuntimeSpecified OrElse DBTV.TVShow.Runtime = "0" OrElse Not Master.eSettings.TVLockShowRuntime) AndAlso ScrapeOptions.bMainRuntime AndAlso _
                scrapedshow.RuntimeSpecified AndAlso Not scrapedshow.Runtime = "0" AndAlso Master.eSettings.TVScraperShowRuntime AndAlso Not new_Runtime Then
                DBTV.TVShow.Runtime = scrapedshow.Runtime
                new_Runtime = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperShowRuntime AndAlso Not Master.eSettings.TVLockShowRuntime Then
                DBTV.TVShow.Runtime = String.Empty
            End If

            'Status
            If (DBTV.TVShow.StatusSpecified OrElse Not Master.eSettings.TVLockShowStatus) AndAlso ScrapeOptions.bMainStatus AndAlso _
                scrapedshow.StatusSpecified AndAlso Master.eSettings.TVScraperShowStatus AndAlso Not new_Status Then
                DBTV.TVShow.Status = scrapedshow.Status
                new_Status = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperShowStatus AndAlso Not Master.eSettings.TVLockShowStatus Then
                DBTV.TVShow.Status = String.Empty
            End If

            'Studios
            If (Not DBTV.TVShow.StudiosSpecified OrElse Not Master.eSettings.TVLockShowStudio) AndAlso ScrapeOptions.bMainStudios AndAlso _
                scrapedshow.StudiosSpecified AndAlso Master.eSettings.TVScraperShowStudio AndAlso Not new_Studio Then
                DBTV.TVShow.Studios.Clear()

                Dim _studios As New List(Of String)
                _studios.AddRange(scrapedshow.Studios)

                'If Master.eSettings.TVScraperShowStudioWithImgOnly Then
                '    For i = _studios.Count - 1 To 0 Step -1
                '        If APIXML.dStudios.ContainsKey(_studios.Item(i).ToLower) = False Then
                '            _studios.RemoveAt(i)
                '        End If
                '    Next
                'End If

                'If Master.eSettings.tvScraperStudioLimit > 0 AndAlso Master.eSettings.MovieScraperStudioLimit < _studios.Count AndAlso _studios.Count > 0 Then
                '    _studios.RemoveRange(Master.eSettings.MovieScraperStudioLimit, _studios.Count - Master.eSettings.MovieScraperStudioLimit)
                'End If


                DBTV.TVShow.Studios.AddRange(_studios)
                'added check if there's any studios left to add, if not then try with results of following scraper...
                If _studios.Count > 0 Then
                    new_Studio = True
                End If


            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperShowStudio AndAlso Not Master.eSettings.TVLockShowStudio Then
                DBTV.TVShow.Studios.Clear()
            End If

            'Title
            If (Not DBTV.TVShow.TitleSpecified OrElse Not Master.eSettings.TVLockShowTitle) AndAlso ScrapeOptions.bMainTitle AndAlso _
                scrapedshow.TitleSpecified AndAlso Master.eSettings.TVScraperShowTitle AndAlso Not new_Title Then
                DBTV.TVShow.Title = scrapedshow.Title
                new_Title = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperShowTitle AndAlso Not Master.eSettings.TVLockShowTitle Then
                DBTV.TVShow.Title = String.Empty
            End If

            '    'Credits
            '    If (DBTV.Movie.Credits.Count < 1 OrElse Not Master.eSettings.MovieLockCredits) AndAlso _
            '        scrapedmovie.Credits.Count > 0 AndAlso Master.eSettings.MovieScraperCredits AndAlso Not new_Credits Then
            '        DBTV.Movie.Credits.Clear()
            '        DBTV.Movie.Credits.AddRange(scrapedmovie.Credits)
            '        new_Credits = True
            '    ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperCredits AndAlso Not Master.eSettings.MovieLockCredits Then
            '        DBTV.Movie.Credits.Clear()
            '    End If

            'Create KnowSeasons index
            For Each kSeason As MediaContainers.SeasonDetails In scrapedshow.KnownSeasons
                If Not KnownSeasonsIndex.Contains(kSeason.Season) Then
                    KnownSeasonsIndex.Add(kSeason.Season)
                End If
            Next

            'Create KnownEpisodes index (season and episode number)
            If withEpisodes Then
                For Each kEpisode As MediaContainers.EpisodeDetails In scrapedshow.KnownEpisodes
                    Dim nKnownEpisode As New KnownEpisode With {.AiredDate = kEpisode.Aired, _
                                                                .Episode = kEpisode.Episode, _
                                                                .EpisodeAbsolute = kEpisode.EpisodeAbsolute, _
                                                                .EpisodeCombined = kEpisode.EpisodeCombined, _
                                                                .EpisodeDVD = kEpisode.EpisodeDVD, _
                                                                .Season = kEpisode.Season, _
                                                                .SeasonCombined = kEpisode.SeasonCombined, _
                                                                .SeasonDVD = kEpisode.SeasonDVD}
                    If KnownEpisodesIndex.Where(Function(f) f.Episode = nKnownEpisode.Episode AndAlso f.Season = nKnownEpisode.Season).Count = 0 Then
                        KnownEpisodesIndex.Add(nKnownEpisode)

                        'try to get an episode information with more numbers
                    ElseIf KnownEpisodesIndex.Where(Function(f) f.Episode = nKnownEpisode.Episode AndAlso f.Season = nKnownEpisode.Season AndAlso _
                                ((nKnownEpisode.EpisodeAbsolute > -1 AndAlso Not f.EpisodeAbsolute = nKnownEpisode.EpisodeAbsolute) OrElse _
                                 (nKnownEpisode.EpisodeCombined > -1 AndAlso Not f.EpisodeCombined = nKnownEpisode.EpisodeCombined) OrElse _
                                 (nKnownEpisode.EpisodeDVD > -1 AndAlso Not f.EpisodeDVD = nKnownEpisode.EpisodeDVD) OrElse _
                                 (nKnownEpisode.SeasonCombined > -1 AndAlso Not f.SeasonCombined = nKnownEpisode.SeasonCombined) OrElse _
                                 (nKnownEpisode.SeasonDVD > -1 AndAlso Not f.SeasonDVD = nKnownEpisode.SeasonDVD))).Count = 1 Then
                        Dim toRemove As KnownEpisode = KnownEpisodesIndex.FirstOrDefault(Function(f) f.Episode = nKnownEpisode.Episode AndAlso f.Season = nKnownEpisode.Season)
                        KnownEpisodesIndex.Remove(toRemove)
                        KnownEpisodesIndex.Add(nKnownEpisode)
                    End If
                Next
            End If
        Next

        'Certification for MPAA
        If DBTV.TVShow.CertificationsSpecified AndAlso Master.eSettings.TVScraperShowCertForMPAA AndAlso _
            (Not Master.eSettings.MovieScraperCertForMPAAFallback AndAlso (Not DBTV.TVShow.MPAASpecified OrElse Not Master.eSettings.TVLockShowMPAA) OrElse _
             Not new_MPAA AndAlso (Not DBTV.TVShow.MPAASpecified OrElse Not Master.eSettings.TVLockShowMPAA)) Then

            Dim tmpstring As String = String.Empty
            tmpstring = If(Master.eSettings.TVScraperShowCertLang = "us", StringUtils.USACertToMPAA(DBTV.TVShow.Certification), If(Master.eSettings.TVScraperShowCertOnlyValue, DBTV.TVShow.Certification.Split(Convert.ToChar(":"))(1), DBTV.TVShow.Certification))
            'only update DBMovie if scraped result is not empty/nothing!
            If Not String.IsNullOrEmpty(tmpstring) Then
                DBTV.TVShow.MPAA = tmpstring
            End If
        End If

        'MPAA value if MPAA is not available
        If Not DBTV.TVShow.MPAASpecified AndAlso Not String.IsNullOrEmpty(Master.eSettings.TVScraperShowMPAANotRated) Then
            DBTV.TVShow.MPAA = Master.eSettings.TVScraperShowMPAANotRated
        End If

        'set ListTitle at the end of merging
        If DBTV.TVShow.TitleSpecified Then
            DBTV.ListTitle = StringUtils.SortTokens_TV(DBTV.TVShow.Title)
        End If


        'Seasons
        For Each aKnownSeason As Integer In KnownSeasonsIndex
            'create a list of specified episode informations from all scrapers
            Dim ScrapedSeasonList As New List(Of MediaContainers.SeasonDetails)
            For Each nShow As MediaContainers.TVShow In ScrapedList
                For Each nSeasonDetails As MediaContainers.SeasonDetails In nShow.KnownSeasons.Where(Function(f) f.Season = aKnownSeason)
                    ScrapedSeasonList.Add(nSeasonDetails)
                Next
            Next
            'check if we have already saved season information for this scraped season
            Dim lSeasonList = DBTV.Seasons.Where(Function(f) f.TVSeason.Season = aKnownSeason)

            If lSeasonList IsNot Nothing AndAlso lSeasonList.Count > 0 Then
                For Each nSeason As Database.DBElement In lSeasonList
                    MergeDataScraperResults_TVSeason(nSeason, ScrapedSeasonList, ScrapeOptions)
                Next
            Else
                'no existing season found -> add it as "missing" season
                Dim mSeason As New Database.DBElement(Enums.ContentType.TVSeason) With {.TVSeason = New MediaContainers.SeasonDetails With {.Season = aKnownSeason}}
                mSeason = Master.DB.AddTVShowInfoToDBElement(mSeason, DBTV)
                DBTV.Seasons.Add(MergeDataScraperResults_TVSeason(mSeason, ScrapedSeasonList, ScrapeOptions))
            End If
        Next
        'add all season informations to TVShow (for saving season informations to tv show NFO)
        DBTV.TVShow.Seasons.Seasons.Clear()
        For Each kSeason As Database.DBElement In DBTV.Seasons.OrderBy(Function(f) f.TVSeason.Season)
            DBTV.TVShow.Seasons.Seasons.Add(kSeason.TVSeason)
        Next

        'Episodes
        If withEpisodes Then
            'update the tvshow information for each local episode
            For Each lEpisode In DBTV.Episodes
                lEpisode = Master.DB.AddTVShowInfoToDBElement(lEpisode, DBTV)
            Next

            For Each aKnownEpisode As KnownEpisode In KnownEpisodesIndex.OrderBy(Function(f) f.Episode).OrderBy(Function(f) f.Season)

                'convert the episode and season number if needed
                Dim iEpisode As Integer = -1
                Dim iSeason As Integer = -1
                Dim strAiredDate As String = aKnownEpisode.AiredDate
                If DBTV.Ordering = Enums.Ordering.Absolute Then
                    iEpisode = aKnownEpisode.EpisodeAbsolute
                ElseIf DBTV.Ordering = Enums.Ordering.DVD Then
                    iEpisode = CInt(aKnownEpisode.EpisodeDVD)
                    iSeason = aKnownEpisode.SeasonDVD
                ElseIf DBTV.Ordering = Enums.Ordering.Standard Then
                    iEpisode = aKnownEpisode.Episode
                    iSeason = aKnownEpisode.Season
                End If

                If Not iEpisode = -1 AndAlso Not iSeason = -1 Then
                    'create a list of specified episode informations from all scrapers
                    Dim ScrapedEpisodeList As New List(Of MediaContainers.EpisodeDetails)
                    For Each nShow As MediaContainers.TVShow In ScrapedList
                        For Each nEpisodeDetails As MediaContainers.EpisodeDetails In nShow.KnownEpisodes.Where(Function(f) f.Episode = aKnownEpisode.Episode AndAlso f.Season = aKnownEpisode.Season)
                            ScrapedEpisodeList.Add(nEpisodeDetails)
                        Next
                    Next

                    'check if we have a local episode file for this scraped episode
                    Dim lEpisodeList = DBTV.Episodes.Where(Function(f) Not String.IsNullOrEmpty(f.Filename) AndAlso f.TVEpisode.Episode = iEpisode AndAlso f.TVEpisode.Season = iSeason)

                    If lEpisodeList IsNot Nothing AndAlso lEpisodeList.Count > 0 Then
                        For Each nEpisode As Database.DBElement In lEpisodeList
                            MergeDataScraperResults_TVEpisode(nEpisode, ScrapedEpisodeList, ScrapeOptions)
                        Next
                    Else
                        'try to get the episode by AiredDate
                        Dim dEpisodeList = DBTV.Episodes.Where(Function(f) Not String.IsNullOrEmpty(f.Filename) AndAlso Not String.IsNullOrEmpty(f.TVEpisode.Aired) AndAlso f.TVEpisode.Aired = strAiredDate)

                        If dEpisodeList IsNot Nothing AndAlso dEpisodeList.Count > 0 Then
                            For Each nEpisode As Database.DBElement In dEpisodeList
                                MergeDataScraperResults_TVEpisode(nEpisode, ScrapedEpisodeList, ScrapeOptions)
                                'we have to add the proper season and episode number if the episode was found by AiredDate
                                nEpisode.TVEpisode.Episode = iEpisode
                                nEpisode.TVEpisode.Season = iSeason
                            Next
                        Else
                            'no local episode found -> add it as "missing" episode
                            Dim mEpisode As New Database.DBElement(Enums.ContentType.TVEpisode) With {.TVEpisode = New MediaContainers.EpisodeDetails With {.Episode = iEpisode, .Season = iSeason}}
                            mEpisode = Master.DB.AddTVShowInfoToDBElement(mEpisode, DBTV)
                            MergeDataScraperResults_TVEpisode(mEpisode, ScrapedEpisodeList, ScrapeOptions)
                            If mEpisode.TVEpisode.TitleSpecified Then
                                DBTV.Episodes.Add(mEpisode)
                            Else
                                logger.Warn(String.Format("Missing Episode Ignored | {0} - S{1}E{2} | No Episode Title found", mEpisode.TVShow.Title, mEpisode.TVEpisode.Season, mEpisode.TVEpisode.Episode))
                            End If
                        End If
                    End If
                Else
                    logger.Warn("No valid episode or season number found")
                End If
            Next
        End If

        'create the "* All Seasons" entry if needed
        Dim tmpAllSeasons As Database.DBElement = DBTV.Seasons.FirstOrDefault(Function(f) f.TVSeason.Season = 999)
        If tmpAllSeasons Is Nothing OrElse tmpAllSeasons.TVSeason Is Nothing Then
            tmpAllSeasons = New Database.DBElement(Enums.ContentType.TVSeason)
            tmpAllSeasons.TVSeason = New MediaContainers.SeasonDetails With {.Season = 999}
            tmpAllSeasons = Master.DB.AddTVShowInfoToDBElement(tmpAllSeasons, DBTV)
            DBTV.Seasons.Add(tmpAllSeasons)
        End If

        'cleanup seasons they don't have any episode
        Dim iIndex As Integer = 0
        While iIndex <= DBTV.Seasons.Count - 1
            Dim iSeason As Integer = DBTV.Seasons.Item(iIndex).TVSeason.Season
            If Not iSeason = 999 AndAlso DBTV.Episodes.Where(Function(f) f.TVEpisode.Season = iSeason).Count = 0 Then
                DBTV.Seasons.RemoveAt(iIndex)
            Else
                iIndex += 1
            End If
        End While

        Return DBTV
    End Function

    Public Shared Function MergeDataScraperResults_TVSeason(ByRef DBTVSeason As Database.DBElement, ByVal ScrapedList As List(Of MediaContainers.SeasonDetails), ByVal ScrapeOptions As Structures.ScrapeOptions) As Database.DBElement

        'protects the first scraped result against overwriting
        Dim new_Aired As Boolean = False
        Dim new_Plot As Boolean = False
        Dim new_Season As Boolean = False
        Dim new_Title As Boolean = False

        For Each scrapedseason In ScrapedList

            'IDs
            If scrapedseason.TMDBSpecified Then
                DBTVSeason.TVSeason.TMDB = scrapedseason.TMDB
            End If
            If scrapedseason.TVDBSpecified Then
                DBTVSeason.TVSeason.TVDB = scrapedseason.TVDB
            End If

            'Season number
            If scrapedseason.SeasonSpecified AndAlso Not new_Season Then
                DBTVSeason.TVSeason.Season = scrapedseason.Season
                new_Season = True
            End If

            'Aired
            If (Not DBTVSeason.TVSeason.AiredSpecified OrElse Not Master.eSettings.TVLockEpisodeAired) AndAlso ScrapeOptions.bSeasonAired AndAlso _
                scrapedseason.AiredSpecified AndAlso Master.eSettings.TVScraperEpisodeAired AndAlso Not new_Aired Then
                DBTVSeason.TVSeason.Aired = scrapedseason.Aired
                new_Aired = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperEpisodeAired AndAlso Not Master.eSettings.TVLockEpisodeAired Then
                DBTVSeason.TVSeason.Aired = String.Empty
            End If

            'Plot
            If (Not DBTVSeason.TVSeason.PlotSpecified OrElse Not Master.eSettings.TVLockEpisodePlot) AndAlso ScrapeOptions.bSeasonPlot AndAlso _
                scrapedseason.PlotSpecified AndAlso Master.eSettings.TVScraperEpisodePlot AndAlso Not new_Plot Then
                DBTVSeason.TVSeason.Plot = scrapedseason.Plot
                new_Plot = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperEpisodePlot AndAlso Not Master.eSettings.TVLockEpisodePlot Then
                DBTVSeason.TVSeason.Plot = String.Empty
            End If

            'Title
            If (Not DBTVSeason.TVSeason.TitleSpecified OrElse Not Master.eSettings.TVLockSeasonTitle) AndAlso ScrapeOptions.bSeasonTitle AndAlso _
                scrapedseason.TitleSpecified AndAlso Master.eSettings.TVScraperSeasonTitle AndAlso Not new_Title Then
                DBTVSeason.TVSeason.Title = scrapedseason.Title
                new_Title = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperSeasonTitle AndAlso Not Master.eSettings.TVLockSeasonTitle Then
                DBTVSeason.TVSeason.Title = String.Empty
            End If
        Next

        Return DBTVSeason
    End Function
    ''' <summary>
    ''' Returns the "merged" result of each data scraper results
    ''' </summary>
    ''' <param name="DBTVEpisode">Episode to be scraped</param>
    ''' <param name="ScrapedList"><c>List(Of MediaContainers.EpisodeDetails)</c> which contains unfiltered results of each data scraper</param>
    ''' <returns>The scrape result of episode (after applying various global scraper settings here)</returns>
    ''' <remarks>
    ''' This is used to determine the result of data scraping by going through all scraperesults of every data scraper and applying global data scraper settings here!
    ''' 
    ''' 2014/09/01 Cocotus - First implementation: Moved all global lock settings in various data scrapers to this function, only apply them once and not in every data scraper module! Should be more maintainable!
    ''' </remarks>
    Private Shared Function MergeDataScraperResults_TVEpisode(ByRef DBTVEpisode As Database.DBElement, ByVal ScrapedList As List(Of MediaContainers.EpisodeDetails), ByVal ScrapeOptions As Structures.ScrapeOptions) As Database.DBElement

        'protects the first scraped result against overwriting
        Dim new_Actors As Boolean = False
        Dim new_Aired As Boolean = False
        Dim new_Countries As Boolean = False
        Dim new_Credits As Boolean = False
        Dim new_Directors As Boolean = False
        Dim new_Episode As Boolean = False
        Dim new_GuestStars As Boolean = False
        Dim new_OriginalTitle As Boolean = False
        Dim new_Plot As Boolean = False
        Dim new_Rating As Boolean = False
        Dim new_Runtime As Boolean = False
        Dim new_Season As Boolean = False
        Dim new_ThumbPoster As Boolean = False
        Dim new_Title As Boolean = False

        ''If "Use Preview Datascraperresults" option is enabled, a preview window which displays all datascraperresults will be opened before showing the Edit Movie page!
        'If (ScrapeType = Enums.ScrapeType_Movie_MovieSet_TV.SingleScrape OrElse ScrapeType = Enums.ScrapeType_Movie_MovieSet_TV.SingleField) AndAlso Master.eSettings.MovieScraperUseDetailView AndAlso ScrapedList.Count > 0 Then
        '    PreviewDataScraperResults(ScrapedList)
        'End If

        For Each scrapedepisode In ScrapedList

            'IDs
            If scrapedepisode.IMDBSpecified Then
                DBTVEpisode.TVEpisode.IMDB = scrapedepisode.IMDB
            End If
            If scrapedepisode.TMDBSpecified Then
                DBTVEpisode.TVEpisode.TMDB = scrapedepisode.TMDB
            End If
            If scrapedepisode.TVDBSpecified Then
                DBTVEpisode.TVEpisode.TVDB = scrapedepisode.TVDB
            End If

            'DisplayEpisode
            If scrapedepisode.DisplayEpisodeSpecified Then
                DBTVEpisode.TVEpisode.DisplayEpisode = scrapedepisode.DisplayEpisode
            End If

            'DisplaySeason
            If scrapedepisode.DisplaySeasonSpecified Then
                DBTVEpisode.TVEpisode.DisplaySeason = scrapedepisode.DisplaySeason
            End If

            'Actors
            If (Not DBTVEpisode.TVEpisode.ActorsSpecified OrElse Not Master.eSettings.TVLockEpisodeActors) AndAlso ScrapeOptions.bEpisodeActors AndAlso _
                scrapedepisode.ActorsSpecified AndAlso Master.eSettings.TVScraperEpisodeActors AndAlso Not new_Actors Then

                'If Master.eSettings.TVScraperEpisodeCastWithImgOnly Then
                '    For i = scrapedepisode.Actors.Count - 1 To 0 Step -1
                '        If String.IsNullOrEmpty(scrapedepisode.Actors(i).ThumbURL) Then
                '            scrapedepisode.Actors.RemoveAt(i)
                '        End If
                '    Next
                'End If

                'If Master.eSettings.TVScraperEpisodeCastLimit > 0 AndAlso scrapedepisode.Actors.Count > Master.eSettings.TVScraperEpisodeCastLimit Then
                '    scrapedepisode.Actors.RemoveRange(Master.eSettings.TVScraperEpisodeCastLimit, scrapedepisode.Actors.Count - Master.eSettings.TVScraperEpisodeCastLimit)
                'End If

                DBTVEpisode.TVEpisode.Actors = scrapedepisode.Actors
                'added check if there's any actors left to add, if not then try with results of following scraper...
                If scrapedepisode.ActorsSpecified Then
                    new_Actors = True
                    'add numbers for ordering
                    Dim iOrder As Integer = 0
                    For Each actor In scrapedepisode.Actors
                        actor.Order = iOrder
                        iOrder += 1
                    Next
                End If

            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperEpisodeActors AndAlso Not Master.eSettings.TVLockEpisodeActors Then
                DBTVEpisode.TVEpisode.Actors.Clear()
            End If

            'Aired
            If (Not DBTVEpisode.TVEpisode.AiredSpecified OrElse Not Master.eSettings.TVLockEpisodeAired) AndAlso ScrapeOptions.bEpisodeAired AndAlso _
                scrapedepisode.AiredSpecified AndAlso Master.eSettings.TVScraperEpisodeAired AndAlso Not new_Aired Then
                DBTVEpisode.TVEpisode.Aired = scrapedepisode.Aired
                new_Aired = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperEpisodeAired AndAlso Not Master.eSettings.TVLockEpisodeAired Then
                DBTVEpisode.TVEpisode.Aired = String.Empty
            End If

            'Credits
            If (Not DBTVEpisode.TVEpisode.CreditsSpecified OrElse Not Master.eSettings.TVLockEpisodeCredits) AndAlso _
                scrapedepisode.CreditsSpecified AndAlso Master.eSettings.TVScraperEpisodeCredits AndAlso Not new_Credits Then
                DBTVEpisode.TVEpisode.Credits = scrapedepisode.Credits
                new_Credits = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperEpisodeCredits AndAlso Not Master.eSettings.TVLockEpisodeCredits Then
                DBTVEpisode.TVEpisode.Credits.Clear()
            End If

            'Directors
            If (Not DBTVEpisode.TVEpisode.DirectorsSpecified OrElse Not Master.eSettings.TVLockEpisodeDirector) AndAlso ScrapeOptions.bEpisodeDirectors AndAlso _
                scrapedepisode.DirectorsSpecified AndAlso Master.eSettings.TVScraperEpisodeDirector AndAlso Not new_Directors Then
                DBTVEpisode.TVEpisode.Directors = scrapedepisode.Directors
                new_Directors = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperEpisodeDirector AndAlso Not Master.eSettings.TVLockEpisodeDirector Then
                DBTVEpisode.TVEpisode.Directors.Clear()
            End If

            'GuestStars
            If (Not DBTVEpisode.TVEpisode.GuestStarsSpecified OrElse Not Master.eSettings.TVLockEpisodeGuestStars) AndAlso ScrapeOptions.bEpisodeGuestStars AndAlso _
                scrapedepisode.GuestStarsSpecified AndAlso Master.eSettings.TVScraperEpisodeGuestStars AndAlso Not new_GuestStars Then

                'If Master.eSettings.TVScraperEpisodeCastWithImgOnly Then
                '    For i = scrapedepisode.Actors.Count - 1 To 0 Step -1
                '        If String.IsNullOrEmpty(scrapedepisode.Actors(i).ThumbURL) Then
                '            scrapedepisode.Actors.RemoveAt(i)
                '        End If
                '    Next
                'End If

                'If Master.eSettings.TVScraperEpisodeCastLimit > 0 AndAlso scrapedepisode.Actors.Count > Master.eSettings.TVScraperEpisodeCastLimit Then
                '    scrapedepisode.Actors.RemoveRange(Master.eSettings.TVScraperEpisodeCastLimit, scrapedepisode.Actors.Count - Master.eSettings.TVScraperEpisodeCastLimit)
                'End If

                DBTVEpisode.TVEpisode.GuestStars = scrapedepisode.GuestStars
                'added check if there's any actors left to add, if not then try with results of following scraper...
                If scrapedepisode.GuestStarsSpecified Then
                    new_GuestStars = True
                    'add numbers for ordering
                    Dim iOrder As Integer = 0
                    For Each aGuestStar In scrapedepisode.GuestStars
                        aGuestStar.Order = iOrder
                        iOrder += 1
                    Next
                End If

            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperEpisodeGuestStars AndAlso Not Master.eSettings.TVLockEpisodeGuestStars Then
                DBTVEpisode.TVEpisode.GuestStars.Clear()
            End If

            'Plot
            If (Not DBTVEpisode.TVEpisode.PlotSpecified OrElse Not Master.eSettings.TVLockEpisodePlot) AndAlso ScrapeOptions.bEpisodePlot AndAlso _
                scrapedepisode.PlotSpecified AndAlso Master.eSettings.TVScraperEpisodePlot AndAlso Not new_Plot Then
                DBTVEpisode.TVEpisode.Plot = scrapedepisode.Plot
                new_Plot = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperEpisodePlot AndAlso Not Master.eSettings.TVLockEpisodePlot Then
                DBTVEpisode.TVEpisode.Plot = String.Empty
            End If

            'Rating/Votes
            If (Not DBTVEpisode.TVEpisode.RatingSpecified OrElse Not Master.eSettings.TVLockEpisodeRating) AndAlso ScrapeOptions.bEpisodeRating AndAlso _
                scrapedepisode.RatingSpecified AndAlso Master.eSettings.TVScraperEpisodeRating AndAlso Not new_Rating Then
                DBTVEpisode.TVEpisode.Rating = scrapedepisode.Rating
                DBTVEpisode.TVEpisode.Votes = Regex.Replace(scrapedepisode.Votes, "\D", String.Empty)
                new_Rating = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperEpisodeRating AndAlso Not Master.eSettings.TVLockEpisodeRating Then
                DBTVEpisode.TVEpisode.Rating = String.Empty
                DBTVEpisode.TVEpisode.Votes = String.Empty
            End If

            'Runtime
            If (Not DBTVEpisode.TVEpisode.RuntimeSpecified OrElse Not Master.eSettings.TVLockEpisodeRuntime) AndAlso ScrapeOptions.bEpisodeRuntime AndAlso _
                scrapedepisode.RuntimeSpecified AndAlso Master.eSettings.TVScraperEpisodeRuntime AndAlso Not new_Runtime Then
                DBTVEpisode.TVEpisode.Runtime = scrapedepisode.Runtime
                new_Runtime = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperEpisodeRuntime AndAlso Not Master.eSettings.TVLockEpisodeRuntime Then
                DBTVEpisode.TVEpisode.Runtime = String.Empty
            End If

            'ThumbPoster
            If (Not String.IsNullOrEmpty(scrapedepisode.ThumbPoster.URLOriginal) OrElse Not String.IsNullOrEmpty(scrapedepisode.ThumbPoster.URLThumb)) AndAlso Not new_ThumbPoster Then
                DBTVEpisode.TVEpisode.ThumbPoster = scrapedepisode.ThumbPoster
                new_ThumbPoster = True
            End If

            'Title
            If (Not DBTVEpisode.TVEpisode.TitleSpecified OrElse Not Master.eSettings.TVLockEpisodeTitle) AndAlso ScrapeOptions.bEpisodeTitle AndAlso _
               scrapedepisode.TitleSpecified AndAlso Master.eSettings.TVScraperEpisodeTitle AndAlso Not new_Title Then
                DBTVEpisode.TVEpisode.Title = scrapedepisode.Title
                new_Title = True
            ElseIf Master.eSettings.TVScraperCleanFields AndAlso Not Master.eSettings.TVScraperEpisodeTitle AndAlso Not Master.eSettings.TVLockEpisodeTitle Then
                DBTVEpisode.TVEpisode.Title = String.Empty
            End If
        Next

        'Add GuestStars to Actors
        If DBTVEpisode.TVEpisode.GuestStarsSpecified AndAlso Master.eSettings.TVScraperEpisodeGuestStarsToActors AndAlso Not Master.eSettings.TVLockEpisodeActors Then
            DBTVEpisode.TVEpisode.Actors.AddRange(DBTVEpisode.TVEpisode.GuestStars)
        End If

        'TV Show Runtime for Episode Runtime
        If Not DBTVEpisode.TVEpisode.RuntimeSpecified AndAlso Master.eSettings.TVScraperUseSRuntimeForEp AndAlso DBTVEpisode.TVShow.RuntimeSpecified Then
            DBTVEpisode.TVEpisode.Runtime = DBTVEpisode.TVShow.Runtime
        End If

        Return DBTVEpisode
    End Function

    Public Shared Function MergeDataScraperResults_TVEpisode_Single(ByRef DBTVEpisode As Database.DBElement, ByVal ScrapedList As List(Of MediaContainers.EpisodeDetails), ByVal ScrapeOptions As Structures.ScrapeOptions) As Database.DBElement
        Dim KnownEpisodesIndex As New List(Of KnownEpisode)

        For Each kEpisode As MediaContainers.EpisodeDetails In ScrapedList
            Dim nKnownEpisode As New KnownEpisode With {.AiredDate = kEpisode.Aired, _
                                                        .Episode = kEpisode.Episode, _
                                                        .EpisodeAbsolute = kEpisode.EpisodeAbsolute, _
                                                        .EpisodeCombined = kEpisode.EpisodeCombined, _
                                                        .EpisodeDVD = kEpisode.EpisodeDVD, _
                                                        .Season = kEpisode.Season, _
                                                        .SeasonCombined = kEpisode.SeasonCombined, _
                                                        .SeasonDVD = kEpisode.SeasonDVD}
            If KnownEpisodesIndex.Where(Function(f) f.Episode = nKnownEpisode.Episode AndAlso f.Season = nKnownEpisode.Season).Count = 0 Then
                KnownEpisodesIndex.Add(nKnownEpisode)

                'try to get an episode information with more numbers
            ElseIf KnownEpisodesIndex.Where(Function(f) f.Episode = nKnownEpisode.Episode AndAlso f.Season = nKnownEpisode.Season AndAlso _
                        ((nKnownEpisode.EpisodeAbsolute > -1 AndAlso Not f.EpisodeAbsolute = nKnownEpisode.EpisodeAbsolute) OrElse _
                         (nKnownEpisode.EpisodeCombined > -1 AndAlso Not f.EpisodeCombined = nKnownEpisode.EpisodeCombined) OrElse _
                         (nKnownEpisode.EpisodeDVD > -1 AndAlso Not f.EpisodeDVD = nKnownEpisode.EpisodeDVD) OrElse _
                         (nKnownEpisode.SeasonCombined > -1 AndAlso Not f.SeasonCombined = nKnownEpisode.SeasonCombined) OrElse _
                         (nKnownEpisode.SeasonDVD > -1 AndAlso Not f.SeasonDVD = nKnownEpisode.SeasonDVD))).Count = 1 Then
                Dim toRemove As KnownEpisode = KnownEpisodesIndex.FirstOrDefault(Function(f) f.Episode = nKnownEpisode.Episode AndAlso f.Season = nKnownEpisode.Season)
                KnownEpisodesIndex.Remove(toRemove)
                KnownEpisodesIndex.Add(nKnownEpisode)
            End If
        Next

        If KnownEpisodesIndex.Count = 1 Then
            'convert the episode and season number if needed
            Dim iEpisode As Integer = -1
            Dim iSeason As Integer = -1
            Dim strAiredDate As String = KnownEpisodesIndex.Item(0).AiredDate
            If DBTVEpisode.Ordering = Enums.Ordering.Absolute Then
                iEpisode = KnownEpisodesIndex.Item(0).EpisodeAbsolute
            ElseIf DBTVEpisode.Ordering = Enums.Ordering.DVD Then
                iEpisode = CInt(KnownEpisodesIndex.Item(0).EpisodeDVD)
                iSeason = KnownEpisodesIndex.Item(0).SeasonDVD
            ElseIf DBTVEpisode.Ordering = Enums.Ordering.Standard Then
                iEpisode = KnownEpisodesIndex.Item(0).Episode
                iSeason = KnownEpisodesIndex.Item(0).Season
            End If

            If Not iEpisode = -1 AndAlso Not iSeason = -1 Then
                MergeDataScraperResults_TVEpisode(DBTVEpisode, ScrapedList, ScrapeOptions)
                If DBTVEpisode.TVEpisode.Episode = -1 Then DBTVEpisode.TVEpisode.Episode = iEpisode
                If DBTVEpisode.TVEpisode.Season = -1 Then DBTVEpisode.TVEpisode.Season = iSeason
            Else
                logger.Warn("No valid episode or season number found")
            End If
        Else
            logger.Warn("Episode could not be clearly determined.")
        End If

        Return DBTVEpisode
    End Function
    ''' <summary>
    ''' Open MovieDataScraperPreview Window
    ''' </summary>
    ''' <param name="ScrapedList"><c>List(Of MediaContainers.Movie)</c> which contains unfiltered results of each data scraper</param>
    ''' <remarks>
    ''' 2014/09/13 Cocotus - First implementation: Display all scrapedresults in preview window, so that user can select the information which should be used
    ''' </remarks>
    Public Shared Sub PreviewDataScraperResults(ByRef ScrapedList As List(Of MediaContainers.Movie))
        Try
            Application.DoEvents()
            'Open/Show preview window
            Using dlgMovieDataScraperPreview As New dlgMovieDataScraperPreview(ScrapedList)
                Select Case dlgMovieDataScraperPreview.ShowDialog()
                    Case Windows.Forms.DialogResult.OK
                        'For now nothing here
                End Select
            End Using
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
    End Sub

    Public Shared Function CleanNFO_Movies(ByVal mNFO As MediaContainers.Movie) As MediaContainers.Movie
        If mNFO IsNot Nothing Then
            mNFO.Genre = String.Join(" / ", mNFO.Genres.ToArray)
            mNFO.Outline = mNFO.Outline.Replace(vbCrLf, vbLf).Replace(vbLf, vbCrLf)
            mNFO.Plot = mNFO.Plot.Replace(vbCrLf, vbLf).Replace(vbLf, vbCrLf)
            mNFO.Votes = Regex.Replace(mNFO.Votes, "\D", String.Empty)
            If mNFO.FileInfoSpecified Then
                If mNFO.FileInfo.StreamDetails.AudioSpecified Then
                    For Each aStream In mNFO.FileInfo.StreamDetails.Audio.Where(Function(f) f.LanguageSpecified AndAlso Not f.LongLanguageSpecified)
                        aStream.LongLanguage = Localization.ISOGetLangByCode3(aStream.Language)
                    Next
                End If
                If mNFO.FileInfo.StreamDetails.SubtitleSpecified Then
                    For Each sStream In mNFO.FileInfo.StreamDetails.Subtitle.Where(Function(f) f.LanguageSpecified AndAlso Not f.LongLanguageSpecified)
                        sStream.LongLanguage = Localization.ISOGetLangByCode3(sStream.Language)
                    Next
                End If
            End If
            If mNFO.Sets.Count > 0 Then
                For i = mNFO.Sets.Count - 1 To 0 Step -1
                    If Not mNFO.Sets(i).TitleSpecified Then
                        mNFO.Sets.RemoveAt(i)
                    End If
                Next
            End If
            Return mNFO
        Else
            Return mNFO
        End If
    End Function

    Public Shared Function CleanNFO_TVEpisodes(ByVal eNFO As MediaContainers.EpisodeDetails) As MediaContainers.EpisodeDetails
        If eNFO IsNot Nothing Then
            eNFO.Votes = Regex.Replace(eNFO.Votes, "\D", String.Empty)
            If eNFO.FileInfoSpecified Then
                If eNFO.FileInfo.StreamDetails.AudioSpecified Then
                    For Each aStream In eNFO.FileInfo.StreamDetails.Audio.Where(Function(f) f.LanguageSpecified AndAlso Not f.LongLanguageSpecified)
                        aStream.LongLanguage = Localization.ISOGetLangByCode3(aStream.Language)
                    Next
                End If
                If eNFO.FileInfo.StreamDetails.SubtitleSpecified Then
                    For Each sStream In eNFO.FileInfo.StreamDetails.Subtitle.Where(Function(f) f.LanguageSpecified AndAlso Not f.LongLanguageSpecified)
                        sStream.LongLanguage = Localization.ISOGetLangByCode3(sStream.Language)
                    Next
                End If
            End If
            Return eNFO
        Else
            Return eNFO
        End If
    End Function

    Public Shared Function FIToString(ByVal miFI As MediaInfo.Fileinfo, ByVal isTV As Boolean) As String
        '//
        ' Convert Fileinfo into a string to be displayed in the GUI
        '\\

        Dim strOutput As New StringBuilder
        Dim iVS As Integer = 1
        Dim iAS As Integer = 1
        Dim iSS As Integer = 1

        Try
            If miFI IsNot Nothing Then

                If miFI.StreamDetails IsNot Nothing Then
                    If miFI.StreamDetails.VideoSpecified Then strOutput.AppendFormat("{0}: {1}{2}", Master.eLang.GetString(595, "Video Streams"), miFI.StreamDetails.Video.Count.ToString, Environment.NewLine)
                    If miFI.StreamDetails.AudioSpecified Then strOutput.AppendFormat("{0}: {1}{2}", Master.eLang.GetString(596, "Audio Streams"), miFI.StreamDetails.Audio.Count.ToString, Environment.NewLine)
                    If miFI.StreamDetails.SubtitleSpecified Then strOutput.AppendFormat("{0}: {1}{2}", Master.eLang.GetString(597, "Subtitle  Streams"), miFI.StreamDetails.Subtitle.Count.ToString, Environment.NewLine)
                    For Each miVideo As MediaInfo.Video In miFI.StreamDetails.Video
                        strOutput.AppendFormat("{0}{1} {2}{0}", Environment.NewLine, Master.eLang.GetString(617, "Video Stream"), iVS)
                        If miVideo.WidthSpecified AndAlso miVideo.HeightSpecified Then strOutput.AppendFormat("- {0}{1}", String.Format(Master.eLang.GetString(269, "Size: {0}x{1}"), miVideo.Width, miVideo.Height), Environment.NewLine)
                        If miVideo.AspectSpecified Then strOutput.AppendFormat("- {0}: {1}{2}", Master.eLang.GetString(614, "Aspect Ratio"), miVideo.Aspect, Environment.NewLine)
                        If miVideo.ScantypeSpecified Then strOutput.AppendFormat("- {0}: {1}{2}", Master.eLang.GetString(605, "Scan Type"), miVideo.Scantype, Environment.NewLine)
                        If miVideo.CodecSpecified Then strOutput.AppendFormat("- {0}: {1}{2}", Master.eLang.GetString(604, "Codec"), miVideo.Codec, Environment.NewLine)
                        If miVideo.BitrateSpecified Then strOutput.AppendFormat("- {0}: {1}{2}", "Bitrate", miVideo.Bitrate, Environment.NewLine)
                        If miVideo.DurationSpecified Then strOutput.AppendFormat("- {0}: {1}{2}", Master.eLang.GetString(609, "Duration"), miVideo.Duration, Environment.NewLine)
                        'for now return filesize in mbytes instead of bytes(default)
                        If miVideo.Filesize > 0 Then strOutput.AppendFormat("- {0}: {1}{2}", Master.eLang.GetString(1455, "Filesize [MB]"), CStr(NumUtils.ConvertBytesTo(CLng(miVideo.Filesize), NumUtils.FileSizeUnit.Megabyte, 0)), Environment.NewLine)
                        If miVideo.LongLanguageSpecified Then strOutput.AppendFormat("- {0}: {1}{2}", Master.eLang.GetString(610, "Language"), miVideo.LongLanguage, Environment.NewLine)
                        If miVideo.MultiViewCountSpecified Then strOutput.AppendFormat("- {0}: {1}{2}", Master.eLang.GetString(1156, "MultiView Count"), miVideo.MultiViewCount, Environment.NewLine)
                        If miVideo.MultiViewLayoutSpecified Then strOutput.AppendFormat("- {0}: {1}{2}", Master.eLang.GetString(1157, "MultiView Layout"), miVideo.MultiViewLayout, Environment.NewLine)
                        If miVideo.StereoModeSpecified Then strOutput.AppendFormat("- {0}: {1} ({2})", Master.eLang.GetString(1286, "StereoMode"), miVideo.StereoMode, miVideo.ShortStereoMode)
                        iVS += 1
                    Next

                    strOutput.Append(Environment.NewLine)

                    For Each miAudio As MediaInfo.Audio In miFI.StreamDetails.Audio
                        'audio
                        strOutput.AppendFormat("{0}{1} {2}{0}", Environment.NewLine, Master.eLang.GetString(618, "Audio Stream"), iAS.ToString)
                        If miAudio.CodecSpecified Then strOutput.AppendFormat("- {0}: {1}{2}", Master.eLang.GetString(604, "Codec"), miAudio.Codec, Environment.NewLine)
                        If miAudio.ChannelsSpecified Then strOutput.AppendFormat("- {0}: {1}{2}", Master.eLang.GetString(611, "Channels"), miAudio.Channels, Environment.NewLine)
                        If miAudio.BitrateSpecified Then strOutput.AppendFormat("- {0}: {1}{2}", "Bitrate", miAudio.Bitrate, Environment.NewLine)
                        If miAudio.LongLanguageSpecified Then strOutput.AppendFormat("- {0}: {1}", Master.eLang.GetString(610, "Language"), miAudio.LongLanguage)
                        iAS += 1
                    Next

                    strOutput.Append(Environment.NewLine)

                    For Each miSub As MediaInfo.Subtitle In miFI.StreamDetails.Subtitle
                        'subtitles
                        strOutput.AppendFormat("{0}{1} {2}{0}", Environment.NewLine, Master.eLang.GetString(619, "Subtitle Stream"), iSS.ToString)
                        If miSub.LongLanguageSpecified Then strOutput.AppendFormat("- {0}: {1}", Master.eLang.GetString(610, "Language"), miSub.LongLanguage)
                        iSS += 1
                    Next
                End If
            End If
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try

        If strOutput.ToString.Trim.Length > 0 Then
            Return strOutput.ToString
        Else
            If isTV Then
                Return Master.eLang.GetString(504, "Meta Data is not available for this episode. Try rescanning.")
            Else
                Return Master.eLang.GetString(419, "Meta Data is not available for this movie. Try rescanning.")
            End If
        End If
    End Function

    ''' <summary>
    ''' Return the "best" or the "prefered language" audio stream of the videofile
    ''' </summary>
    ''' <param name="miFIA"><c>MediaInfo.Fileinfo</c> The Mediafile-container of the videofile</param>
    ''' <returns>The best <c>MediaInfo.Audio</c> stream information of the videofile</returns>
    ''' <remarks>
    ''' This is used to determine which audio stream information should be displayed in Ember main view (icon display)
    ''' The audiostream with most channels will be returned - if there are 2 or more streams which have the same "highest" channelcount then either the "DTSHD" stream or the one with highest bitrate will be returned
    ''' 
    ''' 2014/08/12 cocotus - Should work better: If there's more than one audiostream which highest channelcount, the one with highest bitrate or the DTSHD stream will be returned
    ''' </remarks>
    Public Shared Function GetBestAudio(ByVal miFIA As MediaInfo.Fileinfo, ByVal ForTV As Boolean) As MediaInfo.Audio
        '//
        ' Get the highest values from file info
        '\\

        Dim fiaOut As New MediaInfo.Audio
        Try
            Dim cmiFIA As New MediaInfo.Fileinfo

            Dim getPrefLanguage As Boolean = False
            Dim hasPrefLanguage As Boolean = False
            Dim prefLanguage As String = String.Empty
            Dim sinMostChannels As Single = 0
            Dim sinChans As Single = 0
            Dim sinMostBitrate As Single = 0
            Dim sinBitrate As Single = 0
            Dim sinCodec As String = String.Empty
            fiaOut.Codec = String.Empty
            fiaOut.Channels = String.Empty
            fiaOut.Language = String.Empty
            fiaOut.LongLanguage = String.Empty
            fiaOut.Bitrate = String.Empty

            If ForTV Then
                If Not String.IsNullOrEmpty(Master.eSettings.TVGeneralFlagLang) Then
                    getPrefLanguage = True
                    prefLanguage = Master.eSettings.TVGeneralFlagLang.ToLower
                End If
            Else
                If Not String.IsNullOrEmpty(Master.eSettings.MovieGeneralFlagLang) Then
                    getPrefLanguage = True
                    prefLanguage = Master.eSettings.MovieGeneralFlagLang.ToLower
                End If
            End If

            If getPrefLanguage AndAlso miFIA.StreamDetails.Audio.Where(Function(f) f.LongLanguage.ToLower = prefLanguage).Count > 0 Then
                For Each Stream As MediaInfo.Audio In miFIA.StreamDetails.Audio
                    If Stream.LongLanguage.ToLower = prefLanguage Then
                        cmiFIA.StreamDetails.Audio.Add(Stream)
                    End If
                Next
            Else
                cmiFIA.StreamDetails.Audio.AddRange(miFIA.StreamDetails.Audio)
            End If

            For Each miAudio As MediaInfo.Audio In cmiFIA.StreamDetails.Audio
                If Not String.IsNullOrEmpty(miAudio.Channels) Then
                    sinChans = NumUtils.ConvertToSingle(EmberAPI.MediaInfo.FormatAudioChannel(miAudio.Channels))
                    sinBitrate = 0
                    If Integer.TryParse(miAudio.Bitrate, 0) Then
                        sinBitrate = CInt(miAudio.Bitrate)
                    End If
                    If sinChans >= sinMostChannels AndAlso (sinBitrate > sinMostBitrate OrElse miAudio.Codec.ToLower.Contains("dtshd") OrElse sinBitrate = 0) Then
                        If Integer.TryParse(miAudio.Bitrate, 0) Then
                            sinMostBitrate = CInt(miAudio.Bitrate)
                        End If
                        sinMostChannels = sinChans
                        fiaOut.Bitrate = miAudio.Bitrate
                        fiaOut.Channels = sinChans.ToString
                        fiaOut.Codec = miAudio.Codec
                        fiaOut.Language = miAudio.Language
                        fiaOut.LongLanguage = miAudio.LongLanguage
                    End If
                End If

                If ForTV Then
                    If Not String.IsNullOrEmpty(Master.eSettings.TVGeneralFlagLang) AndAlso miAudio.LongLanguage.ToLower = Master.eSettings.TVGeneralFlagLang.ToLower Then fiaOut.HasPreferred = True
                Else
                    If Not String.IsNullOrEmpty(Master.eSettings.MovieGeneralFlagLang) AndAlso miAudio.LongLanguage.ToLower = Master.eSettings.MovieGeneralFlagLang.ToLower Then fiaOut.HasPreferred = True
                End If
            Next

        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return fiaOut
    End Function

    Public Shared Function GetBestVideo(ByVal miFIV As MediaInfo.Fileinfo) As MediaInfo.Video
        '//
        ' Get the highest values from file info
        '\\

        Dim fivOut As New MediaInfo.Video
        Try
            Dim iWidest As Integer = 0
            Dim iWidth As Integer = 0

            'set some defaults to make it easy on ourselves
            fivOut.Width = String.Empty
            fivOut.Height = String.Empty
            fivOut.Aspect = String.Empty
            fivOut.Codec = String.Empty
            fivOut.Duration = String.Empty
            fivOut.Scantype = String.Empty
            fivOut.Language = String.Empty
            'cocotus, 2013/02 Added support for new MediaInfo-fields
            fivOut.Bitrate = String.Empty
            fivOut.MultiViewCount = String.Empty
            fivOut.MultiViewLayout = String.Empty
            fivOut.Filesize = 0
            'cocotus end

            For Each miVideo As MediaInfo.Video In miFIV.StreamDetails.Video
                If Not String.IsNullOrEmpty(miVideo.Width) Then
                    If Integer.TryParse(miVideo.Width, 0) Then
                        iWidth = Convert.ToInt32(miVideo.Width)
                    Else
                        logger.Warn("[GetBestVideo] Invalid width(not a number!) of videostream: " & miVideo.Width)
                    End If
                    If iWidth > iWidest Then
                        iWidest = iWidth
                        fivOut.Width = miVideo.Width
                        fivOut.Height = miVideo.Height
                        fivOut.Aspect = miVideo.Aspect
                        fivOut.Codec = miVideo.Codec
                        fivOut.Duration = miVideo.Duration
                        fivOut.Scantype = miVideo.Scantype
                        fivOut.Language = miVideo.Language

                        'cocotus, 2013/02 Added support for new MediaInfo-fields

                        'MultiViewCount (3D) handling, simply map field
                        fivOut.MultiViewCount = miVideo.MultiViewCount

                        'MultiViewLayout (3D) handling, simply map field
                        fivOut.MultiViewLayout = miVideo.MultiViewLayout

                        'FileSize handling, simply map field
                        fivOut.Filesize = miVideo.Filesize

                        'Bitrate handling, simply map field
                        fivOut.Bitrate = miVideo.Bitrate
                        'cocotus end

                    End If
                End If
            Next

        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
        Return fivOut
    End Function

    Public Shared Function GetDimensionsFromVideo(ByVal fiRes As MediaInfo.Video) As String
        '//
        ' Get the dimension values of the video from the information provided by MediaInfo.dll
        '\\

        Dim result As String = String.Empty
        Try
            If Not String.IsNullOrEmpty(fiRes.Width) AndAlso Not String.IsNullOrEmpty(fiRes.Height) AndAlso Not String.IsNullOrEmpty(fiRes.Aspect) Then
                Dim iWidth As Integer = Convert.ToInt32(fiRes.Width)
                Dim iHeight As Integer = Convert.ToInt32(fiRes.Height)
                Dim sinADR As Single = NumUtils.ConvertToSingle(fiRes.Aspect)

                result = String.Format("{0}x{1} ({2})", iWidth, iHeight, sinADR.ToString("0.00"))
            End If
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try

        Return result
    End Function

    Public Shared Function GetEpNfoPath(ByVal EpPath As String) As String
        Dim nPath As String = String.Empty

        If File.Exists(String.Concat(FileUtils.Common.RemoveExtFromPath(EpPath), ".nfo")) Then
            nPath = String.Concat(FileUtils.Common.RemoveExtFromPath(EpPath), ".nfo")
        End If

        Return nPath
    End Function

    Public Shared Function GetIMDBFromNonConf(ByVal sPath As String, ByVal isSingle As Boolean) As NonConf
        Dim tNonConf As New NonConf
        Dim dirPath As String = Directory.GetParent(sPath).FullName
        Dim lFiles As New List(Of String)

        If isSingle Then
            Try
                lFiles.AddRange(Directory.GetFiles(dirPath, "*.nfo"))
            Catch
            End Try
            Try
                lFiles.AddRange(Directory.GetFiles(dirPath, "*.info"))
            Catch
            End Try
        Else
            Dim fName As String = StringUtils.CleanStackingMarkers(Path.GetFileNameWithoutExtension(sPath)).ToLower
            Dim oName As String = Path.GetFileNameWithoutExtension(sPath)
            fName = If(fName.EndsWith("*"), fName, String.Concat(fName, "*"))
            oName = If(oName.EndsWith("*"), oName, String.Concat(oName, "*"))

            Try
                lFiles.AddRange(Directory.GetFiles(dirPath, String.Concat(fName, ".nfo")))
            Catch
            End Try
            Try
                lFiles.AddRange(Directory.GetFiles(dirPath, String.Concat(oName, ".nfo")))
            Catch
            End Try
            Try
                lFiles.AddRange(Directory.GetFiles(dirPath, String.Concat(fName, ".info")))
            Catch
            End Try
            Try
                lFiles.AddRange(Directory.GetFiles(dirPath, String.Concat(oName, ".info")))
            Catch
            End Try
        End If

        For Each sFile As String In lFiles
            Using srInfo As New StreamReader(sFile)
                Dim sInfo As String = srInfo.ReadToEnd
                Dim sIMDBID As String = Regex.Match(sInfo, "tt\d\d\d\d\d\d\d", RegexOptions.Multiline Or RegexOptions.Singleline Or RegexOptions.IgnoreCase).ToString

                If Not String.IsNullOrEmpty(sIMDBID) Then
                    tNonConf.IMDBID = sIMDBID
                    'now lets try to see if the rest of the file is a proper nfo
                    If sInfo.ToLower.Contains("</movie>") Then
                        tNonConf.Text = APIXML.XMLToLowerCase(sInfo.Substring(0, sInfo.ToLower.IndexOf("</movie>") + 8))
                    End If
                    Exit For
                Else
                    sIMDBID = Regex.Match(sPath, "tt\d\d\d\d\d\d\d", RegexOptions.Multiline Or RegexOptions.Singleline Or RegexOptions.IgnoreCase).ToString
                    If Not String.IsNullOrEmpty(sIMDBID) Then
                        tNonConf.IMDBID = sIMDBID
                    End If
                End If
            End Using
        Next
        Return tNonConf
    End Function

    Public Shared Function GetNfoPath_MovieSet(ByVal DBElement As Database.DBElement) As String
        For Each a In FileUtils.GetFilenameList.MovieSet(DBElement, Enums.ModifierType.MainNFO)
            If File.Exists(a) Then
                Return a
            End If
        Next

        Return String.Empty

    End Function

    Public Shared Function GetResFromDimensions(ByVal fiRes As MediaInfo.Video) As String
        '//
        ' Get the resolution of the video from the dimensions provided by MediaInfo.dll
        '\\

        Dim resOut As String = String.Empty
        Try
            If Not String.IsNullOrEmpty(fiRes.Width) AndAlso Not String.IsNullOrEmpty(fiRes.Height) AndAlso Not String.IsNullOrEmpty(fiRes.Aspect) Then
                Dim iWidth As Integer = Convert.ToInt32(fiRes.Width)
                Dim iHeight As Integer = Convert.ToInt32(fiRes.Height)
                Dim sinADR As Single = NumUtils.ConvertToSingle(fiRes.Aspect)

                Select Case True
                    Case iWidth < 640
                        resOut = "SD"
                        'exact
                    Case (iWidth = 1920 AndAlso (iHeight = 1080 OrElse iHeight = 800)) OrElse (iWidth = 1440 AndAlso iHeight = 1080) OrElse (iWidth = 1280 AndAlso iHeight = 1080)
                        resOut = "1080"
                    Case (iWidth = 1366 AndAlso iHeight = 768) OrElse (iWidth = 1024 AndAlso iHeight = 768)
                        resOut = "768"
                    Case (iWidth = 960 AndAlso iHeight = 720) OrElse (iWidth = 1280 AndAlso (iHeight = 720 OrElse iHeight = 544))
                        resOut = "720"
                    Case (iWidth = 1024 AndAlso iHeight = 576) OrElse (iWidth = 720 AndAlso iHeight = 576)
                        resOut = "576"
                    Case (iWidth = 720 OrElse iWidth = 960) AndAlso iHeight = 540
                        resOut = "540"
                    Case (iWidth = 852 OrElse iWidth = 720 OrElse iWidth = 704 OrElse iWidth = 640) AndAlso iHeight = 480
                        resOut = "480"
                        'by ADR
                    Case sinADR >= 1.4 AndAlso iWidth = 1920
                        resOut = "1080"
                    Case sinADR >= 1.4 AndAlso iWidth = 1366
                        resOut = "768"
                    Case sinADR >= 1.4 AndAlso iWidth = 1280
                        resOut = "720"
                    Case sinADR >= 1.4 AndAlso iWidth = 1024
                        resOut = "576"
                    Case sinADR >= 1.4 AndAlso iWidth = 960
                        resOut = "540"
                    Case sinADR >= 1.4 AndAlso iWidth = 852
                        resOut = "480"
                        'loose
                    Case iWidth >= 1200 AndAlso iHeight > 768
                        resOut = "1080"
                    Case iWidth >= 1000 AndAlso iHeight > 720
                        resOut = "768"
                    Case iWidth >= 1000 AndAlso iHeight > 500
                        resOut = "720"
                    Case iWidth >= 700 AndAlso iHeight > 540
                        resOut = "576"
                    Case iWidth >= 700 AndAlso iHeight > 480
                        resOut = "540"
                    Case Else
                        resOut = "480"
                End Select
            End If
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try

        If Not String.IsNullOrEmpty(resOut) Then
            If String.IsNullOrEmpty(fiRes.Scantype) Then
                Return String.Concat(resOut)
            Else
                Return String.Concat(resOut, If(fiRes.Scantype.ToLower = "progressive", "p", "i"))
            End If
        Else
            Return String.Empty
        End If
    End Function

    Public Shared Function GetShowNfoPath(ByVal ShowPath As String) As String
        Dim nPath As String = String.Empty

        If File.Exists(Path.Combine(ShowPath, "tvshow.nfo")) Then
            nPath = Path.Combine(ShowPath, "tvshow.nfo")
        End If

        Return nPath
    End Function

    Public Shared Function IsConformingEpNfo(ByVal sPath As String) As Boolean
        Dim testSer As XmlSerializer = New XmlSerializer(GetType(MediaContainers.EpisodeDetails))
        Dim testEp As New MediaContainers.EpisodeDetails

        Try
            If (Path.GetExtension(sPath) = ".nfo" OrElse Path.GetExtension(sPath) = ".info") AndAlso File.Exists(sPath) Then
                Using xmlSR As StreamReader = New StreamReader(sPath)
                    Dim xmlStr As String = xmlSR.ReadToEnd
                    Dim rMatches As MatchCollection = Regex.Matches(xmlStr, "<episodedetails.*?>.*?</episodedetails>", RegexOptions.IgnoreCase Or RegexOptions.Singleline Or RegexOptions.IgnorePatternWhitespace)
                    If rMatches.Count = 1 Then
                        Using xmlRead As StringReader = New StringReader(rMatches(0).Value)
                            testEp = DirectCast(testSer.Deserialize(xmlRead), MediaContainers.EpisodeDetails)
                            testSer = Nothing
                            testEp = Nothing
                            Return True
                        End Using
                    ElseIf rMatches.Count > 1 Then
                        'read them all... if one fails, the entire nfo is non conforming
                        For Each xmlReg As Match In rMatches
                            Using xmlRead As StringReader = New StringReader(xmlReg.Value)
                                testEp = DirectCast(testSer.Deserialize(xmlRead), MediaContainers.EpisodeDetails)
                                testEp = Nothing
                            End Using
                        Next
                        testSer = Nothing
                        Return True
                    Else
                        testSer = Nothing
                        If testEp IsNot Nothing Then
                            testEp = Nothing
                        End If
                        Return False
                    End If
                End Using
            Else
                testSer = Nothing
                testEp = Nothing
                Return False
            End If
        Catch
            If testSer IsNot Nothing Then
                testSer = Nothing
            End If
            If testEp IsNot Nothing Then
                testEp = Nothing
            End If
            Return False
        End Try
    End Function

    Public Shared Function IsConformingNfo(ByVal sPath As String) As Boolean
        Dim testSer As XmlSerializer = Nothing

        Try
            If (Path.GetExtension(sPath) = ".nfo" OrElse Path.GetExtension(sPath) = ".info") AndAlso File.Exists(sPath) Then
                Using testSR As StreamReader = New StreamReader(sPath)
                    testSer = New XmlSerializer(GetType(MediaContainers.Movie))
                    Dim testMovie As MediaContainers.Movie = DirectCast(testSer.Deserialize(testSR), MediaContainers.Movie)
                    testMovie = Nothing
                    testSer = Nothing
                End Using
                Return True
            Else
                Return False
            End If
        Catch
            If testSer IsNot Nothing Then
                testSer = Nothing
            End If

            Return False
        End Try
    End Function

    Public Shared Function IsConformingShowNfo(ByVal sPath As String) As Boolean
        Dim testSer As XmlSerializer = Nothing

        Try
            If (Path.GetExtension(sPath) = ".nfo" OrElse Path.GetExtension(sPath) = ".info") AndAlso File.Exists(sPath) Then
                Using testSR As StreamReader = New StreamReader(sPath)
                    testSer = New XmlSerializer(GetType(MediaContainers.TVShow))
                    Dim testShow As MediaContainers.TVShow = DirectCast(testSer.Deserialize(testSR), MediaContainers.TVShow)
                    testShow = Nothing
                    testSer = Nothing
                End Using
                Return True
            Else
                Return False
            End If
        Catch
            If testSer IsNot Nothing Then
                testSer = Nothing
            End If

            Return False
        End Try
    End Function

    Public Shared Function LoadMovieFromNFO(ByVal sPath As String, ByVal isSingle As Boolean) As MediaContainers.Movie
        '//
        ' Deserialze the NFO to pass all the data to a MediaContainers.Movie
        '\\

        Dim xmlSer As XmlSerializer = Nothing
        Dim xmlMov As New MediaContainers.Movie

        If Not String.IsNullOrEmpty(sPath) Then
            Try
                If File.Exists(sPath) AndAlso Path.GetExtension(sPath).ToLower = ".nfo" Then
                    Using xmlSR As StreamReader = New StreamReader(sPath)
                        xmlSer = New XmlSerializer(GetType(MediaContainers.Movie))
                        xmlMov = DirectCast(xmlSer.Deserialize(xmlSR), MediaContainers.Movie)
                        xmlMov = CleanNFO_Movies(xmlMov)
                    End Using
                Else
                    If Not String.IsNullOrEmpty(sPath) Then
                        Dim sReturn As New NonConf
                        sReturn = GetIMDBFromNonConf(sPath, isSingle)
                        xmlMov.IMDBID = sReturn.IMDBID
                        Try
                            If Not String.IsNullOrEmpty(sReturn.Text) Then
                                Using xmlSTR As StringReader = New StringReader(sReturn.Text)
                                    xmlSer = New XmlSerializer(GetType(MediaContainers.Movie))
                                    xmlMov = DirectCast(xmlSer.Deserialize(xmlSTR), MediaContainers.Movie)
                                    xmlMov.IMDBID = sReturn.IMDBID
                                    xmlMov = CleanNFO_Movies(xmlMov)
                                End Using
                            End If
                        Catch
                        End Try
                    End If
                End If

            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)

                xmlMov.Clear()
                If Not String.IsNullOrEmpty(sPath) Then

                    'go ahead and rename it now, will still be picked up in getimdbfromnonconf
                    If Not Master.eSettings.GeneralOverwriteNfo Then
                        RenameNonConfNfo(sPath, True)
                    End If

                    Dim sReturn As New NonConf
                    sReturn = GetIMDBFromNonConf(sPath, isSingle)
                    xmlMov.IMDBID = sReturn.IMDBID
                    Try
                        If Not String.IsNullOrEmpty(sReturn.Text) Then
                            Using xmlSTR As StringReader = New StringReader(sReturn.Text)
                                xmlSer = New XmlSerializer(GetType(MediaContainers.Movie))
                                xmlMov = DirectCast(xmlSer.Deserialize(xmlSTR), MediaContainers.Movie)
                                xmlMov.IMDBID = sReturn.IMDBID
                                xmlMov = CleanNFO_Movies(xmlMov)
                            End Using
                        End If
                    Catch
                    End Try
                End If
            End Try

            If xmlSer IsNot Nothing Then
                xmlSer = Nothing
            End If
        End If

        Return xmlMov
    End Function

    Public Shared Function LoadMovieSetFromNFO(ByVal sPath As String) As MediaContainers.MovieSet
        '//
        ' Deserialze the NFO to pass all the data to a MediaContainers.Movie
        '\\

        Dim xmlSer As XmlSerializer = Nothing
        Dim xmlMovSet As New MediaContainers.MovieSet

        If Not String.IsNullOrEmpty(sPath) Then
            Try
                If File.Exists(sPath) AndAlso Path.GetExtension(sPath).ToLower = ".nfo" Then
                    Using xmlSR As StreamReader = New StreamReader(sPath)
                        xmlSer = New XmlSerializer(GetType(MediaContainers.MovieSet))
                        xmlMovSet = DirectCast(xmlSer.Deserialize(xmlSR), MediaContainers.MovieSet)
                        xmlMovSet.Plot = xmlMovSet.Plot.Replace(vbCrLf, vbLf).Replace(vbLf, vbCrLf)
                    End Using
                    'Else
                    '    If Not String.IsNullOrEmpty(sPath) Then
                    '        Dim sReturn As New NonConf
                    '        sReturn = GetIMDBFromNonConf(sPath, isSingle)
                    '        xmlMov.IMDBID = sReturn.IMDBID
                    '        Try
                    '            If Not String.IsNullOrEmpty(sReturn.Text) Then
                    '                Using xmlSTR As StringReader = New StringReader(sReturn.Text)
                    '                    xmlSer = New XmlSerializer(GetType(MediaContainers.Movie))
                    '                    xmlMov = DirectCast(xmlSer.Deserialize(xmlSTR), MediaContainers.Movie)
                    '                    xmlMov.Genre = Strings.Join(xmlMov.LGenre.ToArray, " / ")
                    '                    xmlMov.Outline = xmlMov.Outline.Replace(vbCrLf, vbLf).Replace(vbLf, vbCrLf)
                    '                    xmlMovSet.Plot = xmlMovSet.Plot.Replace(vbCrLf, vbLf).Replace(vbLf, vbCrLf)
                    '                    xmlMov.IMDBID = sReturn.IMDBID
                    '                End Using
                    '            End If
                    '        Catch
                    '        End Try
                    '    End If
                End If

            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)

                xmlMovSet.Clear()
                'If Not String.IsNullOrEmpty(sPath) Then

                '    'go ahead and rename it now, will still be picked up in getimdbfromnonconf
                '    If Not Master.eSettings.GeneralOverwriteNfo Then
                '        RenameNonConfNfo(sPath, True)
                '    End If

                '    Dim sReturn As New NonConf
                '    sReturn = GetIMDBFromNonConf(sPath, isSingle)
                '    xmlMov.IMDBID = sReturn.IMDBID
                '    Try
                '        If Not String.IsNullOrEmpty(sReturn.Text) Then
                '            Using xmlSTR As StringReader = New StringReader(sReturn.Text)
                '                xmlSer = New XmlSerializer(GetType(MediaContainers.Movie))
                '                xmlMov = DirectCast(xmlSer.Deserialize(xmlSTR), MediaContainers.Movie)
                '                xmlMov.Genre = Strings.Join(xmlMov.LGenre.ToArray, " / ")
                '                xmlMov.Outline = xmlMov.Outline.Replace(vbCrLf, vbLf).Replace(vbLf, vbCrLf)
                '                xmlMovSet.Plot = xmlMovSet.Plot.Replace(vbCrLf, vbLf).Replace(vbLf, vbCrLf)
                '                xmlMov.IMDBID = sReturn.IMDBID
                '            End Using
                '        End If
                '    Catch
                '    End Try
                'End If
            End Try

            If xmlSer IsNot Nothing Then
                xmlSer = Nothing
            End If
        End If

        Return xmlMovSet
    End Function

    Public Shared Function LoadTVEpFromNFO(ByVal sPath As String, ByVal SeasonNumber As Integer, ByVal EpisodeNumber As Integer) As MediaContainers.EpisodeDetails
        Dim xmlSer As XmlSerializer = New XmlSerializer(GetType(MediaContainers.EpisodeDetails))
        Dim xmlEp As New MediaContainers.EpisodeDetails

        If Not String.IsNullOrEmpty(sPath) AndAlso SeasonNumber >= -1 Then
            Try
                If File.Exists(sPath) AndAlso Path.GetExtension(sPath).ToLower = ".nfo" Then
                    'better way to read multi-root xml??
                    Using xmlSR As StreamReader = New StreamReader(sPath)
                        Dim xmlStr As String = xmlSR.ReadToEnd
                        Dim rMatches As MatchCollection = Regex.Matches(xmlStr, "<episodedetails.*?>.*?</episodedetails>", RegexOptions.IgnoreCase Or RegexOptions.Singleline Or RegexOptions.IgnorePatternWhitespace)
                        If rMatches.Count = 1 Then
                            'only one episodedetail... assume it's the proper one
                            Using xmlRead As StringReader = New StringReader(rMatches(0).Value)
                                xmlEp = DirectCast(xmlSer.Deserialize(xmlRead), MediaContainers.EpisodeDetails)
                                xmlEp = CleanNFO_TVEpisodes(xmlEp)
                                xmlSer = Nothing
                                If xmlEp.FileInfoSpecified Then
                                    If xmlEp.FileInfo.StreamDetails.AudioSpecified Then
                                        For Each aStream In xmlEp.FileInfo.StreamDetails.Audio.Where(Function(f) f.LanguageSpecified AndAlso Not f.LongLanguageSpecified)
                                            aStream.LongLanguage = Localization.ISOGetLangByCode3(aStream.Language)
                                        Next
                                    End If
                                    If xmlEp.FileInfo.StreamDetails.SubtitleSpecified Then
                                        For Each sStream In xmlEp.FileInfo.StreamDetails.Subtitle.Where(Function(f) f.LanguageSpecified AndAlso Not f.LongLanguageSpecified)
                                            sStream.LongLanguage = Localization.ISOGetLangByCode3(sStream.Language)
                                        Next
                                    End If
                                End If
                                Return xmlEp
                            End Using
                        ElseIf rMatches.Count > 1 Then
                            For Each xmlReg As Match In rMatches
                                Using xmlRead As StringReader = New StringReader(xmlReg.Value)
                                    xmlEp = DirectCast(xmlSer.Deserialize(xmlRead), MediaContainers.EpisodeDetails)
                                    xmlEp = CleanNFO_TVEpisodes(xmlEp)
                                    If xmlEp.Episode = EpisodeNumber AndAlso xmlEp.Season = SeasonNumber Then
                                        xmlSer = Nothing
                                        Return xmlEp
                                    End If
                                End Using
                            Next
                        End If
                    End Using

                Else
                    'not really anything else to do with non-conforming nfos aside from rename them
                    If Not Master.eSettings.GeneralOverwriteNfo Then
                        RenameEpNonConfNfo(sPath, True)
                    End If
                End If

            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)

                'not really anything else to do with non-conforming nfos aside from rename them
                If Not Master.eSettings.GeneralOverwriteNfo Then
                    RenameEpNonConfNfo(sPath, True)
                End If
            End Try
        End If

        Return New MediaContainers.EpisodeDetails
    End Function

    Public Shared Function LoadTVEpFromNFO(ByVal sPath As String, ByVal SeasonNumber As Integer, ByVal Aired As String) As MediaContainers.EpisodeDetails
        Dim xmlSer As XmlSerializer = New XmlSerializer(GetType(MediaContainers.EpisodeDetails))
        Dim xmlEp As New MediaContainers.EpisodeDetails

        If Not String.IsNullOrEmpty(sPath) AndAlso SeasonNumber >= -1 Then
            Try
                If File.Exists(sPath) AndAlso Path.GetExtension(sPath).ToLower = ".nfo" Then
                    'better way to read multi-root xml??
                    Using xmlSR As StreamReader = New StreamReader(sPath)
                        Dim xmlStr As String = xmlSR.ReadToEnd
                        Dim rMatches As MatchCollection = Regex.Matches(xmlStr, "<episodedetails.*?>.*?</episodedetails>", RegexOptions.IgnoreCase Or RegexOptions.Singleline Or RegexOptions.IgnorePatternWhitespace)
                        If rMatches.Count = 1 Then
                            'only one episodedetail... assume it's the proper one
                            Using xmlRead As StringReader = New StringReader(rMatches(0).Value)
                                xmlEp = DirectCast(xmlSer.Deserialize(xmlRead), MediaContainers.EpisodeDetails)
                                xmlEp = CleanNFO_TVEpisodes(xmlEp)
                                xmlSer = Nothing
                                If xmlEp.FileInfoSpecified Then
                                    If xmlEp.FileInfo.StreamDetails.AudioSpecified Then
                                        For Each aStream In xmlEp.FileInfo.StreamDetails.Audio.Where(Function(f) f.LanguageSpecified AndAlso Not f.LongLanguageSpecified)
                                            aStream.LongLanguage = Localization.ISOGetLangByCode3(aStream.Language)
                                        Next
                                    End If
                                    If xmlEp.FileInfo.StreamDetails.SubtitleSpecified Then
                                        For Each sStream In xmlEp.FileInfo.StreamDetails.Subtitle.Where(Function(f) f.LanguageSpecified AndAlso Not f.LongLanguageSpecified)
                                            sStream.LongLanguage = Localization.ISOGetLangByCode3(sStream.Language)
                                        Next
                                    End If
                                End If
                                Return xmlEp
                            End Using
                        ElseIf rMatches.Count > 1 Then
                            For Each xmlReg As Match In rMatches
                                Using xmlRead As StringReader = New StringReader(xmlReg.Value)
                                    xmlEp = DirectCast(xmlSer.Deserialize(xmlRead), MediaContainers.EpisodeDetails)
                                    xmlEp = CleanNFO_TVEpisodes(xmlEp)
                                    If xmlEp.Aired = Aired AndAlso xmlEp.Season = SeasonNumber Then
                                        xmlSer = Nothing
                                        Return xmlEp
                                    End If
                                End Using
                            Next
                        End If
                    End Using

                Else
                    'not really anything else to do with non-conforming nfos aside from rename them
                    If Not Master.eSettings.GeneralOverwriteNfo Then
                        RenameEpNonConfNfo(sPath, True)
                    End If
                End If

            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)

                'not really anything else to do with non-conforming nfos aside from rename them
                If Not Master.eSettings.GeneralOverwriteNfo Then
                    RenameEpNonConfNfo(sPath, True)
                End If
            End Try
        End If

        Return New MediaContainers.EpisodeDetails
    End Function

    Public Shared Function LoadTVShowFromNFO(ByVal sPath As String) As MediaContainers.TVShow
        Dim xmlSer As XmlSerializer = Nothing
        Dim xmlShow As New MediaContainers.TVShow

        If Not String.IsNullOrEmpty(sPath) Then
            Try
                If File.Exists(sPath) AndAlso Path.GetExtension(sPath).ToLower = ".nfo" Then
                    Using xmlSR As StreamReader = New StreamReader(sPath)
                        xmlSer = New XmlSerializer(GetType(MediaContainers.TVShow))
                        xmlShow = DirectCast(xmlSer.Deserialize(xmlSR), MediaContainers.TVShow)
                        xmlShow.Genre = String.Join(" / ", xmlShow.Genres.ToArray)
                        xmlShow.Votes = Regex.Replace(xmlShow.Votes, "\D", String.Empty)
                    End Using
                Else
                    'not really anything else to do with non-conforming nfos aside from rename them
                    If Not Master.eSettings.GeneralOverwriteNfo Then
                        RenameShowNonConfNfo(sPath)
                    End If
                End If

            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)

                'not really anything else to do with non-conforming nfos aside from rename them
                If Not Master.eSettings.GeneralOverwriteNfo Then
                    RenameShowNonConfNfo(sPath)
                End If
            End Try

            Try
                Dim params As New List(Of Object)(New Object() {xmlShow})
                Dim doContinue As Boolean = True
                ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.OnNFORead_TVShow, params, doContinue, False)

            Catch ex As Exception
                logger.Error(New StackFrame().GetMethod().Name, ex)
            End Try

            'Boxee support
            If Master.eSettings.TVUseBoxee Then
                If xmlShow.BoxeeTvDbSpecified() Then
                    xmlShow.TVDB = xmlShow.BoxeeTvDb
                    xmlShow.BlankBoxeeId()
                End If
            End If

            If xmlSer IsNot Nothing Then
                xmlSer = Nothing
            End If
        End If

        Return xmlShow
    End Function

    Public Shared Sub SaveMovieToNFO(ByRef movieToSave As Database.DBElement)

        Try
            Try
                Dim params As New List(Of Object)(New Object() {movieToSave})
                Dim doContinue As Boolean = True
                ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.OnNFOSave_Movie, params, doContinue, False)
                If Not doContinue Then Return
            Catch ex As Exception
            End Try

            If Not String.IsNullOrEmpty(movieToSave.Filename) Then
                Dim xmlSer As New XmlSerializer(GetType(MediaContainers.Movie))
                Dim doesExist As Boolean = False
                Dim fAtt As New FileAttributes
                Dim fAttWritable As Boolean = True

                'YAMJ support
                If Master.eSettings.MovieUseYAMJ AndAlso Master.eSettings.MovieNFOYAMJ Then
                    If movieToSave.Movie.TMDBIDSpecified Then
                        movieToSave.Movie.TMDBID = String.Empty
                    End If
                End If

                'digit grouping symbol for Votes count
                If Master.eSettings.GeneralDigitGrpSymbolVotes Then
                    If movieToSave.Movie.VotesSpecified Then
                        Dim vote As String = Double.Parse(movieToSave.Movie.Votes, Globalization.CultureInfo.InvariantCulture).ToString("N0", Globalization.CultureInfo.CurrentCulture)
                        If vote IsNot Nothing Then movieToSave.Movie.Votes = vote
                    End If
                End If

                For Each a In FileUtils.GetFilenameList.Movie(movieToSave, Enums.ModifierType.MainNFO)
                    If Not Master.eSettings.GeneralOverwriteNfo Then
                        RenameNonConfNfo(a, False)
                    End If

                    doesExist = File.Exists(a)
                    If Not doesExist OrElse (Not CBool(File.GetAttributes(a) And FileAttributes.ReadOnly)) Then
                        If doesExist Then
                            fAtt = File.GetAttributes(a)
                            Try
                                File.SetAttributes(a, FileAttributes.Normal)
                            Catch ex As Exception
                                fAttWritable = False
                            End Try
                        End If
                        Using xmlSW As New StreamWriter(a)
                            movieToSave.NfoPath = a
                            xmlSer.Serialize(xmlSW, movieToSave.Movie)
                        End Using
                        If doesExist And fAttWritable Then File.SetAttributes(a, fAtt)
                    End If
                Next
            End If

        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
    End Sub

    Public Shared Sub SaveMovieSetToNFO(ByRef moviesetToSave As Database.DBElement)

        Try
            'Try
            '    Dim params As New List(Of Object)(New Object() {moviesetToSave})
            '    Dim doContinue As Boolean = True
            '    ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.OnMovieSetNFOSave, params, doContinue, False)
            '    If Not doContinue Then Return
            'Catch ex As Exception
            'End Try

            If Not String.IsNullOrEmpty(moviesetToSave.MovieSet.Title) Then
                Dim xmlSer As New XmlSerializer(GetType(MediaContainers.MovieSet))
                Dim doesExist As Boolean = False
                Dim fAtt As New FileAttributes
                Dim fAttWritable As Boolean = True

                For Each a In FileUtils.GetFilenameList.MovieSet(moviesetToSave, Enums.ModifierType.MainNFO)
                    'If Not Master.eSettings.GeneralOverwriteNfo Then
                    '    RenameNonConfNfo(a, False)
                    'End If

                    doesExist = File.Exists(a)
                    If Not doesExist OrElse (Not CBool(File.GetAttributes(a) And FileAttributes.ReadOnly)) Then
                        If doesExist Then
                            fAtt = File.GetAttributes(a)
                            Try
                                File.SetAttributes(a, FileAttributes.Normal)
                            Catch ex As Exception
                                fAttWritable = False
                            End Try
                        End If
                        Using xmlSW As New StreamWriter(a)
                            moviesetToSave.NfoPath = a
                            xmlSer.Serialize(xmlSW, moviesetToSave.MovieSet)
                        End Using
                        If doesExist And fAttWritable Then File.SetAttributes(a, fAtt)
                    End If
                Next
            End If

        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
    End Sub

    Public Shared Sub SaveSingleNFOItem(ByVal sPath As String, ByVal strToWrite As String, ByVal strNode As String)
        '//
        ' Save just one item of an NFO file
        '\\

        Try
            Dim xmlDoc As New XmlDocument()
            'use streamreader to open NFO so we don't get any access violations when trying to save
            Dim xmlSR As New StreamReader(sPath)
            'copy NFO to string
            Dim xmlString As String = xmlSR.ReadToEnd
            'close the streamreader... we're done with it
            xmlSR.Close()
            xmlSR = Nothing

            xmlDoc.LoadXml(xmlString)
            Dim xNode As XmlNode = xmlDoc.SelectSingleNode(strNode)
            xNode.InnerText = strToWrite
            xmlDoc.Save(sPath)

            xmlDoc = Nothing
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
    End Sub

    Public NotInheritable Class Utf8StringWriter
        Inherits StringWriter
        Public Overloads Overrides ReadOnly Property Encoding() As Encoding
            Get
                Return Encoding.UTF8
            End Get
        End Property
    End Class

    Public Shared Sub SaveTVEpToNFO(ByRef tvEpToSave As Database.DBElement)

        Try

            If Not String.IsNullOrEmpty(tvEpToSave.Filename) Then
                Dim xmlSer As New XmlSerializer(GetType(MediaContainers.EpisodeDetails))

                Dim tPath As String = String.Empty
                Dim doesExist As Boolean = False
                Dim fAtt As New FileAttributes
                Dim fAttWritable As Boolean = True
                Dim EpList As New List(Of MediaContainers.EpisodeDetails)
                Dim sBuilder As New StringBuilder

                Dim tmpName As String = Path.GetFileNameWithoutExtension(tvEpToSave.Filename)
                tPath = String.Concat(Path.Combine(Directory.GetParent(tvEpToSave.Filename).FullName, tmpName), ".nfo")

                If Not Master.eSettings.GeneralOverwriteNfo Then
                    RenameEpNonConfNfo(tPath, False)
                End If

                doesExist = File.Exists(tPath)
                If Not doesExist OrElse (Not CBool(File.GetAttributes(tPath) And FileAttributes.ReadOnly)) Then

                    If doesExist Then
                        fAtt = File.GetAttributes(tPath)
                        Try
                            File.SetAttributes(tPath, FileAttributes.Normal)
                        Catch ex As Exception
                            fAttWritable = False
                        End Try
                    End If

                    Using SQLCommand As SQLite.SQLiteCommand = Master.DB.MyVideosDBConn.CreateCommand()
                        SQLCommand.CommandText = "SELECT idEpisode FROM episode WHERE idEpisode <> (?) AND idFile IN (SELECT idFile FROM files WHERE strFilename = (?)) ORDER BY Episode"
                        Dim parID As SQLite.SQLiteParameter = SQLCommand.Parameters.Add("parID", DbType.Int64, 0, "idEpisode")
                        Dim parFilename As SQLite.SQLiteParameter = SQLCommand.Parameters.Add("parFilename", DbType.String, 0, "strFilename")

                        parID.Value = tvEpToSave.ID
                        parFilename.Value = tvEpToSave.Filename

                        Using SQLreader As SQLite.SQLiteDataReader = SQLCommand.ExecuteReader
                            While SQLreader.Read
                                EpList.Add(Master.DB.LoadTVEpisodeFromDB(Convert.ToInt64(SQLreader("idEpisode")), False).TVEpisode)
                            End While
                        End Using

                        EpList.Add(tvEpToSave.TVEpisode)

                        Dim NS As New XmlSerializerNamespaces
                        NS.Add(String.Empty, String.Empty)

                        For Each tvEp As MediaContainers.EpisodeDetails In EpList.OrderBy(Function(s) s.Season)

                            'digit grouping symbol for Votes count
                            If Master.eSettings.GeneralDigitGrpSymbolVotes Then
                                If tvEp.VotesSpecified Then
                                    Dim vote As String = Double.Parse(tvEp.Votes, Globalization.CultureInfo.InvariantCulture).ToString("N0", Globalization.CultureInfo.CurrentCulture)
                                    If vote IsNot Nothing Then tvEp.Votes = vote
                                End If
                            End If

                            'removing <displayepisode> and <displayseason> if disabled
                            If Not Master.eSettings.TVScraperUseDisplaySeasonEpisode Then
                                tvEp.DisplayEpisode = -1
                                tvEp.DisplaySeason = -1
                            End If

                            Using xmlSW As New Utf8StringWriter
                                xmlSer.Serialize(xmlSW, tvEp, NS)
                                If sBuilder.Length > 0 Then
                                    sBuilder.Append(Environment.NewLine)
                                    xmlSW.GetStringBuilder.Remove(0, xmlSW.GetStringBuilder.ToString.IndexOf(Environment.NewLine) + 1)
                                End If
                                sBuilder.Append(xmlSW.ToString)
                            End Using
                        Next

                        tvEpToSave.NfoPath = tPath

                        If sBuilder.Length > 0 Then
                            Using fSW As New StreamWriter(tPath)
                                fSW.Write(sBuilder.ToString)
                            End Using
                        End If
                    End Using
                    If doesExist And fAttWritable Then File.SetAttributes(tPath, fAtt)
                End If
            End If

        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
    End Sub

    Public Shared Sub SaveTVShowToNFO(ByRef tvShowToSave As Database.DBElement)

        Try
            Dim params As New List(Of Object)(New Object() {tvShowToSave})
            Dim doContinue As Boolean = True
            ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.OnNFOSave_TVShow, params, doContinue, False)
            If Not doContinue Then Return
        Catch ex As Exception
        End Try

        Try
            If Not String.IsNullOrEmpty(tvShowToSave.ShowPath) Then
                Dim xmlSer As New XmlSerializer(GetType(MediaContainers.TVShow))

                Dim doesExist As Boolean = False
                Dim fAtt As New FileAttributes
                Dim fAttWritable As Boolean = True

                'Boxee support
                If Master.eSettings.TVUseBoxee Then
                    If tvShowToSave.TVShow.TVDBSpecified() Then
                        tvShowToSave.TVShow.BoxeeTvDb = tvShowToSave.TVShow.TVDB
                        tvShowToSave.TVShow.BlankId()
                    End If
                End If

                'digit grouping symbol for Votes count
                If Master.eSettings.GeneralDigitGrpSymbolVotes Then
                    If tvShowToSave.TVShow.VotesSpecified Then
                        Dim vote As String = Double.Parse(tvShowToSave.TVShow.Votes, Globalization.CultureInfo.InvariantCulture).ToString("N0", Globalization.CultureInfo.CurrentCulture)
                        If vote IsNot Nothing Then tvShowToSave.TVShow.Votes = vote
                    End If
                End If

                For Each a In FileUtils.GetFilenameList.TVShow(tvShowToSave, Enums.ModifierType.MainNFO)
                    If Not Master.eSettings.GeneralOverwriteNfo Then
                        RenameShowNonConfNfo(a)
                    End If

                    doesExist = File.Exists(a)
                    If Not doesExist OrElse (Not CBool(File.GetAttributes(a) And FileAttributes.ReadOnly)) Then

                        If doesExist Then
                            fAtt = File.GetAttributes(a)
                            Try
                                File.SetAttributes(a, FileAttributes.Normal)
                            Catch ex As Exception
                                fAttWritable = False
                            End Try
                        End If

                        Using xmlSW As New StreamWriter(a)
                            tvShowToSave.NfoPath = a
                            xmlSer.Serialize(xmlSW, tvShowToSave.TVShow)
                        End Using

                        If doesExist And fAttWritable Then File.SetAttributes(a, fAtt)
                    End If
                Next
            End If
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
    End Sub

    Private Shared Sub RenameEpNonConfNfo(ByVal sPath As String, ByVal isChecked As Boolean)
        'test if current nfo is non-conforming... rename per setting

        Try
            If File.Exists(sPath) AndAlso Not IsConformingEpNfo(sPath) Then
                RenameToInfo(sPath)
            End If
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
    End Sub

    Private Shared Sub RenameNonConfNfo(ByVal sPath As String, ByVal isChecked As Boolean)
        'test if current nfo is non-conforming... rename per setting

        Try
            If isChecked OrElse Not IsConformingNfo(sPath) Then
                If isChecked OrElse File.Exists(sPath) Then
                    RenameToInfo(sPath)
                End If
            End If
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
    End Sub

    Private Shared Sub RenameShowNonConfNfo(ByVal sPath As String)
        'test if current nfo is non-conforming... rename per setting

        Try
            If File.Exists(sPath) AndAlso Not IsConformingShowNfo(sPath) Then
                RenameToInfo(sPath)
            End If
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
    End Sub

    Private Shared Sub RenameToInfo(ByVal sPath As String)
        Try
            Dim i As Integer = 1
            Dim strNewName As String = String.Concat(FileUtils.Common.RemoveExtFromPath(sPath), ".info")
            'in case there is already a .info file
            If File.Exists(strNewName) Then
                Do
                    strNewName = String.Format("{0}({1}).info", FileUtils.Common.RemoveExtFromPath(sPath), i)
                    i += 1
                Loop While File.Exists(strNewName)
                strNewName = String.Format("{0}({1}).info", FileUtils.Common.RemoveExtFromPath(sPath), i)
            End If
            My.Computer.FileSystem.RenameFile(sPath, Path.GetFileName(strNewName))
        Catch ex As Exception
            logger.Error(New StackFrame().GetMethod().Name, ex)
        End Try
    End Sub

    Public Shared Sub LoadTVEpDuration(ByRef _TVEpDB As Database.DBElement)
        If _TVEpDB.TVEpisode Is Nothing OrElse _TVEpDB.TVEpisode.FileInfo Is Nothing Then Return

        Dim tRuntime As String = String.Empty
        If Master.eSettings.TVScraperUseMDDuration Then
            If _TVEpDB.TVEpisode.FileInfo.StreamDetails IsNot Nothing AndAlso _TVEpDB.TVEpisode.FileInfo.StreamDetails.Video.Count > 0 Then
                Dim cTotal As String = String.Empty
                For Each tVid As MediaInfo.Video In _TVEpDB.TVEpisode.FileInfo.StreamDetails.Video
                    cTotal = cTotal + tVid.Duration
                Next
                tRuntime = MediaInfo.FormatDuration(MediaInfo.DurationToSeconds(cTotal, True), Master.eSettings.TVScraperDurationRuntimeFormat)
            End If
        End If

        If String.IsNullOrEmpty(tRuntime) Then
            If (String.IsNullOrEmpty(_TVEpDB.TVEpisode.Runtime) OrElse Not Master.eSettings.TVLockEpisodeRuntime) AndAlso Not String.IsNullOrEmpty(_TVEpDB.TVShow.Runtime) AndAlso Master.eSettings.TVScraperUseSRuntimeForEp Then
                _TVEpDB.TVEpisode.Runtime = _TVEpDB.TVShow.Runtime
            End If
        Else
            If (String.IsNullOrEmpty(_TVEpDB.TVEpisode.Runtime) OrElse Not Master.eSettings.TVLockEpisodeRuntime) AndAlso Master.eSettings.TVScraperUseMDDuration Then
                _TVEpDB.TVEpisode.Runtime = tRuntime
            End If
        End If
    End Sub

#End Region 'Methods

#Region "Nested Types"

    Public Class NonConf

#Region "Fields"

        Private _imdbid As String
        Private _text As String

#End Region 'Fields

#Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"

        Public Property IMDBID() As String
            Get
                Return Me._imdbid
            End Get
            Set(ByVal value As String)
                Me._imdbid = value
            End Set
        End Property

        Public Property Text() As String
            Get
                Return Me._text
            End Get
            Set(ByVal value As String)
                Me._text = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub Clear()
            Me._imdbid = String.Empty
            Me._text = String.Empty
        End Sub

#End Region 'Methods

    End Class

    Public Class KnownEpisode

#Region "Fields"

        Private _aireddate As String
        Private _episode As Integer
        Private _episodeabsolute As Integer
        Private _episodecombined As Double
        Private _episodedvd As Double
        Private _season As Integer
        Private _seasoncombined As Integer
        Private _seasondvd As Integer

#End Region 'Fields

#Region "Constructors"

        Public Sub New()
            Me.Clear()
        End Sub

#End Region 'Constructors

#Region "Properties"

        Public Property AiredDate() As String
            Get
                Return Me._aireddate
            End Get
            Set(ByVal value As String)
                Me._aireddate = value
            End Set
        End Property

        Public Property Episode() As Integer
            Get
                Return Me._episode
            End Get
            Set(ByVal value As Integer)
                Me._episode = value
            End Set
        End Property

        Public Property EpisodeAbsolute() As Integer
            Get
                Return Me._episodeabsolute
            End Get
            Set(ByVal value As Integer)
                Me._episodeabsolute = value
            End Set
        End Property

        Public Property EpisodeCombined() As Double
            Get
                Return Me._episodecombined
            End Get
            Set(ByVal value As Double)
                Me._episodecombined = value
            End Set
        End Property

        Public Property EpisodeDVD() As Double
            Get
                Return Me._episodedvd
            End Get
            Set(ByVal value As Double)
                Me._episodedvd = value
            End Set
        End Property

        Public Property Season() As Integer
            Get
                Return Me._season
            End Get
            Set(ByVal value As Integer)
                Me._season = value
            End Set
        End Property

        Public Property SeasonCombined() As Integer
            Get
                Return Me._seasoncombined
            End Get
            Set(ByVal value As Integer)
                Me._seasoncombined = value
            End Set
        End Property

        Public Property SeasonDVD() As Integer
            Get
                Return Me._seasondvd
            End Get
            Set(ByVal value As Integer)
                Me._seasondvd = value
            End Set
        End Property

#End Region 'Properties

#Region "Methods"

        Public Sub Clear()
            Me._aireddate = String.Empty
            Me._episode = -1
            Me._episodeabsolute = -1
            Me._episodecombined = -1
            Me._episodedvd = -1
            Me._season = -1
            Me._seasoncombined = -1
            Me._seasondvd = -1
        End Sub

#End Region 'Methods

    End Class

#End Region 'Nested Types

End Class