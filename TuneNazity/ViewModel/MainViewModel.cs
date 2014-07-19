using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using iTunesLib;
using TuneNazity.Model;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using Directory = System.IO.Directory;
using DirectoryInfo = System.IO.DirectoryInfo;
using Path = System.IO.Path;
using SearchOption = System.IO.SearchOption;
using System.Windows.Input;

namespace TuneNazity.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>

        #region ViewModel Constructor

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage",
            "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                MatchRating = "Match Rating";
                FilesScanned = "FoldersScanned";
                PlayList = " TEST PlayList";
                SearchCriteria = @"C:\TEST\MUSIC\CD";


            }
            else
            {
                // Code runs "for real"

                #region Commands

                //BML Command Definitions for UI

                // Initial Property Values
                //
                StartButtonVisibility = Visibility.Hidden;
                ScopeStatusVisibility = Visibility.Hidden;
                BusyStatusVisibility = Visibility.Hidden;
                // Include THIS or it wont work
                this.PlayBusyAnimation = false;


                // Main Window

                // ReSharper disable ConvertClosureToMethodGroup
                CloseAppConfirm = new RelayCommand(() => CloseApp(), () => true);

                TerminateAppNow = new RelayCommand(() => EndAppNow(), () => true);

                CancelTerminateAppNow = new RelayCommand(() => CancelTerminateApp(), () => true);

                MoveWindowNow = new RelayCommand(() => dragWindowNow(), () => true);

                MoveQuitWindowNow = new RelayCommand(() => dragQuitWindowNow(), () => true);

                MinimizeWindowNow = new RelayCommand(() => MinAppNow(), () => true);

                StartApp = new RelayCommand(() => BeginApp(), () => true);

                CancelScan = new RelayCommand(() => CancelCurrenTScan(), () => true);

                // TAB Control

                // Search TAB
                // This command uses a parameter
                SearchTabCheckBoxManager = new RelayCommand<string>(p =>
                {
                    SearchCheckBoxManager(p);

                });

                CheckBoxManagerForSelectedItem = new RelayCommand<string>(p =>
                {
                    CheckBoxManagerForSelectedTreeViewItem
                        (p);

                });


                // Search Dialog

                SelectDrives2Search = new RelayCommand(() => ChooseDrives2Search(), () => true);

                SetSearchParameters = new RelayCommand(() => SetDrives2Search(), () => true);

                MoveSearchWindow = new RelayCommand(() => MoveSearch(), () => true);

                CancelSearchWindow = new RelayCommand(() => CloseSearch(), () => true);


                // Standard Dialog
                //
                DialogAnswerYes = new RelayCommand(() => AnswerFromDialog(), () => true);
                DragStartDialog = new RelayCommand(() => MoveStarDialog(), () => true);
                // ReSharper restore ConvertClosureToMethodGroup

                // Layout Change Events with Parameters
                //
                UpdateControlLayout = new RelayCommand<string>(p =>
                {
                    UpdateElementLayout(p);

                });




                #endregion - command definitions

                #region iTune Events and Library Load

                //
                iTunes.OnPlayerPlayEvent += new _IiTunesEvents_OnPlayerPlayEventEventHandler(delegate(object o)
                {
                    Debug.Print(
                        "Itunes Started Playing...");
                });

                iTunes.OnPlayerStopEvent += new _IiTunesEvents_OnPlayerStopEventEventHandler(delegate(object o)
                {

                    Debug.Print(
                        "Itunes Stoped Playing...");

                });


                // Load iTunes Library
                CreateDictionaryFromItunesXMLLibrary();

                #endregion


                #region Manage Checkboxes for Search Tab

                //



                #endregion



            }
        }

        #endregion


        #region Globals

        //
        TreeViewItem _lastItemSelected;
        private string _searchCriteria;
        private string _playList;
        private string _trackName;
        private string _send2PlayList;
        private string _trackLocation;
        private string _elapsedTime;
        private string _currentTrack;
        private string _totalTracks;
        private string _orphanedTracks;
        private double _currentProgress;
        private string _numberOfDeadTracks;
        private string _generalUse;
        private string _filesScanned;
        private string _matchRating;
        private string _averageTrackLookupTIme;
        public int FilesScannedCounter;
        public int DirectoriesReadSearchingForFile;
        private Visibility _startButtonVisibility;
        private Visibility _findDuplicatesCheckBoxVisibility;
        private Visibility _search4LostTracksCheckBoxVisibility;
        private Visibility _clearLowRatedDeadTracksCheckBoxVisibility;
        private Visibility _busyStatusVisibility;
        private Visibility _scopeStatusVisibility;
        public Dictionary<string, long> SimilarFilesList;
        public Dictionary<int, IITTrack> TrackIndexList;


        // This dictionary contains the Tracks Database ID and the Original File Name
        //
        public Dictionary<string, string> ItunesXmLdb;

        public string CurrentDrivesLookup;

        public bool CancelCurrentJob = false;
        private bool _removeLowRatedTracks = false;
        private bool _findDuplicateTracks = false;
        private bool _findDeadTracks = false;
        public string SearchStatus;
        private bool _playBusyAnimation = false;
        private bool _CheckBoxForCurrentFolder = false;

        private readonly object _dummyNode = null;

        private Stopwatch _stopWatch = null;

        private Dictionary<int, FileIndexTable> fileIndex = new Dictionary<int, FileIndexTable>();


        // This Background Worker is used by the Find Orphaned Function
        //
        private BackgroundWorker _worker = null;

        // This Background Worker is used by the The iTunes Library Location
        //
        public Stopwatch AverageLookupTime = null;

        public List<int> AverageTimeList = null;

        public ObservableCollection<LogData> _LogData = new ObservableCollection<LogData>();

        public System.OperatingSystem OsInfo = System.Environment.OSVersion;

        private bool? _isChecked = false;

        private bool _CancelCurrentBackgroundWorker = false;


        #endregion


        #region Properties

        //


        public bool? IsChecked
        {
            get { return _isChecked; }
            set { _isChecked = value; }
        }



        public ObservableCollection<LogData> LogDataCollection
        {
            get { return _LogData; }
        }



        public string AverageTrackLookupTIme
        {
            get { return _averageTrackLookupTIme; }
            set
            {
                if (_averageTrackLookupTIme == value)
                    return;
                _averageTrackLookupTIme = value;
                RaisePropertyChanged("AverageTrackLookupTIme");
            }
        }

        public string GeneralUse
        {
            get { return _generalUse; }
            set
            {
                if (_generalUse == value)
                    return;
                _generalUse = value;
                RaisePropertyChanged("GeneralUse");
            }
        }


        public string MatchRating
        {
            get { return _matchRating; }
            set
            {
                if (_matchRating == value)
                    return;
                _matchRating = value;
                RaisePropertyChanged("MatchRating");
            }
        }

        public string FilesScanned
        {
            get { return _filesScanned; }
            set
            {
                if (_filesScanned == value)
                    return;
                _filesScanned = value;
                RaisePropertyChanged("FilesScanned");
            }
        }

        public string DeadTracksFound
        {
            get { return _numberOfDeadTracks; }
            set
            {
                if (_numberOfDeadTracks == value)
                    return;
                _numberOfDeadTracks = value;
                RaisePropertyChanged("DeadTracksFound");
            }
        }

        public double CurrentProgress
        {
            get { return _currentProgress; }
            set
            {
                if (_currentProgress == value)
                    return;
                _currentProgress = value;
                RaisePropertyChanged("CurrentProgress");
            }
        }


        public string OrphanedTacks
        {
            get { return _orphanedTracks; }
            set
            {
                if (_orphanedTracks == value)
                    return;
                _orphanedTracks = value;
                RaisePropertyChanged("OrphanedTacks");
            }
        }


        public string TotalTracks
        {
            get { return _totalTracks; }
            set
            {
                if (_totalTracks == value)
                    return;
                _totalTracks = value;
                RaisePropertyChanged("TotalTracks");
            }
        }

        public string CurrentTrack
        {
            get { return _currentTrack; }
            set
            {
                if (_currentTrack == value)
                    return;
                _currentTrack = value;
                RaisePropertyChanged("CurrentTrack");
            }
        }


        public string ElapsedTime
        {
            get { return _elapsedTime; }
            set
            {
                if (_elapsedTime == value)
                    return;
                _elapsedTime = value;
                RaisePropertyChanged("ElapsedTime");
            }
        }

        public string PlayList
        {
            get { return _playList; }
            set
            {
                if (_playList == value)
                    return;
                _playList = value;
                RaisePropertyChanged("PlayList");
            }
        }

        public string TrackName
        {
            get { return _trackName; }
            set
            {
                if (_trackName == value)
                    return;
                _trackName = value;
                RaisePropertyChanged("TrackName");
            }
        }

        public string TrackLocation
        {
            get { return _trackLocation; }
            set
            {
                if (_trackLocation == value)
                    return;
                _trackLocation = value;
                RaisePropertyChanged("TrackLocation");
            }
        }


        public string SearchCriteria
        {
            get { return _searchCriteria; }
            set
            {
                if (_searchCriteria == value)
                    return;
                _searchCriteria = value;
                RaisePropertyChanged("SearchCriteria");
            }
        }

        public string Send2PlayList
        {
            get { return _send2PlayList; }
            set
            {
                if (_send2PlayList == value)
                    return;
                _send2PlayList = value;
                RaisePropertyChanged("Send2PlayList");
            }
        }



        public bool PlayBusyAnimation
        {
            get { return _playBusyAnimation; }
            set
            {
                if (_playBusyAnimation == value)
                    return;
                _playBusyAnimation = value;
                RaisePropertyChanged("PlayBusyAnimation");
            }
        }

        public bool FindDuplicateTracks
        {
            get { return _findDuplicateTracks; }
            set
            {
                if (_findDuplicateTracks == value)
                    return;
                _findDuplicateTracks = value;
                RaisePropertyChanged("FindDuplicateTracks");
            }
        }

        public bool FindDeadTracks
        {
            get { return _findDeadTracks; }
            set
            {
                if (_findDeadTracks == value)
                    return;
                _findDeadTracks = value;
                RaisePropertyChanged("FindDeadTracks");
                checkStartStatus();
            }
        }

        public bool RemoveLowRatedTracks
        {
            get { return _removeLowRatedTracks; }
            set
            {
                if (_removeLowRatedTracks == value)
                    return;
                _removeLowRatedTracks = value;
                RaisePropertyChanged("RemoveLowRatedTracks");
            }
        }


        public bool CheckBoxForCurrentFolder
        {
            get { return _CheckBoxForCurrentFolder; }
            set
            {
                if (_CheckBoxForCurrentFolder == value)
                    return;
                _CheckBoxForCurrentFolder = value;
                RaisePropertyChanged("CheckBoxForCurrentFolder");
            }
        }



        public Visibility ScopeStatusVisibility
        {
            get { return _scopeStatusVisibility; }
            set
            {
                if (_scopeStatusVisibility == value)
                    return;
                _scopeStatusVisibility = value;
                RaisePropertyChanged("ScopeStatusVisibility");
            }
        }

        public Visibility BusyStatusVisibility
        {
            get { return _busyStatusVisibility; }
            set
            {
                if (_busyStatusVisibility == value)
                    return;
                _busyStatusVisibility = value;
                RaisePropertyChanged("BusyStatusVisibility");
            }
        }


        public Visibility FindDuplicatesCheckBoxVisibility
        {
            get { return _findDuplicatesCheckBoxVisibility; }
            set
            {
                if (_findDuplicatesCheckBoxVisibility == value)
                    return;
                _findDuplicatesCheckBoxVisibility = value;
                RaisePropertyChanged("FindDuplicatesCheckBoxVisibility");
            }
        }


        public Visibility Search4LostTracksCheckBoxVisibility
        {
            get { return _search4LostTracksCheckBoxVisibility; }
            set
            {
                if (_search4LostTracksCheckBoxVisibility == value)
                    return;
                _search4LostTracksCheckBoxVisibility = value;
                RaisePropertyChanged("Search4LostTracksCheckBoxVisibility");
            }
        }


        public Visibility ClearLowRatedDeadTracksCheckBoxVisibility
        {
            get { return _clearLowRatedDeadTracksCheckBoxVisibility; }
            set
            {
                if (_clearLowRatedDeadTracksCheckBoxVisibility == value)
                    return;
                _clearLowRatedDeadTracksCheckBoxVisibility = value;
                RaisePropertyChanged("ClearLowRatedDeadTracksCheckBoxVisibility");
            }
        }







        public Visibility StartButtonVisibility
        {
            get { return _startButtonVisibility; }
            set
            {
                if (_startButtonVisibility == value)
                    return;
                _startButtonVisibility = value;
                RaisePropertyChanged("StartButtonVisibility");
            }
        }

        public string SelectedImagePath { get; set; }


        public override void Cleanup()
        {
            // Clean up if needed

            // Clean Itunes COM Interface
            //
            System.Runtime.InteropServices.Marshal.ReleaseComObject(iTunes);
            iTunes = null;
            GC.Collect();

            base.Cleanup();
        }

        #endregion


        #region Command Refference for UI

        // Main Window Commands
        //
        public RelayCommand MoveWindowNow { get; private set; }

        public RelayCommand MoveQuitWindowNow { get; private set; }

        public RelayCommand MinimizeWindowNow { get; private set; }

        public RelayCommand CloseAppConfirm { get; private set; }

        public RelayCommand TerminateAppNow { get; private set; }

        public RelayCommand CancelTerminateAppNow { get; private set; }

        public RelayCommand StartApp { get; private set; }

        public RelayCommand CancelScan { get; private set; }


        // TAB Control

        // Search TAB
        // Command parameter used

        public RelayCommand<string> SearchTabCheckBoxManager { get; private set; }

        public RelayCommand<string> CheckBoxManagerForSelectedItem { get; private set; }

        // Element Layout Updates

        public RelayCommand<string> UpdateControlLayout { get; private set; }



        // Search Dialog Commands
        //
        public RelayCommand SelectDrives2Search { get; private set; }

        public RelayCommand SetSearchParameters { get; private set; }

        public RelayCommand MoveSearchWindow { get; private set; }

        public RelayCommand CancelSearchWindow { get; private set; }

        // Standard Dialog Commands
        //
        public RelayCommand DialogAnswerYes { get; private set; }
        public RelayCommand DragStartDialog { get; private set; }

        #endregion


        #region iTunes Global Object and Event

        private iTunesAppClass iTunes = new iTunesAppClass();
        private delegate void Router(object arg);

        #endregion


        #region Custom Dialogs Reference

        // Have to use null otherwise viewmodel locator error pops up
        //
        public SearchDialog SearchDialogRef = null;
        public QuitDialog QuitDialogRef = null;
        public StartDialog StartDialogRef = null;

        #endregion


        #region Main Window Commands

        private void BeginApp()
        {

            //BMK Start Application

            // First Check which option is checked
            //
            if (FindDeadTracks)
            {
                // feature Check For Windows XP Before Start
                //
                if (Environment.OSVersion.Version.Major.ToString() == "5")
                {

                    StartDialogRef = new StartDialog();
                    StartDialogRef.tbMessage.Text = "SELECTED SCOPE SEARCH WILL BE USED TO LOCATE TRACKS. " + "\n" +
                                                    "WINDOWS XP DETECTED - APP WILL MINIMIZE AFTER A SHORT DELAY TO INCREASE PERFORMANCE." +
                                                    "\n" +
                                                    "SCOPE SELECTED: \n" + SearchCriteria.ToUpper();

                }
                else
                {

                    if (SearchCriteria == null)
                    {
                        MessageBox.Show("Select a search criteria first.", "Tunazity", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    StartDialogRef = new StartDialog();
                    StartDialogRef.tbMessage.Text = "SELECTED SCOPE SEARCH WILL BE USED TO LOCATE TRACKS. " + "\n\n" +
                                                    "SCOPE SELECTED: \n" + SearchCriteria.ToUpper();
                }



                StartDialogRef.ShowDialog();

                if (StartDialogRef.DialogResult.HasValue)
                {
                    // Clicked YES
                    StartDialogRef = null;

                    // Start the Process
                    //
                    // Turn Start Button Off Before
                    StartButtonVisibility = Visibility.Hidden;
                    ScopeStatusVisibility = Visibility.Hidden;
                    BusyStatusVisibility = Visibility.Visible;
                    // Include THIS or it wont work
                    this.PlayBusyAnimation = true;

                    // Do the Work
                    FindDeadOrphanedTracks();
                }
                else
                {
                    // CLicked Cancel
                    // Do Nothing
                }
            }


            if (RemoveLowRatedTracks)
            {

                // Start the Process
                //
                // Turn Start Button Off Before
                StartButtonVisibility = Visibility.Hidden;
                ScopeStatusVisibility = Visibility.Hidden;
                BusyStatusVisibility = Visibility.Visible;
                // Include THIS or it wont work
                this.PlayBusyAnimation = true;

                System.Threading.Thread.Sleep(10);

                // Remove the tracks
                DeleteLowRatedTracks();

            }


            if (FindDuplicateTracks)
            {

                // Start the Process
                //
                // Turn Start Button Off Before
                StartButtonVisibility = Visibility.Hidden;
                ScopeStatusVisibility = Visibility.Hidden;
                BusyStatusVisibility = Visibility.Visible;

                // Include THIS or it wont work
                this.PlayBusyAnimation = true;

                System.Threading.Thread.Sleep(10);

                // Create the Track Index First and then run the program when the thread is done
                CreateDictionary4TracksIndexSearch();

            }


        }

        private void dragWindowNow()
        {
            Application.Current.MainWindow.DragMove();

        }

        private void dragQuitWindowNow()
        {
            QuitDialogRef.DragMove();

        }

        private void CloseApp()
        {

            var quitNow = new QuitDialog();
            QuitDialogRef = quitNow;
            quitNow.ShowDialog();

        }

        private void CancelTerminateApp()
        {

            QuitDialogRef.Close();
            QuitDialogRef = null;

        }

        private void EndAppNow()
        {

            Application.Current.Shutdown();

        }

        private void MinAppNow()
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;

        }

        private void CancelCurrenTScan()
        {

            // General background Worker
            _CancelCurrentBackgroundWorker = true;

            if (_worker != null && _worker.IsBusy)
            {
                _worker.CancelAsync();
                PlayBusyAnimation = false;
                CancelCurrentJob = true;
                CurrentTrack = "Cancel request in progress.  Please wait....";

            }
            else
            {
                TrackLocation = "Cancel request received.  Please wait..";
            }
        }


        private void ChooseDrives2Search()
        {

            // Set the Cursor to wait mode
            Mouse.OverrideCursor = Cursors.Wait;

            // Clear Curret Selection
            SearchCriteria = string.Empty;

            SearchDialogRef = new SearchDialog();

            loadTreewithDrives();

            SearchDialogRef.myTree.SelectedItemChanged += myTree_SelectedItemChanged;

            // This Line is crucial in order to attach the Selected Item Event to the Main View Model
            // The Data Context of the Tree was also set to the main viewModel just in case.
            // This keeps eveything in the viewmodel class!!!

            // Restore the Cursor
            //
            Mouse.OverrideCursor = Cursors.Arrow;

            SearchDialogRef.ShowDialog();


        }


        private void checkStartStatus()
        {
            //BMK Start Button Switch
            // Check that Search and Dead Tracks Options are set
            // Before turning the start button on
            //

            // Turn the button off
            StartButtonVisibility = Visibility.Hidden;

            if ((SearchCriteria != null && SearchCriteria.Length >= 3) && FindDeadTracks == true && TrackName == "Ready.")
            {
                StartButtonVisibility = Visibility.Visible;

                if (RemoveLowRatedTracks == false && SearchCriteria == null)
                {
                    StartButtonVisibility = Visibility.Hidden;
                }

            }


            if (RemoveLowRatedTracks == true && FindDeadTracks == false && TrackName == "Ready.")
            {
                StartButtonVisibility = Visibility.Visible;

            }


            if (FindDuplicateTracks == true && RemoveLowRatedTracks == false && FindDeadTracks == false && TrackName == "Ready.")
            {
                StartButtonVisibility = Visibility.Visible;

            }



        }

        private void SetDrives2Search()
        {

            checkStartStatus();
            SearchDialogRef.Close();
            SearchDialogRef = null;
        }

        private void MoveSearch()
        {
            SearchDialogRef.DragMove();
        }

        private void CloseSearch()
        {

            SearchDialogRef.Close();
        }

        private void AnswerFromDialog()
        {
            StartDialogRef.DialogResult = true;
        }

        private void MoveStarDialog()
        {
            StartDialogRef.DragMove();
        }

        #endregion

        #region iTunes Processing Center

        void findSimilarTracks()
        {
            // Search file index Dictionary for similar tracks instead of hard drive


            #region Prepare iTunes and variables

            // iTunes
            IITTrackCollection tracks = iTunes.LibraryPlaylist.Tracks;



            int trackCount = tracks.Count;
            string similarPlayListName = "Similar Tracks";
            string currentDate = " " + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Year.ToString();
            bool similarPlaylistExists = false;
            string trackFileName = null;

            #region  Create Playlist to store similar tracks

            // Check if Playlist exists
            //
            foreach (IITPlaylist playlist in iTunes.LibrarySource.Playlists)
            {

                if (playlist.Name == similarPlayListName + currentDate)
                {
                    similarPlaylistExists = true;

                }

            }

            // Create List if It doesn't exist
            //
            if (!similarPlaylistExists)
                iTunes.CreatePlaylist(similarPlayListName + currentDate);


            // Get a Reference to similar list
            //
            IITPlaylist tempPlaylist = iTunes.LibrarySource.Playlists.get_ItemByName(similarPlayListName + currentDate);
            IITUserPlaylist thisPlaylist = (IITUserPlaylist)tempPlaylist;


            #endregion

            // Variables
            int similarTracksFound = 0;
            double similarity = 0;


            // Use this counter to make sure the scan picks up in case itunes gets busy or a bad track is rejected
            // otherwise the count will start at zero again
            int globalCounter = 0;
            int deadTracks = 0;

            // Keep Track of Time
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
            AverageLookupTime = new Stopwatch();
            TotalTracks = trackCount.ToString();


            #endregion


            #region Background Worker

            BackgroundWorker backgroundWorker3 = new BackgroundWorker();
            backgroundWorker3.WorkerReportsProgress = true;
            backgroundWorker3.WorkerSupportsCancellation = true;


            backgroundWorker3.DoWork += delegate(object s, DoWorkEventArgs args)
            {

                #region Do Work

                #region Scan iTunes Library


                // Function to handle Busy Issues with iTunes
                //
                bool success = false;
                while (!success)
                {

                    // Handle any errors incurred
                    try
                    {


                        #region Check Tracks

                        // Go through each track in the library
                        //

                        for (int i = 1; i < trackCount; i++)
                        {


                            // If cancel requested - terminate
                            //
                            if (backgroundWorker3.CancellationPending)//checks for cancel request
                            {
                                CurrentTrack = "Cancel request in progress...";
                                args.Cancel = true;
                                success = true;
                                break;
                            }



                            // Pickup where it left off  in case process is interrupted - otherwise will restart again
                            //
                            if (globalCounter > i)
                                i = globalCounter;

                            // UI
                            CurrentTrack = i.ToString();

                            CurrentProgress = ((double)i / (double)trackCount) / 100;



                            #region Process a single Track

                            //Detect first track
                            bool firstTrack = true;


                            if (_CancelCurrentBackgroundWorker == false)
                            {

                                // Current iTunes Track  using dictionary because itunes does not work in this thread
                                IITTrack track = tracks[i];
                                IITFileOrCDTrack firstTrackfound = null;


                                // Debug
                                Debug.Print("Checking track #: " + i.ToString());

                                // Keep track of count in case of iTunes error
                                globalCounter++;

                                // Get the Original Name of the file
                                //
                                int trackID = track.TrackDatabaseID;
                                string key = trackID.ToString();


                                // Get the actual file name of the Track - this line assigns the File Name value to: trackFileName 
                                //
                                ItunesXmLdb.TryGetValue(key, out trackFileName);


                                // If the Track has no name then go to the next track
                                if (string.IsNullOrEmpty(trackFileName))
                                    continue;

                                int scanCounter = 0;
                                AverageLookupTime.Start();


                                // Debug - Bunch of duplicates
                                //if (trackFileName.ToLower().Contains("unknown") == false)
                                //    continue;


                                // Check index for tracks with similar file names
                                //
                                foreach (var pair in ItunesXmLdb)
                                {

                                    scanCounter++;

                                    // SHow Progress only every 3000 scans
                                    if (AverageLookupTime.Elapsed.Seconds % 5 == 0)
                                    {
                                        CurrentProgress = ((double)scanCounter / (double)ItunesXmLdb.Count) * 100;
                                    }
                                    else
                                    {
                                        CurrentProgress = ((double)i / (double)trackCount) / 100;
                                    }


                                    #region Perform similarity test

                                    FilesScanned = scanCounter.ToString();

                                    // Get the File of this track
                                    //
                                    int currentKey = Convert.ToInt32(pair.Key);
                                    string currentFileName = pair.Value;


                                    // Skip if filename is null
                                    if (string.IsNullOrEmpty(currentFileName))
                                        continue;


                                    // Debug - Bunch of duplicates
                                    //if (currentFileName.ToLower().Contains("unknown") == false)
                                    //    continue;

                                    // Add any match over 90% to the list
                                    //
                                    similarity = GetSimilarity(trackFileName, currentFileName);


                                    TrackName = trackFileName;
                                    TrackLocation = currentFileName;

                                    // Stats
                                    // Debug.Print("Checking track {0} - Result: {1}  ", pair.Key.ToString(), similarity.ToString());
                                    MatchRating = similarity.ToString();

                                    if (similarity > .90)
                                    {

                                        MatchRating = similarity.ToString();

                                        //Add the First to storage and add it only if a second one is found
                                        //
                                        if (firstTrack)
                                        {

                                            firstTrackfound = (IITFileOrCDTrack)track;
                                            firstTrack = false;
                                            continue;

                                        }
                                        else if (firstTrackfound != null && firstTrack == false)
                                        {
                                            // Add the first track if the database ID is different
                                            //
                                            if (firstTrackfound.TrackDatabaseID != currentKey)
                                            {
                                                // Add the first track and the following others
                                                firstTrackfound.Enabled = true;

                                                // DO NOT ADD THE FIRST TRACK TO THE LIST SO IT CANNNOT BE DELETED
                                                // thisPlaylist.AddTrack(firstTrackfound); 
                                                firstTrackfound = null;
                                            }



                                        }

                                        // now add the Track that was just found
                                        //
                                        // Locate the Track first
                                        //
                                        IITTrack currentTrack = null;
                                        TrackIndexList.TryGetValue(currentKey, out currentTrack);

                                        if (currentTrack == null)
                                            continue;

                                        var fileTrack = (IITFileOrCDTrack)currentTrack;
                                        fileTrack.Enabled = false;  // Similar track tagged

                                        // Count the number of playlists in the track and if in 2 or more  put two stars on it
                                        if (fileTrack.Playlists.Count >= 4)
                                            fileTrack.Rating = 40;

                                        thisPlaylist.AddTrack(fileTrack); // Possible Duplicate - Added unchecked
                                        similarTracksFound++;
                                        OrphanedTacks = similarTracksFound.ToString();

                                    }

                                    // Update UI
                                    backgroundWorker3.ReportProgress(i);


                                    #endregion  - Perform similarity test

                                }

                                // Time
                                AverageTrackLookupTIme = AverageLookupTime.Elapsed.ToString();
                                AverageLookupTime.Reset();


                            } // Check if Cancel command has been issued
                            else
                            {

                                // Cancel this command now
                                //
                                backgroundWorker3.CancelAsync();

                            }
                            #endregion - Process Track

                        }  // Next track

                        #endregion - Check Tracks


                    }
                    catch (System.Runtime.InteropServices.COMException e)
                    {

                        #region iTunes is Busy

                        if ((e.ErrorCode & 0xFFFF) == 0xC472 || e.ErrorCode == -2147417846)
                        {

                            // iTunes is busy
                            System.Threading.Thread.Sleep(500); // Wait, and...
                            TrackName = "iTunes is busy.  Please close any dialog or messages pending....";

                            Debug.Write("Error detected: " + e.Message);

                            // Reset Counter or progress bar will be off
                            //                                                 
                            success = false; // ...try again
                        }
                        else if (e.ErrorCode == -2147467259)
                        {
                            deadTracks++;
                            DeadTracksFound = deadTracks.ToString();
                            Debug.Print("File Location not accepted by dead track listing..");

                        }
                        else
                        {

                            // Re-throw!
                            throw;

                        }

                        #endregion


                    }

                }

                #endregion


                #endregion - do work

            };


            backgroundWorker3.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {

                #region Progress Report

                // Progress indicator
                //
                int i = args.ProgressPercentage;


                // Update UI
                //
                ElapsedTime = _stopWatch.Elapsed.Hours.ToString() + "h " +
                              _stopWatch.Elapsed.Minutes.ToString() + "m " +
                              _stopWatch.Elapsed.Seconds.ToString() + "s";



                #endregion - Progress report

            };


            backgroundWorker3.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {


                #region Work Completed

                if (args.Cancelled)//it doesn't matter if the BG worker ends normally, or gets cancelled,
                {

                    //both cases RunWorkerCompleted is invoked, so we need to check what has happened
                    //
                    Debug.Print("You've cancelled the backgroundworker!");
                    TrackName = "Ready.";
                    CurrentTrack = "Duplicate search process cancelled.";

                    // Turn the Button Back on Again
                    StartButtonVisibility = Visibility.Visible;
                    ScopeStatusVisibility = Visibility.Visible;
                    BusyStatusVisibility = Visibility.Hidden;
                    // Include THIS or it wont work
                    this.PlayBusyAnimation = false;

                }
                else
                {

                    Debug.Print("Duplicate Search Completed");
                    TrackName = "Ready.";
                    CurrentTrack = "Duplicate Search Completed";
                    _CancelCurrentBackgroundWorker = false;

                    // Turn the Button Back on Again
                    StartButtonVisibility = Visibility.Visible;
                    ScopeStatusVisibility = Visibility.Visible;
                    BusyStatusVisibility = Visibility.Hidden;
                    // Include THIS or it wont work
                    this.PlayBusyAnimation = false;


                }


                // Release the Resource
                //
                backgroundWorker3 = null;

                #endregion - Work Completed


            };


            backgroundWorker3.RunWorkerAsync();
            backgroundWorker3.WorkerReportsProgress = true;


            #endregion - background Worker



        }

        private void FindDeadOrphanedTracks()
        {

            //BMK Find Orphaned Tracks

            // Initialize the Background thread
            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;

            string recoveryPlayListName = "Recovered Tracks";
            string deadPlayListName = "Dead Tracks";
            string currentDate = " " + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Year.ToString();

            BusyStatusVisibility = Visibility.Visible;
            // Include THIS or it wont work
            this.PlayBusyAnimation = true;
            StartButtonVisibility = Visibility.Hidden;

            // Create a dictionary of Dead Tracks already found so they don't repeat on the log
            //
            Dictionary<string, string> deadTrackIndex = new Dictionary<string, string>();


            // Re-Initialize iTunes
            //
            iTunes = new iTunesAppClass();

            bool recoveryListExist = false;
            bool deadListExist = false;


            //feature PLAYLIST - Recovered Tracks
            // Delete Existing Recoevery PlayList
            // if the job was cancelled keep existing playlist

            foreach (IITPlaylist playlist in iTunes.LibrarySource.Playlists)
            {

                if (playlist.Name == recoveryPlayListName + currentDate || playlist.Name == deadPlayListName + currentDate)
                {
                    // Don't Delete Exisiting Recovery List - to preserve exisiting recoveries
                    //
                    if (playlist.Name == recoveryPlayListName + currentDate)
                        recoveryListExist = true;

                    if (playlist.Name == deadPlayListName + currentDate)
                        deadListExist = true;

                }

            }

            // Create List if It doesn't exist
            //
            if (!recoveryListExist)
                iTunes.CreatePlaylist(recoveryPlayListName + currentDate);

            if (!deadListExist)
                iTunes.CreatePlaylist(deadPlayListName + currentDate);


            // Get a Reference to the Recovery List and Dead List
            //
            IITPlaylist tempPlaylist = iTunes.LibrarySource.Playlists.get_ItemByName(recoveryPlayListName + currentDate);
            IITUserPlaylist thisPlaylist = (IITUserPlaylist)tempPlaylist;

            IITPlaylist tempDeadlist = iTunes.LibrarySource.Playlists.get_ItemByName(deadPlayListName + currentDate);
            IITUserPlaylist thisDeadlist = (IITUserPlaylist)tempDeadlist;



            // Keep Track of Time
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
            AverageLookupTime = new Stopwatch();
            AverageTimeList = new List<int>();




            _worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                #region Do work
                // Populate Search Dictionary
                //
                WalkFolder(SearchCriteria, "createSearchIndex");


                //////==================START

                // Initialize Variables
                int trackCount = 0;
                int deadCount = 0;
                int numberOfOrphanesFound = 0;

                // Get all the tracks
                IITTrackCollection tracks = iTunes.LibraryPlaylist.Tracks;

                trackCount = tracks.Count;
                TotalTracks = trackCount.ToString();
                FilesScanned = "";
                DeadTracksFound = "";
                OrphanedTacks = "";
                MatchRating = "";
                CurrentTrack = "";
                AverageTrackLookupTIme = "";

                //setup the progress control
                CurrentProgress = 0;

                // Create Search Array
                string searchDrives = SearchCriteria;


                // Use this counter to make sure the scan picks up in case itunes gets busy or a bad track is rejected
                // otherwise the count will start at zero again
                int globalCounter = 0;

                // Function to handle Busy Issues with iTunes
                //
                bool success = false;
                while (!success)
                {

                    try
                    {
                        #region Find Orphaned Track

                        for (int i = 1; i < trackCount; i++)
                        {

                            // Pickup where it left off  in case process is interrupted - otherwise will restart again
                            if (globalCounter > i)
                                i = globalCounter;


                            if (!_worker.CancellationPending)
                            {

                                IITTrack track = tracks[i];
                                globalCounter++; // Keep track
                                CurrentProgress = ((double)i / (double)trackCount) * 100;
                                CurrentTrack = i.ToString();
                                Debug.Print("Scanning Track: {0} of {1}", i.ToString(), trackCount.ToString());


                                // Start Looking For Dead Tracks
                                //
                                if (track != null && track.Kind == ITTrackKind.ITTrackKindFile)
                                {
                                    var fileTrack = (IITFileOrCDTrack)track;
                                    TrackLocation = fileTrack.Location;
                                    TrackName = fileTrack.Name;

                                    //

                                    //if the file doesn't exist, we'll delete it from iTunes
                                    //
                                    //feature Dead Tracks -- iTunes Location Empty
                                    //
                                    if (fileTrack.Location == null)
                                    {

                                        // Looking for Orphaned Tracks Only
                                        //
                                        FilesScannedCounter = 0;

                                        // Get the Original Name of the file
                                        //
                                        int trackID = fileTrack.TrackDatabaseID;
                                        string key = trackID.ToString();

                                        string fileName = "";



                                        if (ItunesXmLdb.TryGetValue(key, out fileName))
                                        {
                                            // Make sure file name is good
                                            if (!string.IsNullOrEmpty(fileName))
                                            {
                                                // Tracks File Name on disk
                                                Debug.Print("Original File Name: " + fileName);

                                            }
                                            else
                                            {
                                                fileName = "";
                                            }
                                        }

                                        //  Begin Search
                                        // feature Dead Track- Empty String - Begin Track Search
                                        //
                                        AverageLookupTime.Start();


                                        // If the file name doesn't exisit in the XML database, continue
                                        if (fileName == null)
                                            continue;

                                        //BMK Locate Track
                                        //

                                        // Intercept Specific Track
                                        //
                                        string t1 = fileName.ToLower();

                                        if (t1.Contains("so bad"))
                                            Debug.Print("Found Specific track: " + fileName);


                                        // Get the Location
                                        //
                                        string lostTrackLocation = findLostTrack(fileName);

                                        if (lostTrackLocation != null)
                                        {

                                            // Assign File to Itunes
                                            //
                                            fileTrack.Location = lostTrackLocation;

                                            // Enable Track and add it to the recovered Playlist
                                            //
                                            fileTrack.Enabled = true;
                                            fileTrack.Comment = "Recovered by TuneNazity on " +
                                                                DateTime.Now.ToLongDateString();

                                            //add found track to created playlist
                                            thisPlaylist.AddTrack(fileTrack);

                                            // Update the Lost Tracks Found Count
                                            //
                                            numberOfOrphanesFound++;
                                            OrphanedTacks = numberOfOrphanesFound.ToString();
                                            AverageTrackLookupTIme = AverageLookupTime.Elapsed.ToString();
                                            AverageLookupTime.Reset();

                                            SearchStatus = "FOUND";
                                        }
                                        else
                                        {

                                            // Check if this track was already added to the dead list
                                            //
                                            string deadKey = fileName;
                                            string deadfileName;

                                            if (!deadTrackIndex.TryGetValue(deadKey, out deadfileName))
                                            {

                                                // File Not Found! Add it - else skip it
                                                if (string.IsNullOrEmpty(deadfileName))
                                                {
                                                    // Create a temp File to add this to the dead tracks list
                                                    //
                                                    string tempLocation = FakeTempFilePath(fileName);
                                                    
                                                    //BMK Update Dead Track Location
                                                    // Clear this line if you want to update the location of the lost or orphaned track

                                                    //fileTrack.Location = @tempLocation;
                                                    fileTrack.Enabled = false;
                                                    fileTrack.Comment = "Added to Dead Tracks List on: " + DateTime.Now.ToLongDateString();
                                                    thisDeadlist.AddTrack(fileTrack);
                                                    System.Threading.Thread.Sleep(200); // Wait

                                                    // Add it to the list so it won't be added again
                                                    //
                                                    deadTrackIndex.Add(deadKey, fileTrack.Location);

                                                    // Clean up
                                                    deleteTempFakeFile(tempLocation);


                                                    // Count as dead if the location was not found
                                                    //
                                                    deadCount++;
                                                    DeadTracksFound = deadCount.ToString();


                                                }


                                            }



                                        }

                                    }
                                    else
                                    {
                                        // No Possible matches found
                                        //
                                        AverageLookupTime.Reset();
                                        SearchStatus = "SKIPPED";

                                    }
                                }

                                // Track already has  name
                                FilesScanned = "0";

                            }

                            // Update Status
                            //
                            try
                            {
                                int signalReceived = (Convert.ToInt16(CurrentTrack));
                                _worker.ReportProgress(Convert.ToInt16(CurrentTrack));
                            }
                            catch (Exception)
                            {
                                _worker.ReportProgress(1);

                            }




                        }

                        // Scan Complete
                        success = true;

                        #endregion
                    }
                    catch (System.Runtime.InteropServices.COMException e)
                    {

                        #region iTunes is Busy

                        if ((e.ErrorCode & 0xFFFF) == 0xC472 || e.ErrorCode == -2147417846)
                        {
                            // iTunes is busy
                            System.Threading.Thread.Sleep(500); // Wait, and...

                            Debug.Write("Error detected: " + e.Message);

                            TrackName =
                                "iTunes is busy.  Please close any dialog or messages pending....";

                            // Reset Counter or progress bar will be off
                            //                                                 
                            success = false; // ...try again
                        }
                        else if (e.ErrorCode == -2147467259)
                        {
                            Debug.Print("File Location not accepted by dead track listing..");

                        }
                        else if (e.ErrorCode == -1610350077)
                        {
                            Debug.Print("Track " + CurrentTrack + " is not modifiable.");

                        }
                        else
                        {

                            // Re-throw!
                            throw;

                        }

                        #endregion

                    }
                }

                // ====================END
                #endregion

            };



            _worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                #region Progress Changed
                // System.Threading.Thread.Sleep(10);
                int percentage = args.ProgressPercentage;
                ElapsedTime = _stopWatch.Elapsed.Hours.ToString() + "h " +
                              _stopWatch.Elapsed.Minutes.ToString() + "m " +
                              _stopWatch.Elapsed.Seconds.ToString() + "s";


                if (_worker.CancellationPending)
                {
                    // Request Cancel Received
                    TrackLocation = "Request to cancel process received.";
                }


                #region  Update the Log

                //
                //feature UpdateLog
                if (SearchStatus == "FOUND")
                {
                    addLogEntry(SearchStatus);

                }
                else if (SearchStatus == "NOT FOUND")
                {
                    addLogEntry(SearchStatus);

                }

                #endregion


                #region XP Performance Boost

                // Minimize if this is windows XP every 50 Records
                //
                //feature XP Performance Boost
                //
                // Get OperatingSystem information from the system namespace.
                //
                if (Environment.OSVersion.Version.Major.ToString() == "5" &&
                    Convert.ToInt16(CurrentTrack) % 100 == 0)
                {

                    MinAppNow();

                }

                #endregion


                #endregion
            };




            _worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                #region Work completed

                // object result = args.Result;

                _stopWatch.Stop();
                AverageLookupTime.Reset();
                Debug.Print("Current track: " + CurrentTrack.ToString());
                Debug.Print("Current Progress: " + CurrentProgress.ToString());
                Debug.Print("Current Track: " + TrackName);
                Debug.Print("Elapsed Time: " + _stopWatch.Elapsed.ToString());
                _stopWatch.Reset();

                // Turn the Button Back on Again
                StartButtonVisibility = Visibility.Visible;
                ScopeStatusVisibility = Visibility.Visible;
                BusyStatusVisibility = Visibility.Hidden;
                // Include THIS or it wont work
                this.PlayBusyAnimation = false;

                // Clean up all dead temp files created
                deleteAllDeadTempFiles();

                CurrentTrack = "";
                MatchRating = "";
                FilesScanned = "";
                TrackLocation = "";
                TrackName = "Ready.";
                CurrentProgress = 0;

                #endregion
            };



            // Only run it if it's not busy doing another process
            //
            if (!_worker.IsBusy)
                _worker.RunWorkerAsync();

        }

        private void DeleteLowRatedTracks()
        {

            //BMK Delete Low Rated Tracks
            //
            #region Initialize the Background thread

            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;

            BusyStatusVisibility = Visibility.Visible;
            // Include THIS or it wont work
            this.PlayBusyAnimation = true;
            StartButtonVisibility = Visibility.Hidden;

            // Re-Initialize iTunes
            //
            iTunes = new iTunesAppClass();
            int numberChecked = 0;



            // Keep Track of Time
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
            AverageLookupTime = new Stopwatch();
            AverageTimeList = new List<int>();

            #endregion

            #region Do Work
            _worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {

                //////==================START

                // Initialize Variables
                int trackCount = 0;
                int matchingTracks = 0;


                // Get all the tracks
                IITTrackCollection tracks = iTunes.LibraryPlaylist.Tracks;


                //get a reference to the collection of all tracks
                //tracks = iTunes.LibraryPlaylist.Tracks;

                trackCount = tracks.Count;
                numberChecked = 0;
                TotalTracks = trackCount.ToString();
                FilesScanned = "";
                DeadTracksFound = "";
                OrphanedTacks = "";
                MatchRating = "";
                CurrentTrack = "";
                AverageTrackLookupTIme = "";

                //setup the progress control
                CurrentProgress = 0;

                // Use this counter to make sure the scan picks up in case itunes gets busy or a bad track is rejected
                // otherwise the count will start at zero again
                int globalCounter = 0;


                // Function to handle Busy Issues with iTunes
                //
                bool success = false;
                while (!success)
                {
                    try
                    {

                        #region Check for Dead  and Orphaned Tracks
                        //

                        for (int i = 1; i < trackCount; i++)
                        {

                            // Pickup where it left off  in case process is interrupted - otherwise will restart again
                            //
                            if (i < globalCounter)
                                i = globalCounter;


                            if (!_worker.CancellationPending)
                            {

                                IITTrack track = tracks[i];
                                numberChecked++;
                                CurrentProgress = ((double)numberChecked / (double)trackCount) * 100;
                                CurrentTrack = numberChecked.ToString();



                                // Make sure track is not null or process next track
                                //
                                if (track == null) continue;


                                // Only execute if the track is an  actual file on the drive
                                //
                                if (track.Kind == ITTrackKind.ITTrackKindFile)
                                {

                                    IITFileOrCDTrack fileTrack = (IITFileOrCDTrack)track;

                                    TrackLocation = fileTrack.Location;
                                    TrackName = fileTrack.Name;


                                }
                                else
                                {

                                    // This was not a file so move on
                                    //
                                    continue;
                                }


                                SearchStatus = "Low rated track removal in progress...";


                                #region Delete the Low Rated Track

                                // Stars rating - each star is worth 20 for a total of 5 = 100%
                                if (track.Rating == 20)
                                {

                                    try
                                    {

                                        // If the location is not empty
                                        if (!string.IsNullOrEmpty(TrackLocation))
                                        {
                                            // Remove from Disk First
                                            Alphaleonis.Win32.Filesystem.File.Delete(TrackLocation);
                                            
                                            // Remove from iTunes Second
                                            track.Delete();

                                            System.Threading.Thread.Sleep(500); // Wait

                                        }
                                        else
                                        {
                                            // Just Delete the Track
                                            //
                                            track.Delete();

                                            System.Threading.Thread.Sleep(500); // Wait
                                        }

                                        // Update Counters

                                        matchingTracks++;
                                        OrphanedTacks = matchingTracks.ToString();


                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.Print("Unable to erase {0} from disk", ex.Message);

                                    }


                                }


                                #endregion

                                #region Mark Dead Track with a Single Star

                                if (string.IsNullOrEmpty(TrackLocation))
                                {
                                    track.Enabled = false;

                                }


                                #endregion


                                // Keep track of count in case of iTunes error
                                //
                                globalCounter = i;

                            }


                            // Update Status
                            // System.Threading.Thread.Sleep(10);
                            _worker.ReportProgress(numberChecked);


                            success = true;
                        }

                        #endregion check for dead  and orphaned tracks

                    }
                    catch (System.Runtime.InteropServices.COMException e)
                    {

                        #region Error Detection

                        if ((e.ErrorCode & 0xFFFF) == 0xC472 || e.ErrorCode == -2147417846 || e.Message == "Exception from HRESULT: 0xA0040202")
                        {
                            // iTunes is busy
                            System.Threading.Thread.Sleep(500); // Wait, and...
                            TrackName =
                                "iTunes is busy.  Please close any dialog or messages pending....";
                            CurrentTrack = "";
                            MatchRating = "";
                            FilesScanned = "";
                            OrphanedTacks = "";
                            DeadTracksFound = "";
                            TrackLocation = "";
                            CurrentProgress = 0;

                            Debug.Write("Error detected: " + e.Message);

                            success = false; // ...try again
                        }
                        else
                        {
                            // Re-throw!
                            throw;
                        }


                        #endregion Error Detection

                    }
                }

                // ====================END


            };

            #endregion - do work

            #region Progress Changed
            _worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {

                // System.Threading.Thread.Sleep(10);
                int percentage = args.ProgressPercentage;
                ElapsedTime = _stopWatch.Elapsed.Hours.ToString() + "h " +
                              _stopWatch.Elapsed.Minutes.ToString() + "m " +
                              _stopWatch.Elapsed.Seconds.ToString() + "s";


                if (_worker.CancellationPending)
                {
                    // Request Cancel Received
                    TrackLocation = "Request to cancel process received.";
                }



                #region XP Performance Boost

                // Minimize if this is windows XP every 50 Records
                //
                //feature XP Performance Boost
                //
                // Get OperatingSystem information from the system namespace.
                //
                if (Environment.OSVersion.Version.Major.ToString() == "5" &&
                    numberChecked % 100 == 0)
                {

                    MinAppNow();

                }

                #endregion



            };

            #endregion

            #region Work completed

            _worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                // object result = args.Result;

                _stopWatch.Stop();
                Debug.Print("Current track: " + CurrentTrack.ToString());
                Debug.Print("Current Progress: " + CurrentProgress.ToString());
                Debug.Print("Current Track: " + TrackName);
                Debug.Print("Elapsed Time: " + _stopWatch.Elapsed.ToString());
                _stopWatch.Reset();

                // Turn the Button Back on Again
                StartButtonVisibility = Visibility.Visible;
                ScopeStatusVisibility = Visibility.Visible;
                BusyStatusVisibility = Visibility.Hidden;
                // Include THIS or it wont work
                this.PlayBusyAnimation = false;

                TrackName = "Ready.";
                CurrentTrack = "";
                MatchRating = "";
                FilesScanned = "";
                DeadTracksFound = "";
                TrackLocation = "";
                TrackName = "Ready.";
                CurrentProgress = 0;

                // Attach the Data to the Listview
                //



            };

            #endregion


            // Only run it if it's not busy doing another process
            //
            if (!_worker.IsBusy)
                _worker.RunWorkerAsync();


        } // End of delete track


        #endregion -  itunes processing center


        #region Recursive File Scanner

        //BMK New Recursive Method to populate and search Dictionary based search algorithm
        // Long File Name Support Included
        //
        public void WalkFolder(string directory, string function2Run)
        {

            //If not Called with a function name exit
            if (function2Run == null || function2Run == "")
                return;

            // Clear the Dictionary before starting
            fileIndex.Clear();

            // Start the counter
            FilesScannedCounter = 0;

            // Call the Private function to walk the directory
            //WalkFolder(new Alphaleonis.Win32.Filesystem.DirectoryInfo(directory), FilesScannedCounter, function2Run);
            //BMK Start converting from AlphaFS to ZetaLong Here - Test to make sure it works - switch to Zeta in order to use NUGET
            // for deployment in other systems.

            WalkFolder(new Alphaleonis.Win32.Filesystem.DirectoryInfo(directory), FilesScannedCounter, function2Run);
            FilesScannedCounter = 0;
            CurrentTrack = "";
            TrackLocation = "";
            TrackName = "Ready.";

        }

        private void WalkFolder(Alphaleonis.Win32.Filesystem.DirectoryInfo directory, int fileIndexCounter, string function2Run)
        {

            // Scan all files in the current path
            CurrentTrack = "Creating search index.  Please wait...";
            TrackName = "";


            foreach (var file in directory.GetFiles())
            {
                // Use this section to send the file found object for processing in another method
                //
                if (function2Run == "createSearchIndex")
                    createDictionary4Search(file); // Call the Update Search Index Method

            }

            // Create folder object
            Alphaleonis.Win32.Filesystem.DirectoryInfo[] subDirectories = directory.GetDirectories();          
            //
            // Scan the directories in the current directory and call this method, again to go one level into the directory tree
            //
            foreach (var subDirectory in subDirectories)
            {
                WalkFolder(subDirectory, fileIndexCounter, function2Run);
            }

        }

        //BMK Ended last update here 12-12-13 - 12:18pm

        #endregion


        #region Dictionary Search

        string findLostTrack(string trackFileName)
        {

            string trackLocation = null;

            foreach (var pair in fileIndex)
            {
                // Look through all the index and get the location
                // Console.WriteLine("{0}, {1}", pair.Key,pair.Value);
                if (trackFileName == pair.Value.FileName)
                    trackLocation = pair.Value.Path + "\\" + pair.Value.FileName;

                FilesScanned = pair.Key.ToString();
                TrackLocation = pair.Value.Path + "\\" + pair.Value.FileName;

                // Exit if the track was found
                if (trackLocation != null)
                    break;


            }


            return trackLocation;


        }


        void createDictionary4Search(Alphaleonis.Win32.Filesystem.FileInfo file)
        {

            // Add file infor to Search Dictionary
            //
            if (file.Name.ToLower().Contains(".mp3") || file.Name.ToLower().Contains(".m4a") || file.Name.ToLower().Contains(".mp4") || file.Name.ToLower().Contains(".m4p") || file.Name.ToLower().Contains(".mov") || file.Name.ToLower().Contains(".m4v"))
            {

                try
                {
                    FileIndexTable newFile = new FileIndexTable(FilesScannedCounter, file.DirectoryName, file.Name, file.Length, file.LastAccessTime);
                    fileIndex.Add(newFile.ID, newFile);
                    TrackLocation = "Adding file #: " + FilesScannedCounter + " - " + (file.DirectoryName + "\\" + file.Name);
                    FilesScannedCounter++; // File Index

                }
                catch (Exception ex)
                {
                    FileIndexTable newFile = new FileIndexTable(FilesScannedCounter, file.DirectoryName, file.Name, file.Length, file.LastWriteTime);
                    fileIndex.Add(newFile.ID, newFile);
                    TrackLocation = "Adding file #: " + FilesScannedCounter + " - " + (file.DirectoryName + "\\" + file.Name);
                    FilesScannedCounter++; // File Index
                    Debug.Write(Environment.NewLine + "Error adding file to search index... " + ex.Message);
                }

            }


        }

        string FakeTempFilePath(string oriTrackName)
        {
            // This functions creates a dummy .mp3 file that has the exact name as the original mp3 so that it
            // can be addeed to the dead list.  
            // After it's added to the dead list the file is removed and the original name is preserved in the XML library

            // It will also handle video files if necessary
            //

            // get the file name extention and create file accordigly

            // get the temp folder
            //

            string tPath;
            // tPath = ZetaLongPaths.ZlpPathHelper.GetTempDirectoryPath();
            tPath = Alphaleonis.Win32.Filesystem.Path.GetTempPath();


            // Create the File
            //
            if (oriTrackName.ToLower().Contains(".m4p") || oriTrackName.ToLower().Contains(".m4v"))
            {

                try
                {
                    Alphaleonis.Win32.Filesystem.File.WriteAllBytes(@tPath + "\\" + oriTrackName, TuneNazity.Properties.Resources.Temp2);
                }
                catch (Exception e)
                {

                    Debug.Print("Error making fake file path for: " + oriTrackName + "/n" + "Error: " + e.Message);
                }
                
            }
            else
            {

                try
                {
                    Alphaleonis.Win32.Filesystem.File.WriteAllBytes(@tPath + "\\" + oriTrackName, TuneNazity.Properties.Resources.Temp);
                    
                }
                catch (Exception e)
                {

                    Debug.Print("Error making fake file path for: " + oriTrackName + "/n" + "Error: " + e.Message);
                }


            }


            // Return the address of this file

            return @tPath + oriTrackName;

        }

        bool deleteTempFakeFile(string fakeFilePath)
        {

            // Gets rid of the temp File
            //

            if (Alphaleonis.Win32.Filesystem.File.Exists(@fakeFilePath))
            {
                Alphaleonis.Win32.Filesystem.File.Delete(@fakeFilePath);
                return true;
            }

            return false;


        }

        void deleteAllDeadTempFiles()
        {

            // Clean house

            string tPath;
            tPath = Alphaleonis.Win32.Filesystem.Path.GetTempPath();


            //BMK Clear All Temp Files
            //

            try
            {
                // Create an object first to test if the files exist
                //


                var di = new Alphaleonis.Win32.Filesystem.DirectoryInfo(tPath).GetFiles("*.mp3", SearchOption.AllDirectories);
                // var di = Alphaleonis.Win32.Filesystem.Directory.GetFiles(tPath, "*.mp3", SearchOption.AllDirectories);
                if (di != null)
                {
                    foreach (Alphaleonis.Win32.Filesystem.FileInfo f in di)
                    {
                        TrackLocation = "Removing Temp MP3 files - " + f.FullName;
                        f.Delete();
                        _worker.ReportProgress(1);
                        System.Threading.Thread.Sleep(200); // Wait, and...
                    }
                }
                
                di = new Alphaleonis.Win32.Filesystem.DirectoryInfo(tPath).GetFiles("*.m4v", SearchOption.AllDirectories);
                if (di != null)
                {
                    foreach (Alphaleonis.Win32.Filesystem.FileInfo f in di)
                    {
                        TrackLocation = "Removing Temp M4V files - " + f.FullName;
                        f.Delete();
                    }
                }


                di = new Alphaleonis.Win32.Filesystem.DirectoryInfo(tPath).GetFiles("*.m4p", SearchOption.AllDirectories);
                if (di != null)
                {
                    foreach (Alphaleonis.Win32.Filesystem.FileInfo f in di)
                    {
                        TrackLocation = "Removing Temp MP4 files - " + f.FullName;
                        f.Delete();
                    }
                }


                di = new Alphaleonis.Win32.Filesystem.DirectoryInfo(tPath).GetFiles("*.wav", SearchOption.AllDirectories);
                if (di != null)
                {
                    foreach (Alphaleonis.Win32.Filesystem.FileInfo f in di)
                    {
                        TrackLocation = "Removing Temp WAV files - " + f.FullName;
                        f.Delete();
                    }

                }


                di = new Alphaleonis.Win32.Filesystem.DirectoryInfo(tPath).GetFiles("*.mov", SearchOption.AllDirectories);
                if (di != null)
                {
                    foreach (Alphaleonis.Win32.Filesystem.FileInfo f in di)
                    {
                        TrackLocation = "Removing Temp MOV files - " + f.FullName;
                        f.Delete();
                    }
                }



                di = new Alphaleonis.Win32.Filesystem.DirectoryInfo(tPath).GetFiles("*.m4a", SearchOption.AllDirectories);
                if (di != null)
                {
                    foreach (Alphaleonis.Win32.Filesystem.FileInfo f in di)
                    {
                        TrackLocation = "Removing Temp M4A files - " + f.FullName;
                        f.Delete();
                    }
                }

                
            }
            catch (Exception e)
            {
                Debug.Print("Error deleting temp files... Error: " + e.Message);
                
            }

            
        }


        private void CreateDictionary4TracksIndexSearch()
        {



            // Start the Watch
            _stopWatch = new Stopwatch();
            _stopWatch.Start();

            // Initialize the Index
            TrackIndexList = new Dictionary<int, IITTrack>();


            // This will populate the Dictionary will all the tracks needed for a similarity comparison check
            //
            IITTrackCollection tracks = iTunes.LibraryPlaylist.Tracks;
            int trackCount = tracks.Count;

            // Use this counter to make sure the scan picks up in case itunes gets busy or a bad track is rejected
            // otherwise the count will start at zero again
            int globalCounter = 0;


            // Background Worker

            using (var backgroundWorker2 = new BackgroundWorker())
            {

                // Features
                //
                backgroundWorker2.WorkerSupportsCancellation = true;
                backgroundWorker2.WorkerReportsProgress = true;


                backgroundWorker2.DoWork += (s, e) =>
                {
                    #region Do Work

                    // Function to handle Busy Issues with iTunes
                    //
                    bool success = false;
                    while (!success)
                    {

                        // Handle any errors incurred
                        try
                        {

                            // Go through each track in the library
                            //
                            for (int i = 1; i < trackCount; i++)
                            {


                                #region Process a single Track


                                // If cancel requested - terminate
                                //
                                if (backgroundWorker2.CancellationPending)//checks for cancel request
                                {
                                    CurrentTrack = "Cancel request in progress...";
                                    e.Cancel = true;
                                    break;
                                }



                                // Pickup where it left off  in case process is interrupted - otherwise will restart again
                                //
                                if (globalCounter > i && globalCounter <= trackCount)
                                    i = globalCounter;

                                if (_CancelCurrentBackgroundWorker == false)
                                {


                                    // Current iTunes Track
                                    IITTrack track = tracks[i];

                                    // Make sure track is the right kind to add - file based only
                                    //
                                    if (track.Kind == ITTrackKind.ITTrackKindFile)
                                    {

                                        // Create a track with a location property
                                        //
                                        var fileTrack = (IITFileOrCDTrack)track;


                                        // Make sure the track is not repeated due to iTunes being busy
                                        IITTrack trackCheck = null;
                                        TrackIndexList.TryGetValue(fileTrack.TrackDatabaseID, out trackCheck);

                                        if (trackCheck != null)
                                            continue;

                                        // Add this Track to the Index
                                        //
                                        TrackIndexList.Add(fileTrack.TrackDatabaseID, fileTrack);
                                        Debug.Print("Adding track {0} of {1} to TrackIndex Dictionary", i.ToString(), trackCount.ToString());


                                        // Keep track of count in case of iTunes error
                                        //
                                        globalCounter++;


                                        // Update UI
                                        //
                                        FilesScanned = i.ToString();
                                        TrackLocation = System.Web.HttpUtility.UrlDecode(fileTrack.Location);
                                        CurrentProgress = ((double)i / (double)trackCount) * 100;

                                        // Scan all files in the current path
                                        CurrentTrack = "Creating track index.  Please wait...";

                                        TrackName = "";

                                        // Update UI
                                        backgroundWorker2.ReportProgress(i);

                                    }

                                }
                                else
                                {

                                    // Cancel this command now
                                    //
                                    backgroundWorker2.CancelAsync();

                                }

                                // Index Complete
                                success = true;
                            }
                                #endregion process a single track



                        }
                        catch (System.Runtime.InteropServices.COMException ex)
                        {

                            #region iTunes is Busy

                            if ((ex.ErrorCode & 0xFFFF) == 0xC472 || ex.ErrorCode == -2147417846)
                            {
                                // iTunes is busy
                                System.Threading.Thread.Sleep(500); // Wait, and...

                                CurrentTrack = "iTunes is busy.  Please close any dialog or messages pending....";

                                Debug.Write("Error detected: " + ex.Message);

                                // Reset Counter or progress bar will be off
                                //                                                 
                                success = false; // ...try again
                            }
                            else if (ex.ErrorCode == -2147467259)
                            {
                                Debug.Print("File Location not accepted by dead track listing..");

                            }
                            else
                            {

                                // Re-throw!
                                throw;

                            }

                            #endregion

                        }

                    }


                    #endregion - do work

                };


                backgroundWorker2.ProgressChanged += (s, e) =>
                {

                    #region Progress Report

                    // Progress indicator
                    int i = e.ProgressPercentage;

                    ElapsedTime = _stopWatch.Elapsed.Hours.ToString() + "h " +
                                  _stopWatch.Elapsed.Minutes.ToString() + "m " +
                                  _stopWatch.Elapsed.Seconds.ToString() + "s";

                    #endregion - progress report

                };


                backgroundWorker2.RunWorkerCompleted += (s, e) =>
                {

                    #region Work Completed


                    if (e.Cancelled)//it doesn't matter if the BG worker ends normally, or gets cancelled,
                    {

                        //both cases RunWorkerCompleted is invoked, so we need to check what has happened
                        //
                        Debug.Print("You've cancelled the backgroundworker!");
                        TrackName = "Ready.";
                        CurrentTrack = "Duplicate search process cancelled.";
                        _CancelCurrentBackgroundWorker = false;


                        // Turn the Button Back on Again
                        StartButtonVisibility = Visibility.Visible;
                        ScopeStatusVisibility = Visibility.Visible;
                        BusyStatusVisibility = Visibility.Hidden;
                        // Include THIS or it wont work
                        this.PlayBusyAnimation = false;


                    }
                    else
                    {

                        // Dictionary Created
                        //
                        _stopWatch.Reset();
                        TrackName = "Ready.";
                        Debug.Print("TrackIndex Dictionary done! - " + TrackIndexList.Count.ToString());


                        // Initialize the Background thread
                        _worker = null;

                        // Now go to the next step 
                        findSimilarTracks();

                    }

                    #endregion - work completed
                };


                backgroundWorker2.RunWorkerAsync();
                backgroundWorker2.WorkerReportsProgress = true;
            }






        }

        private void CreateDictionaryFromItunesXMLLibrary()
        {


            // Initialize the Background thread
            //
            BackgroundWorker _workerItunesXml = new BackgroundWorker();
            _workerItunesXml.WorkerReportsProgress = true;
            _workerItunesXml.WorkerSupportsCancellation = true;

            Stopwatch XMLSearchStopwatch = new Stopwatch();
            XMLSearchStopwatch.Start();
            int counter = 0;
            int rejectedTracks = 0;

            ScopeStatusVisibility = Visibility.Hidden;
            BusyStatusVisibility = Visibility.Visible;


            if (ItunesXmLdb != null)
            {
                ItunesXmLdb.Clear();
            }
            else
            {
                ItunesXmLdb = new Dictionary<string, string>();
            }




            _workerItunesXml.DoWork += delegate(object s, DoWorkEventArgs args)
            {

                // Turn Busy Light On

                // Function to handle Busy Issues with iTunes
                //
                bool success = false;
                while (!success)
                {
                    try
                    {
                        //BMK Create Dictionary from iTunes XML
                        #region Create Dictionary from iTunes XML


                        // Turn off the UI
                        //
                        FindDuplicatesCheckBoxVisibility = Visibility.Hidden;
                        Search4LostTracksCheckBoxVisibility = Visibility.Hidden;
                        ClearLowRatedDeadTracksCheckBoxVisibility = Visibility.Hidden;


                        // Create the regular expression to find all songs with Alphanumeric Names
                        System.Text.RegularExpressions.Regex searchSongType =
                            new System.Text.RegularExpressions.Regex(@"^\x00-\x7F");




                        // Translate XML Library into LINQ Supported IENUMERABLE Collection
                        //
                        var mySongs =
                            from song in LoadSongsFromITunes(iTunes.LibraryXMLPath)
                            where song.Kind.ToLower().Contains("audio") && song.Location.ToLower().Contains("file")
                            orderby song.Id ascending
                            select song;
                        Debug.Print("Time to load Library Refference: " +
                                    XMLSearchStopwatch.Elapsed.TotalSeconds.ToString() + " -- Tracks loaded: " + mySongs.Count().ToString());

                        TrackLocation = "Loading iTunes database.  Please wait...";

                        // Debug Line
                        int count = 0;

                        //

                        foreach (var song in mySongs)
                        {

                            // BMK Load iTunes XML DB
                            //feature Testing Mode - Change to limit tracks counted
                            if (count == 100000)
                                break;

                            // Add the File Name Refference to the Dictionary for Fast Access to the File Name
                            //

                            // First Clean Up the Name space from the file name
                            //
                            string fileName = Path.GetFileName(song.Location).Trim();

                            if (!string.IsNullOrEmpty(fileName))
                            {
                                string cleanFileName =
                                    System.Web.HttpUtility.UrlDecode(fileName);

                                // Now add it to the Dictionary
                                ItunesXmLdb.Add(song.Id.ToString(), cleanFileName);

                                // Debug.Print(" {0} - {1} ", song.Id, song.Location);

                                
                                TrackName = cleanFileName;
                                // System.Threading.Thread.Sleep(1);

                                // Include THIS or it wont work
                                this.PlayBusyAnimation = true;

                                _workerItunesXml.ReportProgress(counter);

                                count++;
                                Debug.Print("Track #: " + count + "  - Type: " + System.Web.HttpUtility.UrlDecode(song.Location));



                            }
                            else
                            {
                                rejectedTracks++;
                                Debug.Print("Track Not On Disk Rejected: " + count.ToString() + " - Title: " + song.Name + " - Type: " + song.Kind);

                            }

                            

                        }

                        Debug.Print("Total Tracks Rejected: " + rejectedTracks.ToString());

                        #endregion

                        success = true;


                    }

                    catch (System.Runtime.InteropServices.COMException e)
                    {
                        if ((e.ErrorCode & 0xFFFF) == 0xC472 ||
                            e.ErrorCode == -2147417846)
                        {
                            // iTunes is busy
                            System.Threading.Thread.Sleep(500); // Wait, and...
                            TrackName =
                                "iTunes is busy.  Please close any dialog or messages pending....";

                            Debug.Write("Error detected: " + e.Message);

                            success = false; // ...try again
                        }
                        else
                        {
                            // Re-throw!
                            throw;
                        }
                    }
                }





            };


            _workerItunesXml.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {

                // Update Status
                //
                ElapsedTime = XMLSearchStopwatch.Elapsed.Hours.ToString() + "h " +
                              XMLSearchStopwatch.Elapsed.Minutes.ToString() +
                              "m " +
                              XMLSearchStopwatch.Elapsed.Seconds.ToString() +
                              "s";
                CurrentTrack = args.ProgressPercentage.ToString();
                // Console.Write("Animation State: " + PlayBusyAnimation.ToString());

                // Debug.Print("Loading Track: {0} - Elapsed time: {1}", args.ProgressPercentage.ToString() , XMLSearchStopwatch.Elapsed.TotalSeconds.ToString());

            };


            _workerItunesXml.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {

                Debug.Print("Time to load Dictionary: " +
                            XMLSearchStopwatch.Elapsed.TotalSeconds.ToString());
                XMLSearchStopwatch.Reset();

                // Number of Tracks in Dictionary
                //
                int trackCount = ItunesXmLdb.Count;


                //// Show Dictionary
                //foreach (var pair in ItunesXmLdb)
                //{
                //    Debug.Print(pair.Key + " - " + pair.Value);
                //}
                TrackLocation = "";
                CurrentTrack = "";
                TrackName = "Ready.";
                BusyStatusVisibility = Visibility.Hidden;
                // Include THIS or it wont work
                this.PlayBusyAnimation = false;
                ScopeStatusVisibility = Visibility.Visible;

                // Turn off the UI
                //
                FindDuplicatesCheckBoxVisibility = Visibility.Visible;
                Search4LostTracksCheckBoxVisibility = Visibility.Visible;
                ClearLowRatedDeadTracksCheckBoxVisibility = Visibility.Visible;


                checkStartStatus();



            };

            _workerItunesXml.RunWorkerAsync();



        }

        private IEnumerable<Song> LoadSongsFromITunes(string filename)
        {

            // Global

            IEnumerable<XElement> rawsongs = null;

            // Handle COM Busy from iTunes
            bool success = false;
            while (!success)
            {

                try
                {

                    // Use Text Encoding To Prevent language Errors when loading the XML file
                    //
                    using (System.IO.StreamReader oReader = new System.IO.StreamReader(filename, System.Text.Encoding.GetEncoding("ISO-8859-1")))
                    {                     
                    
                        rawsongs =
                                  from song in
                                      XDocument.Load(oReader).Descendants("plist").Elements("dict").Elements("dict").Elements("dict")
                                  select new XElement("song",
                                  from key in song.Descendants("key")
                                  select new XElement(((string)key).Replace(" ", ""),
                                                          (string)(XElement)key.NextNode));

                        // All Done
                        success = true;
                    }


                }
                catch (Exception ex)
                {
                    TrackName = "iTunes is busy.  Please close any dialog or messages pending. \n" + ex.Message;

                    Debug.Write("Error detected: " + ex.Message);

                    // Continue to next file if the song name contains a non alphanumeric character
                    //
                    if (ex.Message.Contains("invalid character"))
                        continue;


                    System.Threading.Thread.Sleep(500);


                }

            }

            var songs = from s in rawsongs
                        select
                            new Song()
                            {
                                Id = s.Element("TrackID").ToInt(0),
                                Album = s.Element("Album").ToString(string.Empty),
                                Artist = s.Element("Artist").ToString(string.Empty),
                                BitRate = s.Element("BitRate").ToInt(0),
                                Comments = s.Element("Comments").ToString(string.Empty),
                                Composer = s.Element("Composer").ToString(string.Empty),
                                Genre = s.Element("Genre").ToString(string.Empty),
                                Kind = s.Element("Kind").ToString(string.Empty),
                                Location = s.Element("Location").ToString(string.Empty),
                                Name = s.Element("Name").ToString(string.Empty),
                                PlayCount = s.Element("PlayCount").ToInt(0),
                                SampleRate = s.Element("SampleRate").ToInt(0),
                                Size = s.Element("Size").ToInt64(0),
                                TotalTime = s.Element("TotalTime").ToInt64(0),
                                TrackNumber = s.Element("TrackNumber").ToInt(0)
                            };
            return songs;
        }

        #endregion


        #region  Levenstein algorithm to Detected Similarities

        private int GetLevensteinDistance(string firstString, string secondString)
        {
            if (firstString == null)
                throw new ArgumentNullException("firstString");
            if (secondString == null)
                throw new ArgumentNullException("secondString");

            if (firstString == secondString)
                return 0;

            int[,] matrix = new int[firstString.Length + 1, secondString.Length + 1];

            for (int i = 0; i <= firstString.Length; i++)
                matrix[i, 0] = i; // deletion
            for (int j = 0; j <= secondString.Length; j++)
                matrix[0, j] = j; // insertion

            for (int i = 0; i < firstString.Length; i++)
                for (int j = 0; j < secondString.Length; j++)
                    if (firstString[i] == secondString[j])
                        matrix[i + 1, j + 1] = matrix[i, j];
                    else
                    {
                        matrix[i + 1, j + 1] = Math.Min(matrix[i, j + 1] + 1, matrix[i + 1, j] + 1);
                        //deletion or insertion
                        matrix[i + 1, j + 1] = Math.Min(matrix[i + 1, j + 1], matrix[i, j] + 1); //substitution
                    }
            return matrix[firstString.Length, secondString.Length];
        }


        private double GetSimilarity(string firstString, string secondString)
        {
            if (String.IsNullOrEmpty(firstString) || (String.IsNullOrEmpty(secondString)))
                return 0;

            if (firstString == secondString)
                return 1;

            int longestLenght = Math.Max(firstString.Length, secondString.Length);
            int distance = GetLevensteinDistance(firstString, secondString);
            double percent = distance / (double)longestLenght;
            return 1 - percent;
        }

        #endregion


        #region Folder Selection Control

        private void loadTreewithDrives()
        {

            #region Load The Tree For the Search Dialog

            // First Load the Global Object
            //
            string global = "Computer";
            TreeViewItem itemRoot = new TreeViewItem();
            itemRoot.Header = global;
            // The Tag ised used by the Checkbox in the Tree to report
            // the Full Path of the Item because the TreeViewItem
            // is not sent when the Checkbox is Click - Same for Subfolder
            // This eliminates the need to walk the tree to find out which TreeViewItems
            // Are Checked!
            itemRoot.Tag = global;
            itemRoot.Items.Add(_dummyNode);
            itemRoot.Expanded += folder_Expanded;
            // Apply the attached property so that 
            // the triggers know that this is root item.
            TreeViewItemProps.SetIsRootLevel(itemRoot, "Root");


            // Load the Drives
            this.SearchDialogRef.myTree.Items.Add(itemRoot);

            foreach (string drive in Directory.GetLogicalDrives())
            {

                // Create thhe space for the Drive Branch
                if (itemRoot.Items.Count == 1 && itemRoot.Items[0] == _dummyNode)
                    itemRoot.Items.Clear();

                var item = new TreeViewItem();
                item.Header = drive;
                item.Tag = drive;
                item.Items.Add(_dummyNode);
                item.Expanded += folder_Expanded;
                // Apply the attached property so that 
                // the triggers know that this is root item.
                TreeViewItemProps.SetIsRootLevel(item, "Drive");
                TreeViewItemProps.SetIsFolderSelected(item, "False");


                // Create Hard Drive Leaf
                //
                itemRoot.Items.Add(item);


            }

            #endregion


        }

        private void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;


            if (item.Items.Count == 1 && item.Items[0] == _dummyNode)
            {
                item.Items.Clear();
                try
                {
                    foreach (string dir in Directory.GetDirectories(item.Tag as string))
                    {

                        #region Exceptions


                        // ReSharper disable StringIndexOfIsCultureSpecific.1
                        if (dir.ToUpper().IndexOf("WINDOWS") != -1)
                            continue;
                        if (dir.ToUpper().ToUpper().IndexOf("$RECYCLE") != -1)
                            continue;
                        if (dir.ToUpper().ToUpper().IndexOf("PROGRAM FILES") != -1)
                            continue;
                        if (dir.ToUpper().IndexOf("PROGRAMDATA") != -1)
                            continue;
                        if (dir.ToUpper().ToUpper().IndexOf("APPDATA") != -1)
                            continue;
                        if (dir.ToUpper().ToUpper().IndexOf("SYSTEM VOLUME INFORMATION") != -1)
                            continue;
                        if (dir.ToUpper().ToUpper().IndexOf(".MSI") != -1)
                            continue;
                        if (dir.ToUpper().ToUpper().IndexOf("RECYCLE") != -1)
                            continue;
                        if (dir.ToUpper().ToUpper().IndexOf("MSOCACHE") != -1)
                            continue;
                        if (dir.ToUpper().ToUpper().IndexOf("INETPUB") != -1)
                            continue;
                        if (dir.ToUpper().ToUpper().IndexOf("PERFLOGS") != -1)
                            continue;
                        if (dir.ToUpper().ToUpper().IndexOf("RECOVERY") != -1)
                            continue;
                        if (dir.ToUpper().ToUpper().IndexOf("SHAREPOINTFILES") != -1)
                            continue;

                        #endregion Exceptions

                        #region  Check for Music files - Long File Support

                        // Filter File Search
                        //
                        string[] musicFiles = null;
                        try
                        {
                            musicFiles = Alphaleonis.Win32.Filesystem.Directory.GetFiles(dir, "*.m??");
                            //ZetaLongPaths.ZlpFileInfo [] musicFiles =  ZetaLongPaths.ZlpIOHelper.GetFiles(dir, "*.m??", SearchOption.AllDirectories);
                            Debug.Print("Folder: {0} - Found: {1}", dir, musicFiles.Length.ToString());

                        }
                        catch (Exception ex)
                        {

                            Debug.Print(Environment.NewLine + "Folder: {0} - Error: {1}", dir.ToUpper(), ex.Message);
                            continue;
                        }
                        
                        
                        #endregion // Check for Music files


                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = new DirectoryInfo(dir).Name;
                        subitem.Tag = new DirectoryInfo(dir).FullName;
                        subitem.Items.Add(_dummyNode);
                        subitem.Expanded += folder_Expanded;

                        // If any music files where found
                        //
                        if (musicFiles.Length > 0)
                        {
                            TreeViewItemProps.SetIsRootLevel(subitem, "Song");

                        }
                        else if (musicFiles.Length == 0)
                        {
                            TreeViewItemProps.SetIsRootLevel(subitem, "False");
                        }

                        // Check if the Parent TreeViewItem was Selected and Mark the Expanded Folders accordingly
                        //

                        if (TreeViewItemProps.GetIsFolderSelected(item) == "True")
                        {

                            TreeViewItemProps.SetIsFolderSelected(subitem, "True");
                        }
                        else if (TreeViewItemProps.GetIsFolderSelected(item) == "False")
                        {
                            TreeViewItemProps.SetIsFolderSelected(subitem, "False");
                        }


                        // Create the Folder Leaf
                        //
                        item.Items.Add(subitem);

                    }

                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                }
            }
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj)
            where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private bool SearchCriteriaStringManager(string pathToCheck, bool singlePathOnly)
        {

            // Remove this if you want to keep adding folders to the SearchCriteria string
            if (singlePathOnly == true)
                SearchCriteria = "";

            //feature:  Folder Selection Manager
            //
            bool folderAdded = false;

            #region  Clean Path to Search - If the SearchString is not My Computer than Take it Out

            // 
            //
            if (pathToCheck != "Computer")
            {
                string cleanRoot = pathToCheck.Replace("Computer\\", "");
                pathToCheck = cleanRoot;
                cleanRoot = pathToCheck.Replace("Computer\\ * ", "");
                pathToCheck = cleanRoot;
                cleanRoot = pathToCheck.Replace(" * Computer\\", "");
                pathToCheck = cleanRoot;
            }

            #endregion


            #region Separate Selections in Array and Check if this one Needs to be added

            // Create the Folders Array
            //
            SearchCriteria = SearchCriteria.Replace(" ", "");   // Take out all spaces before convertion
            string[] foldersSelected = SearchCriteria.Split('*');
            List<string> folderList = foldersSelected.ToList();

            // If My Computer is Received tham ,ake it the only one
            //
            if (pathToCheck == "Computer") pathToCheck = "Computer"; // Check for Special case

            // Check if the Current Selection Exists
            //
            if (!folderList.Contains(pathToCheck))
            {
                // Add the Folder
                //
                folderList.Add(pathToCheck);
                folderAdded = true;
            }
            else
            {
                // Remove the Folder
                //
                folderList.Remove(pathToCheck);

            }

            // Convert the List Back to a String
            //
            folderList.Remove(""); // Remove Empty Selectiions
            foldersSelected = folderList.ToArray();
            string result = string.Join(" * ", foldersSelected);
            SearchCriteria = result;





            #endregion


            // Send response to control checkbox
            //
            return folderAdded;
        }

        private void myTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            System.Windows.Controls.TreeView tree = (System.Windows.Controls.TreeView)sender;
            TreeViewItem temp = ((TreeViewItem)tree.SelectedItem);

            // Keep a refference to this Item to update the checkbox
            //
            TreeViewItem oriTemp = temp;

            if (temp == null)
                return;

            // Check if the Folder was Previously Selected
            //
            string isThisFolderSelected = TreeViewItemProps.GetIsFolderSelected(temp);

            SelectedImagePath = "";
            string temp1 = "";
            string temp2 = "";
            while (true)
            {
                temp1 = temp.Header.ToString();
                if (temp1.Contains(@"\"))
                {
                    temp2 = "";
                }
                SelectedImagePath = temp1 + temp2 + SelectedImagePath;
                if (temp.Parent.GetType().Equals(typeof(System.Windows.Controls.TreeView)))
                {
                    break;
                }
                temp = ((TreeViewItem)temp.Parent);
                temp2 = @"\";

            }


            // Clear the Last selection from the tree if it's a different folder
            // Remove this if you want to select multiple folders
            if (_lastItemSelected != null)
            {

                // Folder was not added so uncheck it
                //
                TreeViewItemProps.SetIsFolderSelected(_lastItemSelected, "False");

                // Unselect all the children if any
                //
                // Find in Child items and set them accordingly
                //
                if (_lastItemSelected.HasItems)
                    WalkTree(_lastItemSelected, "False");


                // Clear the selection if the same folder was clicked again
                if (_lastItemSelected == oriTemp)
                    SelectedImagePath = "";

                // Reset this selection
                _lastItemSelected = null;

            }



            //  Send the Path For Processing
            // Use the second option in SearchCriteriaStringManager to use only one folder instead of multipleentries in the SearchCriteria string
            bool processResult = SearchCriteriaStringManager(SelectedImagePath, true);




            if (processResult)
            {
                // The Folder was added so Check it
                //
                TreeViewItemProps.SetIsFolderSelected(oriTemp, "True");
                // Find Child items and set them accordingly
                //
                if (oriTemp.HasItems)
                    WalkTree(oriTemp, "True");

                // Record this choice
                _lastItemSelected = oriTemp;

            }
            else
            {
                // Folder was not added so uncheck it
                //
                TreeViewItemProps.SetIsFolderSelected(oriTemp, "False");

                // Unselect all the children if any
                //
                // Find in Child items and set them accordingly
                //
                if (oriTemp.HasItems)
                    WalkTree(oriTemp, "False");

                // Record this choice
                _lastItemSelected = null;

            }

            //Feature:  Handle Parent TreeViewItems and Mark as partial if Appropriate
            // Handle Parents and Mark them as Partial if True and False if No folder was added
            //
            TreeViewItem temp5 = null;
            if (oriTemp.Parent.GetType().Equals(typeof(System.Windows.Controls.TreeView)))
            {
                temp5 = oriTemp;
            }
            else
            {
                temp5 = ((TreeViewItem)oriTemp.Parent);
            }


            while (true)
            {

                Debug.Print("Current TreeViewIttem: " + TreeViewItemProps.GetIsRootLevel(temp5));

                if (temp5.Parent.GetType().Equals(typeof(System.Windows.Controls.TreeView)))
                {
                    // This is the Root Object Mark it and Exit
                    //
                    // Mark the Parent Partial if the Result was Added
                    if (processResult)
                    {
                        TreeViewItemProps.SetIsFolderSelected(temp5, "Partial");
                    }
                    else
                    {
                        TreeViewItemProps.SetIsFolderSelected(temp5, "False");
                    }



                    break;
                }


                // Mark the Parent Partial if the Result was Added
                if (processResult)
                {
                    TreeViewItemProps.SetIsFolderSelected(temp5, "Partial");
                }
                else
                {
                    TreeViewItemProps.SetIsFolderSelected(temp5, "False");
                }

                temp5 = ((TreeViewItem)temp5.Parent);
            }



            // Unselect the Item so that another click fires the Change Item Slected
            //
            oriTemp.IsSelected = false;



        }

        #endregion


        #region CheckBox Management

        private void WalkTree(TreeViewItem treeViewItem, string value)
        {

            for (int i = 0; i < treeViewItem.Items.Count; i++)
            {

                TreeViewItem child = (TreeViewItem)

                treeViewItem.ItemContainerGenerator.ContainerFromIndex(i);

                if (child != null)
                {
                    TreeViewItemProps.SetIsFolderSelected(child, value);

                    if (child.HasItems)
                        WalkTree(child, value);
                }

            }

        }

        private void CheckBoxManagerForSelectedTreeViewItem(string pathReceived)
        {

            // Manage the SearchCriteRia Property from the CheckBox
            // The SelectedItemChanged will be managed at the event delegate
            //
            Debug.Print("Checked Path: " + pathReceived);

            // Locate the TreeviewItem that Has the Header
            //

            ItemCollection items = SearchDialogRef.myTree.Items;
            bool endOfTree = false;

            while (!endOfTree)
            {

                foreach (TreeViewItem node in items)
                {
                    if ((node.Header.ToString() == pathReceived))
                    {
                        //TreeViewItem Found
                        //
                        Debug.Print("Bingo!!!");
                    }

                    if (items.Count == 1 && node.Items.Count >= 1)
                    {

                    }
                }

            }

        }

        private void SearchCheckBoxManager(string cbName)
        {

            // Which Checbox Called
            //
            Debug.Print("{0} was just Clicked", cbName);

            // Make sure only one value is on and the rest are off
            if (cbName == "cbRemoveDeadTracks")
            {

                // Turn Off Find Duplicates and Low Rated Trackes
                FindDuplicateTracks = false;
                RemoveLowRatedTracks = false;

                // Show SearchBox
                ScopeStatusVisibility = Visibility.Visible;


                // Activate Selection
                FindDeadTracks = true;


            }
            else if (cbName == "cbFindDuplicates")
            {

                // Turn Find Dead Tracks and Low Rated Off
                FindDeadTracks = false;
                RemoveLowRatedTracks = false;

                // Hide SearchBox
                ScopeStatusVisibility = Visibility.Hidden;

                // Activate Selection
                FindDuplicateTracks = true;



            }
            else if (cbName == "cbRemoveOneStarTracks")
            {
                // Turn Find Dead Tracks and Duplicates  Off
                FindDeadTracks = false;
                FindDuplicateTracks = false;


                // Hide SearchBox
                ScopeStatusVisibility = Visibility.Hidden;

                // Activate Selection
                RemoveLowRatedTracks = true;


            }

            // Check if the start button is available
            checkStartStatus();


        }

        #endregion

        #region Update Program Layout



        private void UpdateElementLayout(string controlName)
        {

            // Detect Control
            //
            if (controlName == "SelectionBorder")
            {

                // Get the Size of the SelectionBorder to See if its EXpanded
                //
                // Compensante for 1, 2 or 3 rows of text.
                if (SearchDialogRef.SelectionBorder.Height >= 50 && SearchDialogRef.SelectionBorder.Height <= 60)
                {
                    // Two Lines add an extra 25 point to the leng of the window
                    //
                    SearchDialogRef.Height = 453 + 25;
                    // Set the Buttons
                    SearchDialogRef.Select.Margin = new Thickness(95, 400, 0, 0);
                    SearchDialogRef.Exit.Margin = new Thickness(268, 400, 0, 0);


                }
                else if (SearchDialogRef.SelectionBorder.Height >= 62 && SearchDialogRef.SelectionBorder.Height <= 84)
                {
                    // Two Lines add an extra 25 point to the leng of the window
                    //
                    SearchDialogRef.Height = 453 + 49;

                    // Set Button margins so they move too with no animation

                }
                else if (SearchDialogRef.SelectionBorder.Height >= 30 && SearchDialogRef.SelectionBorder.Height <= 40)
                {
                    // Two Lines add an extra 25 point to the leng of the window
                    //
                    SearchDialogRef.Height = 453;
                }





            }



        }



        #endregion

        #region Session Log


        private void addLogEntry(string trackStatus)
        {

            // Get the FIle Name Only
            //

            try
            {
                int start = TrackLocation.IndexOf("[") + 1;
                int end = TrackLocation.IndexOf("]") - 1;
                string realFileName = TrackLocation.Substring(start, (end - start)).Trim();

                _LogData.Add(item: new LogData
                {
                    TrackNumber = CurrentTrack,
                    TrackTitle = TrackName,
                    TrackFileName = realFileName,
                    TrackStatus = trackStatus

                });

            }
            catch (Exception e)
            {

                _LogData.Add(item: new LogData
                {
                    TrackNumber = "ERROR",
                    TrackTitle = e.Message,

                });
            }


        }



        #endregion


        #region Notes


        // TreeView - Used to Select Folder and Drives
        //
        // It does not use a viewmodel because I wanted to read the folders on demand.
        // I'm using the CheckBox as a way to MultiSelect diffrent drives and Folders
        // The CheckBox uses the TreeViewItem TAG attribbute in the XML markup to
        // retrive the Full Path of the  TreeView item where the CheckBox Was Clicked.
        // The ChekBox Does not generate a TreeView Item to extract the Path Directly.
        // I instatiated a copy of the SearchDialog with a Global Object that stays Null
        // until I neeed it in order to maintain the MVVM patter.  The checkBox get's its
        // Data from a Function that is used to load the drives and folders when needed.
        // the Attached Property Item was needed tobe able to assign different icons
        // to the folders.  
        // A function was created to handle a common property called SearchCriteria that
        // the checkbox and SelectedItemChanged control and event can use to maintain a 
        // list of the folders or drives selected.  It also checks if a folder is a 
        // child of a folder already selected and it will alert the user that it is not needed.
        //
        // What items are used for the TreeView
        // 1. Attached Properties class: TreeViewItemsProp = for custom folder icons on the tree
        // 2. Instatiated a Global object that will hold the TreeView in another Window and then
        // attach the SelectedItemEvent to the MainViewModel.
        // SelectedItemChanged Function and the LoadTreeWithDrives as well as the CheckBoxManagerForSelectedTreeViewItem
        // Remember the Checkbox does not generate a TreeViewItem. I had to use EventToCommand to process the CheckBox.
        // The Checkbox also has crucial  MARKUP in the XAML file that tells it to get Full Folder Access from the TAG element of the TreeViewItem.
        // This was hard code to build so safe guard it. This trick makes it possible to reduce the number of custom Attached Properties.



        #endregion



    }



}