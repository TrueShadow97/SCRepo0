using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace Separator
{
    public static class Settings
    {
        private static Dictionary<string, Hashtable> SetDictionary = 
            new Dictionary<string, Hashtable>();
        private static string CurrentSetName;
        private const string SETTINGS_FILE_NAME = @"Settings.dat";

        public static string AdminPassword { get; set; }

        public static bool[] bReversePolarity { get; set; } = { false, false };

        public static BitmapImage IndicatorRed = new BitmapImage();
        public static BitmapImage IndicatorGreen = new BitmapImage();
        public static BitmapImage BritishFlag = new BitmapImage();
        public static BitmapImage RussianFlag = new BitmapImage();
        public static BitmapImage UkrainianFlag = new BitmapImage();
        public static BitmapImage EarthPic = new BitmapImage();
        public static BitmapImage AutoBackground = new BitmapImage();
        public static BitmapImage SeparatorBackground = new BitmapImage();
        public static BitmapImage PELogo = new BitmapImage();
        public static BitmapImage KBIcon = new BitmapImage();
        public static BitmapImage AlarmIcon = new BitmapImage();
        public static bool bFirstLaunch { get; set; }
        public static int SetCount
        {
            get
            {
                return SetDictionary.Count;
            }
        }
        public static IEnumerable<string> SetList
        {
            get
            {
                var Res = new List<string>();
                foreach (KeyValuePair<string, Hashtable> KVP in SetDictionary)
                {
                    Res.Add(KVP.Key);
                }
                return Res as IEnumerable<string>;
            }
        }

        public static void Init()
        {
            IndicatorRed.BeginInit();
            IndicatorRed.UriSource = new Uri("32IndicatorRed.png", UriKind.Relative);
            IndicatorRed.CacheOption = BitmapCacheOption.OnLoad;
            IndicatorRed.EndInit();
            IndicatorGreen.BeginInit();
            IndicatorGreen.UriSource = new Uri("32IndicatorGreen.png", UriKind.Relative);
            IndicatorGreen.CacheOption = BitmapCacheOption.OnLoad;
            IndicatorGreen.EndInit();
            BritishFlag.BeginInit();
            BritishFlag.UriSource = new Uri("24British.png", UriKind.Relative);
            BritishFlag.CacheOption = BitmapCacheOption.OnLoad;
            BritishFlag.EndInit();
            RussianFlag.BeginInit();
            RussianFlag.UriSource = new Uri("24Russian.png", UriKind.Relative);
            RussianFlag.CacheOption = BitmapCacheOption.OnLoad;
            RussianFlag.EndInit();
            UkrainianFlag.BeginInit();
            UkrainianFlag.UriSource = new Uri("24Ukrainian.png", UriKind.Relative);
            UkrainianFlag.CacheOption = BitmapCacheOption.OnLoad;
            UkrainianFlag.EndInit();
            EarthPic.BeginInit();
            EarthPic.UriSource = new Uri("24Earth.png", UriKind.Relative);
            EarthPic.CacheOption = BitmapCacheOption.OnLoad;
            EarthPic.EndInit();
            AutoBackground.BeginInit();
            AutoBackground.UriSource = new Uri("BG_new2.png", UriKind.Relative);
            AutoBackground.CacheOption = BitmapCacheOption.OnLoad;
            AutoBackground.EndInit();
            SeparatorBackground.BeginInit();
            SeparatorBackground.UriSource = new Uri("Separator2.png", UriKind.Relative);
            SeparatorBackground.CacheOption = BitmapCacheOption.OnLoad;
            SeparatorBackground.EndInit();
            PELogo.BeginInit();
            PELogo.UriSource = new Uri("Logo.png", UriKind.Relative);
            PELogo.CacheOption = BitmapCacheOption.OnLoad;
            PELogo.EndInit();
            KBIcon.BeginInit();
            KBIcon.UriSource = new Uri("KBIcon.png", UriKind.Relative);
            KBIcon.CacheOption = BitmapCacheOption.OnLoad;
            KBIcon.EndInit();
            AlarmIcon.BeginInit();
            AlarmIcon.UriSource = new Uri("Alarm.png", UriKind.Relative);
            AlarmIcon.CacheOption = BitmapCacheOption.OnLoad;
            AlarmIcon.EndInit();
        }

        public static void LoadAll()
        {
            var BF = new BinaryFormatter();
            try
            {
                using (FileStream FS = new FileStream(SETTINGS_FILE_NAME, FileMode.Open))
                {
                    SetDictionary = (Dictionary<string, Hashtable>)BF.Deserialize(FS);
                }
            }
            catch
            {
                SetDictionary.Add("Default", new Hashtable());
            }
            LoadSet("Default");
            CheckFirstLaunch();
            LoadCalibration();
            if (bFirstLaunch)
                SaveCalibration();
        }

        public static void CheckFirstLaunch()
        {
            try
            {
                BinaryFormatter BF = new BinaryFormatter();
                bFirstLaunch = (bool)BF.Deserialize(new FileStream(
                    "Launch.dat", FileMode.Open));
            }
            catch
            {
                bFirstLaunch = true;
                var BF = new BinaryFormatter();
                BF.Serialize(new FileStream("Launch.dat", FileMode.Create), false);
            }
        }

        public static void LoadCalibration()
        {
            if(bFirstLaunch)
            {
                try
                {
                    var CurrentSet = SetDictionary[CurrentSetName];
                    for (int i = 0; i < Technology.HVPSes.Count; i++)
                    {
                        var CurrentHVPS = Technology.HVPSes[i];
                        CurrentHVPS.MinOutput =
                            (CurrentSet["HVPS.MinOutput"] as decimal[])[i];
                        CurrentHVPS.MaxOutput =
                            (CurrentSet["HVPS.MaxOutput"] as decimal[])[i];
                        CurrentHVPS.MaxmA =
                            (CurrentSet["HVPS.MaxmA"] as decimal[])[i];
                        CurrentHVPS.RealInputAt0kV =
                            (CurrentSet["HVPS.RealInputAt0kV"] as decimal[])[i];
                        CurrentHVPS.RealInputAt0mA =
                            (CurrentSet["HVPS.RealInputAt0mA"] as decimal[])[i];
                        CurrentHVPS.RealInputAt30kV =
                            (CurrentSet["HVPS.RealInputAt30kV"] as decimal[])[i];
                        CurrentHVPS.RealInputAtMaxmA =
                            (CurrentSet["HVPS.RealInputAtMaxmA"] as decimal[])[i];
                        CurrentHVPS.ReverseMinOutput =
                            (CurrentSet["HVPS.ReverseMinOutput"] as decimal[])[i];
                        CurrentHVPS.ReverseMaxOutput =
                            (CurrentSet["HVPS.ReverseMaxOutput"] as decimal[])[i];
                        CurrentHVPS.ReverseMaxmA =
                            (CurrentSet["HVPS.ReverseMaxmA"] as decimal[])[i];
                        CurrentHVPS.ReverseRealInputAt0kV =
                            (CurrentSet["HVPS.ReverseRealInputAt0kV"] as decimal[])[i];
                        CurrentHVPS.ReverseRealInputAt0mA =
                            (CurrentSet["HVPS.ReverseRealInputAt0mA"] as decimal[])[i];
                        CurrentHVPS.ReverseRealInputAt30kV =
                            (CurrentSet["HVPS.ReverseRealInputAt30kV"] as decimal[])[i];
                        CurrentHVPS.ReverseRealInputAtMaxmA =
                            (CurrentSet["HVPS.ReverseRealInputAtMaxmA"] as decimal[])[i];
                    }
                    for (int i = 0; i < Technology.FCs.Count; i++)
                    {
                        var CurrentFC = Technology.FCs[i];
                        CurrentFC.CalibrationMaxFrequency =
                            (CurrentSet["FC.CalibrationMaxFrequency"] as int[])[i];
                        CurrentFC.CalibrationMinFrequency =
                            (CurrentSet["FC.CalibrationMinFrequency"] as int[])[i];
                    }
                }
                catch
                { }
            }
            else
            {
                try
                {
                    var BF = new BinaryFormatter();
                    var CurrentSet = (Hashtable)BF.Deserialize(new FileStream(
                        "Calibration.dat", FileMode.Open));
                    for (int i = 0; i < Technology.HVPSes.Count; i++)
                    {
                        var CurrentHVPS = Technology.HVPSes[i];
                        CurrentHVPS.MinOutput =
                            (CurrentSet["HVPS.MinOutput"] as decimal[])[i];
                        CurrentHVPS.MaxOutput =
                            (CurrentSet["HVPS.MaxOutput"] as decimal[])[i];
                        CurrentHVPS.MaxmA =
                            (CurrentSet["HVPS.MaxmA"] as decimal[])[i];
                        CurrentHVPS.RealInputAt0kV =
                            (CurrentSet["HVPS.RealInputAt0kV"] as decimal[])[i];
                        CurrentHVPS.RealInputAt0mA =
                            (CurrentSet["HVPS.RealInputAt0mA"] as decimal[])[i];
                        CurrentHVPS.RealInputAt30kV =
                            (CurrentSet["HVPS.RealInputAt30kV"] as decimal[])[i];
                        CurrentHVPS.RealInputAtMaxmA =
                            (CurrentSet["HVPS.RealInputAtMaxmA"] as decimal[])[i];
                        CurrentHVPS.ReverseMinOutput =
                            (CurrentSet["HVPS.ReverseMinOutput"] as decimal[])[i];
                        CurrentHVPS.ReverseMaxOutput =
                            (CurrentSet["HVPS.ReverseMaxOutput"] as decimal[])[i];
                        CurrentHVPS.ReverseMaxmA =
                            (CurrentSet["HVPS.ReverseMaxmA"] as decimal[])[i];
                        CurrentHVPS.ReverseRealInputAt0kV =
                            (CurrentSet["HVPS.ReverseRealInputAt0kV"] as decimal[])[i];
                        CurrentHVPS.ReverseRealInputAt0mA =
                            (CurrentSet["HVPS.ReverseRealInputAt0mA"] as decimal[])[i];
                        CurrentHVPS.ReverseRealInputAt30kV =
                            (CurrentSet["HVPS.ReverseRealInputAt30kV"] as decimal[])[i];
                        CurrentHVPS.ReverseRealInputAtMaxmA =
                            (CurrentSet["HVPS.ReverseRealInputAtMaxmA"] as decimal[])[i];
                    }
                    for (int i = 0; i < Technology.FCs.Count; i++)
                    {
                        var CurrentFC = Technology.FCs[i];
                        CurrentFC.CalibrationMaxFrequency =
                            (CurrentSet["FC.CalibrationMaxFrequency"] as int[])[i];
                        CurrentFC.CalibrationMinFrequency =
                            (CurrentSet["FC.CalibrationMinFrequency"] as int[])[i];
                    }
                }
                catch
                { }
            }
        }

        public static void SaveCalibration()
        {
            var NewSet = new Hashtable();
            var HVPSCount = Technology.HVPSes.Count;
            var FCCount = Technology.FCs.Count;
            NewSet.Add("HVPS.MinOutput", new decimal[HVPSCount]);
            for (int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.MinOutput"] as decimal[];
                CurrentSetting[i] = Technology.HVPSes[i].MinOutput;
            }
            NewSet.Add("HVPS.MaxOutput", new decimal[HVPSCount]);
            for (int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.MaxOutput"] as decimal[];
                CurrentSetting[i] = Technology.HVPSes[i].MaxOutput;
            }
            NewSet.Add("HVPS.MaxmA", new decimal[HVPSCount]);
            for (int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.MaxmA"] as decimal[];
                CurrentSetting[i] = Technology.HVPSes[i].MaxmA;
            }
            NewSet.Add("HVPS.RealInputAt0kV", new decimal[HVPSCount]);
            for (int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.RealInputAt0kV"] as decimal[];
                CurrentSetting[i] = Technology.HVPSes[i].RealInputAt0kV;
            }
            NewSet.Add("HVPS.RealInputAt0mA", new decimal[HVPSCount]);
            for (int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.RealInputAt0mA"] as decimal[];
                CurrentSetting[i] = Technology.HVPSes[i].RealInputAt0mA;
            }
            NewSet.Add("HVPS.RealInputAt30kV", new decimal[HVPSCount]);
            for (int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.RealInputAt30kV"] as decimal[];
                CurrentSetting[i] = Technology.HVPSes[i].RealInputAt30kV;
            }
            NewSet.Add("HVPS.RealInputAtMaxmA", new decimal[HVPSCount]);
            for (int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.RealInputAtMaxmA"] as decimal[];
                CurrentSetting[i] = Technology.HVPSes[i].RealInputAtMaxmA;
            }
            NewSet.Add("HVPS.ReverseMinOutput", new decimal[HVPSCount]);
            for (int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.ReverseMinOutput"] as decimal[];
                CurrentSetting[i] = Technology.HVPSes[i].ReverseMinOutput;
            }
            NewSet.Add("HVPS.ReverseMaxOutput", new decimal[HVPSCount]);
            for (int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.ReverseMaxOutput"] as decimal[];
                CurrentSetting[i] = Technology.HVPSes[i].ReverseMaxOutput;
            }
            NewSet.Add("HVPS.ReverseMaxmA", new decimal[HVPSCount]);
            for (int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.ReverseMaxmA"] as decimal[];
                CurrentSetting[i] = Technology.HVPSes[i].ReverseMaxmA;
            }
            NewSet.Add("HVPS.ReverseRealInputAt0kV", new decimal[HVPSCount]);
            for (int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.ReverseRealInputAt0kV"] as decimal[];
                CurrentSetting[i] = Technology.HVPSes[i].ReverseRealInputAt0kV;
            }
            NewSet.Add("HVPS.ReverseRealInputAt0mA", new decimal[HVPSCount]);
            for (int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.ReverseRealInputAt0mA"] as decimal[];
                CurrentSetting[i] = Technology.HVPSes[i].ReverseRealInputAt0mA;
            }
            NewSet.Add("HVPS.ReverseRealInputAt30kV", new decimal[HVPSCount]);
            for (int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.ReverseRealInputAt30kV"] as decimal[];
                CurrentSetting[i] = Technology.HVPSes[i].ReverseRealInputAt30kV;
            }
            NewSet.Add("HVPS.ReverseRealInputAtMaxmA", new decimal[HVPSCount]);
            for (int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.ReverseRealInputAtMaxmA"] as decimal[];
                CurrentSetting[i] = Technology.HVPSes[i].ReverseRealInputAtMaxmA;
            }
            NewSet.Add("FC.CalibrationMaxFrequency", new int[FCCount]);
            for (int i = 0; i < FCCount; i++)
            {
                var CurrentSetting = NewSet["FC.CalibrationMaxFrequency"] as int[];
                CurrentSetting[i] = Technology.FCs[i].CalibrationMaxFrequency;
            }
            NewSet.Add("FC.CalibrationMinFrequency", new int[FCCount]);
            for (int i = 0; i < FCCount; i++)
            {
                var CurrentSetting = NewSet["FC.CalibrationMinFrequency"] as int[];
                CurrentSetting[i] = Technology.FCs[i].CalibrationMinFrequency;
            }
            var BF = new BinaryFormatter();
            BF.Serialize(new FileStream("Calibration.dat", FileMode.OpenOrCreate),
                NewSet);
        }

        public static void LoadSet(string SetName)
        {
            try
            {
                var CurrentSet = SetDictionary[SetName];
                CurrentSetName = SetName;
                for (int i = 0; i < Technology.HVPSes.Count; i++)
                {
                    var CurrentHVPS = Technology.HVPSes[i];
                    CurrentHVPS.bEnabled =
                        (CurrentSet["HVPS.bEnabled"] as bool[])[i];
                    if (i == 0)
                    {
                        CurrentHVPS.bPolarity =
                            (CurrentSet["HVPS.bPolarity"] as bool[])[i];
                    }
                    CurrentHVPS.VoltageOut =
                        (CurrentSet["HVPS.OutVoltage"] as decimal[])[i];
                    CurrentHVPS.StartDelay =
                        (CurrentSet["HVPS.StartDelay"] as decimal[])[i];
                    CurrentHVPS.StopDelay =
                        (CurrentSet["HVPS.StopDelay"] as decimal[])[i];
                }
                for (int i = 0; i < Technology.Cleaners.Count; i++)
                {
                    var CurrentCleaner = Technology.Cleaners[i];
                    CurrentCleaner.bEnabled =
                        (CurrentSet["Cleaner.bEnabled"] as bool[])[i];
                    CurrentCleaner.CleanPause =
                        (CurrentSet["Cleaner.CleanPause"] as long[])[i];
                    CurrentCleaner.MaxCleanTimeout =
                        (CurrentSet["Cleaner.MaxCleanTimeout"] as long[])[i];
                    CurrentCleaner.StartDelay =
                        (CurrentSet["Cleaner.StartDelay"] as decimal[])[i];
                    CurrentCleaner.StopDelay =
                        (CurrentSet["Cleaner.StopDelay"] as decimal[])[i];
                }
                for (int i = 0; i < Technology.Contactors.Count; i++)
                {
                    var CurrentContactor = Technology.Contactors[i];
                    CurrentContactor.bEnabled =
                        (CurrentSet["Contactor.bEnabled"] as bool[])[i];
                    CurrentContactor.Impulse =
                        (CurrentSet["Contactor.Impulse"] as decimal[])[i];
                    CurrentContactor.Pause =
                        (CurrentSet["Contactor.Pause"] as decimal[])[i];
                    CurrentContactor.StartDelay =
                        (CurrentSet["Contactor.StartDelay"] as decimal[])[i];
                    CurrentContactor.StopDelay =
                        (CurrentSet["Contactor.StopDelay"] as decimal[])[i];
                }
                
                for (int i = 0; i < Technology.FCs.Count; i++)
                {
                    var CurrentFC = Technology.FCs[i];
                    CurrentFC.bEnabled =
                        (CurrentSet["FC.bEnabled"] as bool[])[i];
                    CurrentFC.CurrentFrequency =
                        (CurrentSet["FC.CurrentFrequency"] as int[])[i];
                    CurrentFC.StartDelay =
                        (CurrentSet["FC.StartDelay"] as decimal[])[i];
                    CurrentFC.StopDelay =
                        (CurrentSet["FC.StopDelay"] as decimal[])[i];
                }
                AdminPassword = CurrentSet["AdminPassword"] as string;
                if(AdminPassword == null)
                {
                    AdminPassword = "";
                }
            }
            catch
            { }
        }

        public static void SaveSet(string SetName)
        {
            CurrentSetName = SetName;
            SaveSet();
        }

        public static void SaveSet()
        {
            var NewSet = new Hashtable();
            var HVPSCount = Technology.HVPSes.Count;
            var ContactorCount = Technology.Contactors.Count;
            var FCCount = Technology.FCs.Count;
            var CleanerCount = Technology.Cleaners.Count;
            NewSet.Add("HVPS.bEnabled", new bool[HVPSCount]);
            for(int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.bEnabled"] as bool[];
                    CurrentSetting[i] = Technology.HVPSes[i].bEnabled;
            }
            NewSet.Add("HVPS.OutVoltage", new decimal[HVPSCount]);
            for(int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.OutVoltage"] as decimal[];
                    CurrentSetting[i] = Technology.HVPSes[i].VoltageOut;
            }
            NewSet.Add("HVPS.bPolarity", new bool[HVPSCount]);
            for(int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.bPolarity"] as bool[];
                CurrentSetting[i] = Technology.HVPSes[i].bPolarity;
            }
            NewSet.Add("HVPS.StartDelay", new decimal[HVPSCount]);
            for (int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.StartDelay"] as decimal[];
                CurrentSetting[i] = Technology.HVPSes[i].StartDelay;
            }
            NewSet.Add("HVPS.StopDelay", new decimal[HVPSCount]);
            for (int i = 0; i < HVPSCount; i++)
            {
                var CurrentSetting = NewSet["HVPS.StopDelay"] as decimal[];
                CurrentSetting[i] = Technology.HVPSes[i].StopDelay;
            }
            NewSet.Add("Cleaner.bEnabled", new bool[CleanerCount]);
            for (int i = 0; i < CleanerCount; i++)
            {
                var CurrentSetting = NewSet["Cleaner.bEnabled"] as bool[];
                CurrentSetting[i] = Technology.Cleaners[i].bEnabled;
            }
            NewSet.Add("Cleaner.CleanPause", new long[CleanerCount]);
            for (int i = 0; i < CleanerCount; i++)
            {
                var CurrentSetting = NewSet["Cleaner.CleanPause"] as long[];
                CurrentSetting[i] = Technology.Cleaners[i].CleanPause;
            }
            NewSet.Add("Cleaner.MaxCleanTimeout", new long[CleanerCount]);
            for (int i = 0; i < CleanerCount; i++)
            {
                var CurrentSetting = NewSet["Cleaner.MaxCleanTimeout"] as long[];
                CurrentSetting[i] = Technology.Cleaners[i].MaxCleanTimeout;
            }
            NewSet.Add("Cleaner.StartDelay", new decimal[CleanerCount]);
            for (int i = 0; i < CleanerCount; i++)
            {
                var CurrentSetting = NewSet["Cleaner.StartDelay"] as decimal[];
                CurrentSetting[i] = Technology.Cleaners[i].StartDelay;
            }
            NewSet.Add("Cleaner.StopDelay", new decimal[CleanerCount]);
            for (int i = 0; i < CleanerCount; i++)
            {
                var CurrentSetting = NewSet["Cleaner.StopDelay"] as decimal[];
                CurrentSetting[i] = Technology.Cleaners[i].StopDelay;
            }
            NewSet.Add("Contactor.bEnabled", new bool[ContactorCount]);
            for (int i = 0; i < ContactorCount; i++)
            {
                var CurrentSetting = NewSet["Contactor.bEnabled"] as bool[];
                CurrentSetting[i] = Technology.Contactors[i].bEnabled;
            }
            NewSet.Add("Contactor.Impulse", new decimal[ContactorCount]);
            for (int i = 0; i < ContactorCount; i++)
            {
                var CurrentSetting = NewSet["Contactor.Impulse"] as decimal[];
                CurrentSetting[i] = Technology.Contactors[i].Impulse;
            }
            NewSet.Add("Contactor.Pause", new decimal[ContactorCount]);
            for (int i = 0; i < ContactorCount; i++)
            {
                var CurrentSetting = NewSet["Contactor.Pause"] as decimal[];
                CurrentSetting[i] = Technology.Contactors[i].Pause;
            }
            NewSet.Add("Contactor.StartDelay", new decimal[ContactorCount]);
            for (int i = 0; i < ContactorCount; i++)
            {
                var CurrentSetting = NewSet["Contactor.StartDelay"] as decimal[];
                CurrentSetting[i] = Technology.Contactors[i].StartDelay;
            }
            NewSet.Add("Contactor.StopDelay", new decimal[ContactorCount]);
            for (int i = 0; i < ContactorCount; i++)
            {
                var CurrentSetting = NewSet["Contactor.StopDelay"] as decimal[];
                CurrentSetting[i] = Technology.Contactors[i].StopDelay;
            }
            NewSet.Add("FC.bEnabled", new bool[FCCount]);
            for (int i = 0; i < FCCount; i++)
            {
                var CurrentSetting = NewSet["FC.bEnabled"] as bool[];
                CurrentSetting[i] = Technology.FCs[i].bEnabled;
            }
            NewSet.Add("FC.CurrentFrequency", new int[FCCount]);
            for (int i = 0; i < FCCount; i++)
            {
                var CurrentSetting = NewSet["FC.CurrentFrequency"] as int[];
                CurrentSetting[i] = Technology.FCs[i].CurrentFrequency;
            }
            NewSet.Add("FC.StartDelay", new decimal[FCCount]);
            for (int i = 0; i < FCCount; i++)
            {
                var CurrentSetting = NewSet["FC.StartDelay"] as decimal[];
                CurrentSetting[i] = Technology.FCs[i].StartDelay;
            }
            NewSet.Add("FC.StopDelay", new decimal[FCCount]);
            for (int i = 0; i < FCCount; i++)
            {
                var CurrentSetting = NewSet["FC.StopDelay"] as decimal[];
                CurrentSetting[i] = Technology.FCs[i].StopDelay;
            }
            NewSet.Add("AdminPassword", AdminPassword);
            if(SetDictionary.ContainsKey(CurrentSetName))
            {
                SetDictionary[CurrentSetName] = NewSet;
            }
            else
            {
                SetDictionary.Add(CurrentSetName, NewSet);
            }
        }

        public static void DeleteSet(string SetName)
        {
            SetDictionary.Remove(SetName);
        }

        public static void SaveAll()
        {
            var BF = new BinaryFormatter();
            try
            {
                using (FileStream FS = new FileStream(SETTINGS_FILE_NAME, FileMode.OpenOrCreate))
                {
                    BF.Serialize(FS, SetDictionary);
                }
            }
            catch
            { }
            SaveCalibration();
        }
    }
}
