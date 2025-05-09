﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO.Compression;
using Project_127.MySettings;
using Project_127.HelperClasses;
using System.Diagnostics;
using GSF.IO;
using System.Security.Cryptography;
using CefSharp;

namespace Project_127.Popups
{
    /// <summary>
    /// Interaction logic for CopyFileProgress.xaml
    /// This is responsible for ProgressBar on File copying (etc.) and Extracting the ZIP File
    /// </summary>
    public partial class PopupProgress : Window
    {


        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            if (MainWindow.MW.IsVisible)
            {
                this.Left = MainWindow.MW.Left + (MainWindow.MW.Width / 2) - (this.Width / 2);
                this.Top = MainWindow.MW.Top + (MainWindow.MW.Height / 2) - (this.Height / 2);
            }
        }


        /// <summary>
        /// Enum of ProgressTypes
        /// </summary>
        public enum ProgressTypes
        {
            ZIPFile,
            FileOperation,
            Upgrade,
            Downgrade,
            MD5,
        }

        /// <summary>
        /// ProgressType of Instance
        /// </summary>
        ProgressTypes ProgressType;

        /// <summary>
        /// List of File Operations
        /// </summary>
        List<HelperClasses.MyFileOperation> MyFileOperations;

        /// <summary>
        /// List of File Operations
        /// </summary>
        public List<HelperClasses.MyFileOperation> RtrnMyFileOperations;

        /// <summary>
        /// Using this to return a Bool
        /// </summary>
        public bool RtrnBool = false;

        private string FilePath = "";

        public string RtrnMD5 = "";

        /// <summary>
        /// Name of Operation for GUI
        /// </summary>
        string Operation;

        /// <summary>
        /// Path of ZIP File (on disk) we are extracting
        /// </summary>
        string ZipFileWeWannaExtract;

        /// <summary>
        /// Path where ZIP File is extracted
        /// </summary>
        string myZipExtractionPath;

