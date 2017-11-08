using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Separator
{
    public static class Technology
    {
        static public Hashtable Roster { get; set; } = new Hashtable();
        static public List<Locker> Lockers = new List<Locker>();
        static public List<Contactor> Contactors = new List<Contactor>();
        static public List<FrequencyConverter> FCs = new List<FrequencyConverter>();
        static public List<Cleaner> Cleaners = new List<Cleaner>();
        static public List<HVPS> HVPSes = new List<HVPS>();
        static public TimeSpan MotorTime { get; set; } = new TimeSpan();
        static private Stopwatch Watch { get; set; } = new Stopwatch();
        static private bool bLaunched;
        static private object LaunchLock = new object();
        static public string StateString;
        static public object StateStringLock = new object();
        static public DataPoint EmergencySignal = new DataPoint(
            CommunicationLoop.ModbusBuffers[0], EDataMode.Discrete, 0x1000 + 100, 0);
        static public DataPoint BlockerIndicationSignal = new DataPoint(
            CommunicationLoop.ModbusBuffers[1], EDataMode.Discrete, 0x1000 + 201, 9);
        static public DataPoint EmergencyStopIndicationSignal = new DataPoint(
            CommunicationLoop.ModbusBuffers[1], EDataMode.Discrete, 0x1000 + 201, 8);
        static public DataPoint EmergencyIndicationSignal = new DataPoint(
            CommunicationLoop.ModbusBuffers[1], EDataMode.Discrete, 0x1000 + 201, 4);
        static public DataPoint WorkIndicationSignal { get; set; } = new DataPoint(
            CommunicationLoop.ModbusBuffers[1], EDataMode.Discrete, 0x1000 + 201, 5);
        static public Timer MotorTimeSaveTimer;

        static public void InitRoster()
        {
            byte[][] LockerAddresses =
            {
                new byte[] {100, 1},//{100, 2},
                new byte[] {101, 4},//{100, 3},
                new byte[] {101, 5},//{101, 6},
                new byte[] {100, 2},//{101, 7},
                new byte[] {100, 3},//{100, 4},
                new byte[] {100, 4},//{100, 5},
                new byte[] {100, 5},//{101, 4},
                new byte[] {101, 6},//{101, 5},
                new byte[] {101, 7},//{0, 0}, // isn't used
                new byte[] {101, 8},
                new byte[] {101, 10},
                new byte[] {101, 9},
                new byte[] {101, 11},
            };
            for(int i = 0; i < 13; i++)
            {
                Lockers.Add(new Locker("Locker " + (i + 1)));
                if(i < 3)
                {
                    Lockers[i].Group = 0;
                }
                else
                {
                    Lockers[i].Group = 1;
                }
                Lockers[i].bEnabled = true;
                Lockers[i].StateSignal = new DataPoint(CommunicationLoop.ModbusBuffers[0],
                    EDataMode.Discrete, (ushort)(0x1000 + LockerAddresses[i][0]),
                    LockerAddresses[i][1]);
                Roster.Add("Locker " + i, Lockers[i]);
            }
            /*var CommonDirectionSignal = new DataPoint(CommunicationLoop.ModbusBuffers[1],
                    EDataMode.Discrete, 0x1000 + 202, 6);
            CommonCleanerGeneratorPower = new DataPoint
            (CommunicationLoop.ModbusBuffers[1], EDataMode.Discrete, 0x1000 + 202, 5);
            for (int i = 0; i < 2; i++)
            {
                Cleaners.Add(new Cleaner("Cleaner " + i));
                Cleaners[i].Edge1Signal = new DataPoint(CommunicationLoop.ModbusBuffers[0],
                    EDataMode.Discrete, 0x1000 + 101, (byte)(12 + i * 2));
                Cleaners[i].Edge2Signal = new DataPoint(CommunicationLoop.ModbusBuffers[0],
                    EDataMode.Discrete, 0x1000 + 101, (byte)(13 + i * 2));
                Cleaners[i].LocationSignal = CommonDirectionSignal;
                Cleaners[i].PowerStateSignal = new DataPoint(CommunicationLoop.ModbusBuffers[1],
                    EDataMode.Discrete, 0x1000 + 201, (byte)(12 + i));
                Cleaners[i].bEnabled = true;
                Roster.Add("Cleaner " + i, Cleaners[i]);
            }*/
            byte[][] ContactorBCStateAddresses =
            {
                new byte[] {100, 10},
                new byte[] {100, 11},
                new byte[] {100, 13},
                new byte[] {100, 14},
                new byte[] {100, 15},
                new byte[] {101, 0},
                new byte[] {100, 12},
                new byte[] {101, 1},
                new byte[] {100, 9},
                new byte[] {100, 8}
            };
            byte[][] ContactorPowerStateAddresses =
            {
                new byte[] {200, 10},
                new byte[] {200, 11},
                new byte[] {200, 13},
                new byte[] {200, 14},
                new byte[] {200, 15},
                new byte[] {201, 0},
                new byte[] {200, 12},
                new byte[] {201, 1},
                new byte[] {200, 9},
                new byte[] {200, 8}
            };
            for (int i = 0; i < 10; i++)
            {
                Contactors.Add(new Contactor("Contactor " + (i + 1)));
                Contactors[i].BCDelay = 1.5m;
                Contactors[i].BCStateSignal = new DataPoint(CommunicationLoop.ModbusBuffers[0],
                    EDataMode.Discrete, (ushort)(0x1000 + ContactorBCStateAddresses[i][0]),
                    ContactorBCStateAddresses[i][1]);
                Contactors[i].PowerStateSignal = new DataPoint(CommunicationLoop.ModbusBuffers[1],
                    EDataMode.Discrete, (ushort)(0x1000 + ContactorPowerStateAddresses[i][0]),
                    ContactorPowerStateAddresses[i][1]);
                Contactors[i].bEnabled = true;
                Roster.Add("Contactor " + i, Contactors[i]);
            }
            var FCBufferNumbers = new int[] { 6, 5, 4, 2, 3, 1, 0 };
            for (int i = 0; i < 7; i++)
            {
                FCs.Add(new FrequencyConverter("FC " + i));
                if (!CommunicationLoop.bDisableFCs)
                {
                    FCs[i].ApplyBuffer(CommunicationLoop.USSBuffers[i]);
                }
                FCs[i].bEnabled = true;
                Roster.Add("FC " + i, FCs[i]);
            }
            for (int i = 0; i < 2; i++)
            {
                HVPSes.Add(new HVPS("HVPS " + (i + 1)));
                if (i == 0)
                {
                    HVPSes[i].CurrentInSignal = new DataPoint(CommunicationLoop.ModbusBuffers[2],
                        EDataMode.Analog, 0x1000 + 301);
                    HVPSes[i].EmergencySignal = new DataPoint(CommunicationLoop.ModbusBuffers[0],
                        EDataMode.Discrete, 0x1000 + 101, 3);
                }
                HVPSes[i].PowerStateSignal = new DataPoint(CommunicationLoop.ModbusBuffers[1],
                    EDataMode.Discrete, 0x1000 + 201, (byte)(6 - i * 3));
                HVPSes[i].VoltageInSignal = new DataPoint(CommunicationLoop.ModbusBuffers[2],
                    EDataMode.Analog, 0x1000 + 300);
                HVPSes[i].VoltageOutSignal = new DataPoint(CommunicationLoop.ModbusBuffers[3],
                    EDataMode.Analog, (ushort)(0x1000 + 400 + i));
                HVPSes[i].bEnabled = true;
                Roster.Add("HVPS " + i, HVPSes[i]);
            }
            HVPSes[1].CurrentInSignal = new DataPoint(CommunicationLoop.ModbusBuffers[4],
                EDataMode.Analog, 0x1000 + 506);
            HVPSes[1].bPolarity = false;
            HVPSes[0].PolaritySignal = new DataPoint(CommunicationLoop.ModbusBuffers[1],
                EDataMode.Discrete, 0x1000 + 201, 7);
            HVPSes[0].MaxScaleOut = 440;
        }

        static private void LoadMotorHours()
        {
            var bf = new BinaryFormatter();
            try
            {
                MotorTime = (TimeSpan)bf.Deserialize(new FileStream
                    ("motortime.dat", FileMode.Open));
            }
            catch
            {
                MotorTime = new TimeSpan(0, 0, 0);
            }
        }
        static private void SaveMotorHours(object State)
        {
            var bf = new BinaryFormatter();
            try
            {
                bf.Serialize(new FileStream("motortime.dat", FileMode.Create), MotorTime);
            }
            catch (Exception e)
            {
                Program.Log("Error during saving motor time data: " + e.Message + "; " +
                    e.StackTrace, ELogType.Error);
            }
        }

        /// <summary>
        /// Start technology loop that processes all engines.
        /// </summary>
        static public void StartLoop()
        {
            var bLocalLaunchParameter = false;
            lock(LaunchLock)
            {
                bLocalLaunchParameter = bLaunched;
            }
            if (!bLocalLaunchParameter)
            {
                LoadMotorHours();
                MotorTimeSaveTimer = new Timer(SaveMotorHours, null, 0, 30000);
                lock (LaunchLock)
                {
                    bLaunched = true;
                }
                Task.Factory.StartNew(Loop);
            }
        }

        /// <summary>
        /// Stop the technology loop.
        /// </summary>
        static public void EndLoop()
        {
            var bLocalLaunchParameter = false;
            lock(LaunchLock)
            {
                bLocalLaunchParameter = bLaunched;
            }
            if(bLocalLaunchParameter)
            {
                lock(LaunchLock)
                {
                    bLaunched = false;
                }
            }
        }

        static public bool bCloneWorkIndication { get; set; }
        static bool bCommonEmergency { get; set; }

        static private void Loop()
        {
            bool bRecentEmergency = false;
            decimal EmergencyButtonBuffer = 0;
            Thread.Sleep(2500);
            try
            {
                var bLocalLaunchParameter = false;
                int UpdateCounter = 0;
                do
                {
                    var SelectedLib = MainWindow.Instance.SelectedLib;
                    bool bSystemLaunched = false;
                    long ElapsedMilliseconds = 0;
                    if (Watch.ElapsedMilliseconds > 0 || !Watch.IsRunning)
                    {
                        ElapsedMilliseconds = Watch.ElapsedMilliseconds;
                        Watch.Restart();
                    }
                    if (ElapsedMilliseconds > 0)
                    {
                        bCloneWorkIndication = false;
                        BlockerIndicationSignal.SetBit(false);
                        foreach (DictionaryEntry DE in Roster)
                        {
                            var Key = DE.Key;
                            var Value = DE.Value;
                            if (Value is Engine)
                            {
                                var CurrentEngine = Value as Engine;
                                CurrentEngine.Tick(ElapsedMilliseconds / (decimal)1000);
                                if (CurrentEngine.bEmergency)
                                {
                                    if (!CurrentEngine.bEmergencyAcknowledged)
                                    {
                                        if (SelectedLib != null)
                                        {
                                            string ErrorDescription = "";
                                            if (CurrentEngine is Contactor)
                                            {
                                                ErrorDescription = SelectedLib["Encountered a BC Error"];
                                            }
                                            else if (CurrentEngine is HVPS)
                                            {
                                                ErrorDescription = SelectedLib[
                                                    "Encountered a Hardware Error"];
                                            }
                                            else if (CurrentEngine is FrequencyConverter)
                                            {
                                                ErrorDescription = SelectedLib[
                                                    "Encountered an Error"] +
                                                    ", " + SelectedLib["code"] + " " +
                                                    (CurrentEngine as FrequencyConverter).ErrorCode;
                                            }
                                            try
                                            {
                                                StateString =
                                                    SelectedLib[CurrentEngine.Name] +
                                                    " " + ErrorDescription;
                                            }
                                            catch
                                            {
                                                Program.Log(CurrentEngine.Name + " has no entry in language libraries", ELogType.Error);
                                            }
                                        }
                                        CutAllPowerOff();
                                    }
                                    EmergencyIndicationSignal.SetBit(true);
                                }
                                if (CurrentEngine.bPowerState)
                                {
                                    bCloneWorkIndication = true;
                                    bSystemLaunched = true;
                                }
                            }
                            if (Value is Locker)
                            {
                                var CurrentEngine = Value as Locker;
                                CurrentEngine.Tick(ElapsedMilliseconds / (decimal)1000);
                                if (CurrentEngine.bEmergency)
                                {
                                    bCommonEmergency = true;
                                    BlockerIndicationSignal.SetBit(true);
                                    EmergencyIndicationSignal.SetBit(true);
                                }
                            }
                        }
                        WorkIndicationSignal.SetBit(bCloneWorkIndication);
                        if (bSystemLaunched)
                        {
                            MotorTime += new TimeSpan(0, 0, 0, 0, (int)ElapsedMilliseconds);
                        }
                        if (!EmergencySignal.GetBit())
                        {
                            EmergencyButtonBuffer++;
                            if(EmergencyButtonBuffer > 3)
                            {
                                if (bRecentEmergency)
                                {
                                    Program.Log("Emergency button pressed", ELogType.Operator);
                                }
                                bCommonEmergency = true;
                                EmergencyStopIndicationSignal.SetBit(true);
                                bRecentEmergency = false;
                            }
                        }
                        else
                        {
                            if(!bRecentEmergency)
                            {
                                Program.Log("Emergency button released", ELogType.Operator);
                            }
                            EmergencyStopIndicationSignal.SetBit(false);
                            EmergencyButtonBuffer = 0;
                            bRecentEmergency = true;
                        }
                        if (bCommonEmergency && Program.bComEnabled)
                        {
                            CutAllPowerOff();
                            EmergencyIndicationSignal.SetBit(true);
                        }
                        /*CommonCleanerGeneratorPower.SetBit
                            (Cleaners[0].bPowerState || Cleaners[1].bPowerState);*/

                    }
                    UpdateCounter++;
                    if (UpdateCounter >= 100)
                    {
                        App.Current.Dispatcher.Invoke(MainWindow.Instance.UpdateInterface);
                        UpdateCounter = 0;
                    }
                    // state string update
                    if (SelectedLib != null)
                    {
                        if (!EmergencySignal.GetBit() && Program.bComEnabled)
                        {
                            StateString = SelectedLib["Emergency Button Pressed"];
                        }
                        else if (!bCommonEmergency)
                        {
                            StateString = SelectedLib["Ready"];
                        }
                    }
                    Thread.Sleep(1);
                    lock (LaunchLock)
                    {
                        bLocalLaunchParameter = bLaunched;
                    }
                } while (bLocalLaunchParameter);
            }
            catch (Exception e)
            {
                Program.Log(e.Message + " at " + e.StackTrace, ELogType.Error);
                MessageBox.Show(e.Message, "Unhandled exception in the main execution loop",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        static private void CutAllPowerOff()
        {
            foreach (DictionaryEntry DE in Roster)
            {
                var Key = DE.Key;
                var Value = DE.Value;
                if (Value is Engine)
                {
                    var CurrentEngine = Value as Engine;
                    CurrentEngine.ForcePowerOff();
                }
            }
        }

        static private void GlobalChangeState(bool bState)
        {
            foreach (Contactor C in Contactors)
            {
                if (C.bInitialState != bState)
                {
                    C.SwitchState();
                }
            }
            foreach (Cleaner C in Cleaners)
            {
                if (C.bInitialState != bState)
                {
                    C.SwitchState();
                }
            }
            foreach (FrequencyConverter FC in FCs)
            {
                if (FC.bInitialState != bState)
                {
                    FC.SwitchState();
                }
            }
            foreach (HVPS H in HVPSes)
            {
                if (H.bInitialState != bState)
                {
                    H.SwitchState();
                }
            }
        }

        static public void StartAll()
        {
            GlobalChangeState(true);
        }

        static public void StopAll()
        {
            GlobalChangeState(false);
        }

        static bool bFCStopping = false;
        static public void ResetAll()
        {
            foreach (Contactor C in Contactors)
            {
                C.ResetEmergency();
            }
            foreach (Cleaner C in Cleaners)
            {
                C.ResetEmergency();
            }
            foreach (HVPS H in HVPSes)
            {
                Task.Factory.StartNew(() =>
                {
                    H.EmergencyResetPush();
                    Thread.Sleep(1000);
                    H.EmergencyResetRelease();
                });
            }
            foreach (Locker L in Lockers)
            {
                L.ResetEmergency();
            }
            if (!bFCStopping)
            {
                Task.Factory.StartNew(() =>
                {
                    bFCStopping = true;
                    foreach (FrequencyConverter FC in FCs)
                    {
                        FC.AcknowledgeError();
                    }
                    EmergencyIndicationSignal.SetBit(false);
                    bCommonEmergency = false;
                    CutAllPowerOff();
                    bFCStopping = false;
                });
            }
            else
            {
                EmergencyIndicationSignal.SetBit(false);
                bCommonEmergency = false;
                CutAllPowerOff();
            }
        }
    }
}