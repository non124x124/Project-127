﻿using Project_127.HelperClasses;
using Project_127.MySettings;
using Project_127.Popups;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Project_127.LauncherLogic;

namespace Project_127
{
    partial class MainWindow
    {


        /// <summary>
        /// Method which gets called when Hamburger Menu Button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Hamburger_Click(object sender, RoutedEventArgs e)
        {
            // If is visible
            if (Globals.HamburgerMenuState == Globals.HamburgerMenuStates.Visible)
            {
                Globals.HamburgerMenuState = Globals.HamburgerMenuStates.Hidden;
            }
            // If is not visible
            else if (Globals.HamburgerMenuState == Globals.HamburgerMenuStates.Hidden)
            {
                Globals.HamburgerMenuState = Globals.HamburgerMenuStates.Visible;
            }
        }

        /// <summary>
        /// Rightclick on Hamburger Button
        /// </summary> 
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Hamburger_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Globals.P127Branch != "master")
            {
                // Opens the File
                HelperClasses.ProcessHandler.StartProcess(@"C:\Windows\System32\notepad.exe", pCommandLineArguments: Globals.Logfile);
            }
        }

        /// <summary>
        /// Middle Mouse on Hamburger Button
        /// </summary> 
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Hamburger_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
            {
                LauncherLogic.Launch();
            }
        }

        /// <summary>
        /// Method which gets called when the Auth Button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Auth_Click(object sender, RoutedEventArgs e)
        {
            LauncherLogic.AuthClick();

            // Yes this is correct
            SetButtonMouseOverMagic(btn_Auth);
        }


        /// <summary>
        /// Right click on Auth button. Gives proper Debug Output
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Auth_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            HelperClasses.Logger.GenerateDebug();
        }


        /// <summary>
        /// Method which gets called when the exit Button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Exit_Click(object sender, RoutedEventArgs e)
        {
            if (Globals.PageState == Globals.PageStates.GTA)
            {
                if (Settings.ExitWay == Settings.ExitWays.Close)
                {
                    bool yesno = PopupWrapper.PopupYesNo("Do you really want to quit?");
                    if (yesno == true)
                    {
                        Globals.ProperExit();
                    }
                }
                else if (Settings.ExitWay == Settings.ExitWays.HideInTray)
                {
                    MI_ExitToTray_Click(null, null);
                }
                else if (Settings.ExitWay == Settings.ExitWays.Minimize)
                {
                    MI_Minimize_Click(null, null);
                }
            }
            else
            {
                Globals.PageState = Globals.PageStates.GTA;
            }
        }

        /// <summary>
        /// Right Mouse Button Down on Exit forces Close instantly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Exit_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            GenerateExitRightClickContextMenu();
        }







        /// <summary>
        /// Method which gets called when the Update Button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Upgrade_Click(object sender, RoutedEventArgs e)
        {
            // Confirmation Popup
            bool conf = PopupWrapper.PopupYesNo("Do you want to Upgrade?");
            if (conf == false)
            {
                return;
            }

            // Actual Upgrade Button Code
            HelperClasses.Logger.Log("Clicked the Upgrade Button");

            LauncherLogic.Upgrade();

            // Call Update GUI Method
            UpdateGUIDispatcherTimer();
        }


        /// <summary>
        /// Shoutouts to crapideot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_Upgrade_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // LaunchAlternative.SocialClubUpgrade(0,"manually clicked");
        }


        /// <summary>
        /// Method which gets called when the Downgrade Button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Downgrade_Click(object sender, RoutedEventArgs e)
        {
            // Confirmation Popup
            bool conf = PopupWrapper.PopupYesNo("Do you want to Downgrade?");
            if (conf == false)
            {
                return;
            }

            // Actual Upgrade Button Code
            HelperClasses.Logger.Log("Clicked the Downgrade Button");

            LauncherLogic.Downgrade();

            UpdateGUIDispatcherTimer();
        }


        /// <summary>
        /// Shoutouts to crapideot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Downgrade_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // LaunchAlternative.SocialClubDowngrade(0, "manually clicked");
        }


        /// <summary>
        /// Method which gets called when the SaveFileHandler Button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_SaveFiles_Click(object sender, RoutedEventArgs e)
        {
            if (Globals.PageState == Globals.PageStates.SaveFileHandler)
            {
                Globals.PageState = Globals.PageStates.GTA;
            }
            else
            {
                Globals.PageState = Globals.PageStates.SaveFileHandler;
            }

        }


        private void btn_SaveFiles_MouseRightButtonDown(object sender, RoutedEventArgs e)
        {
            //new HelperClasses.SocialClubDebug().Show();
            //HelperClasses.FileHandling.AddToDebug("Manual GetState, currState:");
            //HelperClasses.FileHandling.AddToDebug("");
        }

        private void btn_NoteOverlay_Click(object sender, RoutedEventArgs e)
        {
            if (Globals.PageState == Globals.PageStates.NoteOverlay)
            {
                Globals.PageState = Globals.PageStates.GTA;
            }
            else
            {
                Globals.PageState = Globals.PageStates.NoteOverlay;
            }
        }



        private void btn_ComponentManager_Click(object sender, RoutedEventArgs e)
        {
            if (Globals.PageState == Globals.PageStates.ComponentManager)
            {
                Globals.PageState = Globals.PageStates.GTA;
            }
            else
            {
                Globals.PageState = Globals.PageStates.ComponentManager;
            }
        }


        /// <summary>
        /// Method which gets called when the Settings Button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Settings_Click(object sender, RoutedEventArgs e)
        {
            if (Globals.PageState == Globals.PageStates.Settings)
            {
                Globals.PageState = Globals.PageStates.GTA;
            }
            else
            {
                Globals.PageState = Globals.PageStates.Settings;
            }
        }

        /// <summary>
        /// Method which gets called when you click on the ReadMe Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ReadMe_Click(object sender, RoutedEventArgs e)
        {
            if (Globals.PageState == Globals.PageStates.ReadMe)
            {
                Globals.PageState = Globals.PageStates.GTA;
            }
            else
            {
                Globals.PageState = Globals.PageStates.ReadMe;
            }
        }





        /// <summary>
        /// Static BTN GTA RightClick MouseRightDown
        /// </summary>
        public static void btn_GTA_MouseRightButtonDown_Static()
        {
            bool yesno = PopupWrapper.PopupYesNo("Do you want to close GTAV?");
            if (yesno == true)
            {
                HelperClasses.ProcessHandler.KillRockstarProcessesAsync();
            }
            //FocusManager.SetFocusedElement(this, null);
            MainWindow.MW.UpdateGUIDispatcherTimer();
        }



        /// <summary>
        /// Static BTN GTA Click
        /// </summary>
        public static async void btn_GTA_Click_Static()
        {
            if (LauncherLogic.GameState == LauncherLogic.GameStates.Running)
            {
                HelperClasses.Logger.Log("Game deteced running.", 1);
                btn_GTA_MouseRightButtonDown_Static();
                return;
            }
            else if (LauncherLogic.GameState == LauncherLogic.GameStates.NonRunning)
            {
                HelperClasses.Logger.Log("User wantst to Launch", 1);
                LauncherLogic.Launch();
            }
            else
            {
                await HandleStuckGTAHailMary(true);
            }
            //FocusManager.SetFocusedElement(this, null);
            MainWindow.MW.UpdateGUIDispatcherTimer();
        }



        /// <summary>
        /// Mode / Branch thingy rightclick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_lbl_Mode_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Settings.P127Mode.ToLower() != "default")
            {
                if (e.ClickCount >= 3)
                {
                    PopupWrapper.PopupMode();
                }
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
