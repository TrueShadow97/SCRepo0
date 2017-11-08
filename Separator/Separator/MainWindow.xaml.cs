using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.Threading;
using System.Windows.Threading;

namespace Separator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    
    public enum ECommHealthDisplayMode
    {
        Indicators,
        Numbers
    }
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;
        public Dictionary<string, string> SelectedLib { get; private set; }
        private bool bControlEnginesFromScheme = false;
        Dictionary<string, TextBox> CustomLanguageTexts = new Dictionary<string, TextBox>();
        public List<string> LogQueue = new List<string>();
        private string FCControlBoxNameKey = "";
        private bool bWorkIndicationDebugMode { get; set; } = false;
        private bool bShowModbusDOFrames { get; set; } = false;
        private bool bShowWorkIndicationVariables { get; set; } = false;

        private string LastChangedProperty { get; set; }
        private decimal LastChangedPropertyValue { get; set; }
        private Timer LastChangedPropertyLogTimer { get; set; }
        public ECommHealthDisplayMode CommHealthDisplayMode { get; set; } =
            ECommHealthDisplayMode.Numbers;

        public MainWindow()
        {
            Instance = this;
            Program.Init();
            LastChangedPropertyLogTimer = new Timer(LogLastPropertyChange, null,
                Timeout.Infinite, Timeout.Infinite);
            InitializeComponent();
            SetLanguage(ELanguage.Ukrainian);
            UpdateSettingsControls();
            MinHeight = 1024;
            imgBritish.Source = Settings.BritishFlag;
            imgRussian.Source = Settings.RussianFlag;
            imgUkrainian.Source = Settings.UkrainianFlag;
            imgEarth.Source = Settings.EarthPic;
            imgPELogo.Source = Settings.PELogo;
            imgAutoBackground.Source = Settings.AutoBackground;
            imgKBIcon.Source = Settings.KBIcon;
            imgAlarmImage.Source = Settings.AlarmIcon;
            RenderOptions.SetBitmapScalingMode(imgAutoBackground, BitmapScalingMode.HighQuality);
            imgAutoBackground.HorizontalAlignment = HorizontalAlignment.Left;
            imgSeparatorBackground.Source = Settings.SeparatorBackground;
            RenderOptions.SetBitmapScalingMode(imgSeparatorBackground,
                BitmapScalingMode.HighQuality);
            cmbCurrentSettingsSet.Text = "Default";
            cmbCurrentSettingsSet.Items.AddRange(Settings.SetList);
            btnAutoMode.Background = ActiveButtonBG;
            DefaultButtonBG = btnSettings.Background;
            LanguageSetupInit();
            if (Program.bComEnabled)
            {
                KillExplorer();
            }
        }

        private void LanguageSetupInit()
        {
            int CurrentRow = 1;
            foreach(KeyValuePair<string, string> KVP in MUI.EnglishLib)
            {
                grdLanguageSetupGrid.RowDefinitions.Add(new RowDefinition());
                var EnglishLabel = new Label();
                var RussianLabel = new Label();
                var UkrainianLabel = new Label();
                var CustomTextBox = new TextBox();
                grdLanguageSetupGrid.Children.Add(EnglishLabel);
                grdLanguageSetupGrid.Children.Add(RussianLabel);
                grdLanguageSetupGrid.Children.Add(UkrainianLabel);
                grdLanguageSetupGrid.Children.Add(CustomTextBox);
                EnglishLabel.Foreground = new SolidColorBrush(Colors.Lime);
                EnglishLabel.Background = new SolidColorBrush(Colors.Black);
                EnglishLabel.Margin = new Thickness(5);
                EnglishLabel.Content = MUI.EnglishLib[KVP.Key];
                Grid.SetRow(EnglishLabel, CurrentRow);
                Grid.SetColumn(EnglishLabel, 0);
                RussianLabel.Foreground = new SolidColorBrush(Colors.Lime);
                RussianLabel.Background = new SolidColorBrush(Colors.Black);
                RussianLabel.Margin = new Thickness(5);
                RussianLabel.Content = MUI.RussianLib[KVP.Key];
                Grid.SetRow(RussianLabel, CurrentRow);
                Grid.SetColumn(RussianLabel, 1);
                UkrainianLabel.Foreground = new SolidColorBrush(Colors.Lime);
                UkrainianLabel.Background = new SolidColorBrush(Colors.Black);
                UkrainianLabel.Margin = new Thickness(5);
                UkrainianLabel.Content = MUI.UkrainianLib[KVP.Key];
                Grid.SetRow(UkrainianLabel, CurrentRow);
                Grid.SetColumn(UkrainianLabel, 2);
                CustomTextBox.Foreground = new SolidColorBrush(Colors.Lime);
                CustomTextBox.Background = new SolidColorBrush(Colors.Black);
                CustomTextBox.Margin = new Thickness(5);
                CustomTextBox.Text = MUI.CustomLib[KVP.Key];
                Grid.SetRow(CustomTextBox, CurrentRow++);
                Grid.SetColumn(CustomTextBox, 3);
                CustomLanguageTexts.Add(KVP.Key, CustomTextBox);
            }
        }

        #region Interface Events
        Brush DefaultButtonBG = (new Button()).Background;
        Brush ActiveButtonBG = new LinearGradientBrush(new GradientStopCollection(
            new GradientStop[]
            {
                new GradientStop(Color.FromRgb(64, 255, 64), 0),
                new GradientStop(Color.FromRgb(64, 255, 64), 0.4),
                new GradientStop(Color.FromRgb(0, 192, 0), 0.6),
                new GradientStop(Color.FromRgb(0, 192, 0), 1)
            }), 90);
        private void btnAutoMode_Click(object sender, RoutedEventArgs e)
        {
            grdAutoModeGrid.Visibility = Visibility.Visible;
            grdManualModeGrid.Visibility = Visibility.Hidden;
            grdSettingsGrid.Visibility = Visibility.Hidden;
            btnAutoMode.Background = ActiveButtonBG;
            btnManualMode.Background = DefaultButtonBG;
            btnManualSchemeMode.Background = DefaultButtonBG;
            btnStartAuto.Visibility = Visibility.Visible;
            btnStopAuto.Visibility = Visibility.Visible;
            bControlEnginesFromScheme = false;
        }

        private void btnManualMode_Click(object sender, RoutedEventArgs e)
        {
            grdAutoModeGrid.Visibility = Visibility.Hidden;
            grdManualModeGrid.Visibility = Visibility.Visible;
            grdSettingsGrid.Visibility = Visibility.Hidden;
            btnAutoMode.Background = DefaultButtonBG;
            btnManualMode.Background = ActiveButtonBG;
            btnManualSchemeMode.Background = DefaultButtonBG;
            bControlEnginesFromScheme = true;
        }

        private void btnManualSchemeMode_Click(object sender, RoutedEventArgs e)
        {
            btnAutoMode.Background = DefaultButtonBG;
            btnManualMode.Background = DefaultButtonBG;
            btnManualSchemeMode.Background = ActiveButtonBG;
            grdAutoModeGrid.Visibility = Visibility.Visible;
            grdManualModeGrid.Visibility = Visibility.Hidden;
            grdSettingsGrid.Visibility = Visibility.Hidden;
            btnStartAuto.Visibility = Visibility.Collapsed;
            btnStopAuto.Visibility = Visibility.Collapsed;
            bControlEnginesFromScheme = true;
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            if(pwdAdminPassword.Password == Settings.AdminPassword || pwdAdminPassword.Password ==
                "" && Settings.AdminPassword == null)
            {
                grdAutoModeGrid.Visibility = Visibility.Hidden;
                grdManualModeGrid.Visibility = Visibility.Hidden;
                grdSettingsGrid.Visibility = Visibility.Visible;
                pwdAdminPassword.Password = "";
                Program.Log("Entered the settings menu as administrator", ELogType.Operator);
            }
        }

        private void UpdateCustomLib()
        {
            foreach(string Key in CustomLanguageTexts.Keys)
            {
                MUI.CustomLib[Key] = CustomLanguageTexts[Key].Text;
            }
        }

        private void btnSaveSet_Click(object sender, RoutedEventArgs e)
        {
            var SettingsSetList = new List<string>();
            var CurrentSet = cmbCurrentSettingsSet.Text;
            foreach (string str in cmbCurrentSettingsSet.Items)
            {
                SettingsSetList.Add(str);
            }
            new Thread(delegate()
            {
                if (SettingsSetList.Contains(CurrentSet))
                {
                    var Answer = MessageBox.Show(
                        SelectedLib["Do you really want to overwrite the "] +
                        CurrentSet + SelectedLib[" settings set"] + "?",
                        SelectedLib["Confirmation"], MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    if (Answer == MessageBoxResult.No)
                    {
                        return;
                    }
                }
                App.Current.Dispatcher.Invoke(() =>
                {
                    UpdateCustomLib();
                    if (!SettingsSetList.Contains(CurrentSet))
                    {
                        cmbCurrentSettingsSet.Items.Add(CurrentSet);
                    }
                    MUI.SaveLibs();
                    Settings.SaveSet(cmbCurrentSettingsSet.Text);
                    Settings.SaveAll();
                });
            }).Start();
        }

        private void btnLoadSet_Click(object sender, RoutedEventArgs e)
        {
            Settings.LoadSet(cmbCurrentSettingsSet.Text);
            UpdateSettingsControls();
        }

        private void btnDeleteSet_Click(object sender, RoutedEventArgs e)
        {
            var SettingsSetList = new List<string>();
            var CurrentSet = cmbCurrentSettingsSet.Text;
            foreach (string str in cmbCurrentSettingsSet.Items)
            {
                SettingsSetList.Add(str);
            }
            if(!SettingsSetList.Contains(CurrentSet) ||
                cmbCurrentSettingsSet.SelectedItem == null ||
                cmbCurrentSettingsSet.Items.Count < 2)
            {
                return;
            }
            var Index = cmbCurrentSettingsSet.SelectedIndex;
            Task.Factory.StartNew(() =>
            {
                var Answer = MessageBox.Show(
                    SelectedLib["Do you really want to delete the "] +
                    CurrentSet + SelectedLib[" settings set"] + "?",
                    SelectedLib["Confirmation"], MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (Answer == MessageBoxResult.No)
                {
                    return;
                }
                App.Current.Dispatcher.Invoke(() =>
                {
                    cmbCurrentSettingsSet.Items.Remove(cmbCurrentSettingsSet.SelectedItem);
                    Settings.DeleteSet(CurrentSet);
                    Settings.SaveAll();
                    Settings.LoadSet(cmbCurrentSettingsSet.Items[Index - 1].ToString());
                    UpdateSettingsControls();
                    cmbCurrentSettingsSet.Text = cmbCurrentSettingsSet.Items[Index - 1].
                        ToString();
                });
            });
        }

        private void btnStartAuto_Click(object sender, RoutedEventArgs e)
        {
            Program.Log("Pressed the Start All button", ELogType.Operator);
            Technology.StartAll();
        }

        private void btnStopAuto_Click(object sender, RoutedEventArgs e)
        {
            Program.Log("Pressed the Stop All button", ELogType.Operator);
            Technology.StopAll();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Program.Log("Pressed the Emergency Reset button", ELogType.Operator);
            Technology.ResetAll();
        }

        private void btnContentSettings_Click(object sender, RoutedEventArgs e)
        {
            stpContentSettingsPanel.Visibility = Visibility.Visible;
            scvLanguageSettingsViewer.Visibility = Visibility.Hidden;
            scvTechnologySettingsViewer.Visibility = Visibility.Hidden;
            scvCalibrationSettingsViewer.Visibility = Visibility.Hidden;
            lbxLogs.Visibility = Visibility.Hidden;
        }

        private void btnLanguageSettings_Click(object sender, RoutedEventArgs e)
        {
            stpContentSettingsPanel.Visibility = Visibility.Hidden;
            scvLanguageSettingsViewer.Visibility = Visibility.Visible;
            scvTechnologySettingsViewer.Visibility = Visibility.Hidden;
            scvCalibrationSettingsViewer.Visibility = Visibility.Hidden;
            lbxLogs.Visibility = Visibility.Hidden;
        }

        private void btnTechnologySettings_Click(object sender, RoutedEventArgs e)
        {
            stpContentSettingsPanel.Visibility = Visibility.Hidden;
            scvLanguageSettingsViewer.Visibility = Visibility.Hidden;
            scvTechnologySettingsViewer.Visibility = Visibility.Visible;
            scvCalibrationSettingsViewer.Visibility = Visibility.Hidden;
            lbxLogs.Visibility = Visibility.Hidden;
        }

        private void btnCalibrationSettings_Click(object sender, RoutedEventArgs e)
        {
            stpContentSettingsPanel.Visibility = Visibility.Hidden;
            scvLanguageSettingsViewer.Visibility = Visibility.Hidden;
            scvTechnologySettingsViewer.Visibility = Visibility.Hidden;
            scvCalibrationSettingsViewer.Visibility = Visibility.Visible;
            lbxLogs.Visibility = Visibility.Hidden;
        }

        private void btnLogs_Click(object sender, RoutedEventArgs e)
        {
            stpContentSettingsPanel.Visibility = Visibility.Hidden;
            scvLanguageSettingsViewer.Visibility = Visibility.Hidden;
            scvTechnologySettingsViewer.Visibility = Visibility.Hidden;
            scvCalibrationSettingsViewer.Visibility = Visibility.Hidden;
            lbxLogs.Visibility = Visibility.Visible;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private bool OutVoltage0ChangeLock = false; // recursion prevention
        private void OutVoltage0_Changed(object sender, TextChangedEventArgs e)
        {
            if(OutVoltage0ChangeLock)
            {
                return;
            }
            OutVoltage0ChangeLock = true;
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text.Replace(',', '.'), NumberStyles.Number,
                CultureInfo.InvariantCulture, out Res);
            if (Res != Technology.HVPSes[0].VoltageOut)
            {
                Technology.HVPSes[0].VoltageOut = Res;
                (sender as TextBox).Text = Res.ToString("F2", CultureInfo.InvariantCulture);
            }
            if(LastChangedProperty != "HVPS out voltage (kV)")
            {
                LogLastPropertyChange(null);
            }
            LastChangedProperty = "HVPS out voltage (kV)";
            LastChangedPropertyValue = Res;
            LastChangedPropertyLogTimer.Change(1000, Timeout.Infinite);
            try
            {
                scrOutputVoltage0.Value = (double)Res;
                if(CurrentFCIndex == 100)
                {
                    scrFrequencySetter.Value = (double)Res;
                }
            }
            catch { }
            OutVoltage0ChangeLock = false;
        }

        private bool OutVoltage1ChangeLock = false;
        private void OutVoltage1_Changed(object sender, TextChangedEventArgs e)
        {
            if(OutVoltage1ChangeLock)
            {
                return;
            }
            OutVoltage1ChangeLock = true;
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text.Replace(',', '.'), NumberStyles.Number,
                CultureInfo.InvariantCulture, out Res);
            if (Res != Technology.HVPSes[1].VoltageOut)
            {
                Technology.HVPSes[1].VoltageOut = Res;
                (sender as TextBox).Text = Res.ToString("F2", CultureInfo.InvariantCulture);
            }
            if (LastChangedProperty != "Vibrator out voltage (kV)")
            {
                LogLastPropertyChange(null);
            }
            LastChangedProperty = "Vibrator out voltage (kV)";
            LastChangedPropertyValue = Res;
            LastChangedPropertyLogTimer.Change(1000, Timeout.Infinite);
            try
            {
                scrOutputCurrent1.Value = (double)Res;
                if (CurrentFCIndex == 101)
                {
                    scrFrequencySetter.Value = (double)Res;
                }
            }
            catch { }
            OutVoltage1ChangeLock = false;
        }

        private bool Frequency0ChangeLock = false;
        private void Frequency0_Changed(object sender, TextChangedEventArgs e)
        {
            if(Frequency0ChangeLock)
            {
                return;
            }
            Frequency0ChangeLock = true;
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if(Res != Technology.FCs[0].CurrentFrequency)
            {
                Technology.FCs[0].CurrentFrequency = (int)Res;
                (sender as TextBox).Text = ((int)Res).ToString();
            }
            if (LastChangedProperty != "FC 0 frequency (Hz)")
            {
                LogLastPropertyChange(null);
            }
            LastChangedProperty = "FC 0 frequency (Hz)";
            LastChangedPropertyValue = (int)Res;
            LastChangedPropertyLogTimer.Change(1000, Timeout.Infinite);
            try
            {
                scrFrequency0.Value = (int)Res;
                if (CurrentFCIndex == 0)
                {
                    scrFrequencySetter.Value = (int)Res;
                }
            }
            catch { }
            Frequency0ChangeLock = false;
        }

        private bool Frequency1ChangeLock = false;
        private void Frequency1_Changed(object sender, TextChangedEventArgs e)
        {
            if(Frequency1ChangeLock)
            {
                return;
            }
            Frequency1ChangeLock = true;
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[1].CurrentFrequency)
            {
                Technology.FCs[1].CurrentFrequency = (int)Res;
                (sender as TextBox).Text = ((int)Res).ToString();
            }
            if (LastChangedProperty != "FC 1 frequency (Hz)")
            {
                LogLastPropertyChange(null);
            }
            LastChangedProperty = "FC 1 frequency (Hz)";
            LastChangedPropertyValue = (int)Res;
            LastChangedPropertyLogTimer.Change(1000, Timeout.Infinite);
            try
            {
                scrFrequency1.Value = (int)Res;
                if (CurrentFCIndex == 1)
                {
                    scrFrequencySetter.Value = (int)Res;
                }
            }
            catch { }
            Frequency1ChangeLock = false;
        }
        private bool Frequency2ChangeLock = false;
        private void Frequency2_Changed(object sender, TextChangedEventArgs e)
        {
            if(Frequency2ChangeLock)
            {
                return;
            }
            Frequency2ChangeLock = true;
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[2].CurrentFrequency)
            {
                Technology.FCs[2].CurrentFrequency = (int)Res;
                (sender as TextBox).Text = ((int)Res).ToString();
            }
            if (LastChangedProperty != "FC 2 frequency (Hz)")
            {
                LogLastPropertyChange(null);
            }
            LastChangedProperty = "FC 2 frequency (Hz)";
            LastChangedPropertyValue = (int)Res;
            LastChangedPropertyLogTimer.Change(1000, Timeout.Infinite);
            try
            {
                scrFrequency2.Value = (int)Res;
                if (CurrentFCIndex == 2)
                {
                    scrFrequencySetter.Value = (int)Res;
                }
            }
            catch { }
            Frequency2ChangeLock = false;
        }
        private bool Frequency3ChangeLock = false;
        private void Frequency3_Changed(object sender, TextChangedEventArgs e)
        {
            if(Frequency3ChangeLock)
            {
                return;
            }
            Frequency3ChangeLock = true;
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[3].CurrentFrequency)
            {
                Technology.FCs[3].CurrentFrequency = (int)Res;
                (sender as TextBox).Text = ((int)Res).ToString();
            }
            if (LastChangedProperty != "FC 3 frequency (Hz)")
            {
                LogLastPropertyChange(null);
            }
            LastChangedProperty = "FC 3 frequency (Hz)";
            LastChangedPropertyValue = (int)Res;
            LastChangedPropertyLogTimer.Change(1000, Timeout.Infinite);
            try
            {
                scrFrequency3.Value = (int)Res;
                if (CurrentFCIndex == 3)
                {
                    scrFrequencySetter.Value = (int)Res;
                }
            }
            catch { }
            Frequency3ChangeLock = false;
        }
        private bool Frequency4ChangeLock = false;
        private void Frequency4_Changed(object sender, TextChangedEventArgs e)
        {
            if(Frequency4ChangeLock)
            {
                return;
            }
            Frequency4ChangeLock = true;
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[4].CurrentFrequency)
            {
                Technology.FCs[4].CurrentFrequency = (int)Res;
                (sender as TextBox).Text = ((int)Res).ToString();
            }
            if (LastChangedProperty != "FC 4 frequency (Hz)")
            {
                LogLastPropertyChange(null);
            }
            LastChangedProperty = "FC 4 frequency (Hz)";
            LastChangedPropertyValue = (int)Res;
            LastChangedPropertyLogTimer.Change(1000, Timeout.Infinite);
            try
            {
                scrFrequency4.Value = (int)Res;
                if (CurrentFCIndex == 4)
                {
                    scrFrequencySetter.Value = (int)Res;
                }
            }
            catch { }
            Frequency4ChangeLock = false;
        }
        private bool Frequency5ChangeLock = false;
        private void Frequency5_Changed(object sender, TextChangedEventArgs e)
        {
            if(Frequency5ChangeLock)
            {
                return;
            }
            Frequency5ChangeLock = true;
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[5].CurrentFrequency)
            {
                Technology.FCs[5].CurrentFrequency = (int)Res;
                (sender as TextBox).Text = ((int)Res).ToString();
            }
            if (LastChangedProperty != "FC 5 frequency (Hz)")
            {
                LogLastPropertyChange(null);
            }
            LastChangedProperty = "FC 5 frequency (Hz)";
            LastChangedPropertyValue = (int)Res;
            LastChangedPropertyLogTimer.Change(1000, Timeout.Infinite);
            try
            {
                scrFrequency5.Value = (int)Res;
                if (CurrentFCIndex == 5)
                {
                    scrFrequencySetter.Value = (int)Res;
                }
            }
            catch { }
            Frequency5ChangeLock = false;
        }
        private bool Frequency6ChangeLock = false;
        private void Frequency6_Changed(object sender, TextChangedEventArgs e)
        {
            if(Frequency6ChangeLock)
            {
                return;
            }
            Frequency6ChangeLock = true;
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[6].CurrentFrequency)
            {
                Technology.FCs[6].CurrentFrequency = (int)Res;
                (sender as TextBox).Text = ((int)Res).ToString();
            }
            if (LastChangedProperty != "FC 6 frequency (Hz)")
            {
                LogLastPropertyChange(null);
            }
            LastChangedProperty = "FC 6 frequency (Hz)";
            LastChangedPropertyValue = (int)Res;
            LastChangedPropertyLogTimer.Change(1000, Timeout.Infinite);
            try
            {
                scrFrequency6.Value = (int)Res;
                if (CurrentFCIndex == 6)
                {
                    scrFrequencySetter.Value = (int)Res;
                }
            }
            catch { }
            Frequency6ChangeLock = false;
        }
        private void Frequency7_Changed(object sender, TextChangedEventArgs e)
        {
            int Res = 0;
            int.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[7].CurrentFrequency)
            {
                Technology.FCs[7].CurrentFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
        }
        private void Frequency8_Changed(object sender, TextChangedEventArgs e)
        {
            int Res = 0;
            int.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[8].CurrentFrequency)
            {
                Technology.FCs[8].CurrentFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
        }

        private void HVPS0_SwitchState(object sender, RoutedEventArgs e)
        {
            Technology.HVPSes[0].SwitchState();
            Technology.HVPSes[0].ForceState();
            Program.Log("Switched the state of " +
                Technology.HVPSes[0].Name, ELogType.Operator);
            if (!Technology.HVPSes[0].bPowerState && !Program.bUnlimitedManual)
            {
                if (Technology.Contactors[8].bInitialState)
                    Contactor8_SwitchState(null, null);
                if (Technology.FCs[0].bPowerState)
                    FC0_SwitchState(null, null);
                if (Technology.FCs[1].bInitialState)
                    FC1_SwitchState(null, null);
                if (Technology.FCs[2].bPowerState)
                    FC2_SwitchState(null, null);
                if (Technology.FCs[4].bInitialState)
                    FC4_SwitchState(null, null);
                if (Technology.Contactors[9].bInitialState)
                    Contactor9_SwitchState(null, null);
                if (Technology.FCs[3].bInitialState)
                    FC3_SwitchState(null, null);
                if (Technology.FCs[6].bInitialState)
                    FC6_SwitchState(null, null);
            }
        }

        private void HVPS1_SwitchState(object sender, RoutedEventArgs e)
        {
            Technology.HVPSes[1].SwitchState();
            Technology.HVPSes[1].ForceState();
            Program.Log("Switched the state of " +
                Technology.HVPSes[1].Name, ELogType.Operator);
            if (!Technology.HVPSes[1].bPowerState && !Program.bUnlimitedManual)
            {
                if (Technology.Contactors[8].bInitialState)
                    Contactor8_SwitchState(null, null);
                if (Technology.FCs[0].bPowerState)
                    FC0_SwitchState(null, null);
                if (Technology.FCs[1].bInitialState)
                    FC1_SwitchState(null, null);
                if (Technology.FCs[2].bPowerState)
                    FC2_SwitchState(null, null);
                if (Technology.FCs[4].bInitialState)
                    FC4_SwitchState(null, null);
                if (Technology.Contactors[9].bInitialState)
                    Contactor9_SwitchState(null, null);
                if (Technology.FCs[3].bInitialState)
                    FC3_SwitchState(null, null);
                if (Technology.FCs[6].bInitialState)
                    FC6_SwitchState(null, null);
            }
        }

        private void FC0_SwitchState(object sender, RoutedEventArgs e)
        {
            if (!Program.bUnlimitedManual && Technology.Contactors[8].bPowerState ||
                Technology.FCs[0].bPowerState ||
                Program.bUnlimitedManual)
            {
                Technology.FCs[0].SwitchState();
                Technology.FCs[0].ForceState();
                Program.Log("Switched the state of " +
                    Technology.FCs[0].Name, ELogType.Operator);
            }
        }

        private void FC1_SwitchState(object sender, RoutedEventArgs e)
        {
            if (!Program.bUnlimitedManual && Technology.FCs[2].bPowerState ||
                Technology.FCs[1].bPowerState ||
                Program.bUnlimitedManual)
            {
                Technology.FCs[1].SwitchState();
                Technology.FCs[1].ForceState();
                Program.Log("Switched the state of " +
                    Technology.FCs[1].Name, ELogType.Operator);
                if (!Technology.FCs[1].bPowerState && !Program.bUnlimitedManual)
                {
                    if (Technology.Contactors[8].bInitialState)
                        Contactor8_SwitchState(null, null);
                    if (Technology.FCs[0].bPowerState)
                        FC0_SwitchState(null, null);
                }
            }
        }

        private void FC2_SwitchState(object sender, RoutedEventArgs e)
        {
            if (!Program.bUnlimitedManual && Technology.FCs[4].bPowerState &&
                Technology.Contactors[9].bPowerState ||
                Technology.FCs[2].bPowerState ||
                Program.bUnlimitedManual)
            {
                Technology.FCs[2].SwitchState();
                Technology.FCs[2].ForceState();
                Program.Log("Switched the state of " +
                    Technology.FCs[2].Name, ELogType.Operator);
                if (!Technology.FCs[2].bPowerState && !Program.bUnlimitedManual)
                {
                    if (Technology.Contactors[8].bInitialState)
                        Contactor8_SwitchState(null, null);
                    if (Technology.FCs[0].bPowerState)
                        FC0_SwitchState(null, null);
                    if (Technology.FCs[1].bInitialState)
                        FC1_SwitchState(null, null);
                }
            }
        }

        private void FC3_SwitchState(object sender, RoutedEventArgs e)
        {
            if (!Program.bUnlimitedManual && Technology.FCs[6].bPowerState ||
                Technology.FCs[3].bPowerState ||
                Program.bUnlimitedManual)
            {
                Technology.FCs[3].SwitchState();
                Technology.FCs[3].ForceState();
                Program.Log("Switched the state of " +
                    Technology.FCs[3].Name, ELogType.Operator);
                if (!Technology.FCs[3].bPowerState && !Program.bUnlimitedManual)
                {
                    if (Technology.Contactors[8].bInitialState)
                        Contactor8_SwitchState(null, null);
                    if (Technology.FCs[0].bPowerState)
                        FC0_SwitchState(null, null);
                    if (Technology.FCs[1].bInitialState)
                        FC1_SwitchState(null, null);
                    if (Technology.FCs[2].bPowerState)
                        FC2_SwitchState(null, null);
                    if (Technology.FCs[4].bInitialState)
                        FC4_SwitchState(null, null);
                    if (Technology.Contactors[9].bInitialState)
                        Contactor9_SwitchState(null, null);
                }
            }
        }

        private void FC4_SwitchState(object sender, RoutedEventArgs e)
        {
            if (!Program.bUnlimitedManual && Technology.FCs[3].bPowerState ||
                Technology.FCs[4].bPowerState ||
                Program.bUnlimitedManual)
            {
                Technology.FCs[4].SwitchState();
                Technology.FCs[4].ForceState();
                Program.Log("Switched the state of " +
                    Technology.FCs[4].Name, ELogType.Operator);
                if (!Technology.FCs[4].bPowerState && !Program.bUnlimitedManual)
                {
                    if (Technology.Contactors[8].bInitialState)
                        Contactor8_SwitchState(null, null);
                    if (Technology.FCs[0].bPowerState)
                        FC0_SwitchState(null, null);
                    if (Technology.FCs[1].bInitialState)
                        FC1_SwitchState(null, null);
                    if (Technology.FCs[2].bInitialState)
                        FC2_SwitchState(null, null);
                }
            }
        }

        private void FC5_SwitchState(object sender, RoutedEventArgs e)
        {
            Technology.FCs[5].SwitchState();
            Technology.FCs[5].ForceState();
            Program.Log("Switched the state of " +
                Technology.FCs[5].Name, ELogType.Operator);
            if (!Technology.FCs[5].bPowerState && !Program.bUnlimitedManual)
            {
                if (Technology.Contactors[8].bInitialState)
                    Contactor8_SwitchState(null, null);
                if (Technology.FCs[0].bPowerState)
                    FC0_SwitchState(null, null);
                if (Technology.FCs[1].bInitialState)
                    FC1_SwitchState(null, null);
                if (Technology.FCs[2].bPowerState)
                    FC2_SwitchState(null, null);
                if (Technology.FCs[4].bInitialState)
                    FC4_SwitchState(null, null);
                if (Technology.Contactors[9].bInitialState)
                    Contactor9_SwitchState(null, null);
                if (Technology.FCs[3].bInitialState)
                    FC3_SwitchState(null, null);
                if (Technology.FCs[6].bInitialState)
                    FC6_SwitchState(null, null);
            }
        }

        private void FC6_SwitchState(object sender, RoutedEventArgs e)
        {
            if (!Program.bUnlimitedManual && Technology.HVPSes[0].bPowerState &&
                Technology.FCs[5].bPowerState &&
                Technology.HVPSes[1].bPowerState &&
                Technology.Contactors[2].bPowerState &&
                Technology.Contactors[3].bPowerState &&
                Technology.Contactors[4].bPowerState &&
                Technology.Contactors[5].bPowerState &&
                Technology.Contactors[6].bPowerState ||
                Technology.FCs[6].bPowerState ||
                Program.bUnlimitedManual)
            {
                Technology.FCs[6].SwitchState();
                Technology.FCs[6].ForceState();
                Program.Log("Switched the state of " +
                    Technology.FCs[6].Name, ELogType.Operator);
                if(!Technology.FCs[6].bPowerState && !Program.bUnlimitedManual)
                {
                    if (Technology.Contactors[8].bInitialState)
                        Contactor8_SwitchState(null, null);
                    if (Technology.FCs[0].bPowerState)
                        FC0_SwitchState(null, null);
                    if (Technology.FCs[1].bInitialState)
                        FC1_SwitchState(null, null);
                    if (Technology.FCs[2].bPowerState)
                        FC2_SwitchState(null, null);
                    if (Technology.FCs[4].bInitialState)
                        FC4_SwitchState(null, null);
                    if (Technology.Contactors[9].bInitialState)
                        Contactor9_SwitchState(null, null);
                    if (Technology.FCs[3].bInitialState)
                        FC3_SwitchState(null, null); 
                }
            }
        }

        private void FC7_SwitchState(object sender, MouseButtonEventArgs e)
        {
            Technology.FCs[7].SwitchState();
            Technology.FCs[7].ForceState();
        }

        private void FC8_SwitchState(object sender, MouseButtonEventArgs e)
        {
            Technology.FCs[8].SwitchState();
            Technology.FCs[8].ForceState();
        }

        private void Contactor0_SwitchState(object sender, RoutedEventArgs e)
        {
            if (!bControlEnginesFromScheme)
            {
                return;
            }
            Technology.Contactors[0].SwitchState();
            Technology.Contactors[0].ForceState();
            Program.Log("Switched the state of " +
                Technology.Contactors[0].Name, ELogType.Operator);
            if (!Technology.Contactors[0].bPowerState && !Program.bUnlimitedManual)
            {
                if (Technology.Contactors[8].bInitialState)
                    Contactor8_SwitchState(null, null);
                if (Technology.FCs[0].bPowerState)
                    FC0_SwitchState(null, null);
            }
        }

        private void Contactor1_SwitchState(object sender, RoutedEventArgs e)
        {
            if (!bControlEnginesFromScheme)
            {
                return;
            }
            Technology.Contactors[1].SwitchState();
            Technology.Contactors[1].ForceState();
            Program.Log("Switched the state of " +
                Technology.Contactors[1].Name, ELogType.Operator);
            if (!Technology.Contactors[1].bPowerState && !Program.bUnlimitedManual)
            {
                if (Technology.Contactors[8].bPowerState)
                    Contactor8_SwitchState(null, null);
                if (Technology.FCs[0].bPowerState)
                    FC0_SwitchState(null, null);
            }
        }

        private void Contactor2_SwitchState(object sender, RoutedEventArgs e)
        {
            if (!bControlEnginesFromScheme)
            {
                return;
            }
            Technology.Contactors[2].SwitchState();
            Technology.Contactors[2].ForceState();
            Program.Log("Switched the state of " +
                Technology.Contactors[2].Name, ELogType.Operator);
            if (!Technology.Contactors[2].bPowerState && !Program.bUnlimitedManual)
            {
                if (Technology.Contactors[8].bInitialState)
                    Contactor8_SwitchState(null, null);
                if (Technology.FCs[0].bPowerState)
                    FC0_SwitchState(null, null);
                if (Technology.FCs[1].bInitialState)
                    FC1_SwitchState(null, null);
                if (Technology.FCs[2].bPowerState)
                    FC2_SwitchState(null, null);
                if (Technology.FCs[4].bInitialState)
                    FC4_SwitchState(null, null);
                if (Technology.Contactors[9].bInitialState)
                    Contactor9_SwitchState(null, null);
                if (Technology.FCs[3].bInitialState)
                    FC3_SwitchState(null, null);
                if (Technology.FCs[6].bPowerState)
                    FC6_SwitchState(null, null);
            }
        }

        private void Contactor3_SwitchState(object sender, RoutedEventArgs e)
        {
            if (!bControlEnginesFromScheme)
            {
                return;
            }
            Technology.Contactors[3].SwitchState();
            Technology.Contactors[3].ForceState();
            Program.Log("Switched the state of " +
                Technology.Contactors[3].Name, ELogType.Operator);
            if (!Technology.Contactors[3].bPowerState && !Program.bUnlimitedManual)
            {
                if (Technology.Contactors[8].bInitialState)
                    Contactor8_SwitchState(null, null);
                if (Technology.FCs[0].bPowerState)
                    FC0_SwitchState(null, null);
                if (Technology.FCs[1].bInitialState)
                    FC1_SwitchState(null, null);
                if (Technology.FCs[2].bPowerState)
                    FC2_SwitchState(null, null);
                if (Technology.FCs[4].bInitialState)
                    FC4_SwitchState(null, null);
                if (Technology.Contactors[9].bInitialState)
                    Contactor9_SwitchState(null, null);
                if (Technology.FCs[3].bInitialState)
                    FC3_SwitchState(null, null);
                if (Technology.FCs[6].bPowerState)
                    FC6_SwitchState(null, null);
            }
        }

        private void Contactor4_SwitchState(object sender, RoutedEventArgs e)
        {
            if (!bControlEnginesFromScheme)
            {
                return;
            }
            Technology.Contactors[4].SwitchState();
            Technology.Contactors[4].ForceState();
            Program.Log("Switched the state of " +
                Technology.Contactors[4].Name, ELogType.Operator);
            if (!Technology.Contactors[4].bPowerState && !Program.bUnlimitedManual)
            {
                if (Technology.Contactors[8].bInitialState)
                    Contactor8_SwitchState(null, null);
                if (Technology.FCs[0].bPowerState)
                    FC0_SwitchState(null, null);
                if (Technology.FCs[1].bInitialState)
                    FC1_SwitchState(null, null);
                if (Technology.FCs[2].bPowerState)
                    FC2_SwitchState(null, null);
                if (Technology.FCs[4].bInitialState)
                    FC4_SwitchState(null, null);
                if (Technology.Contactors[9].bInitialState)
                    Contactor9_SwitchState(null, null);
                if (Technology.FCs[3].bInitialState)
                    FC3_SwitchState(null, null);
                if (Technology.FCs[6].bPowerState)
                    FC6_SwitchState(null, null);
            }
        }

        private void Contactor5_SwitchState(object sender, RoutedEventArgs e)
        {
            if (!bControlEnginesFromScheme)
            {
                return;
            }
            Technology.Contactors[5].SwitchState();
            Technology.Contactors[5].ForceState();
            Program.Log("Switched the state of " +
                Technology.Contactors[5].Name, ELogType.Operator);
            if (!Technology.Contactors[5].bPowerState && !Program.bUnlimitedManual)
            {
                if (Technology.Contactors[8].bInitialState)
                    Contactor8_SwitchState(null, null);
                if (Technology.FCs[0].bPowerState)
                    FC0_SwitchState(null, null);
                if (Technology.FCs[1].bInitialState)
                    FC1_SwitchState(null, null);
                if (Technology.FCs[2].bPowerState)
                    FC2_SwitchState(null, null);
                if (Technology.FCs[4].bInitialState)
                    FC4_SwitchState(null, null);
                if (Technology.Contactors[9].bInitialState)
                    Contactor9_SwitchState(null, null);
                if (Technology.FCs[3].bInitialState)
                    FC3_SwitchState(null, null);
                if (Technology.FCs[6].bPowerState)
                    FC6_SwitchState(null, null);
            }
        }

        private void Contactor6_SwitchState(object sender, RoutedEventArgs e)
        {
            if (!bControlEnginesFromScheme)
            {
                return;
            }
            Technology.Contactors[6].SwitchState();
            Technology.Contactors[6].ForceState();
            Program.Log("Switched the state of " +
                Technology.Contactors[6].Name, ELogType.Operator);
            if (!Technology.Contactors[6].bPowerState && !Program.bUnlimitedManual)
            {
                if (Technology.Contactors[8].bInitialState)
                    Contactor8_SwitchState(null, null);
                if (Technology.FCs[0].bPowerState)
                    FC0_SwitchState(null, null);
                if (Technology.FCs[1].bInitialState)
                    FC1_SwitchState(null, null);
                if (Technology.FCs[2].bPowerState)
                    FC2_SwitchState(null, null);
                if (Technology.FCs[4].bInitialState)
                    FC4_SwitchState(null, null);
                if (Technology.Contactors[9].bInitialState)
                    Contactor9_SwitchState(null, null);
                if (Technology.FCs[3].bInitialState)
                    FC3_SwitchState(null, null);
                if (Technology.FCs[6].bPowerState)
                    FC6_SwitchState(null, null);
            }
        }

        private void Contactor7_SwitchState(object sender, RoutedEventArgs e)
        {
            if (!bControlEnginesFromScheme)
            {
                return;
            }
            Technology.Contactors[7].SwitchState();
            Technology.Contactors[7].ForceState();
            Program.Log("Switched the state of " +
                Technology.Contactors[7].Name, ELogType.Operator);
        }

        private void Contactor8_SwitchState(object sender, RoutedEventArgs e)
        {
            if (!bControlEnginesFromScheme)
            {
                return;
            }
            if (!Program.bUnlimitedManual && Technology.FCs[1].bPowerState &&
                Technology.Contactors[0].bPowerState &&
                Technology.Contactors[1].bPowerState ||
                Technology.Contactors[8].bPowerState ||
                Program.bUnlimitedManual)
            {
                Technology.Contactors[8].SwitchState();
                Technology.Contactors[8].ForceState();
                Program.Log("Switched the state of " +
                    Technology.Contactors[8].Name, ELogType.Operator);
                if (!Technology.Contactors[8].bPowerState && !Program.bUnlimitedManual)
                {
                    if (Technology.FCs[0].bPowerState)
                        FC0_SwitchState(null, null);
                }
            }
        }

        private void Contactor9_SwitchState(object sender, RoutedEventArgs e)
        {
            if (!bControlEnginesFromScheme)
            {
                return;
            }
            if (!Program.bUnlimitedManual && Technology.FCs[3].bPowerState ||
                Technology.Contactors[9].bPowerState ||
                Program.bUnlimitedManual)
            {
                Technology.Contactors[9].SwitchState();
                Technology.Contactors[9].ForceState();
                Program.Log("Switched the state of " +
                    Technology.Contactors[9].Name, ELogType.Operator);
                if (!Technology.Contactors[9].bPowerState && !Program.bUnlimitedManual)
                {
                    if (Technology.Contactors[8].bInitialState)
                        Contactor8_SwitchState(null, null);
                    if (Technology.FCs[0].bPowerState)
                        FC0_SwitchState(null, null);
                    if (Technology.FCs[1].bInitialState)
                        FC1_SwitchState(null, null);
                    if (Technology.FCs[2].bPowerState)
                        FC2_SwitchState(null, null);
                }
            }
        }

        private void Cleaner0_SwitchState(object sender, MouseButtonEventArgs e)
        {
            Technology.Cleaners[0].SwitchState();
            Technology.Cleaners[1].SwitchState();
        }

        private void Cleaner1_SwitchState(object sender, MouseButtonEventArgs e)
        {
            Technology.Cleaners[0].SwitchState();
            Technology.Cleaners[1].SwitchState();
        }

        private void Cleaner0_Launch1(object sender, RoutedEventArgs e)
        {
            Technology.Cleaners[0].LaunchOnce(1);
            Technology.Cleaners[1].LaunchOnce(1);
        }

        private void Cleaner0_Launch2(object sender, RoutedEventArgs e)
        {
            Technology.Cleaners[0].LaunchOnce(2);
            Technology.Cleaners[1].LaunchOnce(2);
        }

        private void Cleaner1_Launch1(object sender, RoutedEventArgs e)
        {
            Technology.Cleaners[0].LaunchOnce(1);
            Technology.Cleaners[1].LaunchOnce(1);
        }

        private void Cleaner1_Launch2(object sender, RoutedEventArgs e)
        {
            Technology.Cleaners[0].LaunchOnce(2);
            Technology.Cleaners[1].LaunchOnce(2);
        }

        private void HVPS1Enabling(object sender, RoutedEventArgs e)
        {
            Technology.HVPSes[1].bEnabled = (sender as CheckBox).IsChecked.Value;
            Program.Log(MUI.EnglishLib["HVPS 1"] + " " + (Technology.HVPSes[1].bEnabled ?
                "enabled" : "disabled"), ELogType.Operator);
        }

        private void FC1Enabling(object sender, RoutedEventArgs e)
        {
            Technology.FCs[1].bEnabled = (sender as CheckBox).IsChecked.Value;
            Program.Log(MUI.EnglishLib["FC 1"] + " " + (Technology.FCs[1].bEnabled ?
                "enabled" : "disabled"), ELogType.Operator);
        }

        private void FC2Enabling(object sender, RoutedEventArgs e)
        {
            Technology.FCs[2].bEnabled = (sender as CheckBox).IsChecked.Value;
            Program.Log(MUI.EnglishLib["FC 2"] + " " + (Technology.FCs[2].bEnabled ?
                "enabled" : "disabled"), ELogType.Operator);
        }

        private void FC3Enabling(object sender, RoutedEventArgs e)
        {
            Technology.FCs[3].bEnabled = (sender as CheckBox).IsChecked.Value;
            Program.Log(MUI.EnglishLib["FC 3"] + " " + (Technology.FCs[3].bEnabled ?
                "enabled" : "disabled"), ELogType.Operator);
        }

        private void FC4Enabling(object sender, RoutedEventArgs e)
        {
            Technology.FCs[4].bEnabled = (sender as CheckBox).IsChecked.Value;
            Program.Log(MUI.EnglishLib["FC 4"] + " " + (Technology.FCs[4].bEnabled ?
                "enabled" : "disabled"), ELogType.Operator);
        }

        private void FC5Enabling(object sender, RoutedEventArgs e)
        {
            Technology.FCs[5].bEnabled = (sender as CheckBox).IsChecked.Value;
            Program.Log(MUI.EnglishLib["FC 5"] + " " + (Technology.FCs[5].bEnabled ?
                "enabled" : "disabled"), ELogType.Operator);
        }

        private void FC6Enabling(object sender, RoutedEventArgs e)
        {
            Technology.FCs[6].bEnabled = (sender as CheckBox).IsChecked.Value;
            Program.Log(MUI.EnglishLib["FC 6"] + " " + (Technology.FCs[6].bEnabled ?
                "enabled" : "disabled"), ELogType.Operator);
        }

        private void FC7Enabling(object sender, RoutedEventArgs e)
        {
            //TechnologyProcessLoop.FCs[7].bEnabled = (sender as CheckBox).IsChecked.Value;
        }

        private void FC8Enabling(object sender, RoutedEventArgs e)
        {
            //TechnologyProcessLoop.FCs[8].bEnabled = (sender as CheckBox).IsChecked.Value;
        }

        private void Cleaner0Enabling(object sender, RoutedEventArgs e)
        {
            //TechnologyProcessLoop.Cleaners[0].bEnabled = (sender as CheckBox).IsChecked.Value;
        }

        private void Cleaner1Enabling(object sender, RoutedEventArgs e)
        {
            //TechnologyProcessLoop.Cleaners[1].bEnabled = (sender as CheckBox).IsChecked.Value;
        }

        private void Contactor1Enabling(object sender, RoutedEventArgs e)
        {
            Technology.Contactors[1].bEnabled = (sender as CheckBox).IsChecked.Value;
            Program.Log(MUI.EnglishLib["Contactor 1"] + " " + (
                Technology.Contactors[1].bEnabled ? "enabled" : "disabled"), 
                ELogType.Operator);
        }

        private void Contactor2Enabling(object sender, RoutedEventArgs e)
        {
            Technology.Contactors[2].bEnabled = (sender as CheckBox).IsChecked.Value;
            Program.Log(MUI.EnglishLib["Contactor 2"] + " " + (
                Technology.Contactors[2].bEnabled ? "enabled" : "disabled"),
                ELogType.Operator);
        }

        private void Contactor3Enabling(object sender, RoutedEventArgs e)
        {
            Technology.Contactors[3].bEnabled = (sender as CheckBox).IsChecked.Value;
            Program.Log(MUI.EnglishLib["Contactor 2"] + " " + (
                Technology.Contactors[2].bEnabled ? "enabled" : "disabled"),
                ELogType.Operator);
        }

        private void Contactor4Enabling(object sender, RoutedEventArgs e)
        {
            Technology.Contactors[4].bEnabled = (sender as CheckBox).IsChecked.Value;
            Program.Log(MUI.EnglishLib["Contactor 4"] + " " + (
                Technology.Contactors[4].bEnabled ? "enabled" : "disabled"),
                ELogType.Operator);
        }

        private void Contactor5Enabling(object sender, RoutedEventArgs e)
        {
            Technology.Contactors[5].bEnabled = (sender as CheckBox).IsChecked.Value;
            Program.Log(MUI.EnglishLib["Contactor 5"] + " " + (
                Technology.Contactors[5].bEnabled ? "enabled" : "disabled"),
                ELogType.Operator);
        }

        private void Contactor6Enabling(object sender, RoutedEventArgs e)
        {
            Technology.Contactors[6].bEnabled = (sender as CheckBox).IsChecked.Value;
            Program.Log(MUI.EnglishLib["Contactor 6"] + " " + (
                Technology.Contactors[6].bEnabled ? "enabled" : "disabled"),
                ELogType.Operator);
        }

        private void Contactor7Enabling(object sender, RoutedEventArgs e)
        {
            Technology.Contactors[7].bEnabled = (sender as CheckBox).IsChecked.Value;
            Program.Log(MUI.EnglishLib["Contactor 7"] + " " + (
                Technology.Contactors[7].bEnabled ? "enabled" : "disabled"),
                ELogType.Operator);
        }

        private void Contactor8Enabling(object sender, RoutedEventArgs e)
        {
            Technology.Contactors[8].bEnabled = (sender as CheckBox).IsChecked.Value;
            Program.Log(MUI.EnglishLib["Contactor 8"] + " " + (
                Technology.Contactors[8].bEnabled ? "enabled" : "disabled"),
                ELogType.Operator);
        }

        private void Contactor9Enabling(object sender, RoutedEventArgs e)
        {
            Technology.Contactors[9].bEnabled = (sender as CheckBox).IsChecked.Value;
            Program.Log(MUI.EnglishLib["Contactor 9"] + " " + (
                Technology.Contactors[9].bEnabled ? "enabled" : "disabled"),
                ELogType.Operator);
        }

        private void LogLastPropertyChange(object Data)
        {
            Program.Log(LastChangedProperty + " was changed to " + LastChangedPropertyValue,
                ELogType.Operator);
        }

        private void HVPS0StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[0].StartDelay)
            {
                Technology.HVPSes[0].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("HVPS start delay changed to " + Technology.HVPSes[0].StartDelay
                + " seconds", ELogType.Operator);
        }

        private void HVPS0StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[0].StopDelay)
            {
                Technology.HVPSes[0].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("HVPS stop delay changed to " + Technology.HVPSes[0].StopDelay
                + " seconds", ELogType.Operator);
        }

        private void HVPS0PolaritySetup(object sender, RoutedEventArgs e)
        {
            Technology.HVPSes[0].bPolarity = (sender as CheckBox).IsChecked.Value;
            Program.Log("HVPS polarity set to " + Technology.HVPSes[0].bPolarity,
                ELogType.Operator);
        }

        private void HVPS1StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[1].StartDelay)
            {
                Technology.HVPSes[1].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Vibrofeeder start delay changed to " +
                Technology.HVPSes[1].StartDelay + " seconds", ELogType.Operator);
        }

        private void HVPS1StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[1].StopDelay)
            {
                Technology.HVPSes[1].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Vibrofeeder stop delay changed to " +
                Technology.HVPSes[1].StopDelay + " seconds", ELogType.Operator);
        }

        private void HVPS1PolaritySetup(object sender, RoutedEventArgs e)
        {
            Technology.HVPSes[1].bPolarity = (sender as CheckBox).IsChecked.Value;
        }

        private void FC0StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[0].StartDelay)
            {
                Technology.FCs[0].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("FC 0 start delay changed to " +
                Technology.FCs[0].StartDelay + " seconds", ELogType.Operator);
        }

        private void FC0StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[0].StopDelay)
            {
                Technology.FCs[0].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("FC 0 stop delay changed to " +
                Technology.FCs[0].StopDelay + " seconds", ELogType.Operator);
        }

        private void FC1StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[1].StartDelay)
            {
                Technology.FCs[1].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("FC 1 start delay changed to " +
                Technology.FCs[1].StartDelay + " seconds", ELogType.Operator);
        }

        private void FC1StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[1].StopDelay)
            {
                Technology.FCs[1].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("FC 1 stop delay changed to " +
                Technology.FCs[1].StopDelay + " seconds", ELogType.Operator);
        }

        private void FC2StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[2].StartDelay)
            {
                Technology.FCs[2].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("FC 2 start delay changed to " +
                Technology.FCs[2].StartDelay + " seconds", ELogType.Operator);
        }

        private void FC2StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[2].StopDelay)
            {
                Technology.FCs[2].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("FC 2 stop delay changed to " +
                Technology.FCs[2].StopDelay + " seconds", ELogType.Operator);
        }

        private void FC3StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[3].StartDelay)
            {
                Technology.FCs[3].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("FC 3 start delay changed to " +
                Technology.FCs[3].StartDelay + " seconds", ELogType.Operator);
        }

        private void FC3StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[3].StopDelay)
            {
                Technology.FCs[3].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("FC 3 stop delay changed to " +
                Technology.FCs[3].StopDelay + " seconds", ELogType.Operator);
        }

        private void FC4StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[4].StartDelay)
            {
                Technology.FCs[4].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("FC 4 start delay changed to " +
                Technology.FCs[4].StartDelay + " seconds", ELogType.Operator);
        }

        private void FC4StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[4].StopDelay)
            {
                Technology.FCs[4].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("FC 4 stop delay changed to " +
                Technology.FCs[4].StopDelay + " seconds", ELogType.Operator);
        }

        private void FC5StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[5].StartDelay)
            {
                Technology.FCs[5].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("FC 5 start delay changed to " +
                Technology.FCs[5].StartDelay + " seconds", ELogType.Operator);
        }

        private void FC5StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[5].StopDelay)
            {
                Technology.FCs[5].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("FC 5 stop delay changed to " +
                Technology.FCs[5].StopDelay + " seconds", ELogType.Operator);
        }

        private void FC6StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[6].StartDelay)
            {
                Technology.FCs[6].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("FC 6 start delay changed to " +
                Technology.FCs[6].StartDelay + " seconds", ELogType.Operator);
        }

        private void FC6StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[6].StopDelay)
            {
                Technology.FCs[6].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("FC 6 stop delay changed to " +
                Technology.FCs[6].StopDelay + " seconds", ELogType.Operator);
        }

        private void FC7StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[7].StartDelay)
            {
                Technology.FCs[7].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("FC 7 start delay changed to " +
                Technology.FCs[7].StartDelay + " seconds", ELogType.Operator);
        }

        private void FC7StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[7].StopDelay)
            {
                Technology.FCs[7].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("FC 7 stop delay changed to " +
                Technology.FCs[7].StopDelay + " seconds", ELogType.Operator);
        }

        private void FC8StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[8].StartDelay)
            {
                Technology.FCs[8].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
        }

        private void FC8StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[8].StopDelay)
            {
                Technology.FCs[8].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
        }

        private void SwipePause0_Changed(object sender, TextChangedEventArgs e)
        {
            long Res = 0;
            long.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Cleaners[0].CleanPause)
            {
                Technology.Cleaners[0].CleanPause = Res;
                (sender as TextBox).Text = Res.ToString();
            }
        }

        private void SwipeDuration0_Changed(object sender, TextChangedEventArgs e)
        {
            long Res = 0;
            long.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Cleaners[0].MaxCleanTimeout)
            {
                Technology.Cleaners[0].MaxCleanTimeout = Res;
                (sender as TextBox).Text = Res.ToString();
            }
        }

        private void Cleaner0StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Cleaners[0].StartDelay)
            {
                Technology.Cleaners[0].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
        }

        private void Cleaner0StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Cleaners[0].StopDelay)
            {
                Technology.Cleaners[0].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
        }

        private void SwipePause1_Changed(object sender, TextChangedEventArgs e)
        {
            long Res = 0;
            long.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Cleaners[1].CleanPause)
            {
                Technology.Cleaners[1].CleanPause = Res;
                (sender as TextBox).Text = Res.ToString();
            }
        }

        private void SwipeDuration1_Changed(object sender, TextChangedEventArgs e)
        {
            long Res = 0;
            long.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Cleaners[1].MaxCleanTimeout)
            {
                Technology.Cleaners[1].MaxCleanTimeout = Res;
                (sender as TextBox).Text = Res.ToString();
            }
        }

        private void Cleaner1StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Cleaners[1].StartDelay)
            {
                Technology.Cleaners[1].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
        }

        private void Cleaner1StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Cleaners[1].StopDelay)
            {
                Technology.Cleaners[1].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
        }

        private void Contactor0StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[0].StartDelay)
            {
                Technology.Contactors[0].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 0 start delay changed to " +
                Technology.Contactors[0].StartDelay + " seconds", ELogType.Operator);
        }

        private void Contactor0StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[0].StopDelay)
            {
                Technology.Contactors[0].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 0 stop delay changed to " +
                Technology.Contactors[0].StopDelay + " seconds", ELogType.Operator);
        }

        private void Contactor0ImpulseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[0].Impulse)
            {
                Technology.Contactors[0].Impulse = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 0 impulse changed to " +
                Technology.Contactors[0].Impulse + " seconds", ELogType.Operator);
        }

        private void Contactor0PauseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[0].Pause)
            {
                Technology.Contactors[0].Pause = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 0 pause changed to " +
                Technology.Contactors[0].Pause + " seconds", ELogType.Operator);
        }

        private void Contactor1StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[1].StartDelay)
            {
                Technology.Contactors[1].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 1 start delay changed to " +
                Technology.Contactors[1].StartDelay + " seconds", ELogType.Operator);
        }

        private void Contactor1StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[1].StopDelay)
            {
                Technology.Contactors[1].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 1 stop delay changed to " +
                Technology.Contactors[1].StopDelay + " seconds", ELogType.Operator);
        }

        private void Contactor1ImpulseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[1].Impulse)
            {
                Technology.Contactors[1].Impulse = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 1 impulse changed to " +
                Technology.Contactors[1].Impulse + " seconds", ELogType.Operator);
        }

        private void Contactor1PauseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[1].Pause)
            {
                Technology.Contactors[1].Pause = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 1 pause changed to " +
                Technology.Contactors[1].Pause + " seconds", ELogType.Operator);
        }

        private void Contactor2StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[2].StartDelay)
            {
                Technology.Contactors[2].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 2 start delay changed to " +
                Technology.Contactors[2].StartDelay + " seconds", ELogType.Operator);
        }

        private void Contactor2StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[2].StopDelay)
            {
                Technology.Contactors[2].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 2 stop delay changed to " +
                Technology.Contactors[2].StopDelay + " seconds", ELogType.Operator);
        }

        private void Contactor2ImpulseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[2].Impulse)
            {
                Technology.Contactors[2].Impulse = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 2 impulse changed to " +
                Technology.Contactors[2].Impulse + " seconds", ELogType.Operator);
        }

        private void Contactor2PauseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[2].Pause)
            {
                Technology.Contactors[2].Pause = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 2 pause changed to " +
                Technology.Contactors[2].Pause + " seconds", ELogType.Operator);
        }

        private void Contactor3StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[3].StartDelay)
            {
                Technology.Contactors[3].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 3 start delay changed to " +
                Technology.Contactors[3].StartDelay + " seconds", ELogType.Operator);
        }

        private void Contactor3StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[3].StopDelay)
            {
                Technology.Contactors[3].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 3 stop delay changed to " +
                Technology.Contactors[3].StopDelay + " seconds", ELogType.Operator);
        }

        private void Contactor3ImpulseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[3].Impulse)
            {
                Technology.Contactors[3].Impulse = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 3 impulse changed to " +
                Technology.Contactors[3].Impulse + " seconds", ELogType.Operator);
        }

        private void Contactor3PauseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[3].Pause)
            {
                Technology.Contactors[3].Pause = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 3 pause changed to " +
                Technology.Contactors[3].Pause + " seconds", ELogType.Operator);
        }

        private void Contactor4StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[4].StartDelay)
            {
                Technology.Contactors[4].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 4 start delay changed to " +
                Technology.Contactors[4].StartDelay + " seconds", ELogType.Operator);
        }

        private void Contactor4StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[4].StopDelay)
            {
                Technology.Contactors[4].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 4 stop delay changed to " +
                Technology.Contactors[4].StopDelay + " seconds", ELogType.Operator);
        }

        private void Contactor4ImpulseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[4].Impulse)
            {
                Technology.Contactors[4].Impulse = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 4 impulse changed to " +
                Technology.Contactors[4].Impulse + " seconds", ELogType.Operator);
        }

        private void Contactor4PauseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[4].Pause)
            {
                Technology.Contactors[4].Pause = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 4 pause changed to " +
                Technology.Contactors[4].Pause + " seconds", ELogType.Operator);
        }

        private void Contactor5StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[5].StartDelay)
            {
                Technology.Contactors[5].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 5 start delay changed to " +
                Technology.Contactors[5].StartDelay + " seconds", ELogType.Operator);
        }

        private void Contactor5StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[5].StopDelay)
            {
                Technology.Contactors[5].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 5 stop delay changed to " +
                Technology.Contactors[5].StopDelay + " seconds", ELogType.Operator);
        }

        private void Contactor5ImpulseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[5].Impulse)
            {
                Technology.Contactors[5].Impulse = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 5 impulse changed to " +
                Technology.Contactors[5].Impulse + " seconds", ELogType.Operator);
        }

        private void Contactor5PauseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[5].Pause)
            {
                Technology.Contactors[5].Pause = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 5 pause changed to " +
                Technology.Contactors[5].Pause + " seconds", ELogType.Operator);
        }

        private void Contactor6StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[6].StartDelay)
            {
                Technology.Contactors[6].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 6 start delay changed to " +
                Technology.Contactors[6].StartDelay + " seconds", ELogType.Operator);
        }

        private void Contactor6StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[6].StopDelay)
            {
                Technology.Contactors[6].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 6 stop delay changed to " +
                Technology.Contactors[6].StopDelay + " seconds", ELogType.Operator);
        }

        private void Contactor6ImpulseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[6].Impulse)
            {
                Technology.Contactors[6].Impulse = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 6 impulse changed to " +
                Technology.Contactors[6].Impulse + " seconds", ELogType.Operator);
        }

        private void Contactor6PauseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[6].Pause)
            {
                Technology.Contactors[6].Pause = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 6 pause changed to " +
                Technology.Contactors[6].Pause + " seconds", ELogType.Operator);
        }

        private void Contactor7StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[7].StartDelay)
            {
                Technology.Contactors[7].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 7 start delay changed to " +
                Technology.Contactors[7].StartDelay + " seconds", ELogType.Operator);
        }

        private void Contactor7StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[7].StopDelay)
            {
                Technology.Contactors[7].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 7 stop delay changed to " +
                Technology.Contactors[7].StopDelay + " seconds", ELogType.Operator);
        }

        private void Contactor7ImpulseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[7].Impulse)
            {
                Technology.Contactors[7].Impulse = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 7 impulse changed to " +
                Technology.Contactors[7].Impulse + " seconds", ELogType.Operator);
        }

        private void Contactor7PauseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[7].Pause)
            {
                Technology.Contactors[7].Pause = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 7 pause changed to " +
                Technology.Contactors[7].Pause + " seconds", ELogType.Operator);
        }

        private void Contactor8StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[8].StartDelay)
            {
                Technology.Contactors[8].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 8 start delay changed to " +
                Technology.Contactors[8].StartDelay + " seconds", ELogType.Operator);
        }

        private void Contactor8StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[8].StopDelay)
            {
                Technology.Contactors[8].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 8 stop delay changed to " +
                Technology.Contactors[8].StopDelay + " seconds", ELogType.Operator);
        }

        private void Contactor8ImpulseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[8].Impulse)
            {
                Technology.Contactors[8].Impulse = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 8 impulse changed to " +
                Technology.Contactors[8].Impulse + " seconds", ELogType.Operator);
        }

        private void Contactor8PauseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[8].Pause)
            {
                Technology.Contactors[8].Pause = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 8 pause changed to " +
                Technology.Contactors[8].Pause + " seconds", ELogType.Operator);
        }

        private void Contactor9StartDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[9].StartDelay)
            {
                Technology.Contactors[9].StartDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 9 start delay changed to " +
                Technology.Contactors[9].StartDelay + " seconds", ELogType.Operator);
        }

        private void Contactor9StopDelayChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[9].StopDelay)
            {
                Technology.Contactors[9].StopDelay = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 9 stop delay changed to " +
                Technology.Contactors[9].StopDelay + " seconds", ELogType.Operator);
        }

        private void Contactor9ImpulseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[9].Impulse)
            {
                Technology.Contactors[9].Impulse = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 9 impulse changed to " +
                Technology.Contactors[9].Impulse + " seconds", ELogType.Operator);
        }

        private void Contactor9PauseChanged(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.Contactors[9].Pause)
            {
                Technology.Contactors[9].Pause = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Contactor 9 pause changed to " +
                Technology.Contactors[9].Pause + " seconds", ELogType.Operator);
        }

        private void MinOutVoltage0_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[0].MinOutput)
            {
                Technology.HVPSes[0].MinOutput = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if(!Technology.HVPSes[0].bPolarity)
                {
                    return;
                }
                if (CurrentFCIndex == 100)
                {
                    scrFrequencySetter.Minimum = (double)Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value < scrFrequencySetter.Minimum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Minimum;
                    }
                }
                scrOutputVoltage0.Minimum = (double)Res;
                scrOutputVoltage0.ViewportSize = (scrOutputVoltage0.Maximum -
                    scrOutputVoltage0.Minimum) * 0.25 / 0.75;
                if (scrOutputVoltage0.Value < scrOutputVoltage0.Minimum)
                {
                    scrOutputVoltage0.Value = scrOutputVoltage0.Minimum;
                }
                Program.Log("HVPS min out voltage changed to " +
                    Res + " kV", ELogType.Operator);
            }
            catch { }
        }

        private void MinOutVoltage0R_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[0].ReverseMinOutput)
            {
                Technology.HVPSes[0].ReverseMinOutput = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if (Technology.HVPSes[0].bPolarity)
                {
                    return;
                }
                if (CurrentFCIndex == 100)
                {
                    scrFrequencySetter.Minimum = (double)Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value < scrFrequencySetter.Minimum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Minimum;
                    }
                }
                scrOutputVoltage0.Minimum = (double)Res;
                scrOutputVoltage0.ViewportSize = (scrOutputVoltage0.Maximum -
                    scrOutputVoltage0.Minimum) * 0.25 / 0.75;
                if (scrOutputVoltage0.Value < scrOutputVoltage0.Minimum)
                {
                    scrOutputVoltage0.Value = scrOutputVoltage0.Minimum;
                }
                Program.Log("HVPS reverse min out voltage changed to " +
                    Res + " kV", ELogType.Operator);
            }
            catch { }
        }

        private void MinOutVoltage1_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[1].MinOutput)
            {
                Technology.HVPSes[1].MinOutput = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if (CurrentFCIndex == 101)
                {
                    scrFrequencySetter.Minimum = (double)Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value < scrFrequencySetter.Minimum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Minimum;
                    }
                }
                scrOutputCurrent1.Minimum = (double)Res;
                scrOutputCurrent1.ViewportSize = (scrOutputCurrent1.Maximum -
                    scrOutputCurrent1.Minimum) * 0.25 / 0.75;
                if (scrOutputCurrent1.Value < scrOutputCurrent1.Minimum)
                {
                    scrOutputCurrent1.Value = scrOutputCurrent1.Minimum;
                }
                Program.Log("Vibrator min out voltage changed to " +
                    Res + " kV", ELogType.Operator);
            }
            catch { }
        }

        private void MaxOutVoltage0_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[0].MaxOutput)
            {
                Technology.HVPSes[0].MaxOutput = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if (Technology.HVPSes[0].bPolarity)
                {
                    return;
                }
                if (CurrentFCIndex == 100)
                {
                    scrFrequencySetter.Maximum = (double)Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value > scrFrequencySetter.Maximum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Maximum;
                    }
                }
                scrOutputVoltage0.Maximum = (double)Res;
                scrOutputVoltage0.ViewportSize = (scrOutputVoltage0.Maximum -
                    scrOutputVoltage0.Minimum) * 0.25 / 0.75;
                if (scrOutputVoltage0.Value > scrOutputVoltage0.Maximum)
                {
                    scrOutputVoltage0.Value = scrOutputVoltage0.Maximum;
                }
                Program.Log("HVPS max out voltage changed to " +
                    Res + " kV", ELogType.Operator);
            }
            catch { }
        }

        private void MaxOutVoltage0R_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[0].ReverseMaxOutput)
            {
                Technology.HVPSes[0].ReverseMaxOutput = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if (!Technology.HVPSes[0].bPolarity)
                {
                    return;
                }
                if (CurrentFCIndex == 100)
                {
                    scrFrequencySetter.Maximum = (double)Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value > scrFrequencySetter.Maximum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Maximum;
                    }
                }
                scrOutputVoltage0.Maximum = (double)Res;
                scrOutputVoltage0.ViewportSize = (scrOutputVoltage0.Maximum -
                    scrOutputVoltage0.Minimum) * 0.25 / 0.75;
                if (scrOutputVoltage0.Value > scrOutputVoltage0.Maximum)
                {
                    scrOutputVoltage0.Value = scrOutputVoltage0.Maximum;
                }
                Program.Log("HVPS reverse max out voltage changed to " +
                    Res + " kV", ELogType.Operator);
            }
            catch { }
        }

        private void MaxOutVoltage1_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[1].MaxOutput)
            {
                Technology.HVPSes[1].MaxOutput = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if (CurrentFCIndex == 101)
                {
                    scrFrequencySetter.Maximum = (double)Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value > scrFrequencySetter.Maximum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Maximum;
                    }
                }
                scrOutputCurrent1.Maximum = (double)Res;
                scrOutputCurrent1.ViewportSize = (scrOutputCurrent1.Maximum -
                    scrOutputCurrent1.Minimum) * 0.25 / 0.75;
                if (scrOutputCurrent1.Value > scrOutputCurrent1.Maximum)
                {
                    scrOutputCurrent1.Value = scrOutputCurrent1.Maximum;
                }
                Program.Log("Vibrator max out voltage changed to " +
                    Res + " kV", ELogType.Operator);
            }
            catch { }
        }

        private void MinInVoltage0_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[0].RealInputAt0kV)
            {
                Technology.HVPSes[0].RealInputAt0kV = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("HVPS min input voltage changed to " +
                    Res + " kV", ELogType.Operator);
        }

        private void MinInVoltage0R_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[0].ReverseRealInputAt0kV)
            {
                Technology.HVPSes[0].ReverseRealInputAt0kV = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("HVPS reverse min input voltage changed to " +
                    Res + " kV", ELogType.Operator);
        }

        private void MinInVoltage1_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[1].RealInputAt0kV)
            {
                Technology.HVPSes[1].RealInputAt0kV = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Vibrator min input voltage changed to " +
                    Res + " kV", ELogType.Operator);
        }

        private void MaxInVoltage0_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[0].RealInputAt30kV)
            {
                Technology.HVPSes[0].RealInputAt30kV = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("HVPS max input voltage changed to " +
                    Res + " kV", ELogType.Operator);
        }

        private void MaxInVoltage0R_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[0].ReverseRealInputAt30kV)
            {
                Technology.HVPSes[0].ReverseRealInputAt30kV = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("HVPS reverse max input voltage changed to " +
                    Res + " kV", ELogType.Operator);
        }

        private void MaxInVoltage1_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[1].RealInputAt30kV)
            {
                Technology.HVPSes[1].RealInputAt30kV = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Vibrator max input voltage changed to " +
                    Res + " kV", ELogType.Operator);
        }

        private void MaxInCurrent0_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[0].MaxmA)
            {
                Technology.HVPSes[0].MaxmA = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("HVPS max input current changed to " +
                    Res + " mA", ELogType.Operator);
        }

        private void MaxInCurrent0R_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[0].ReverseMaxmA)
            {
                Technology.HVPSes[0].ReverseMaxmA = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("HVPS reverse max input current changed to " +
                    Res + " mA", ELogType.Operator);
        }

        private void MaxInCurrent1_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[1].MaxmA)
            {
                Technology.HVPSes[1].MaxmA = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Vibrator max input current changed to " +
                    Res + " mA", ELogType.Operator);
        }

        private void SensorVoltageAt0mA0_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[0].RealInputAt0mA)
            {
                Technology.HVPSes[0].RealInputAt0mA = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("HVPS sensor voltage at 0 mA changed to " +
                    Res + " V", ELogType.Operator);
        }

        private void SensorVoltageAt0mA0R_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[0].ReverseRealInputAt0mA)
            {
                Technology.HVPSes[0].ReverseRealInputAt0mA = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("HVPS reverse sensor voltage at 0 mA changed to " +
                    Res + " V", ELogType.Operator);
        }

        private void SensorVoltageAt0mA1_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[1].RealInputAt0mA)
            {
                Technology.HVPSes[1].RealInputAt0mA = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Vibrator sensor voltage at 0 mA changed to " +
                    Res + " V", ELogType.Operator);
        }

        private void SensorVoltageAtMaxmA0_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[0].RealInputAtMaxmA)
            {
                Technology.HVPSes[0].RealInputAtMaxmA = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("HVPS sensor voltage at max mA changed to " +
                    Res + " V", ELogType.Operator);
        }

        private void SensorVoltageAtMaxmA0R_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[0].ReverseRealInputAtMaxmA)
            {
                Technology.HVPSes[0].ReverseRealInputAtMaxmA = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("HVPS reverse sensor voltage at max mA changed to " +
                    Res + " V", ELogType.Operator);
        }

        private void SensorVoltageAtMaxmA1_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.HVPSes[1].RealInputAtMaxmA)
            {
                Technology.HVPSes[1].RealInputAtMaxmA = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            Program.Log("Vibrator sensor voltage at max mA changed to " +
                    Res + " V", ELogType.Operator);
        }

        private void MinFrequency0_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if ((int)Res != Technology.FCs[0].CalibrationMinFrequency)
            {
                Technology.FCs[0].CalibrationMinFrequency = (int)Res;
                (sender as TextBox).Text = ((int)Res).ToString();
            }
            try
            {
                if(CurrentFCIndex == 0)
                {
                    scrFrequencySetter.Minimum = (int)Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value < scrFrequencySetter.Minimum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Minimum;
                    }
                }
                scrFrequency0.Minimum = (int)Res;
                scrFrequency0.ViewportSize = (scrFrequency0.Maximum -
                    scrFrequency0.Minimum) * 0.25 / 0.75;
                if (scrFrequency0.Value < scrFrequency0.Minimum)
                {
                    scrFrequency0.Value = scrFrequency0.Minimum;
                }
                Program.Log("FC 0 min frequency changed to " +
                    (int)Res + " Hz", ELogType.Operator);
            }
            catch { }
        }

        private void MaxFrequency0_Changed(object sender, TextChangedEventArgs e)
        {
            decimal Res = 0;
            decimal.TryParse((sender as TextBox).Text, out Res);
            if ((int)Res != Technology.FCs[0].CalibrationMaxFrequency)
            {
                Technology.FCs[0].CalibrationMaxFrequency = (int)Res;
                (sender as TextBox).Text = ((int)Res).ToString();
            }
            try
            {
                if (CurrentFCIndex == 0)
                {
                    scrFrequencySetter.Maximum = (int)Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value > scrFrequencySetter.Maximum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Maximum;
                    }
                }
                scrFrequency0.Maximum = (int)Res;
                scrFrequency0.ViewportSize = (scrFrequency0.Maximum -
                    scrFrequency0.Minimum) * 0.25 / 0.75;
                if (scrFrequency0.Value > scrFrequency0.Maximum)
                {
                    scrFrequency0.Value = scrFrequency0.Maximum;
                }
                Program.Log("FC 0 max frequency changed to " +
                    (int)Res + " Hz", ELogType.Operator);
            }
            catch { }
        }

        private void MinFrequency1_Changed(object sender, TextChangedEventArgs e)
        {
            decimal DRes = 0;
            decimal.TryParse((sender as TextBox).Text, out DRes);
            int Res = (int)DRes;
            if (Res != Technology.FCs[1].CalibrationMinFrequency)
            {
                Technology.FCs[1].CalibrationMinFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if (CurrentFCIndex == 1)
                {
                    scrFrequencySetter.Minimum = Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value < scrFrequencySetter.Minimum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Minimum;
                    }
                }
                scrFrequency1.Minimum = (double)Res;
                scrFrequency1.ViewportSize = (scrFrequency1.Maximum -
                    scrFrequency1.Minimum) * 0.25 / 0.75;
                if (scrFrequency1.Value < scrFrequency1.Minimum)
                {
                    scrFrequency1.Value = scrFrequency1.Minimum;
                }
                Program.Log("FC 1 min frequency changed to " +
                    Res + " Hz", ELogType.Operator);
            }
            catch { }
        }

        private void MaxFrequency1_Changed(object sender, TextChangedEventArgs e)
        {
            decimal DRes = 0;
            decimal.TryParse((sender as TextBox).Text, out DRes);
            int Res = (int)DRes;
            if (Res != Technology.FCs[1].CalibrationMaxFrequency)
            {
                Technology.FCs[1].CalibrationMaxFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if (CurrentFCIndex == 1)
                {
                    scrFrequencySetter.Maximum = Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value > scrFrequencySetter.Maximum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Maximum;
                    }
                }
                scrFrequency1.Maximum = (double)Res;
                scrFrequency1.ViewportSize = (scrFrequency1.Maximum -
                    scrFrequency1.Minimum) * 0.25 / 0.75;
                if (scrFrequency1.Value > scrFrequency1.Maximum)
                {
                    scrFrequency1.Value = scrFrequency1.Maximum;
                }
                Program.Log("FC 1 max frequency changed to " +
                    Res + " Hz", ELogType.Operator);
            }
            catch { }
        }

        private void MinFrequency2_Changed(object sender, TextChangedEventArgs e)
        {
            decimal DRes = 0;
            decimal.TryParse((sender as TextBox).Text, out DRes);
            int Res = (int)DRes;
            if (Res != Technology.FCs[2].CalibrationMinFrequency)
            {
                Technology.FCs[2].CalibrationMinFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if (CurrentFCIndex == 2)
                {
                    scrFrequencySetter.Minimum = Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value < scrFrequencySetter.Minimum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Minimum;
                    }
                }
                scrFrequency2.Minimum = (double)Res;
                scrFrequency2.ViewportSize = (scrFrequency2.Maximum -
                    scrFrequency2.Minimum) * 0.25 / 0.75;
                if (scrFrequency2.Value < scrFrequency2.Minimum)
                {
                    scrFrequency2.Value = scrFrequency2.Minimum;
                }
                Program.Log("FC 2 min frequency changed to " +
                    Res + " Hz", ELogType.Operator);
            }
            catch { }
        }

        private void MaxFrequency2_Changed(object sender, TextChangedEventArgs e)
        {
            decimal DRes = 0;
            decimal.TryParse((sender as TextBox).Text, out DRes);
            int Res = (int)DRes;
            if (Res != Technology.FCs[2].CalibrationMaxFrequency)
            {
                Technology.FCs[2].CalibrationMaxFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if (CurrentFCIndex == 2)
                {
                    scrFrequencySetter.Maximum = Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value > scrFrequencySetter.Maximum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Maximum;
                    }
                }
                scrFrequency2.Maximum = (double)Res;
                scrFrequency2.ViewportSize = (scrFrequency2.Maximum -
                    scrFrequency2.Minimum) * 0.25 / 0.75;
                if (scrFrequency2.Value > scrFrequency2.Maximum)
                {
                    scrFrequency2.Value = scrFrequency2.Maximum;
                }
                Program.Log("FC 2 max frequency changed to " +
                    Res + " Hz", ELogType.Operator);
            }
            catch { }
        }

        private void MinFrequency3_Changed(object sender, TextChangedEventArgs e)
        {
            decimal DRes = 0;
            decimal.TryParse((sender as TextBox).Text, out DRes);
            int Res = (int)DRes;
            if (Res != Technology.FCs[3].CalibrationMinFrequency)
            {
                Technology.FCs[3].CalibrationMinFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if (CurrentFCIndex == 3)
                {
                    scrFrequencySetter.Minimum = Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value < scrFrequencySetter.Minimum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Minimum;
                    }
                }
                scrFrequency3.Minimum = (double)Res;
                scrFrequency3.ViewportSize = (scrFrequency3.Maximum -
                    scrFrequency3.Minimum) * 0.25 / 0.75;
                if (scrFrequency3.Value < scrFrequency3.Minimum)
                {
                    scrFrequency3.Value = scrFrequency3.Minimum;
                }
                Program.Log("FC 3 min frequency changed to " +
                    Res + " Hz", ELogType.Operator);
            }
            catch { }
        }

        private void MaxFrequency3_Changed(object sender, TextChangedEventArgs e)
        {
            decimal DRes = 0;
            decimal.TryParse((sender as TextBox).Text, out DRes);
            int Res = (int)DRes;
            if (Res != Technology.FCs[3].CalibrationMaxFrequency)
            {
                Technology.FCs[3].CalibrationMaxFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if (CurrentFCIndex == 3)
                {
                    scrFrequencySetter.Maximum = Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value > scrFrequencySetter.Maximum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Maximum;
                    }
                }
                scrFrequency3.Maximum = (double)Res;
                scrFrequency3.ViewportSize = (scrFrequency3.Maximum -
                    scrFrequency3.Minimum) * 0.25 / 0.75;
                if (scrFrequency3.Value > scrFrequency3.Maximum)
                {
                    scrFrequency3.Value = scrFrequency3.Maximum;
                }
                Program.Log("FC 3 max frequency changed to " +
                    Res + " Hz", ELogType.Operator);
            }
            catch { }
        }

        private void MinFrequency4_Changed(object sender, TextChangedEventArgs e)
        {
            decimal DRes = 0;
            decimal.TryParse((sender as TextBox).Text, out DRes);
            int Res = (int)DRes;
            if (Res != Technology.FCs[4].CalibrationMinFrequency)
            {
                Technology.FCs[4].CalibrationMinFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if (CurrentFCIndex == 4)
                {
                    scrFrequencySetter.Minimum = Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value < scrFrequencySetter.Minimum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Minimum;
                    }
                }
                scrFrequency4.Minimum = (double)Res;
                scrFrequency4.ViewportSize = (scrFrequency4.Maximum -
                    scrFrequency4.Minimum) * 0.25 / 0.75;
                if (scrFrequency4.Value < scrFrequency4.Minimum)
                {
                    scrFrequency4.Value = scrFrequency4.Minimum;
                }
                Program.Log("FC 4 min frequency changed to " +
                    Res + " Hz", ELogType.Operator);
            }
            catch { }
        }

        private void MaxFrequency4_Changed(object sender, TextChangedEventArgs e)
        {
            decimal DRes = 0;
            decimal.TryParse((sender as TextBox).Text, out DRes);
            int Res = (int)DRes;
            if (Res != Technology.FCs[4].CalibrationMaxFrequency)
            {
                Technology.FCs[4].CalibrationMaxFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if (CurrentFCIndex == 4)
                {
                    scrFrequencySetter.Maximum = Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value > scrFrequencySetter.Maximum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Maximum;
                    }
                }
                scrFrequency4.Maximum = (double)Res;
                scrFrequency4.ViewportSize = (scrFrequency4.Maximum -
                    scrFrequency4.Minimum) * 0.25 / 0.75;
                if (scrFrequency4.Value > scrFrequency4.Maximum)
                {
                    scrFrequency4.Value = scrFrequency4.Maximum;
                }
                Program.Log("FC 4 max frequency changed to " +
                    Res + " Hz", ELogType.Operator);
            }
            catch { }
        }

        private void MinFrequency5_Changed(object sender, TextChangedEventArgs e)
        {
            decimal DRes = 0;
            decimal.TryParse((sender as TextBox).Text, out DRes);
            int Res = (int)DRes;
            if (Res != Technology.FCs[5].CalibrationMinFrequency)
            {
                Technology.FCs[5].CalibrationMinFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if (CurrentFCIndex == 5)
                {
                    scrFrequencySetter.Minimum = Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value < scrFrequencySetter.Minimum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Minimum;
                    }
                }
                scrFrequency5.Minimum = (double)Res;
                scrFrequency5.ViewportSize = (scrFrequency5.Maximum -
                    scrFrequency5.Minimum) * 0.25 / 0.75;
                if (scrFrequency5.Value < scrFrequency5.Minimum)
                {
                    scrFrequency5.Value = scrFrequency5.Minimum;
                }
                Program.Log("FC 5 min frequency changed to " +
                    Res + " Hz", ELogType.Operator);
            }
            catch { }
        }

        private void MaxFrequency5_Changed(object sender, TextChangedEventArgs e)
        {
            decimal DRes = 0;
            decimal.TryParse((sender as TextBox).Text, out DRes);
            int Res = (int)DRes;
            if (Res != Technology.FCs[5].CalibrationMaxFrequency)
            {
                Technology.FCs[5].CalibrationMaxFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if (CurrentFCIndex == 5)
                {
                    scrFrequencySetter.Maximum = Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value > scrFrequencySetter.Maximum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Maximum;
                    }
                }
                scrFrequency5.Maximum = (double)Res;
                scrFrequency5.ViewportSize = (scrFrequency5.Maximum -
                    scrFrequency5.Minimum) * 0.25 / 0.75;
                if (scrFrequency5.Value > scrFrequency5.Maximum)
                {
                    scrFrequency5.Value = scrFrequency5.Maximum;
                }
                Program.Log("FC 5 max frequency changed to " +
                    Res + " Hz", ELogType.Operator);
            }
            catch { }
        }

        private void MinFrequency6_Changed(object sender, TextChangedEventArgs e)
        {
            decimal DRes = 0;
            decimal.TryParse((sender as TextBox).Text, out DRes);
            int Res = (int)DRes;
            if (Res != Technology.FCs[6].CalibrationMinFrequency)
            {
                Technology.FCs[6].CalibrationMinFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if (CurrentFCIndex == 6)
                {
                    scrFrequencySetter.Minimum = Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value < scrFrequencySetter.Minimum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Minimum;
                    }
                }
                scrFrequency6.Minimum = (double)Res;
                scrFrequency6.ViewportSize = (scrFrequency6.Maximum -
                    scrFrequency6.Minimum) * 0.25 / 0.75;
                if (scrFrequency6.Value < scrFrequency6.Minimum)
                {
                    scrFrequency6.Value = scrFrequency6.Minimum;
                }
                Program.Log("FC 6 min frequency changed to " +
                    Res + " Hz", ELogType.Operator);
            }
            catch { }
        }

        private void MaxFrequency6_Changed(object sender, TextChangedEventArgs e)
        {
            decimal DRes = 0;
            decimal.TryParse((sender as TextBox).Text, out DRes);
            int Res = (int)DRes;
            if (Res != Technology.FCs[6].CalibrationMaxFrequency)
            {
                Technology.FCs[6].CalibrationMaxFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
            try
            {
                if (CurrentFCIndex == 6)
                {
                    scrFrequencySetter.Maximum = Res;
                    scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                        scrFrequencySetter.Minimum) * 0.25 / 0.75;
                    if (scrFrequencySetter.Value > scrFrequencySetter.Maximum)
                    {
                        scrFrequencySetter.Value = scrFrequencySetter.Maximum;
                    }
                }
                scrFrequency6.Maximum = (double)Res;
                scrFrequency6.ViewportSize = (scrFrequency6.Maximum -
                    scrFrequency6.Minimum) * 0.25 / 0.75;
                if (scrFrequency6.Value > scrFrequency6.Maximum)
                {
                    scrFrequency6.Value = scrFrequency6.Maximum;
                }
                Program.Log("FC 6 max frequency changed to " +
                    Res + " Hz", ELogType.Operator);
            }
            catch { }
        }

        private void MinFrequency7_Changed(object sender, TextChangedEventArgs e)
        {
            int Res = 0;
            int.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[7].CalibrationMinFrequency)
            {
                Technology.FCs[7].CalibrationMinFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
        }

        private void MaxFrequency7_Changed(object sender, TextChangedEventArgs e)
        {
            int Res = 0;
            int.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[7].CalibrationMaxFrequency)
            {
                Technology.FCs[7].CalibrationMaxFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
        }

        private void MinFrequency8_Changed(object sender, TextChangedEventArgs e)
        {
            int Res = 0;
            int.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[8].CalibrationMinFrequency)
            {
                Technology.FCs[8].CalibrationMinFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
        }

        private void MaxFrequency8_Changed(object sender, TextChangedEventArgs e)
        {
            int Res = 0;
            int.TryParse((sender as TextBox).Text, out Res);
            if (Res != Technology.FCs[8].CalibrationMaxFrequency)
            {
                Technology.FCs[8].CalibrationMaxFrequency = Res;
                (sender as TextBox).Text = Res.ToString();
            }
        }

        private void AdminPasswordChanged(object sender, RoutedEventArgs e)
        {
            Settings.AdminPassword = (sender as PasswordBox).Password;
        }
        #endregion

        bool[] CachedBlockerStates = new bool[13];

        public void Log(string LogString)
        {
            var Logs = lbxLogs.Items;
            Logs.Add(LogString);
            while (Logs.Count > 1000)
            {
                Logs.RemoveAt(0);
            }
        }

        public void UpdateAutoModePanel()
        {
            imgAlarmImage.Visibility = !Technology.EmergencySignal.GetBit() &&
                Program.bComEnabled ?
                Visibility.Visible : Visibility.Hidden;
            btnPolaritySwitch.IsEnabled = !Technology.HVPSes[0].bInitialState;
            var WhiteButton = new LinearGradientBrush(new GradientStopCollection(
                new GradientStop[]
            {
                new GradientStop(Color.FromRgb(220, 220, 220), 0),
                new GradientStop(Color.FromRgb(220, 220, 220), 0.45),
                new GradientStop(Color.FromRgb(160, 160, 160), 0.55),
                new GradientStop(Color.FromRgb(160, 160, 160), 1)
            }), 90);
            var BlueButton =
                new LinearGradientBrush(new GradientStopCollection(
                new GradientStop[]
            {
                new GradientStop(Color.FromRgb(64, 64, 255), 0),
                new GradientStop(Color.FromRgb(64, 64, 255), 0.45),
                new GradientStop(Color.FromRgb(32, 32, 192), 0.55),
                new GradientStop(Color.FromRgb(32, 32, 192), 1)
            }), 90);
            btnPolaritySwitch.Background = Technology.HVPSes[0].bPolarity ?
                BlueButton : WhiteButton;
            btnPolaritySwitch.Content = SelectedLib["Polarity"] + ": " +
                (Technology.HVPSes[0].bPolarity ? "-" : "+");
            var EngineIndicators = new Ellipse[]
            {
                elpEngineIndicator1,
                elpEngineIndicator2,
                elpEngineIndicator3,
                elpEngineIndicator4,
                elpEngineIndicator5,
                elpEngineIndicator6,
                elpEngineIndicator6_1,
                elpEngineIndicator7,
                elpEngineIndicator8,
                elpEngineIndicator9,
                elpEngineIndicator9_1,
                elpEngineIndicator10,
                elpEngineIndicator11,
                elpEngineIndicator12,
                elpEngineIndicator13,
                elpEngineIndicator14,
                elpEngineIndicator15,
                elpEngineIndicator16,
                elpEngineIndicator17,
                elpEngineIndicator18,
                elpEngineIndicator19
            };
            var FrequencyIndicators = new Label[]
            {
                lblEngine1FrequencyIndicator,
                lblEngine2FrequencyIndicator,
                lblEngine3FrequencyIndicator,
                lblEngine4FrequencyIndicator,
                lblEngine5FrequencyIndicator,
                lblEngine6FrequencyIndicator,
                lblEngine6FrequencyIndicator_1,
                lblEngine7FrequencyIndicator
            };
            Ellipse CurrentIndicator;
            for (int i = 0; i < 8; i++)
            {
                var CurrentFC = Technology.FCs[i > 5 ? i - 1 : i];
                var CurrentFI = FrequencyIndicators[i];
                CurrentIndicator = EngineIndicators[i];
                if (CurrentFC.Buffer.bDebugMode)
                {
                    CurrentFI.Content = CurrentFC.Buffer.LastMessage;
                }
                else if (CurrentFC.bEmergency)
                {
                    try
                    {
                        if (CurrentFI.Content.ToString() !=
                            SelectedLib["FC Error " + CurrentFC.ErrorCode.ToString("F1")])
                        {
                            CurrentIndicator.Fill = new SolidColorBrush(Colors.Red);
                            CurrentFI.Content = SelectedLib["FC Error " +
                                CurrentFC.ErrorCode.ToString("F1")];
                        }
                    }
                    catch
                    {
                        CurrentFI.Content = CurrentFC.ErrorCode;
                    }
                }
                else if (!CurrentFC.bConnected && CurrentFI.Content.ToString() !=
                    SelectedLib["Connection lost"])
                {
                    CurrentIndicator.Fill = new SolidColorBrush(Colors.Orange);
                    CurrentFI.Content = SelectedLib["Connection lost"];
                }
                else if (CurrentFC.bPowerState)
                {
                    if (CurrentFI.Content.ToString() != CurrentFC.CurrentFrequency + " " + SelectedLib["Hz"])
                    {
                        CurrentIndicator.Fill = new SolidColorBrush(Colors.Lime);
                        CurrentFI.Content = CurrentFC.CurrentFrequency + " " + SelectedLib["Hz"];
                    }
                }
                else if (CurrentFI.Content.ToString() != "")
                {
                    CurrentIndicator.Fill = new SolidColorBrush(Colors.Yellow);
                    CurrentFI.Content = "";
                }
            }
            HVPS CurrentHVPS = Technology.HVPSes[1];
            CurrentIndicator = elpEngineIndicator8;
            if (CurrentHVPS.bEmergency && lblEngine8CurrentIndicator.Content.ToString() != "")
            {
                CurrentIndicator.Fill = new SolidColorBrush(Colors.Red);
                lblEngine8CurrentIndicator.Content = "";
            }
            else if (CurrentHVPS.bPowerState && lblEngine8CurrentIndicator.Content.ToString() !=
                CurrentHVPS.CurrentIn.ToString("F2", CultureInfo.InvariantCulture) + " " +
                SelectedLib["A"])
            {
                CurrentIndicator.Fill = new SolidColorBrush(Colors.Lime);
                lblEngine8CurrentIndicator.Content =
                    CurrentHVPS.CurrentIn.ToString("F2", CultureInfo.InvariantCulture) + " " +
                    SelectedLib["A"];
            }
            else if (lblEngine8CurrentIndicator.Content.ToString() != "")
            {
                CurrentIndicator.Fill = new SolidColorBrush(Colors.Yellow);
                lblEngine8CurrentIndicator.Content = "";
            }
            CurrentHVPS = Technology.HVPSes[0];
            var HVPSIndicators = new Label[]
            {
                lblEngine9VoltageIndicator,
                lblEngine9VoltageIndicator_1,
                lblEngine9CurrentIndicator,
                lblEngine9CurrentIndicator_1,
            };
            for (int i = 0; i < 2; i++)
            {
                var CurrentIndicatorC = HVPSIndicators[i + 2];
                var CurrentIndicatorV = HVPSIndicators[i];
                var CurrentIndicatorE = EngineIndicators[i + 9];

                if (CurrentHVPS.bEmergency &&
                    CurrentIndicatorC.Content.ToString() != "")
                {
                    CurrentIndicatorE.Fill = new SolidColorBrush(Colors.Red);
                    CurrentIndicatorC.Content = "";
                    CurrentIndicatorV.Content = "";
                }
                else if (CurrentHVPS.bPowerState && CurrentIndicatorC.Content.ToString() !=
                        CurrentHVPS.CurrentIn.ToString("F2", CultureInfo.InvariantCulture) +
                        " " + SelectedLib["mA"])
                {
                    CurrentIndicatorE.Fill = new SolidColorBrush(Colors.Lime);
                    CurrentIndicatorC.Content =
                        CurrentHVPS.CurrentIn.ToString("F2", CultureInfo.InvariantCulture) + " " +
                        SelectedLib["mA"];
                    CurrentIndicatorV.Content =
                        CurrentHVPS.VoltageIn.ToString("F2", CultureInfo.InvariantCulture) + " " +
                        SelectedLib["kV"];
                }
                else if (CurrentIndicatorC.Content.ToString() != "")
                {
                    CurrentIndicatorE.Fill = new SolidColorBrush(Colors.Yellow);
                    CurrentIndicatorC.Content = "";
                    CurrentIndicatorV.Content = "";
                }
            }
            for (int i = 0; i < 10; i++)
            {
                CurrentIndicator = EngineIndicators[i + 11];
                var CurrentContactor = Technology.Contactors[i];

                if (CurrentContactor.bEmergency)
                {
                    if (!(CurrentIndicator.Fill is SolidColorBrush)
                        || (CurrentIndicator.Fill as SolidColorBrush).Color != Colors.Red)
                        CurrentIndicator.Fill = new SolidColorBrush(Colors.Red);
                }
                else if (CurrentContactor.bPowerState)
                {
                    if (!(CurrentIndicator.Fill is SolidColorBrush) ||
                    (CurrentIndicator.Fill as SolidColorBrush).Color != Colors.Lime)
                    {
                        CurrentIndicator.Fill = new SolidColorBrush(Colors.Lime);
                    }
                }
                else if (!(CurrentIndicator.Fill is SolidColorBrush) ||
                    (CurrentIndicator.Fill as SolidColorBrush).Color != Colors.Yellow)
                {
                    CurrentIndicator.Fill = new SolidColorBrush(Colors.Yellow);
                }
            }
            Rectangle[] BlockerIndicators =
            {
                sqrBlockerIndicator1,
                sqrBlockerIndicator2,
                sqrBlockerIndicator3,
                sqrBlockerIndicator4,
                sqrBlockerIndicator5,
                sqrBlockerIndicator6,
                sqrBlockerIndicator7,
                sqrBlockerIndicator8,
                sqrBlockerIndicator9,
                sqrBlockerIndicator10,
                sqrBlockerIndicator11,
                sqrBlockerIndicator12,
                sqrBlockerIndicator13,
            };
            for (int i = 0; i < 13; i++)
            {
                if (!Technology.Lockers[i].bEnabled)
                {
                    if (!(BlockerIndicators[i].Fill is
                    SolidColorBrush) || (BlockerIndicators[i].Fill as SolidColorBrush).Color
                    != Colors.Yellow)
                    {
                        BlockerIndicators[i].Fill = new SolidColorBrush(Colors.Yellow);
                    }
                }
                else if (!Technology.Lockers[i].bState)
                {
                    if (!(BlockerIndicators[i].Fill is
                    SolidColorBrush) || (BlockerIndicators[i].Fill as SolidColorBrush).Color
                    != Colors.Red)
                    {
                        BlockerIndicators[i].Fill = new SolidColorBrush(Colors.Red);
                    }
                }
                else if (!(BlockerIndicators[i].Fill is SolidColorBrush) ||
                    (BlockerIndicators[i].Fill as SolidColorBrush).Color != Colors.Lime)
                {
                    BlockerIndicators[i].Fill = new SolidColorBrush(Colors.Lime);
                }
            }
            var bBlockersFired = false;
            Grid[] BlockerWarningGrids =
            {
                grdBlocker1WarningGrid,
                grdBlocker2WarningGrid,
                grdBlocker3WarningGrid,
                grdBlocker4WarningGrid,
                grdBlocker5WarningGrid,
                grdBlocker6WarningGrid,
                grdBlocker7WarningGrid,
                grdBlocker8WarningGrid,
                grdBlocker9WarningGrid,
                grdBlocker10WarningGrid,
                grdBlocker11WarningGrid,
                grdBlocker12WarningGrid,
                grdBlocker13WarningGrid,
            };
            for (int i = 0; i < 13; i++)
            {
                if (Technology.Lockers[i].bEnabled &&
                    !Technology.Lockers[i].bState)
                {
                    bBlockersFired = true;
                    BlockerWarningGrids[i].Visibility = Visibility.Visible;
                }
                else
                {
                    BlockerWarningGrids[i].Visibility = Visibility.Collapsed;
                }
            }
            if (bBlockersFired)
            {
                stpBlockerWarningPanel.Visibility = Visibility.Visible;
            }
            else
            {
                stpBlockerWarningPanel.Visibility = Visibility.Collapsed;
            }
            bool[] CurrentBlockerStates = new bool[13];
            for (int i = 0; i < 13; i++)
            {
                CurrentBlockerStates[i] = Technology.Lockers[i].bEmergency;
            }
            if ((CachedBlockerStates[9] == false && CurrentBlockerStates[9] == true ||
                CachedBlockerStates[10] == false && CurrentBlockerStates[10] == true) &&
                CurrentBlockerStates[0] == false &&
                CurrentBlockerStates[1] == false &&
                CurrentBlockerStates[2] == false &&
                CurrentBlockerStates[3] == false &&
                CurrentBlockerStates[4] == false &&
                CurrentBlockerStates[5] == false &&
                CurrentBlockerStates[6] == false &&
                CurrentBlockerStates[7] == false &&
                CurrentBlockerStates[8] == false &&
                CurrentBlockerStates[11] == false &&
                CurrentBlockerStates[12] == false)
            {
                OpenSeparatorGrid(null, null);
            }
            if (CachedBlockerStates[0] == false && CurrentBlockerStates[0] == true ||
                CachedBlockerStates[1] == false && CurrentBlockerStates[1] == true ||
                CachedBlockerStates[2] == false && CurrentBlockerStates[2] == true ||
                CachedBlockerStates[3] == false && CurrentBlockerStates[3] == true ||
                CachedBlockerStates[4] == false && CurrentBlockerStates[4] == true ||
                CachedBlockerStates[5] == false && CurrentBlockerStates[5] == true ||
                CachedBlockerStates[6] == false && CurrentBlockerStates[6] == true ||
                CachedBlockerStates[7] == false && CurrentBlockerStates[7] == true ||
                CachedBlockerStates[8] == false && CurrentBlockerStates[8] == true ||
                CachedBlockerStates[11] == false && CurrentBlockerStates[11] == true ||
                CachedBlockerStates[12] == false && CurrentBlockerStates[12] == true)
            {
                CloseSeparatorGrid(null, null);
            }
            CachedBlockerStates = CurrentBlockerStates;
            var RightEnginePanels = new StackPanel[]
            {
                stpEngineRightPanel1,
                stpEngineRightPanel2,
                stpEngineRightPanel3,
                stpEngineRightPanel4,
                stpEngineRightPanel5,
                stpEngineRightPanel6,
                stpEngineRightPanel7,
                stpEngineRightPanel8,
                stpEngineRightPanel9
            };
            for (int i = 0; i < 9; i++)
            {
                bool? bState = null;
                if (i < 7)
                {
                    var CurrentEngine = Technology.FCs[i];
                    if (CurrentEngine.bEmergency)
                    {
                        bState = false;
                    }
                    else if (CurrentEngine.bPowerState)
                    {
                        bState = true;
                    }
                }
                else
                {
                    var CurrentEngine = Technology.HVPSes[8 - i];
                    if (CurrentEngine.bEmergency)
                    {
                        bState = false;
                    }
                    else if (CurrentEngine.bPowerState)
                    {
                        bState = true;
                    }
                }
                if (!bState.HasValue)
                {
                    if (!(RightEnginePanels[i].Background is SolidColorBrush)
                    || (RightEnginePanels[i].Background as SolidColorBrush).Color !=
                    Color.FromRgb(224, 224, 0))
                    {
                        RightEnginePanels[i].Background = new SolidColorBrush
                        (Color.FromRgb(224, 224, 0));
                    }
                }
                else if (bState.Value)
                {
                    if (!(RightEnginePanels[i].Background
                    is SolidColorBrush) || (RightEnginePanels[i].Background as SolidColorBrush).
                    Color != Color.FromRgb(0, 224, 0))
                    {
                        RightEnginePanels[i].Background = new SolidColorBrush
                            (Color.FromRgb(0, 224, 0));
                    }
                }
                else if (!(RightEnginePanels[i].Background is SolidColorBrush)
                    || (RightEnginePanels[i].Background as SolidColorBrush).Color !=
                    Color.FromRgb(224, 0, 0))
                {
                    RightEnginePanels[i].Background = new SolidColorBrush
                        (Color.FromRgb(224, 0, 0));
                }
            }
            if (CurrentBlockerStates[9] || CurrentBlockerStates[10])
            {
                if (!(elpSeparatorState.Fill is SolidColorBrush) ||
                    (elpSeparatorState.Fill as SolidColorBrush).Color == Colors.Red)
                {
                    elpSeparatorState.Fill = new SolidColorBrush(Colors.Red);
                }
            }
            else
            {
                if (!(elpSeparatorState.Fill is SolidColorBrush) ||
                    (elpSeparatorState.Fill as SolidColorBrush).Color == Colors.LightGray)
                {
                    elpSeparatorState.Fill = new SolidColorBrush(Colors.LightGray);
                }
            }
            if (tbkFrequencyValue1.Text != Technology.FCs[0].CurrentFrequency.ToString() +
                " " + SelectedLib["Hz"])
            {
                tbkFrequencyValue1.Text = Technology.FCs[0].CurrentFrequency.ToString() +
                    " " + SelectedLib["Hz"];
            }
            if (tbkFrequencyValue2.Text != Technology.FCs[1].CurrentFrequency.ToString() +
                " " + SelectedLib["Hz"])
            {
                tbkFrequencyValue2.Text = Technology.FCs[1].CurrentFrequency.ToString() +
                    " " + SelectedLib["Hz"];
            }
            if (tbkFrequencyValue3.Text != Technology.FCs[2].CurrentFrequency.ToString() +
                " " + SelectedLib["Hz"])
            {
                tbkFrequencyValue3.Text = Technology.FCs[2].CurrentFrequency.ToString() +
                    " " + SelectedLib["Hz"];
            }
            if (tbkFrequencyValue4.Text != Technology.FCs[3].CurrentFrequency.ToString() +
                " " + SelectedLib["Hz"])
            {
                tbkFrequencyValue4.Text = Technology.FCs[3].CurrentFrequency.ToString() +
                    " " + SelectedLib["Hz"];
            }
            if (tbkFrequencyValue5.Text != Technology.FCs[4].CurrentFrequency.ToString() +
                " " + SelectedLib["Hz"])
            {
                tbkFrequencyValue5.Text = Technology.FCs[4].CurrentFrequency.ToString() +
                    " " + SelectedLib["Hz"];
            }
            if (tbkFrequencyValue6.Text != Technology.FCs[5].CurrentFrequency.ToString() +
                " " + SelectedLib["Hz"])
            {
                tbkFrequencyValue6.Text = Technology.FCs[5].CurrentFrequency.ToString() +
                    " " + SelectedLib["Hz"];
            }
            if (tbkFrequencyValue7.Text != Technology.FCs[6].CurrentFrequency.ToString() +
                " " + SelectedLib["Hz"])
            {
                tbkFrequencyValue7.Text = Technology.FCs[6].CurrentFrequency.ToString() +
                    " " + SelectedLib["Hz"];
            }
            if (tbkCurrentValue8.Text != Technology.HVPSes[1].CurrentIn.ToString("F2") +
                " " + SelectedLib["A"])
            {
                tbkCurrentValue8.Text = Technology.HVPSes[1].CurrentIn.ToString("F2") +
                    " " + SelectedLib["A"];
            }
            if (tbkCurrentValue9.Text != Technology.HVPSes[0].CurrentIn.ToString("F2") +
                " " + SelectedLib["mA"])
            {
                tbkCurrentValue9.Text = Technology.HVPSes[0].CurrentIn.ToString("F2") +
                    " " + SelectedLib["mA"];
            }
            if (tbkVoltageValue9.Text != Technology.HVPSes[0].VoltageIn.ToString("F2") +
                " " + SelectedLib["kV"])
            {
                tbkVoltageValue9.Text = Technology.HVPSes[0].VoltageIn.ToString("F2") +
                    " " + SelectedLib["kV"];
            }
            grdManualLimitsDisabled.Visibility = Program.bUnlimitedManual ?
                Visibility.Visible : Visibility.Hidden;
            Engine CurrentControlBoxEngine = null;
            if (CurrentFCIndex >= 0 && CurrentFCIndex < 100)
            {
                CurrentControlBoxEngine = Technology.FCs[CurrentFCIndex];
            }
            else if (CurrentFCIndex >= 100)
            {
                CurrentControlBoxEngine = Technology.HVPSes[CurrentFCIndex - 100];
            }
            if (CurrentControlBoxEngine != null)
            {
                if (CurrentControlBoxEngine.bPowerState)
                {
                    if (!(brdFCControlBox.Background is SolidColorBrush) ||
                        (brdFCControlBox.Background as SolidColorBrush).Color !=
                        Color.FromRgb(0, 200, 0))
                    {
                        brdFCControlBox.Background = new SolidColorBrush(Color.FromRgb(0, 200, 0));
                    }
                }
                else if (CurrentControlBoxEngine.bEmergency)
                {
                    if (!(brdFCControlBox.Background is SolidColorBrush) ||
                        (brdFCControlBox.Background as SolidColorBrush).Color !=
                        Color.FromRgb(200, 0, 0))
                    {
                        brdFCControlBox.Background = new SolidColorBrush(Color.FromRgb(200, 0, 0));
                    }
                }
                else if (CurrentControlBoxEngine is FrequencyConverter &&
                    !(CurrentControlBoxEngine as FrequencyConverter).bConnected)
                {
                    if (!(brdFCControlBox.Background is SolidColorBrush) ||
                        (brdFCControlBox.Background as SolidColorBrush).Color !=
                        Color.FromRgb(200, 100, 0))
                    {
                        brdFCControlBox.Background = new SolidColorBrush(Color.FromRgb(200, 100, 0));
                    }
                }
                else
                {
                    if (!(brdFCControlBox.Background is SolidColorBrush) ||
                        (brdFCControlBox.Background as SolidColorBrush).Color !=
                        Color.FromRgb(200, 200, 0))
                    {
                        brdFCControlBox.Background = new SolidColorBrush(Color.FromRgb(200, 200, 0));
                    }
                }
            }
        }

        public void UpdateManualModePanel()
        {
            Button[] ContactorStateLabels =
            {
                lblContactorStateLabel0_1,
                lblContactorStateLabel1_1,
                lblContactorStateLabel2_1,
                lblContactorStateLabel3_1,
                lblContactorStateLabel4_1,
                lblContactorStateLabel5_1,
                lblContactorStateLabel6_1,
                lblContactorStateLabel7_1,
                lblContactorStateLabel8_1,
                lblContactorStateLabel9_1
            };
            for (int i = 0; i < 10; i++)
            {
                var CurrentLabel = ContactorStateLabels[i];
                var CurrentContactor = Technology.Contactors[i];
                if (CurrentContactor.bEmergency)
                {
                    if (CurrentLabel.Content.ToString() != SelectedLib["Emergency"])
                    {
                        CurrentLabel.Foreground = new SolidColorBrush(Colors.DarkRed);
                        CurrentLabel.Content = SelectedLib["Emergency"];
                    }
                }
                else if (CurrentContactor.bInitialState)
                {
                    if (CurrentContactor.bPowerState)
                    {
                        if (CurrentLabel.Content.ToString() != SelectedLib["Launched"])
                        {
                            CurrentLabel.Foreground = new SolidColorBrush(Colors.Green);
                            CurrentLabel.Content = SelectedLib["Launched"];
                        }
                    }
                    else if (CurrentLabel.Content.ToString() != SelectedLib["Standby"])
                    {
                        CurrentLabel.Foreground = new SolidColorBrush
                            (Color.FromArgb(255, 128, 128, 0));
                        CurrentLabel.Content = SelectedLib["Standby"];
                    }
                }
                else if (CurrentLabel.Content.ToString() != SelectedLib["Stopped"])
                {
                    CurrentLabel.Foreground = new SolidColorBrush(Colors.Black);
                    CurrentLabel.Content = SelectedLib["Stopped"];
                }
            }
            Button[] FCStateLabels =
            {
                lblFCStateLabel0_1,
                lblFCStateLabel1_1,
                lblFCStateLabel2_1,
                lblFCStateLabel3_1,
                lblFCStateLabel4_1,
                lblFCStateLabel5_1,
                lblFCStateLabel6_1,
            };
            for (int i = 0; i < 7; i++)
            {
                var CurrentLabel = FCStateLabels[i];
                var CurrentFC = Technology.FCs[i];
                if (CurrentFC.bEmergency)
                {
                    if (CurrentLabel.Content.ToString() != SelectedLib["Emergency"] + " " + CurrentFC.ErrorCode)
                    {
                        CurrentLabel.Foreground = new SolidColorBrush(Colors.DarkRed);
                        CurrentLabel.Content = SelectedLib["Emergency"] + " " + CurrentFC.ErrorCode;
                    }
                }
                else if (CurrentFC.bInitialState)
                {
                    if (CurrentFC.bPowerState)
                    {
                        if (CurrentLabel.Content.ToString() != SelectedLib["Launched"])
                        {
                            CurrentLabel.Foreground = new SolidColorBrush(Colors.Green);
                            CurrentLabel.Content = SelectedLib["Launched"];
                        }
                    }
                    else if (CurrentLabel.Content.ToString() !=
                        SelectedLib["Standby"])
                    {
                        CurrentLabel.Foreground = new SolidColorBrush
                            (Color.FromArgb(255, 128, 128, 0));
                        CurrentLabel.Content = SelectedLib["Standby"];
                    }
                }
                else if (CurrentLabel.Content.ToString() != SelectedLib["Stopped"])
                {
                    CurrentLabel.Foreground = new SolidColorBrush(Colors.Black);
                    CurrentLabel.Content = SelectedLib["Stopped"];
                }
            }
            Button[] HVPSStateLabels =
            {
                lblHVPSStateLabel0_1,
                lblHVPSStateLabel1_1
            };
            HVPS CurrentHVPS;
            for (int i = 0; i < 2; i++)
            {
                var CurrentLabel = HVPSStateLabels[i];
                CurrentHVPS = Technology.HVPSes[i];
                if (CurrentHVPS.bEmergency)
                {
                    if (CurrentLabel.Content.ToString() != SelectedLib["Emergency"])
                    {
                        CurrentLabel.Foreground = new SolidColorBrush(Colors.DarkRed);
                        CurrentLabel.Content = SelectedLib["Emergency"];
                    }
                }
                else if (CurrentHVPS.bInitialState)
                {
                    if (CurrentHVPS.bPowerState)
                    {
                        if (CurrentLabel.Content.ToString() != SelectedLib["Launched"])
                        {
                            CurrentLabel.Foreground = new SolidColorBrush(Colors.Green);
                            CurrentLabel.Content = SelectedLib["Launched"];
                        }
                    }
                    else if (CurrentLabel.Content.ToString() !=
                        SelectedLib["Standby"])
                    {
                        CurrentLabel.Foreground = new SolidColorBrush
                            (Color.FromArgb(255, 128, 128, 0));
                        CurrentLabel.Content = SelectedLib["Standby"];
                    }
                }
                else if (CurrentLabel.Content.ToString() != SelectedLib["Stopped"])
                {
                    CurrentLabel.Foreground = new SolidColorBrush(Colors.Black);
                    CurrentLabel.Content = SelectedLib["Stopped"];
                }
            }
            try
            {
                if (lblInCurrent0Value_1.Content.ToString() != Technology.HVPSes[0].CurrentIn.
                    ToString("F2", CultureInfo.InvariantCulture))
                {
                    lblInCurrent0Value_1.Content = Technology.HVPSes[0].CurrentIn.
                        ToString("F2", CultureInfo.InvariantCulture);
                }
                if (lblInCurrent1Value_1.Content.ToString() != Technology.HVPSes[1].CurrentIn.
                    ToString("F2", CultureInfo.InvariantCulture))
                {
                    lblInCurrent1Value_1.Content = Technology.HVPSes[1].CurrentIn.
                        ToString("F2", CultureInfo.InvariantCulture);
                }
                if (lblInVoltage0Value_1.Content.ToString() != Technology.HVPSes[0].VoltageIn.
                    ToString("F2", CultureInfo.InvariantCulture))
                {
                    lblInVoltage0Value_1.Content = Technology.HVPSes[0].VoltageIn.
                        ToString("F2", CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                lblInCurrent0Value_1.Content = "";
                lblInCurrent1Value_1.Content = "";
                lblInVoltage0Value_1.Content = "";
            }
            Image[] BlockerImages =
            {
                imgBlockerIndicator0_1,
                imgBlockerIndicator1_1,
                imgBlockerIndicator2_1,
                imgBlockerIndicator3_1,
                imgBlockerIndicator4_1,
                imgBlockerIndicator5_1
            };
            for (int i = 0; i < 6; i++)
            {
                var CurrentImage = BlockerImages[i];
                var CurrentBlocker = Technology.Lockers[i];
                if (CurrentBlocker.bState)
                {
                    if (CurrentImage.Source != Settings.IndicatorGreen)
                    {
                        CurrentImage.Source = Settings.IndicatorGreen;
                    }
                }
                else
                {
                    if (CurrentImage.Source != Settings.IndicatorRed)
                    {
                        CurrentImage.Source = Settings.IndicatorRed;
                    }
                }
            }
        }

        public void UpdateCalibrationSettingsPanel()
        {

        }

        public void UpdateLanguageSettingsPanel()
        {

        }

        public void UpdateTechnologySettingsPanel()
        {

        }

        public void UpdateInterface()
        {
            if (grdAutoModeGrid.Visibility == Visibility.Visible)
            {
                UpdateAutoModePanel();
            }
            if(grdManualModeGrid.Visibility == Visibility.Visible)
            {
                UpdateManualModePanel();
            }
            if(grdSettingsGrid.Visibility == Visibility.Visible)
            {
                if(scvCalibrationSettingsViewer.Visibility == Visibility.Visible)
                {
                    UpdateCalibrationSettingsPanel();
                }
                if(scvLanguageSettingsViewer.Visibility == Visibility.Visible)
                {
                    UpdateLanguageSettingsPanel();
                }
                if(scvTechnologySettingsViewer.Visibility == Visibility.Visible)
                {
                    UpdateTechnologySettingsPanel();
                }
            }
            if(bShowModbusDOFrames)
            {
                lblLastModbusDOFrame.Visibility = Visibility.Visible;
                lblLastModbusDOFrame.Content = CommunicationLoop.ModbusBuffers[1].
                    LastSentCommand;
            }
            else if(bShowWorkIndicationVariables)
            {
                lblLastModbusDOFrame.Visibility = Visibility.Visible;
                lblLastModbusDOFrame.Content = Technology.bCloneWorkIndication.ToString() +
                    " " + Technology.WorkIndicationSignal.GetBit().ToString();
            }
            else
            {
                lblLastModbusDOFrame.Visibility = Visibility.Collapsed;
            }
            while(LogQueue.Count > 0)
            {
                Log(LogQueue[0]);
                LogQueue.RemoveAt(0);
            }
            /*Label[] CleanerStateLabels =
            {
                lblCleanerStateLabel0_1,
                lblCleanerStateLabel1_1
            };
            for(int i = 0; i < 2; i++)
            {
                var CurrentLabel = CleanerStateLabels[i];
                var CurrentCleaner = TechnologyProcessLoop.Cleaners[i];
                if (CurrentCleaner.bEmergency)
                {
                    CurrentLabel.Foreground = new SolidColorBrush(Colors.DarkRed);
                    CurrentLabel.Content = SelectedLib["Emergency"];
                }
                else if (CurrentCleaner.bInitialState)
                {
                    if (!CurrentCleaner.bPowerState)
                    {
                        CurrentLabel.Foreground = new SolidColorBrush
                            (Color.FromArgb(255, 128, 128, 0));
                        CurrentLabel.Content = SelectedLib["Standby"];
                    }
                    else if (CurrentCleaner.bDesiredLocation)
                    {
                        CurrentLabel.Foreground = new SolidColorBrush(Colors.Green);
                        CurrentLabel.Content = SelectedLib["Launched in direction 1"];
                    }
                    else
                    {
                        CurrentLabel.Foreground = new SolidColorBrush(Colors.Green);
                        CurrentLabel.Content = SelectedLib["Launched in direction 2"];
                    }
                }
                else
                {
                    CurrentLabel.Foreground = new SolidColorBrush(Colors.Black);
                    CurrentLabel.Content = SelectedLib["Stopped"];
                }
            }*/
            if (lblMotorHourLabel.Content.ToString() != SelectedLib["Motorhours"] + ": " +
                Technology.MotorTime.Hours)
            {
                lblMotorHourLabel.Content = SelectedLib["Motorhours"] + ": " +
                    Technology.MotorTime.Hours;
            }
            lock (Technology.StateStringLock)
            {
                try
                {
                    if (lblStateLabel.Content.ToString() != Technology.StateString)
                    {
                        lblStateLabel.Content = Technology.StateString;
                    }
                }
                catch { }
            }
            if (lblTimeLabel.Content.ToString() != DateTime.Now.ToShortDateString() + " " +
                DateTime.Now.ToLongTimeString())
            {
                lblTimeLabel.Content = DateTime.Now.ToShortDateString() + " " +
                    DateTime.Now.ToLongTimeString();
            }
            rctModbusHealthIndicator.Visibility = Visibility.Collapsed;
            rctUSSHealthIndicator.Visibility = Visibility.Collapsed;
            lblModbusHealthIndicator.Visibility = Visibility.Collapsed;
            lblUSSHealthIndicator.Visibility = Visibility.Collapsed;
            if (CommHealthDisplayMode == ECommHealthDisplayMode.Numbers)
            {
                var HealthList = new List<decimal>();
                var IndicatorList = new List<Label>();
                if (CommunicationLoop.CommDebugMode == ECommDebugMode.Modbus ||
                    CommunicationLoop.CommDebugMode == ECommDebugMode.Both)
                {
                    HealthList.Add(CommunicationLoop.ModbusCommHealth);
                    IndicatorList.Add(lblModbusHealthIndicator);
                }
                if (CommunicationLoop.CommDebugMode == ECommDebugMode.USS ||
                    CommunicationLoop.CommDebugMode == ECommDebugMode.Both)
                {
                    HealthList.Add(CommunicationLoop.USSCommHealth);
                    IndicatorList.Add(lblUSSHealthIndicator);
                }
                for (int i = 0; i < HealthList.Count; i++)
                {
                    decimal FinalPercent;
                    try
                    {
                        FinalPercent = Math.Round
                        (HealthList[i] * 100, 2, MidpointRounding.AwayFromZero);
                    }
                    catch
                    {
                        FinalPercent = 0;
                    }
                    if (IndicatorList[i].Content.ToString() != FinalPercent.ToString() + "%")
                    {
                        IndicatorList[i].Content = FinalPercent.ToString() + "%";
                    }
                    byte R = 0, G = 0, B = 0;
                    IndicatorList[i].Visibility = Visibility.Visible;
                    if (HealthList[i] == 1)
                    {
                        G = 128;
                    }
                    else if (HealthList[i] >= 0.95m)
                    {
                        decimal FinalProduct = 1 - HealthList[i];
                        FinalProduct *= 64m / 0.05m;
                        R = (byte)FinalProduct;
                        G = 128;
                    }
                    else if (HealthList[i] >= 0.9m)
                    {
                        decimal FinalProduct = 0.95m - HealthList[i];
                        FinalProduct *= 64m / 0.05m;
                        R = 128;
                        G = (byte)(128 - FinalProduct);
                    }
                    else
                    {
                        decimal FinalProduct = HealthList[i];
                        FinalProduct *= 128m / 0.9m;
                        R = (byte)FinalProduct;
                    }
                    if (!(IndicatorList[i].Foreground is SolidColorBrush) ||
                        (IndicatorList[i].Foreground as SolidColorBrush).Color !=
                        Color.FromRgb(R, G, B))
                    {
                        IndicatorList[i].Foreground = new SolidColorBrush(Color.FromRgb(R, G, B));
                    }
                }
            }
            else if (CommHealthDisplayMode == ECommHealthDisplayMode.Indicators)
            {
                var HealthList = new List<decimal>();
                var IndicatorList = new List<Rectangle>();
                if (CommunicationLoop.CommDebugMode == ECommDebugMode.Modbus ||
                    CommunicationLoop.CommDebugMode == ECommDebugMode.Both)
                {
                    HealthList.Add(CommunicationLoop.ModbusCommHealth);
                    IndicatorList.Add(rctModbusHealthIndicator);
                }
                if (CommunicationLoop.CommDebugMode == ECommDebugMode.USS ||
                    CommunicationLoop.CommDebugMode == ECommDebugMode.Both)
                {
                    HealthList.Add(CommunicationLoop.USSCommHealth);
                    IndicatorList.Add(rctUSSHealthIndicator);
                }
                for(int i = 0; i < HealthList.Count; i++)
                {
                    byte R = 0, G = 0, B = 0;
                    IndicatorList[i].Visibility = Visibility.Visible;
                    if (HealthList[i] == 1)
                    {
                        G = 128;
                    }
                    else if (HealthList[i] >= 0.95m)
                    {
                        decimal FinalProduct = 1 - HealthList[i];
                        FinalProduct *= 64m / 0.05m;
                        R = (byte)FinalProduct;
                        G = 128;
                    }
                    else if (HealthList[i] >= 0.9m)
                    {
                        decimal FinalProduct = 0.95m - HealthList[i];
                        FinalProduct *= 64m / 0.05m;
                        R = 128;
                        G = (byte)(128 - FinalProduct);
                    }
                    else
                    {
                        decimal FinalProduct = HealthList[i];
                        FinalProduct *= 128m / 0.9m;
                        R = (byte)FinalProduct;
                    }
                    if (!(IndicatorList[i].Fill is SolidColorBrush) ||
                        (IndicatorList[i].Fill as SolidColorBrush).Color !=
                        Color.FromRgb(R, G, B))
                    {
                        IndicatorList[i].Fill = new SolidColorBrush(Color.FromRgb(R, G, B));
                    }
                }
            }
            rctWorkIndicator.Visibility = bWorkIndicationDebugMode ?
                Visibility.Visible : Visibility.Collapsed;
            if(bWorkIndicationDebugMode)
            {
                if (!(rctWorkIndicator.Fill is SolidColorBrush) ||
                    (rctWorkIndicator.Fill as SolidColorBrush).Color !=
                    (Technology.WorkIndicationSignal.GetBit() ? Colors.Lime : Colors.Red))
                {
                    rctWorkIndicator.Fill = Technology.WorkIndicationSignal.GetBit() ?
                        new SolidColorBrush(Colors.Lime) : new SolidColorBrush(Colors.Red);
                }
            }
        }

        public void UpdateSettingsControls()
        {
            pwdAdminPasswordSource.Password = Settings.AdminPassword;
            //cbxCleaner0Enabled.IsChecked = TechnologyProcessLoop.Cleaners[0].bEnabled;
            //cbxCleaner1Enabled.IsChecked = TechnologyProcessLoop.Cleaners[1].bEnabled;
            cbxContactor0Enabled.IsChecked = Technology.Contactors[0].bEnabled;
            cbxContactor1Enabled.IsChecked = Technology.Contactors[1].bEnabled;
            cbxContactor2Enabled.IsChecked = Technology.Contactors[2].bEnabled;
            cbxContactor3Enabled.IsChecked = Technology.Contactors[3].bEnabled;
            cbxContactor4Enabled.IsChecked = Technology.Contactors[4].bEnabled;
            cbxContactor5Enabled.IsChecked = Technology.Contactors[5].bEnabled;
            cbxContactor6Enabled.IsChecked = Technology.Contactors[6].bEnabled;
            cbxContactor7Enabled.IsChecked = Technology.Contactors[7].bEnabled;
            cbxContactor8Enabled.IsChecked = Technology.Contactors[8].bEnabled;
            cbxContactor9Enabled.IsChecked = Technology.Contactors[9].bEnabled;
            cbxFC0Enabled.IsChecked = Technology.FCs[0].bEnabled;
            cbxFC1Enabled.IsChecked = Technology.FCs[1].bEnabled;
            cbxFC2Enabled.IsChecked = Technology.FCs[2].bEnabled;
            cbxFC3Enabled.IsChecked = Technology.FCs[3].bEnabled;
            cbxFC4Enabled.IsChecked = Technology.FCs[4].bEnabled;
            cbxFC5Enabled.IsChecked = Technology.FCs[5].bEnabled;
            cbxFC6Enabled.IsChecked = Technology.FCs[6].bEnabled;
            //cbxFC7Enabled.IsChecked = TechnologyProcessLoop.FCs[7].bEnabled;
            //cbxFC8Enabled.IsChecked = TechnologyProcessLoop.FCs[8].bEnabled;
            cbxHVPS0Enabled.IsChecked = Technology.HVPSes[0].bEnabled;
            cbxHVPS1Enabled.IsChecked = Technology.HVPSes[1].bEnabled;
            cbxRevertPolarity0.IsChecked = Technology.HVPSes[0].bPolarity;
            cbxRevertPolarity1.IsChecked = Technology.HVPSes[1].bPolarity;
            /*txtCleanerStartDelay0.Text = TechnologyProcessLoop.Cleaners[0].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtCleanerStartDelay1.Text = TechnologyProcessLoop.Cleaners[1].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtCleanerStopDelay0.Text = TechnologyProcessLoop.Cleaners[0].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtCleanerStopDelay1.Text = TechnologyProcessLoop.Cleaners[1].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);*/
            txtContactorStartDelay0.Text = Technology.Contactors[0].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStartDelay1.Text = Technology.Contactors[1].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStartDelay2.Text = Technology.Contactors[2].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStartDelay3.Text = Technology.Contactors[3].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStartDelay4.Text = Technology.Contactors[4].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStartDelay5.Text = Technology.Contactors[5].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStartDelay6.Text = Technology.Contactors[6].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStartDelay7.Text = Technology.Contactors[7].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStartDelay8.Text = Technology.Contactors[8].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStartDelay9.Text = Technology.Contactors[9].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStopDelay0.Text = Technology.Contactors[0].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStopDelay1.Text = Technology.Contactors[1].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStopDelay2.Text = Technology.Contactors[2].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStopDelay3.Text = Technology.Contactors[3].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStopDelay4.Text = Technology.Contactors[4].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStopDelay5.Text = Technology.Contactors[5].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStopDelay6.Text = Technology.Contactors[6].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStopDelay7.Text = Technology.Contactors[7].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStopDelay8.Text = Technology.Contactors[8].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtContactorStopDelay9.Text = Technology.Contactors[9].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFCStartDelay0.Text = Technology.FCs[0].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFCStartDelay1.Text = Technology.FCs[1].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFCStartDelay2.Text = Technology.FCs[2].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFCStartDelay3.Text = Technology.FCs[3].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFCStartDelay4.Text = Technology.FCs[4].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFCStartDelay5.Text = Technology.FCs[5].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFCStartDelay6.Text = Technology.FCs[6].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            /*txtFCStartDelay7.Text = TechnologyProcessLoop.FCs[7].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFCStartDelay8.Text = TechnologyProcessLoop.FCs[8].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);*/
            txtFCStopDelay0.Text = Technology.FCs[0].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFCStopDelay1.Text = Technology.FCs[1].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFCStopDelay2.Text = Technology.FCs[2].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFCStopDelay3.Text = Technology.FCs[3].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFCStopDelay4.Text = Technology.FCs[4].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFCStopDelay5.Text = Technology.FCs[5].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFCStopDelay6.Text = Technology.FCs[6].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            /*txtFCStopDelay7.Text = TechnologyProcessLoop.FCs[7].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFCStopDelay8.Text = TechnologyProcessLoop.FCs[8].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);*/
            txtFrequency0_1.Text = Technology.FCs[0].CurrentFrequency.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFrequency1_1.Text = Technology.FCs[1].CurrentFrequency.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFrequency2_1.Text = Technology.FCs[2].CurrentFrequency.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFrequency3_1.Text = Technology.FCs[3].CurrentFrequency.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFrequency4_1.Text = Technology.FCs[4].CurrentFrequency.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFrequency5_1.Text = Technology.FCs[5].CurrentFrequency.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFrequency6_1.Text = Technology.FCs[6].CurrentFrequency.
                ToString("0.##", CultureInfo.InvariantCulture);
            /*txtFrequency7_1.Text = TechnologyProcessLoop.FCs[7].CurrentFrequency.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtFrequency8_1.Text = TechnologyProcessLoop.FCs[8].CurrentFrequency.
                ToString("0.##", CultureInfo.InvariantCulture);*/
            txtHVPSStartDelay0.Text = Technology.HVPSes[0].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtHVPSStartDelay1.Text = Technology.HVPSes[1].StartDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtHVPSStopDelay0.Text = Technology.HVPSes[0].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtHVPSStopDelay1.Text = Technology.HVPSes[1].StopDelay.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtImpulse0.Text = Technology.Contactors[0].Impulse.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtImpulse1.Text = Technology.Contactors[1].Impulse.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtImpulse2.Text = Technology.Contactors[2].Impulse.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtImpulse3.Text = Technology.Contactors[3].Impulse.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtImpulse4.Text = Technology.Contactors[4].Impulse.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtImpulse5.Text = Technology.Contactors[5].Impulse.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtImpulse6.Text = Technology.Contactors[6].Impulse.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtImpulse7.Text = Technology.Contactors[7].Impulse.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtImpulse8.Text = Technology.Contactors[8].Impulse.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtImpulse9.Text = Technology.Contactors[9].Impulse.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtMaxFrequency0.Text = Technology.FCs[0].CalibrationMaxFrequency
                .ToString();
            txtMaxFrequency1.Text = Technology.FCs[1].CalibrationMaxFrequency
                .ToString();
            txtMaxFrequency2.Text = Technology.FCs[2].CalibrationMaxFrequency
                .ToString();
            txtMaxFrequency3.Text = Technology.FCs[3].CalibrationMaxFrequency
                .ToString();
            txtMaxFrequency4.Text = Technology.FCs[4].CalibrationMaxFrequency
                .ToString();
            txtMaxFrequency5.Text = Technology.FCs[5].CalibrationMaxFrequency
                .ToString();
            txtMaxFrequency6.Text = Technology.FCs[6].CalibrationMaxFrequency
                .ToString();
            /*txtMaxFrequency7.Text = TechnologyProcessLoop.FCs[7].CalibrationMaxFrequency
                .ToString();
            txtMaxFrequency8.Text = TechnologyProcessLoop.FCs[8].CalibrationMaxFrequency
                .ToString();*/
            txtMinFrequency0.Text = Technology.FCs[0].CalibrationMinFrequency
                .ToString();
            txtMinFrequency1.Text = Technology.FCs[1].CalibrationMinFrequency
                .ToString();
            txtMinFrequency2.Text = Technology.FCs[2].CalibrationMinFrequency
                .ToString();
            txtMinFrequency3.Text = Technology.FCs[3].CalibrationMinFrequency
                .ToString();
            txtMinFrequency4.Text = Technology.FCs[4].CalibrationMinFrequency
                .ToString();
            txtMinFrequency5.Text = Technology.FCs[5].CalibrationMinFrequency
                .ToString();
            txtMinFrequency6.Text = Technology.FCs[6].CalibrationMinFrequency
                .ToString();
            /*txtMinFrequency7.Text = TechnologyProcessLoop.FCs[7].CalibrationMinFrequency
                .ToString();
            txtMinFrequency8.Text = TechnologyProcessLoop.FCs[8].CalibrationMinFrequency
                .ToString();*/
            txtMaxInCurrent0.Text = Technology.HVPSes[0].MaxmA.
                ToString("0.##",CultureInfo.InvariantCulture);
            txtMaxInCurrent0R.Text = Technology.HVPSes[0].ReverseMaxmA.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtMaxInCurrent1.Text = Technology.HVPSes[1].MaxmA.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtMaxInVoltage0.Text = Technology.HVPSes[0].RealInputAt30kV.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtMaxInVoltage0R.Text = Technology.HVPSes[0].ReverseRealInputAt30kV.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtMaxOutVoltage0.Text = Technology.HVPSes[0].MaxOutput.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtMaxOutVoltage0R.Text = Technology.HVPSes[0].ReverseMaxOutput.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtMaxOutVoltage1.Text = Technology.HVPSes[1].MaxOutput.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtMinInVoltage0.Text = Technology.HVPSes[0].RealInputAt0kV.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtMinInVoltage0R.Text = Technology.HVPSes[0].ReverseRealInputAt0kV.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtMinOutVoltage0.Text = Technology.HVPSes[0].MinOutput.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtMinOutVoltage0R.Text = Technology.HVPSes[0].ReverseMinOutput.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtMinOutVoltage1.Text = Technology.HVPSes[1].MinOutput.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtOutVoltage0_1.Text = Technology.HVPSes[0].VoltageOut.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtOutVoltage1_1.Text = Technology.HVPSes[1].VoltageOut.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtPause0.Text = Technology.Contactors[0].Pause.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtPause1.Text = Technology.Contactors[1].Pause.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtPause2.Text = Technology.Contactors[2].Pause.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtPause3.Text = Technology.Contactors[3].Pause.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtPause4.Text = Technology.Contactors[4].Pause.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtPause5.Text = Technology.Contactors[5].Pause.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtPause6.Text = Technology.Contactors[6].Pause.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtPause7.Text = Technology.Contactors[7].Pause.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtPause8.Text = Technology.Contactors[8].Pause.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtPause9.Text = Technology.Contactors[9].Pause.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtSensorVoltageAt0mA0.Text = Technology.HVPSes[0].RealInputAt0mA.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtSensorVoltageAt0mA0R.Text = Technology.HVPSes[0].ReverseRealInputAt0mA.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtSensorVoltageAt0mA1.Text = Technology.HVPSes[1].RealInputAt0mA.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtSensorVoltageAtMaxmA0.Text = Technology.HVPSes[0].RealInputAtMaxmA.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtSensorVoltageAtMaxmA0R.Text = Technology.HVPSes[0].
                ReverseRealInputAtMaxmA.ToString("0.##", CultureInfo.InvariantCulture);
            txtSensorVoltageAtMaxmA1.Text = Technology.HVPSes[1].RealInputAtMaxmA.
                ToString("0.##", CultureInfo.InvariantCulture);
            /*txtSwipeDuration0.Text = TechnologyProcessLoop.Cleaners[0].MaxCleanTimeout.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtSwipeDuration1.Text = TechnologyProcessLoop.Cleaners[1].MaxCleanTimeout.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtSwipePause0.Text = TechnologyProcessLoop.Cleaners[0].CleanPause.
                ToString("0.##", CultureInfo.InvariantCulture);
            txtSwipePause1.Text = TechnologyProcessLoop.Cleaners[1].CleanPause.
                ToString("0.##", CultureInfo.InvariantCulture);*/
            txtOutVoltage0_1.Text = Technology.HVPSes[0].VoltageOut.
                ToString("F2", CultureInfo.InvariantCulture);
            txtOutVoltage1_1.Text = Technology.HVPSes[1].VoltageOut.
                ToString("F2", CultureInfo.InvariantCulture);
            txtFrequency0_1.Text = Technology.FCs[0].CurrentFrequency.
                ToString();
            txtFrequency1_1.Text = Technology.FCs[1].CurrentFrequency.
                ToString();
            txtFrequency2_1.Text = Technology.FCs[2].CurrentFrequency.
                ToString();
            txtFrequency3_1.Text = Technology.FCs[3].CurrentFrequency.
                ToString();
            txtFrequency4_1.Text = Technology.FCs[4].CurrentFrequency.
                ToString();
            txtFrequency5_1.Text = Technology.FCs[5].CurrentFrequency.
                ToString();
            txtFrequency6_1.Text = Technology.FCs[6].CurrentFrequency.
                ToString();
        }

        public enum ELanguage
        {
            English,
            Russian,
            Ukrainian,
            Custom
        };

        public void SetLanguage(ELanguage Language)
        {
            switch (Language)
            {
                case ELanguage.Russian:
                    SelectedLib = MUI.RussianLib;
                    break;
                case ELanguage.Ukrainian:
                    SelectedLib = MUI.UkrainianLib;
                    break;
                case ELanguage.Custom:
                    SelectedLib = MUI.CustomLib;
                    break;
                case ELanguage.English:
                default:
                    SelectedLib = MUI.EnglishLib;
                    break;
            }
            var Controls = new Dictionary<UIElement, string>();
            if(FCControlBoxNameKey != "")
            {
                Controls.Add(lblFCControlBoxHeader, FCControlBoxNameKey);
            }
            Controls.Add(tbkElectrostaticSeparator, "Electrostatic Separator");
            Controls.Add(tbkEngineName1, "Load Screw 1");
            Controls.Add(tbkEngineName2, "Load Screw 2.1");
            Controls.Add(tbkEngineName3, "Load Screw 2.2");
            Controls.Add(tbkEngineName4, "Load Screw 3");
            Controls.Add(tbkEngineName5, "Zig-zag Off-center Fan");
            Controls.Add(tbkEngineName6, "Settling Electrode");
            Controls.Add(tbkEngineName7, "Distributing Screw");
            Controls.Add(tbkEngineName8, "Vibrofeeder");
            Controls.Add(tbkEngineName9, "HVPS");
            Controls.Add(tbkManualLimitsDisabled, "Manual Mode Limitations Disabled");
            Controls.Add(tbkResponsible, "The operator is fully responsible for any actions" +
                " taken in this mode.");
            Controls.Add(lblAdminPassword, "AdminPassword");
            Controls.Add(lblBlockers1, "Blockers");
            Controls.Add(lblBlockingSystem, "Blocking System");
            Controls.Add(lblBlockerMessage1, "Blocker Message 1");
            Controls.Add(lblBlockerMessage2, "Blocker Message 2");
            Controls.Add(lblBlockerMessage3, "Blocker Message 3");
            Controls.Add(lblBlockerMessage4, "Blocker Message 4");
            Controls.Add(lblBlockerMessage5, "Blocker Message 5");
            Controls.Add(lblBlockerMessage6, "Blocker Message 6");
            Controls.Add(lblBlockerMessage7, "Blocker Message 7");
            Controls.Add(lblBlockerMessage8, "Blocker Message 8");
            Controls.Add(lblBlockerMessage9, "Blocker Message 9");
            Controls.Add(lblBlockerMessage10, "Blocker Message 10");
            Controls.Add(lblBlockerMessage11, "Blocker Message 11");
            Controls.Add(lblBlockerMessage12, "Blocker Message 12");
            Controls.Add(lblBlockerMessage13, "Blocker Message 13");
            Controls.Add(lblCleaners0, "Cleaners");
            Controls.Add(lblContactors0, "Contactors");
            Controls.Add(lbldC2, "dC");
            Controls.Add(lbldC3, "dC");
            Controls.Add(lblFCs0, "FCs");
            Controls.Add(lblFC0Name, "FC 1");
            Controls.Add(lblFC1Name, "FC 2");
            Controls.Add(lblFC2Name, "FC 3");
            Controls.Add(lblFC3Name, "FC 4");
            Controls.Add(lblFC4Name, "FC 5");
            Controls.Add(lblFC5Name, "FC 6");
            Controls.Add(lblFC6Name, "FC 7");
            Controls.Add(lblFrequency9, "Frequency");
            Controls.Add(lblFrequency10, "Frequency");
            Controls.Add(lblFrequency11, "Frequency");
            Controls.Add(lblFrequency12, "Frequency");
            Controls.Add(lblFrequency13, "Frequency");
            Controls.Add(lblFrequency14, "Frequency");
            Controls.Add(lblFrequency15, "Frequency");
            Controls.Add(lblFrequency16, "Frequency");
            Controls.Add(lblFrequency17, "Frequency");
            Controls.Add(lblHumidity, "Humidity");
            Controls.Add(lblHumidity2, "Humidity 1");
            Controls.Add(lblHumidity3, "Humidity 2");
            Controls.Add(lblHVPSes0, "HVPSes");
            Controls.Add(lblHVPS0Name, "HVPS");
            Controls.Add(lblVibrofeederName, "Vibrofeeder");
            Controls.Add(lblHz0, "Hz");
            Controls.Add(lblHz9, "Hz");
            Controls.Add(lblHz10, "Hz");
            Controls.Add(lblHz11, "Hz");
            Controls.Add(lblHz12, "Hz");
            Controls.Add(lblHz13, "Hz");
            Controls.Add(lblHz14, "Hz");
            Controls.Add(lblHz15, "Hz");
            Controls.Add(lblHz16, "Hz");
            Controls.Add(lblHz17, "Hz");
            Controls.Add(lblImpulse0, "Impulse");
            Controls.Add(lblImpulse1, "Impulse");
            Controls.Add(lblImpulse2, "Impulse");
            Controls.Add(lblImpulse3, "Impulse");
            Controls.Add(lblImpulse4, "Impulse");
            Controls.Add(lblImpulse5, "Impulse");
            Controls.Add(lblImpulse6, "Impulse");
            Controls.Add(lblImpulse7, "Impulse");
            Controls.Add(lblImpulse8, "Impulse");
            Controls.Add(lblImpulse9, "Impulse");
            Controls.Add(lblInCurrent2, "Input current");
            Controls.Add(lblInCurrent3, "Input current");
            Controls.Add(lblInside, "Inside");
            Controls.Add(lblInVoltage2, "Input voltage");
            Controls.Add(lblkV4, "kV");
            Controls.Add(lblkV5, "kV");
            Controls.Add(lblA0, "A");
            Controls.Add(lblmA2, "mA");
            Controls.Add(lblA1, "A");
            Controls.Add(lblMaxFrequency, "Max frequency");
            Controls.Add(lblMaxInCurrent, "Max input current");
            Controls.Add(lblMaxInVoltage, "Max input voltage");
            Controls.Add(lblMaxOutVoltage, "Max output voltage");
            Controls.Add(lblMinFrequency, "Min frequency");
            Controls.Add(lblMinInVoltage, "Min input voltage");
            Controls.Add(lblMinOutVoltage, "Min output voltage");
            Controls.Add(lblOutside, "Outside");
            Controls.Add(lblOutVoltage2, "Output voltage");
            Controls.Add(lblOutCurrent1, "Output Current");
            Controls.Add(lblPause0, "Pause");
            Controls.Add(lblPause1, "Pause");
            Controls.Add(lblPause2, "Pause");
            Controls.Add(lblPause3, "Pause");
            Controls.Add(lblPause4, "Pause");
            Controls.Add(lblPause5, "Pause");
            Controls.Add(lblPause6, "Pause");
            Controls.Add(lblPause7, "Pause");
            Controls.Add(lblPause8, "Pause");
            Controls.Add(lblPause9, "Pause");
            Controls.Add(lblPercent2, "%");
            Controls.Add(lblPercent3, "%");
            Controls.Add(lblS0, "s");
            Controls.Add(lblS1, "s");
            Controls.Add(lblS2, "s");
            Controls.Add(lblS3, "s");
            Controls.Add(lblS4, "s");
            Controls.Add(lblS5, "s");
            Controls.Add(lblS6, "s");
            Controls.Add(lblS7, "s");
            Controls.Add(lblS8, "s");
            Controls.Add(lblS9, "s");
            Controls.Add(lblS10, "s");
            Controls.Add(lblS11, "s");
            Controls.Add(lblS12, "s");
            Controls.Add(lblS13, "s");
            Controls.Add(lblS14, "s");
            Controls.Add(lblS15, "s");
            Controls.Add(lblS16, "s");
            Controls.Add(lblS17, "s");
            Controls.Add(lblS18, "s");
            Controls.Add(lblS19, "s");
            Controls.Add(lblS20, "s");
            Controls.Add(lblS21, "s");
            Controls.Add(lblS22, "s");
            Controls.Add(lblS23, "s");
            Controls.Add(lblS24, "s");
            Controls.Add(lblS25, "s");
            Controls.Add(lblS26, "s");
            Controls.Add(lblS27, "s");
            Controls.Add(lblS28, "s");
            Controls.Add(lblS29, "s");
            Controls.Add(lblS30, "s");
            Controls.Add(lblS31, "s");
            Controls.Add(lblS32, "s");
            Controls.Add(lblS33, "s");
            Controls.Add(lblS34, "s");
            Controls.Add(lblS35, "s");
            Controls.Add(lblS36, "s");
            Controls.Add(lblS37, "s");
            Controls.Add(lblS38, "s");
            Controls.Add(lblS39, "s");
            Controls.Add(lblS40, "s");
            Controls.Add(lblS41, "s");
            Controls.Add(lblS42, "s");
            Controls.Add(lblS43, "s");
            Controls.Add(lblS44, "s");
            Controls.Add(lblS45, "s");
            Controls.Add(lblS46, "s");
            Controls.Add(lblS47, "s");
            Controls.Add(lblS48, "s");
            Controls.Add(lblS49, "s");
            Controls.Add(lblS50, "s");
            Controls.Add(lblS51, "s");
            Controls.Add(lblS52, "s");
            Controls.Add(lblS53, "s");
            Controls.Add(lblS54, "s");
            Controls.Add(lblS55, "s");
            Controls.Add(lblS56, "s");
            Controls.Add(lblS57, "s");
            Controls.Add(lblS58, "s");
            Controls.Add(lblS59, "s");
            Controls.Add(lblS60, "s");
            Controls.Add(lblS61, "s");
            Controls.Add(lblS62, "s");
            Controls.Add(lblS63, "s");
            Controls.Add(lblS64, "s");
            Controls.Add(lblS65, "s");
            Controls.Add(lblS66, "s");
            Controls.Add(lblS67, "s");
            Controls.Add(lblS68, "s");
            Controls.Add(lblS69, "s");
            Controls.Add(lblSensorVoltageAt0mA, "Sensor voltage at 0 mA");
            Controls.Add(lblSensorVoltageAtMaxmA, "Sensor voltage at max mA");
            Controls.Add(lblStartDelay0, "Start delay");
            Controls.Add(lblStartDelay1, "Start delay");
            Controls.Add(lblStartDelay2, "Start delay");
            Controls.Add(lblStartDelay3, "Start delay");
            Controls.Add(lblStartDelay4, "Start delay");
            Controls.Add(lblStartDelay5, "Start delay");
            Controls.Add(lblStartDelay6, "Start delay");
            Controls.Add(lblStartDelay7, "Start delay");
            Controls.Add(lblStartDelay8, "Start delay");
            Controls.Add(lblStartDelay9, "Start delay");
            Controls.Add(lblStartDelay10, "Start delay");
            Controls.Add(lblStartDelay11, "Start delay");
            Controls.Add(lblStartDelay12, "Start delay");
            Controls.Add(lblStartDelay13, "Start delay");
            Controls.Add(lblStartDelay14, "Start delay");
            Controls.Add(lblStartDelay15, "Start delay");
            Controls.Add(lblStartDelay16, "Start delay");
            Controls.Add(lblStartDelay17, "Start delay");
            Controls.Add(lblStartDelay18, "Start delay");
            Controls.Add(lblStartDelay19, "Start delay");
            Controls.Add(lblStartDelay20, "Start delay");
            Controls.Add(lblStartDelay21, "Start delay");
            Controls.Add(lblStartDelay22, "Start delay");
            Controls.Add(lblStopDelay0, "Stop delay");
            Controls.Add(lblStopDelay1, "Stop delay");
            Controls.Add(lblStopDelay2, "Stop delay");
            Controls.Add(lblStopDelay3, "Stop delay");
            Controls.Add(lblStopDelay4, "Stop delay");
            Controls.Add(lblStopDelay5, "Stop delay");
            Controls.Add(lblStopDelay6, "Stop delay");
            Controls.Add(lblStopDelay7, "Stop delay");
            Controls.Add(lblStopDelay8, "Stop delay");
            Controls.Add(lblStopDelay9, "Stop delay");
            Controls.Add(lblStopDelay10, "Stop delay");
            Controls.Add(lblStopDelay11, "Stop delay");
            Controls.Add(lblStopDelay12, "Stop delay");
            Controls.Add(lblStopDelay13, "Stop delay");
            Controls.Add(lblStopDelay14, "Stop delay");
            Controls.Add(lblStopDelay15, "Stop delay");
            Controls.Add(lblStopDelay16, "Stop delay");
            Controls.Add(lblStopDelay17, "Stop delay");
            Controls.Add(lblStopDelay18, "Stop delay");
            Controls.Add(lblStopDelay19, "Stop delay");
            Controls.Add(lblStopDelay20, "Stop delay");
            Controls.Add(lblStopDelay21, "Stop delay");
            Controls.Add(lblStopDelay22, "Stop delay");
            Controls.Add(lblSwipeDuration0, "Max swipe duration");
            Controls.Add(lblSwipeDuration1, "Max swipe duration");
            Controls.Add(lblSwipePause0, "Swipe pause");
            Controls.Add(lblSwipePause1, "Swipe pause");
            Controls.Add(lblTemperature, "Temperature");
            Controls.Add(lblTemperature2, "Temperature 1");
            Controls.Add(lblTemperature3, "Temperature 2");
            Controls.Add(btnAdjustHVPSVoltage, "Adjust");
            Controls.Add(btnAutoMode, "Auto mode");
            Controls.Add(btnCalibrationSettings, "Calibration settings");
            Controls.Add(btnClose, "Close");
            Controls.Add(btnCloseSeparatorGrid, "Back");
            Controls.Add(btnContentSettings, "Content settings");
            Controls.Add(btnKBBackspace, "Erase");
            Controls.Add(btnKBWrite, "Write");
            Controls.Add(btnKBCancel, "Cancel");
            Controls.Add(btnLanguageSettings, "Language settings");
            Controls.Add(btnLaunchCleaner0to1, "Launch in direction 1");
            Controls.Add(btnLaunchCleaner0to2, "Launch in direction 2");
            Controls.Add(btnLaunchCleaner1to1, "Launch in direction 1");
            Controls.Add(btnLaunchCleaner1to2, "Launch in direction 2");
            Controls.Add(btnLoadSet, "Load");
            Controls.Add(btnLogs, "Logs");
            Controls.Add(btnManualMode, "Manual mode");
            Controls.Add(btnManualSchemeMode, "Manual Mode: Scheme");
            Controls.Add(btnPolaritySwitch, "Polarity");
            Controls.Add(btnReset, "Reset");
            Controls.Add(btnReset2, "Reset");
            Controls.Add(btnSaveSet, "Save");
            Controls.Add(btnDeleteSet, "Delete");
            Controls.Add(btnSettings, "Settings");
            Controls.Add(btnStartAuto, "Start");
            Controls.Add(btnStopAuto, "Stop");
            Controls.Add(btnTechnologySettings, "Technology settings");
            Controls.Add(gbxCleaner0_1, "Cleaner 1");
            Controls.Add(gbxCleaner1_1, "Cleaner 2");
            Controls.Add(gbxCleanerSettings0, "Cleaner 1 settings");
            Controls.Add(gbxCleanerSettings1, "Cleaner 2 settings");
            Controls.Add(gbxContactor0_1, "Contactor 1");
            Controls.Add(gbxContactor1_1, "Contactor 2");
            Controls.Add(gbxContactor2_1, "Contactor 3");
            Controls.Add(gbxContactor3_1, "Contactor 4");
            Controls.Add(gbxContactor4_1, "Contactor 5");
            Controls.Add(gbxContactor5_1, "Contactor 6");
            Controls.Add(gbxContactor6_1, "Contactor 7");
            Controls.Add(gbxContactor7_1, "Contactor 8");
            Controls.Add(gbxContactor8_1, "Contactor 9");
            Controls.Add(gbxContactor9_1, "Contactor 10");
            Controls.Add(gbxContactorSettings0, "Contactor 1 settings");
            Controls.Add(gbxContactorSettings1, "Contactor 2 settings");
            Controls.Add(gbxContactorSettings2, "Contactor 3 settings");
            Controls.Add(gbxContactorSettings3, "Contactor 4 settings");
            Controls.Add(gbxContactorSettings4, "Contactor 5 settings");
            Controls.Add(gbxContactorSettings5, "Contactor 6 settings");
            Controls.Add(gbxContactorSettings6, "Contactor 7 settings");
            Controls.Add(gbxContactorSettings7, "Contactor 8 settings");
            Controls.Add(gbxContactorSettings8, "Contactor 9 settings");
            Controls.Add(gbxContactorSettings9, "Contactor 10 settings");
            Controls.Add(gbxFC0_1, "FC 1");
            Controls.Add(gbxFC1_1, "FC 2");
            Controls.Add(gbxFC2_1, "FC 3");
            Controls.Add(gbxFC3_1, "FC 4");
            Controls.Add(gbxFC4_1, "FC 5");
            Controls.Add(gbxFC5_1, "FC 6");
            Controls.Add(gbxFC6_1, "FC 7");
            Controls.Add(gbxFC7_1, "FC 8");
            Controls.Add(gbxFC8_1, "FC 9");
            Controls.Add(gbxFCSettings0, "FC 1 settings");
            Controls.Add(gbxFCSettings1, "FC 2 settings");
            Controls.Add(gbxFCSettings2, "FC 3 settings");
            Controls.Add(gbxFCSettings3, "FC 4 settings");
            Controls.Add(gbxFCSettings4, "FC 5 settings");
            Controls.Add(gbxFCSettings5, "FC 6 settings");
            Controls.Add(gbxFCSettings6, "FC 7 settings");
            Controls.Add(gbxFCSettings7, "FC 8 settings");
            Controls.Add(gbxFCSettings8, "FC 9 settings");
            Controls.Add(gbxHVPS0_1, "HVPS 1");
            Controls.Add(gbxHVPS1_1, "HVPS 2");
            Controls.Add(gbxHVPSSettings0, "HVPS 1 settings");
            Controls.Add(gbxHVPSSettings1, "HVPS 2 settings");
            Controls.Add(cbxCleaner0Enabled, "Cleaner 1");
            Controls.Add(cbxCleaner1Enabled, "Cleaner 2");
            Controls.Add(cbxContactor0Enabled, "Contactor 1");
            Controls.Add(cbxContactor1Enabled, "Contactor 2");
            Controls.Add(cbxContactor2Enabled, "Contactor 3");
            Controls.Add(cbxContactor3Enabled, "Contactor 4");
            Controls.Add(cbxContactor4Enabled, "Contactor 5");
            Controls.Add(cbxContactor5Enabled, "Contactor 6");
            Controls.Add(cbxContactor6Enabled, "Contactor 7");
            Controls.Add(cbxContactor7Enabled, "Contactor 8");
            Controls.Add(cbxContactor8Enabled, "Contactor 9");
            Controls.Add(cbxContactor9Enabled, "Contactor 10");
            Controls.Add(cbxFC0Enabled, "FC 1");
            Controls.Add(cbxFC1Enabled, "FC 2");
            Controls.Add(cbxFC2Enabled, "FC 3");
            Controls.Add(cbxFC3Enabled, "FC 4");
            Controls.Add(cbxFC4Enabled, "FC 5");
            Controls.Add(cbxFC5Enabled, "FC 6");
            Controls.Add(cbxFC6Enabled, "FC 7");
            Controls.Add(cbxFC7Enabled, "FC 8");
            Controls.Add(cbxFC8Enabled, "FC 9");
            Controls.Add(cbxHVPS0Enabled, "HVPS 1");
            Controls.Add(cbxHVPS1Enabled, "HVPS 2");
            Controls.Add(cbxRevertPolarity0, "Revert polarity");
            Controls.Add(cbxRevertPolarity1, "Revert polarity");
            Controls.Add(cbxManualLimitDisable, "Disable Manual Mode Limitations");
            foreach (KeyValuePair<UIElement, string> KVP in Controls)
            {
                try
                {
                    if (KVP.Key is Button)
                    {
                        var LocalButton = KVP.Key as Button;
                        LocalButton.Content = SelectedLib[KVP.Value];
                    }
                    if (KVP.Key is Label)
                    {
                        var LocalLabel = KVP.Key as Label;
                        LocalLabel.Content = SelectedLib[KVP.Value];
                    }
                    if(KVP.Key is GroupBox)
                    {
                        var LocalGroupBox = KVP.Key as GroupBox;
                        LocalGroupBox.Header = SelectedLib[KVP.Value];
                    }
                    if(KVP.Key is CheckBox)
                    {
                        var LocalCheckBox = KVP.Key as CheckBox;
                        LocalCheckBox.Content = SelectedLib[KVP.Value];
                    }
                    if(KVP.Key is TextBlock)
                    {
                        var LocalTextBlock = KVP.Key as TextBlock;
                        LocalTextBlock.Text = SelectedLib[KVP.Value];
                    }
                }
                catch
                { }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            RestartExplorer();
            Program.Finish();
        }

        private void btnBritish_Click(object sender, RoutedEventArgs e)
        {
            SetLanguage(ELanguage.English);
        }

        private void btnRussian_Click(object sender, RoutedEventArgs e)
        {
            SetLanguage(ELanguage.Russian);
        }

        private void btnUkrainian_Click(object sender, RoutedEventArgs e)
        {
            SetLanguage(ELanguage.Ukrainian);
        }

        private void btnEarth_Click(object sender, RoutedEventArgs e)
        {
            SetLanguage(ELanguage.Custom);
        }
        int CurrentFCIndex = 0;
        private void ShowFCControlBox(object sender, MouseButtonEventArgs e)
        {
            var CurrentLabel = sender as Label;
            if(CurrentLabel != null && CurrentLabel.Content.ToString() == "1" || 
                sender == stpEngineRightPanel1)
            {
                CurrentFCIndex = 0;
            }
            if (CurrentLabel != null && CurrentLabel.Content.ToString() == "2" ||
                sender == stpEngineRightPanel2)
            {
                CurrentFCIndex = 1;
            }
            if (CurrentLabel != null && CurrentLabel.Content.ToString() == "3" ||
                sender == stpEngineRightPanel3)
            {
                CurrentFCIndex = 2;
            }
            if (CurrentLabel != null && CurrentLabel.Content.ToString() == "4" ||
                sender == stpEngineRightPanel4)
            {
                CurrentFCIndex = 3;
            }
            if (CurrentLabel != null && CurrentLabel.Content.ToString() == "5" ||
                sender == stpEngineRightPanel5)
            {
                CurrentFCIndex = 4;
            }
            if (CurrentLabel != null && CurrentLabel.Content.ToString() == "6" ||
                sender == stpEngineRightPanel6)
            {
                CurrentFCIndex = 5;
            }
            if (CurrentLabel != null && CurrentLabel.Content.ToString() == "7" ||
                sender == stpEngineRightPanel7)
            {
                CurrentFCIndex = 6;
            }
            if (CurrentLabel != null && CurrentLabel.Content.ToString() == "8" ||
                sender == stpEngineRightPanel8)
            {
                CurrentFCIndex = 101;
            }
            if (CurrentLabel != null && CurrentLabel.Content.ToString() == "9" ||
                sender == stpEngineRightPanel9)
            {
                CurrentFCIndex = 100;
            }
            if (CurrentFCIndex < 100)
            {
                FCControlBoxNameKey = "FC " + (CurrentFCIndex + 1);
                lblFCControlBoxHeader.Content = SelectedLib[FCControlBoxNameKey];
                var CurrentFC = Technology.FCs[CurrentFCIndex];
                txtFrequencySetter.Text = CurrentFC.CurrentFrequency.ToString();
                scrFrequencySetter.Minimum = CurrentFC.CalibrationMinFrequency;
                scrFrequencySetter.Maximum = CurrentFC.CalibrationMaxFrequency;
                scrFrequencySetter.Value = CurrentFC.CurrentFrequency;
                scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                    scrFrequencySetter.Minimum) * 0.25 / 0.75;
                lblHz0.Content = SelectedLib["Hz"];
                if (CurrentFC.bPowerState)
                {
                    btnStateSwitcher.Background = btnStartAuto.Background;
                }
                else
                {
                    btnStateSwitcher.Background = btnStopAuto.Background;
                }
            }
            else
            {
                FCControlBoxNameKey = "HVPS " + (CurrentFCIndex - 99);
                lblFCControlBoxHeader.Content = SelectedLib[FCControlBoxNameKey];
                var CurrentHVPS = Technology.HVPSes[CurrentFCIndex - 100];
                txtFrequencySetter.Text = CurrentHVPS.VoltageOut.ToString("F2", 
                    CultureInfo.InvariantCulture);
                scrFrequencySetter.Minimum = (double)CurrentHVPS.MinOutput;
                scrFrequencySetter.Maximum = (double)CurrentHVPS.MaxOutput;
                scrFrequencySetter.Value = (double)CurrentHVPS.VoltageOut;
                scrFrequencySetter.ViewportSize = (scrFrequencySetter.Maximum -
                    scrFrequencySetter.Minimum) * 0.25 / 0.75;
                if (CurrentFCIndex == 101)
                {
                    lblHz0.Content = SelectedLib["A"];
                }
                else if (CurrentFCIndex == 100)
                {
                    lblHz0.Content = SelectedLib["kV"];
                }
                if (CurrentHVPS.bPowerState)
                {
                    btnStateSwitcher.Background = btnStartAuto.Background;
                }
                else
                {
                    btnStateSwitcher.Background = btnStopAuto.Background;
                }
            }
            brdFCControlBox.Visibility = Visibility.Visible;
        }

        private void btnStateSwitcher_Click(object sender, RoutedEventArgs e)
        {
            if(!bControlEnginesFromScheme)
            {
                return;
            }
            if (CurrentFCIndex < 100)
            {
                switch(CurrentFCIndex)
                {
                    case 0:
                        FC0_SwitchState(null, null);
                        break;
                    case 1:
                        FC1_SwitchState(null, null);
                        break;
                    case 2:
                        FC2_SwitchState(null, null);
                        break;
                    case 3:
                        FC3_SwitchState(null, null);
                        break;
                    case 4:
                        FC4_SwitchState(null, null);
                        break;
                    case 5:
                        FC5_SwitchState(null, null);
                        break;
                    case 6:
                        FC6_SwitchState(null, null);
                        break;
                }
                var CurrentFC = Technology.FCs[CurrentFCIndex];
                if (CurrentFC.bPowerState)
                {
                    btnStateSwitcher.Background = btnStartAuto.Background;
                }
                else
                {
                    btnStateSwitcher.Background = btnStopAuto.Background;
                }
            }
            else
            {
                if(CurrentFCIndex == 100)
                {
                    HVPS0_SwitchState(null, null);
                }
                else
                {
                    HVPS1_SwitchState(null, null);
                }
                var CurrentHVPS = Technology.HVPSes[CurrentFCIndex - 100];
                if (CurrentHVPS.bPowerState)
                {
                    btnStateSwitcher.Background = btnStartAuto.Background;
                }
                else
                {
                    btnStateSwitcher.Background = btnStopAuto.Background;
                }
            }
        }
        private void scrFrequencySetter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txtFrequencySetter.Text = Math.Round((sender as ScrollBar).Value, 
                CurrentFCIndex < 100 ? 0 : 2,
                MidpointRounding.AwayFromZero).ToString();
        }

        private void btnAcceptFrequency_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentFCIndex < 100)
            {
                var CurrentFC = Technology.FCs[CurrentFCIndex];
                decimal NewFrequency;
                if (decimal.TryParse(txtFrequencySetter.Text, out NewFrequency))
                {
                    if ((int)NewFrequency >= CurrentFC.CalibrationMinFrequency &&
                        (int)NewFrequency <= CurrentFC.CalibrationMaxFrequency)
                    {
                        CurrentFC.CurrentFrequency = (int)NewFrequency;
                    }
                }
            }
            else
            {
                var CurrentHVPS = Technology.HVPSes[CurrentFCIndex - 100];
                decimal NewVoltage;
                if (decimal.TryParse(txtFrequencySetter.Text, out NewVoltage))
                {
                    if (NewVoltage >= CurrentHVPS.MinOutput &&
                        NewVoltage <= CurrentHVPS.MaxOutput)
                    {
                        CurrentHVPS.VoltageOut = NewVoltage;
                    }
                }
            }
            brdFCControlBox.Visibility = Visibility.Hidden;
        }

        private void CloseSeparatorGrid(object sender, RoutedEventArgs e)
        {
            grdSeparatorViewGrid.Visibility = Visibility.Hidden;
        }

        private void OpenSeparatorGrid(object sender, MouseButtonEventArgs e)
        {
            grdSeparatorViewGrid.Visibility = Visibility.Visible;
        }

        private void scrOutputVoltage0_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scrOutputVoltage0.Value = Math.Round(scrOutputVoltage0.Value, 1, 
                MidpointRounding.AwayFromZero);
            Technology.HVPSes[0].VoltageOut = (decimal)scrOutputVoltage0.Value;
            txtOutVoltage0_1.Text = scrOutputVoltage0.Value.ToString("F2", CultureInfo.InvariantCulture);
        }

        private void scrOutputCurrent1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scrOutputCurrent1.Value = Math.Round(scrOutputCurrent1.Value, 1,
                MidpointRounding.AwayFromZero);
            Technology.HVPSes[1].VoltageOut = (decimal)scrOutputCurrent1.Value;
            txtOutVoltage1_1.Text = scrOutputCurrent1.Value.ToString("F2", CultureInfo.InvariantCulture);
        }

        private void scrFrequency0_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scrFrequency0.Value = Math.Round(scrFrequency0.Value, 0,
                MidpointRounding.AwayFromZero);
            Technology.FCs[0].CurrentFrequency = (int)scrFrequency0.Value;
            txtFrequency0_1.Text = scrFrequency0.Value.ToString();
        }

        private void scrFrequency1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scrFrequency1.Value = Math.Round(scrFrequency1.Value, 0,
                MidpointRounding.AwayFromZero);
            Technology.FCs[1].CurrentFrequency = (int)scrFrequency1.Value;
            txtFrequency1_1.Text = scrFrequency1.Value.ToString();
        }

        private void scrFrequency2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scrFrequency2.Value = Math.Round(scrFrequency2.Value, 0,
                MidpointRounding.AwayFromZero);
            Technology.FCs[2].CurrentFrequency = (int)scrFrequency2.Value;
            txtFrequency2_1.Text = scrFrequency2.Value.ToString();
        }

        private void scrFrequency3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scrFrequency3.Value = Math.Round(scrFrequency3.Value, 0,
                MidpointRounding.AwayFromZero);
            Technology.FCs[3].CurrentFrequency = (int)scrFrequency3.Value;
            txtFrequency3_1.Text = scrFrequency3.Value.ToString();
        }

        private void scrFrequency4_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scrFrequency4.Value = Math.Round(scrFrequency4.Value, 0,
                MidpointRounding.AwayFromZero);
            Technology.FCs[4].CurrentFrequency = (int)scrFrequency4.Value;
            txtFrequency4_1.Text = scrFrequency4.Value.ToString();
        }

        private void scrFrequency5_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scrFrequency5.Value = Math.Round(scrFrequency5.Value, 0,
                MidpointRounding.AwayFromZero);
            Technology.FCs[5].CurrentFrequency = (int)scrFrequency5.Value;
            txtFrequency5_1.Text = scrFrequency5.Value.ToString();
        }

        private void scrFrequency6_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scrFrequency6.Value = Math.Round(scrFrequency6.Value, 0,
                MidpointRounding.AwayFromZero);
            Technology.FCs[6].CurrentFrequency = (int)scrFrequency6.Value;
            txtFrequency6_1.Text = scrFrequency6.Value.ToString();
        }

        private void cbxManualLimitDisable_Checked(object sender, RoutedEventArgs e)
        {
            Program.bUnlimitedManual = true;
            Program.Log("Disabled the manual mode limitations", ELogType.Operator);
        }

        private void cbxManualLimitDisable_Unchecked(object sender, RoutedEventArgs e)
        {
            Program.bUnlimitedManual = false;
            Program.Log("Enabled the manual mode limitations", ELogType.Operator);
        }

        private void btnAdjustHVPSVoltage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtOutVoltage0_1.Text = Math.Round(Math.Pow(
                    (double)Technology.HVPSes[0].VoltageOut, 2) /
                    (double)Technology.HVPSes[0].VoltageIn, 2).ToString("F2", 
                    CultureInfo.InvariantCulture);
            }
            catch (DivideByZeroException)
            { }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hwnd);
        private void btnCallOSK_Click(object sender, RoutedEventArgs e)
        {
            const string processName = "osk";
            //string processFilePath = @"C:\Program Files\B\B.exe";
            //get the process
            Process bProcess = Process.GetProcessesByName(processName).FirstOrDefault();
            //check if the process is nothing or not.
            if (bProcess != null)
            {
                //get the  hWnd of the process
                IntPtr hwnd = bProcess.MainWindowHandle;

                //set user the focus to the window
                SetForegroundWindow(bProcess.MainWindowHandle);
            }
            else
            {
                //tthe process is nothing, so start it
                Process.Start(processName);
            }
        }

        private void KillExplorer()
        {
            const string ProcessName = "explorer";
            var Other = Process.GetProcessesByName(ProcessName);
            foreach(Process P in Other)
            {
                P.Kill();
            }
        }

        private void RestartExplorer()
        {
            const string ProcessName = "explorer";
            Process Other = Process.GetProcessesByName(ProcessName).FirstOrDefault();
            if(Other == null)
            {
                Process.Start(ProcessName);
            }
        }

        private void NumberTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            KBShow(sender as TextBox);
        }

        private enum EKBInputOrder
        {
            Integer,
            Decimal
        }
        private EKBInputOrder KBInputOrder = EKBInputOrder.Integer;
        private TextBox KBTargetTextBox;
        private bool bKBMinusInput = false;
        private void KBShow(TextBox Target)
        {
            decimal Value;
            KBTargetTextBox = Target;
            if(!decimal.TryParse(Target.Text, out Value))
            {
                Value = 0;
            }
            lblKBIntValue.Content = Value.IntPart().ToString();
            var CurrentDecimal = Value.DecimalPart();
            while((int)CurrentDecimal != CurrentDecimal)
            {
                CurrentDecimal *= 10;
            }
            lblKBDecValue.Content = ((int)CurrentDecimal).ToString();
            KBInputOrder = EKBInputOrder.Integer;
            btnSwitchOrder.Content = SelectedLib["Integer"];
            bKBMinusInput = false;
            btnKBSignSwitch.Content = "+";
            brdNumberKeyboard.Visibility = Visibility.Visible;
        }
        private void KBNumberInput(object sender, RoutedEventArgs e)
        {
            var Other = sender as Button;
            if(KBInputOrder == EKBInputOrder.Integer)
            {
                lblKBIntValue.Content += Other.Content.ToString();
            }
            else
            {
                lblKBDecValue.Content += Other.Content.ToString();
            }
        }

        private void btnKBSignSwitch_Click(object sender, RoutedEventArgs e)
        {
            bKBMinusInput = !bKBMinusInput;
            btnKBSignSwitch.Content = bKBMinusInput ? "-" : "+";
        }

        private void btnSwitchOrder_Click(object sender, RoutedEventArgs e)
        {
            if(KBInputOrder == EKBInputOrder.Integer)
            {
                KBInputOrder = EKBInputOrder.Decimal;
                btnSwitchOrder.Content = SelectedLib["Decimal"];
            }
            else
            {
                KBInputOrder = EKBInputOrder.Integer;
                btnSwitchOrder.Content = SelectedLib["Integer"];
            }
        }

        private void KBNumberErase(object sender, RoutedEventArgs e)
        {
            var Other = sender as Button;
            if (KBInputOrder == EKBInputOrder.Integer)
            {
                if (lblKBIntValue.Content.ToString() != "")
                {
                    lblKBIntValue.Content = lblKBIntValue.Content.ToString().
                        Substring(0, lblKBIntValue.Content.ToString().Length - 1);
                }
            }
            else
            {
                if (lblKBDecValue.Content.ToString() != "")
                {
                    lblKBDecValue.Content = lblKBDecValue.Content.ToString().
                        Substring(0, lblKBDecValue.Content.ToString().Length - 1);
                }
            }
        }

        private void btnKBCancel_Click(object sender, RoutedEventArgs e)
        {
            lblKBIntValue.Content = "0";
            lblKBDecValue.Content = "0";
            brdNumberKeyboard.Visibility = Visibility.Hidden;
        }

        private void btnKBWrite_Click(object sender, RoutedEventArgs e)
        {
            KBTargetTextBox.Text = bKBMinusInput ? "-" : "" + lblKBIntValue.Content + 
                "." + lblKBDecValue.Content;
            brdNumberKeyboard.Visibility = Visibility.Hidden;
        }

        private void btnPolaritySwitch_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                var Res = MessageBox.Show(SelectedLib["Please confirm the polarity switch."],
                    SelectedLib["Confirmation"], MessageBoxButton.OKCancel,
                    MessageBoxImage.Exclamation);
                if (Res == MessageBoxResult.OK)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        cbxRevertPolarity0.IsChecked = !cbxRevertPolarity0.IsChecked.Value;
                    });
                }
            });
        }
    }
}