        /// <summary>
        /// Constructor of PopupProgress.
        /// String pParam1 is either the GUI Text for File Operations or the ZIP File physical location
        /// </summary>
        /// <param name="pProgressType"></param>
        /// <param name="pParam1"></param>
        /// <param name="pMyFileOperations"></param>
        public PopupProgress(ProgressTypes pProgressType, string pParam1, List<HelperClasses.MyFileOperation> pMyFileOperations = null, string zipExtractionPath = "", bool betterPercentage = false)
        {
            // Sorry you have to look at this spaghetti
            // Basically, based on the pProgressType the other params have different meanings or are not used etc. Kinda messy...really sucks

            if (MainWindow.MW.IsVisible)
            {
                this.Owner = MainWindow.MW;
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            if (zipExtractionPath == "")
            {
                zipExtractionPath = LauncherLogic.ZIPFilePath;
            }
            myZipExtractionPath = zipExtractionPath;

            if (MainWindow.MW.IsVisible)
            {
                this.Owner = MainWindow.MW;
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            InitializeComponent();

            // Setting all Properties needed later
            ProgressType = pProgressType;

            if (ProgressType == ProgressTypes.FileOperation)
            {
                Operation = pParam1;
                MyFileOperations = pMyFileOperations;
                myLBL.Content = Operation + "...(0%)";
            }
            else if (ProgressType == ProgressTypes.MD5)
            {
                FilePath = pParam1;
                string tmp = HelperClasses.FileHandling.PathSplitUp(pParam1)[1];
                Operation = "Checking local file: " + tmp;
                myLBL.Content = Operation + "...(0%)";
            }
            else if (ProgressType == ProgressTypes.ZIPFile)
            {
                ZipFileWeWannaExtract = pParam1;
                myLBL.Content = "Extracting ZIP...(0%)";
            }
            else if (ProgressType == ProgressTypes.Downgrade)
            {
                myPB.Value = 0;
                myLBL.Content = "Gathering Information";
            }
            else if (ProgressType == ProgressTypes.Upgrade)
            {
                myPB.Value = 0;
                myLBL.Content = "Gathering Information";
            }

            // Lets do some shit
            StartWork();
        }


        /// <summary>
        /// Starting the Task 
        /// </summary>
        [STAThread]
        public async void StartWork()
        {
            // Awaiting the Task of the Actual Work
            await Task.Run(new Action(ActualWork));

            // Close this
            this.Close();
        }

        /// <summary>
        /// Task of the actual work being done
        /// </summary>
        [STAThread]
        public void ActualWork()
        {
            //Basically just executing a list of MyFileOperations
            if (ProgressType == ProgressTypes.FileOperation)
            {
                double count = MyFileOperations.Count;
                double j = 0;

                bool WarnedUserOfStuckProcessAlready = false;
                bool CancelFileOperations = false;

                LauncherLogic.InFileOperationWrapperLoop = true;

                HelperClasses.Logger.Log("PopupProgress Lets do some File Operation Stuff");
                for (int i = 0; i <= MyFileOperations.Count - 1; i++)
                {
                    if (!CancelFileOperations)
                    {
                        MyFileOperation.ExecuteWrapper(MyFileOperations[i], ref WarnedUserOfStuckProcessAlready, ref CancelFileOperations);
                    }
                    else
                    {
                        HelperClasses.Logger.Log("File Operation Stuff canceled by user");
                        break;
                    }

                    j++;
                    this.Dispatcher.Invoke(() =>
                    {
                        myPB.Value = (int)(j / count * 100);
                        myLBL.Content = Operation + "...(" + myPB.Value + "%)";
                    });
                }
                LauncherLogic.InFileOperationWrapperLoop = false;
                HelperClasses.Logger.Log("Done with File Operation Stuff");
            }


            else if (ProgressType == ProgressTypes.MD5)
            {
                if (HelperClasses.FileHandling.doesFileExist(FilePath))
                {
                    string filePath = FilePath;

                    byte[] buffer;
                    int bytesRead;
                    long size;
                    long totalBytesRead = 0;

                    using (Stream file = File.OpenRead(filePath))
                    {
                        size = file.Length;

                        using (HashAlgorithm hasher = MD5.Create())
                        {
                            do
                            {
                                buffer = new byte[4096];
                                bytesRead = file.Read(buffer, 0, buffer.Length);
                                totalBytesRead += bytesRead;
                                hasher.TransformBlock(buffer, 0, bytesRead, null, 0);

                                Application.Current.Dispatcher.Invoke((Action)delegate
                                {
                                    long progress = (100 * totalBytesRead / size);
                                    myPB.Value = progress;
                                    myLBL.Content = Operation + "...(" + progress + "%)";
                                });
                            }
                            while (bytesRead != 0);

                            hasher.TransformFinalBlock(buffer, 0, 0);

                            StringBuilder hash = new StringBuilder();
                            foreach (byte b in hasher.Hash)
                            {
                                hash.Append(b.ToString("X2"));
                            }
                            RtrnMD5 = hash.ToString().ToLower();
                        }
                    }
                    HelperClasses.Logger.Log("MD5 Hash (PopupProgress) of '" + FilePath + "' is: '" + RtrnMD5 + "'");
                }
                else
                {
                    HelperClasses.Logger.Log("MD5 Hash (PopupProgress) of '" + FilePath + "' not possible because file doesnt exist.");
                }
            }


            // Extracting a ZIPFile
            else if (ProgressType == ProgressTypes.ZIPFile)
            {
                HelperClasses.Logger.Log("PopupProgress ZipFileWeWannaExtract: '" + ZipFileWeWannaExtract + "'");
                HelperClasses.Logger.Log("PopupProgress ZIPExtractPath: '" + myZipExtractionPath + "'");

                List<System.IO.Compression.ZipArchiveEntry> fileList = new List<System.IO.Compression.ZipArchiveEntry>();
                var totalFiles = 0;
                var filesExtracted = 0;

                if (!HelperClasses.FileHandling.doesFileExist(ZipFileWeWannaExtract))
                {
                    HelperClasses.Logger.Log("ERROR. ZIP File we are extracting in Popup window doesnt exist..", true, 0);
                    PopupWrapper.PopupOk("ERROR. ZIP File we are extracting in Popup window doesnt exist..");
                    return;
                }

                try
                {
                    using (var archive = ZipFile.OpenRead(ZipFileWeWannaExtract))
                    {
                        totalFiles = archive.Entries.Count();

                        // Looping through all Files in the ZIPFile
                        foreach (var file in archive.Entries)
                        {
                            // If the File exists and is not a folder
                            if (!string.IsNullOrEmpty(file.Name))
                            {
                                bool doExtract = true;
                                string PathOnDisk = myZipExtractionPath.TrimEnd('\\') + @"\" + file.FullName.Replace(@"/", @"\");
                                HelperClasses.FileHandling.createPathOfFile(PathOnDisk); // 99% Chance I fixed this with the createZipPaths Method. Lets keep this to make sure...
                                if (HelperClasses.FileHandling.doesFileExist(PathOnDisk))
                                {
                                    if (PathOnDisk.Contains("UpgradeFiles"))
                                    //	PathOnDisk.Contains("SupportFiles"))
                                    //PathOnDisk.Contains(@"DowngradeFiles\GTA5.exe") ||
                                    //PathOnDisk.Contains(@"DowngradeFiles\x64a.rpf") ||
                                    //PathOnDisk.Contains(@"DowngradeFiles\update\update.rpf"))
                                    {
                                        doExtract = false;
                                    }
                                    else
                                    {
                                        HelperClasses.FileHandling.deleteFile(PathOnDisk);
                                    }
                                }

                                if (doExtract)
                                {
                                    file.ExtractToFile(PathOnDisk);
                                }
                            }

                            // Update GUI
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {
                                filesExtracted++;
                                long progress = (100 * filesExtracted / totalFiles);
                                myPB.Value = progress;
                                myLBL.Content = "Extracting ZIP...(" + progress + "%)";
                            });
                        }
                    }
                }
                catch (Exception e)
                {
                    HelperClasses.Logger.Log("TryCatch failed while extracting ZIP with progressbar." + e.ToString());
                    PopupWrapper.PopupError("trycatch failed while extracting zip with progressbar\n" + e.ToString());
                }
            }

            // Generating the list of MyFileOperations we need to do for a Downgrade
            else if (ProgressType == ProgressTypes.Downgrade)
            {
                bool UpdatePopupThrownAlready = false;

                // Saving all the File Operations I want to do, executing this at the end of this Method
                List<MyFileOperation> MyFileOperationsTmp = new List<MyFileOperation>();

                HelperClasses.Logger.Log("Initiating PopupProgress DOWNGRADE", 0);
                HelperClasses.Logger.Log("GTAV Installation Path: " + LauncherLogic.GTAVFilePath, 1);
                HelperClasses.Logger.Log("InstallationLocation: " + Globals.ProjectInstallationPath, 1);
                HelperClasses.Logger.Log("InstallationLocationBinary: " + Globals.ProjectInstallationPathBinary, 1);
                HelperClasses.Logger.Log("ZIP File Location: " + LauncherLogic.ZIPFilePath, 1);
                HelperClasses.Logger.Log("DowngradeFilePath: " + LauncherLogic.DowngradeFilePath, 1);
                HelperClasses.Logger.Log("UpgradeFilePath: " + LauncherLogic.UpgradeFilePath, 1);

                // Those are WITH the "\" at the end
                string[] FilesInDowngradeFiles = HelperClasses.FileHandling.GetFilesFromFolderAndSubFolder(LauncherLogic.DowngradeFilePath);
                string[] CorrespondingFilePathInGTALocation = new string[FilesInDowngradeFiles.Length];
                string[] CorrespondingFilePathInUpgradeFiles = new string[FilesInDowngradeFiles.Length];

                HelperClasses.Logger.Log("Found " + FilesInDowngradeFiles.Length.ToString() + " Files in Downgrade Folder.");
                HelperClasses.Logger.Log("Found " + HelperClasses.FileHandling.GetFilesFromFolderAndSubFolder(LauncherLogic.UpgradeFilePath).Length.ToString() + " Files in Upgrade Folder.");

                // Loop through all Files in Downgrade Files Folder
                for (int i = 0; i <= FilesInDowngradeFiles.Length - 1; i++)
                {
                    // Update GUI
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        long progress = ((i + 1) * 100 / FilesInDowngradeFiles.Length);
                        myPB.Value = progress;
                        myLBL.Content = "Gathering Information (" + (i + 1).ToString() + "/" + (FilesInDowngradeFiles.Length).ToString() + ")";
                    });


                    // Build the Corresponding theoretical Filenames for Upgrade Folder and GTA V Installation Folder
                    CorrespondingFilePathInGTALocation[i] = LauncherLogic.GTAVFilePath + FilesInDowngradeFiles[i].Substring(LauncherLogic.DowngradeFilePath.Length);
                    CorrespondingFilePathInUpgradeFiles[i] = LauncherLogic.UpgradeFilePath + FilesInDowngradeFiles[i].Substring(LauncherLogic.DowngradeFilePath.Length);

                    string tmpFileWePlace = FilesInDowngradeFiles[i].Substring(LauncherLogic.DowngradeFilePath.Length).TrimStart('\\');


                    if (LauncherLogic.IgnoreNewFilesWhileUpgradeDowngradeLogic)
                    {
                        // Move to $UpgradeFiles
                        MyFileOperationsTmp.Add(new MyFileOperation(MyFileOperation.FileOperations.Delete, CorrespondingFilePathInGTALocation[i], "", "Deleting '" + CorrespondingFilePathInGTALocation + "' from $GTAInstallationDirectory, since this is a simple Downgrade after using a Backup", 1));
                    }
                    // Normal Downgrade Logic
                    else
                    {
                        // If the File exists in GTA V Installation Path
                        if (HelperClasses.FileHandling.doesFileExist(CorrespondingFilePathInGTALocation[i]))
                        {
                            // If the File we are replacing is the same as in DowngradeFiles
                            if (HelperClasses.FileHandling.AreFilesEqual(CorrespondingFilePathInGTALocation[i], FilesInDowngradeFiles[i], MySettings.Settings.EnableSlowCompare))
                            {
                                // Delete from GTA V Installation Path 
                                MyFileOperationsTmp.Add(new MyFileOperation(MyFileOperation.FileOperations.Delete, CorrespondingFilePathInGTALocation[i], "", "Found '" + CorrespondingFilePathInGTALocation[i] + "' in GTA V Installation Path and its the same file as from $Downgrade_Files. Will delelte from GTA V Installation", 1));
                            }

                            // If the File we are replacing is the same as in UpgradeFiles
                            else if (HelperClasses.FileHandling.AreFilesEqual(CorrespondingFilePathInGTALocation[i], CorrespondingFilePathInUpgradeFiles[i], MySettings.Settings.EnableSlowCompare))
                            {
                                // Delete from GTA V Installation Path
                                MyFileOperationsTmp.Add(new MyFileOperation(MyFileOperation.FileOperations.Delete, CorrespondingFilePathInGTALocation[i], "", "Found '" + CorrespondingFilePathInGTALocation[i] + "' in GTA V Installation Path and its the same file as from $Upgrade_Files. Will delelte from GTA V Installation", 1));
                            }

                            // If the file we are replacing matches no file from UpgradeFiles or DowngradeFiles
                            else
                            {
                                // if gta5 is upgraded
                                if (BuildVersionTable.GetGameVersionOfBuild(Globals.GTABuild) > new Version(1, 30))
                                {
                                    // offer backup
                                    if (!UpdatePopupThrownAlready)
                                    {
                                        Application.Current.Dispatcher.Invoke((Action)delegate
                                        {
                                            bool yesno = PopupWrapper.PopupYesNo("Detected new Files inside your GTA Installation.\nP127 will use these as the files you revert to when Upgrading.\nDo you want me to back up your previous Upgrade - Files?");
                                            if (yesno == true)
                                            {
                                                HelperClasses.Logger.Log("User does want it. Initiating CreateBackup()");

                                                HelperClasses.ProcessHandler.KillRockstarProcesses();

                                                LauncherLogic.CreateBackup();
                                            }
                                            else
                                            {
                                                HelperClasses.Logger.Log("User doesnt want it. Alright then");
                                            }
                                        });
                                        UpdatePopupThrownAlready = true;
                                    }


                                    // If it exists in UpgradeFiles (but is an outdated Upgrade...)
                                    if (HelperClasses.FileHandling.doesFileExist(CorrespondingFilePathInUpgradeFiles[i]))
                                    {
                                        // Delte from $UpgradeFiles
                                        MyFileOperationsTmp.Add(new MyFileOperation(MyFileOperation.FileOperations.Delete, CorrespondingFilePathInUpgradeFiles[i], "", "We are overwriting a file which is not equal to the existing files in $Downgrade_Files and $Upgrade_Files. Deleting '" + CorrespondingFilePathInUpgradeFiles[i] + "' from $Upgrade_Files to use the existing File as new Backup", 1));
                                    }

                                    // Move to $UpgradeFiles
                                    Settings.AllFilesEverPlacedInsideGTAMyAdd(tmpFileWePlace);
                                    MyFileOperationsTmp.Add(new MyFileOperation(MyFileOperation.FileOperations.Move, CorrespondingFilePathInGTALocation[i], CorrespondingFilePathInUpgradeFiles[i], "Backing up '" + CorrespondingFilePathInGTALocation[i] + "' from GTA V Installation Path to $UpgradeFiles via Moving, since it either doenst exist there yet, or the file from $GTA_Installation_Path is a new one", 1));
                                }
                                else
                                {
                                    MyFileOperationsTmp.Add(new MyFileOperation(MyFileOperation.FileOperations.Delete, CorrespondingFilePathInGTALocation[i], "", "Deleting up '" + CorrespondingFilePathInGTALocation[i] + "' from GTA V Installation Path, since file matchings not $UpgradeFiles, not $DowngradeFiles and GTA from $GTA_DIR is NOT upgraded, so we dont want to move files to $UpgradeFiles", 1));
                                }
                            }
                        }
                    }

                    // Creates actual Hard Link (this will further down check if we should copy based on settings in MyFileOperation.Execute())
                    Settings.AllFilesEverPlacedInsideGTAMyAdd(tmpFileWePlace);
                    MyFileOperationsTmp.Add(new MyFileOperation(MyFileOperation.FileOperations.Hardlink, FilesInDowngradeFiles[i], CorrespondingFilePathInGTALocation[i], "Will create HardLink in '" + CorrespondingFilePathInGTALocation[i] + "' to the file in '" + FilesInDowngradeFiles[i] + "'", 1));
                }

                HelperClasses.ProcessHandler.KillRockstarProcesses();

                RtrnMyFileOperations = MyFileOperationsTmp;
            }

            // Generating the list of MyFileOperations we need to do for a Upgrade
            else if (ProgressType == ProgressTypes.Upgrade)
            {
                // Saving all the File Operations I want to do, executing this at the end of this Method
                List<MyFileOperation> MyFileOperationsTmp = new List<MyFileOperation>();
                bool UpdatePopupThrownAlready = false;

                HelperClasses.Logger.Log("Initiating PopupProgress UPGRADE", 0);
                HelperClasses.Logger.Log("GTAV Installation Path: " + LauncherLogic.GTAVFilePath, 1);
                HelperClasses.Logger.Log("InstallationLocation: " + Globals.ProjectInstallationPath, 1);
                HelperClasses.Logger.Log("InstallationLocationBinary: " + Globals.ProjectInstallationPathBinary, 1);
                HelperClasses.Logger.Log("ZIP File Location: " + LauncherLogic.ZIPFilePath, 1);
                HelperClasses.Logger.Log("DowngradeFilePath: " + LauncherLogic.DowngradeFilePath, 1);
                HelperClasses.Logger.Log("UpgradeFilePath: " + LauncherLogic.UpgradeFilePath, 1);

                // Those are WITH the "\" at the end
                List<string> FilesInDowngradeAndUpgradePathInDowngradedPathFormat = HelperClasses.FileHandling.GetFilesFromFolderAndSubFolder(LauncherLogic.DowngradeFilePath).ToList();

                List<string> ListOfFilePathsAboutToBeInUpgradeFiles = new List<string>();

                HelperClasses.Logger.Log("Found " + FilesInDowngradeAndUpgradePathInDowngradedPathFormat.Count.ToString() + " Files in Downgrade Folder.");
                HelperClasses.Logger.Log("Found " + HelperClasses.FileHandling.GetFilesFromFolderAndSubFolder(LauncherLogic.UpgradeFilePath).Length.ToString() + " Files in Upgrade Folder.");

                // We need to loop through Downgrade AND Upgrade Files here...
                foreach (string tmp in HelperClasses.FileHandling.GetFilesFromFolderAndSubFolder(LauncherLogic.UpgradeFilePath))
                {
                    string CorrPathInDowngrades = LauncherLogic.DowngradeFilePath + tmp.Substring(LauncherLogic.UpgradeFilePath.Length);
                    if (!FilesInDowngradeAndUpgradePathInDowngradedPathFormat.Contains(CorrPathInDowngrades))
                    {
                        FilesInDowngradeAndUpgradePathInDowngradedPathFormat.Add(CorrPathInDowngrades);
                    }
                }

                string[] CorrespondingFilePathInGTALocation = new string[FilesInDowngradeAndUpgradePathInDowngradedPathFormat.Count];
                string[] CorrespondingFilePathInUpgradeFiles = new string[FilesInDowngradeAndUpgradePathInDowngradedPathFormat.Count];


                // Loop through all Files in Downgrade Files Folder
                for (int i = 0; i <= FilesInDowngradeAndUpgradePathInDowngradedPathFormat.Count - 1; i++)
                {
                    // Update GUI
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        long progress = ((i + 1) * 100 / FilesInDowngradeAndUpgradePathInDowngradedPathFormat.Count);
                        myPB.Value = progress;
                        myLBL.Content = "Gathering Information (" + (i + 1).ToString() + "/" + (FilesInDowngradeAndUpgradePathInDowngradedPathFormat.Count).ToString() + ")";
                    });


                    // Build the Corresponding theoretical Filenames for Upgrade Folder and GTA V Installation Folder
                    CorrespondingFilePathInGTALocation[i] = LauncherLogic.GTAVFilePath + FilesInDowngradeAndUpgradePathInDowngradedPathFormat[i].Substring(LauncherLogic.DowngradeFilePath.Length);
                    CorrespondingFilePathInUpgradeFiles[i] = LauncherLogic.UpgradeFilePath + FilesInDowngradeAndUpgradePathInDowngradedPathFormat[i].Substring(LauncherLogic.DowngradeFilePath.Length);
                    string tmpFileWePlace = FilesInDowngradeAndUpgradePathInDowngradedPathFormat[i].Substring(LauncherLogic.DowngradeFilePath.Length).TrimStart('\\');


                    if (LauncherLogic.IgnoreNewFilesWhileUpgradeDowngradeLogic)
                    {
                        // Move to $UpgradeFiles
                        MyFileOperationsTmp.Add(new MyFileOperation(MyFileOperation.FileOperations.Delete, CorrespondingFilePathInGTALocation[i], "", "Deleting '" + CorrespondingFilePathInGTALocation[i] + "' from $GTAInstallationDirectory, since this is a simple Downgrade after using a Backup", 1));
                    }
                    // Normal Downgrade Logic
                    else
                    {
                        // If the File exists in GTA V Installation Path
                        if (HelperClasses.FileHandling.doesFileExist(CorrespondingFilePathInGTALocation[i]))
                        {
                            // If the File we are replacing is the same as in DowngradeFiles
                            if (HelperClasses.FileHandling.AreFilesEqual(CorrespondingFilePathInGTALocation[i], FilesInDowngradeAndUpgradePathInDowngradedPathFormat[i], MySettings.Settings.EnableSlowCompare))
                            {
                                // Delete from GTA V Installation Path
                                MyFileOperationsTmp.Add(new MyFileOperation(MyFileOperation.FileOperations.Delete, CorrespondingFilePathInGTALocation[i], "", "Found '" + CorrespondingFilePathInGTALocation[i] + "' in GTA V Installation Path and its the same file as from $Downgrade_Files. Will delelte from GTA V Installation", 1));
                            }

                            // If the File we are replacing is the same as in UpgradeFiles
                            else if (HelperClasses.FileHandling.AreFilesEqual(CorrespondingFilePathInGTALocation[i], CorrespondingFilePathInUpgradeFiles[i], MySettings.Settings.EnableSlowCompare))
                            {
                                // Delete from GTA V Installation Path
                                MyFileOperationsTmp.Add(new MyFileOperation(MyFileOperation.FileOperations.Delete, CorrespondingFilePathInGTALocation[i], "", "Found '" + CorrespondingFilePathInGTALocation[i] + "' in GTA V Installation Path and its the same file as from $Upgrade_Files. Will delelte from GTA V Installation", 1));
                            }

                            // If the file inside GTA Installation Directory matches no file from UpgradeFiles or DowngradeFiles
                            else
                            {
                                // if gta5 is upgraded
                                if (BuildVersionTable.GetGameVersionOfBuild(Globals.GTABuild) > new Version(1, 30))
                                {
                                    // offer backup
                                    if (!UpdatePopupThrownAlready)
                                    {
                                        Application.Current.Dispatcher.Invoke((Action)delegate
                                        {
                                            bool yesno = PopupWrapper.PopupYesNo("Detected new Files inside your GTA Installation.\nP127 will use these as the files you revert to when Upgrading.\nDo you want me to back up your previous Upgrade - Files?");
                                            if (yesno == true)
                                            {
                                                HelperClasses.Logger.Log("User does want it. Initiating CreateBackup()");

                                                HelperClasses.ProcessHandler.KillRockstarProcesses();

                                                LauncherLogic.CreateBackup();
                                            }
                                            else
                                            {
                                                HelperClasses.Logger.Log("User doesnt want it. Alright then");
                                            }
                                        });
                                        UpdatePopupThrownAlready = true;
                                    }


                                    // If it exists in UpgradeFiles (but is an outdated Upgrade...)
                                    if (HelperClasses.FileHandling.doesFileExist(CorrespondingFilePathInUpgradeFiles[i]))
                                    {
                                        // Delte from $UpgradeFiles
                                        MyFileOperationsTmp.Add(new MyFileOperation(MyFileOperation.FileOperations.Delete, CorrespondingFilePathInUpgradeFiles[i], "", "We are overwriting a file which is not equal to the existing files in $Downgrade_Files and $Upgrade_Files. Deleting '" + CorrespondingFilePathInUpgradeFiles[i] + "' from $Upgrade_Files to use the existing File as new Backup", 1));
                                    }

                                    // Move to $UpgradeFiles
                                    Settings.AllFilesEverPlacedInsideGTAMyAdd(tmpFileWePlace);
                                    MyFileOperationsTmp.Add(new MyFileOperation(MyFileOperation.FileOperations.Move, CorrespondingFilePathInGTALocation[i], CorrespondingFilePathInUpgradeFiles[i], "Backing up '" + CorrespondingFilePathInGTALocation[i] + "' from GTA V Installation Path to $UpgradeFiles via Moving, since it either doenst exist there yet, or the file from $GTA_Installation_Path is a new one", 1));
                                    ListOfFilePathsAboutToBeInUpgradeFiles.Add(CorrespondingFilePathInUpgradeFiles[i]);
                                }
                                else
                                {
                                    MyFileOperationsTmp.Add(new MyFileOperation(MyFileOperation.FileOperations.Delete, CorrespondingFilePathInGTALocation[i], "", "Deleting up '" + CorrespondingFilePathInGTALocation[i] + "' from GTA V Installation Path, since file matchings not $UpgradeFiles, not $DowngradeFiles and GTA from $GTA_DIR is NOT upgraded, so we dont want to move files to $UpgradeFiles", 1));
                                }
                            }
                        }
                    }


                    // file we are moving there if file inside GTA matches no files is non existing at this point
                    Settings.AllFilesEverPlacedInsideGTAMyAdd(tmpFileWePlace);
                    if (HelperClasses.FileHandling.doesFileExist(CorrespondingFilePathInUpgradeFiles[i]) || ListOfFilePathsAboutToBeInUpgradeFiles.Contains(CorrespondingFilePathInUpgradeFiles[i]))
                    {
                        // Creates actual Hard Link (this will further down check if we should copy based on settings in MyFileOperation.Execute())
                        MyFileOperationsTmp.Add(new MyFileOperation(MyFileOperation.FileOperations.Hardlink, CorrespondingFilePathInUpgradeFiles[i], CorrespondingFilePathInGTALocation[i], "Will create HardLink in '" + CorrespondingFilePathInGTALocation[i] + "' to the file in '" + CorrespondingFilePathInUpgradeFiles[i] + "'", 1));
                    }

                }

                HelperClasses.ProcessHandler.KillRockstarProcesses();

                RtrnMyFileOperations = MyFileOperationsTmp;
            }
        }

        /// <summary>
        /// Method which makes the Window draggable, which moves the whole window when holding down Mouse1 on the background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove(); // Pre-Defined Method
        }

    }
}
