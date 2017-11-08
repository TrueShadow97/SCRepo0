using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Separator
{
    public enum EFCCalibrationMode
    {
        Scaling,
        Cutoff
    }
    public class FrequencyConverter: Engine, IDelayable
    {
        public FCBuffer Buffer { get; protected set; }
        public decimal StartDelay { get; set; }
        public decimal StopDelay { get; set; }
        protected decimal StartDelayCounter { get; set; }
        protected decimal StopDelayCounter { get; set; }
        public decimal ErrorCode { get; protected set; }
        public int CurrentFrequency
        {
            get
            {
                return _CurrentFrequency;
            }
            set
            {
                _CurrentFrequency = value;
                if(_CurrentFrequency > CalibrationMaxFrequency)
                {
                    _CurrentFrequency = CalibrationMaxFrequency;
                }
                if(_CurrentFrequency < CalibrationMinFrequency)
                {
                    _CurrentFrequency = CalibrationMinFrequency;
                }
            }
        }
        protected int _CurrentFrequency;
        public int CalibrationMinFrequency { get; set; }
        public int CalibrationMaxFrequency { get; set; }
        public int ConvertedOutput { get; protected set; }
        public bool bConnected { get; protected set; }
        public EFCCalibrationMode CalibrationMode { get; set; } = EFCCalibrationMode.Cutoff;

        public FrequencyConverter(string Name)
            : base(Name)
        { }

        public FrequencyConverter()
            : this("Frequency Converter")
        { }

        public void ApplyBuffer(FCBuffer Buffer)
        {
            this.Buffer = Buffer;
        }

        public void SwitchState()
        {
            bInitialState = !bInitialState;
        }

        public void ForceState()
        {
            bPowerState = bInitialState;
            if (bPowerState)
            {
                StartDelayCounter = StartDelay;
            }
            else
            {
                StopDelayCounter = StopDelay;
            }
        }

        public override void ForcePowerOff()
        {
            bInitialState = false;
            bPowerState = false;
            StartDelayCounter = 0;
            StopDelayCounter = 0;
        }

        public void AcknowledgeError()
        {
            if (!bEmergency)
                return;
            if (Buffer != null)
            {
                Buffer.ErrorProcessStatus = EErrorProcessStatus.AcknowledgedByUser;
            }
            int i = 0;
            while (Buffer.State > 6)
            {
                Thread.Sleep(2);
                i++;
                if (i >= 500)
                {
                    return;
                }
            }
            bEmergency = false;
            ErrorCode = 0;
        }

        bool bRecentPowerState;

        public override void Tick(decimal DeltaTime)
        {
            var bRecentEmergency = bEmergency;
            var RecentErrorCode = ErrorCode;
            if (!bEnabled) return;

            if (Buffer != null)
            {
                Buffer.Tick(DeltaTime);
                bConnected = Buffer.TimeSinceLastResponse <= 5 || !Program.bComEnabled;
                bEmergency |= Buffer.State > 6;
            }

            if (bEmergency)
            {
                if(!bRecentEmergency)
                {
                    Program.Log(Name + " got an error", ELogType.Error);
                }
                bInitialState = false;
                bPowerState = false;
                StartDelayCounter = 0;
                StopDelayCounter = 0;
                if (Buffer != null)
                {
                    //if (Buffer.ErrorProcessStatus == EErrorProcessStatus.GotCode)
                    //{
                        ErrorCode = Buffer.ErrorCode / 10;
                        if (RecentErrorCode != ErrorCode)
                        {
                            Program.Log(Name + " received error code " + ErrorCode.ToString("F1"),
                                ELogType.Info);
                        }
                    //}
                }
            }
            else
            {
                var bCrossState = CountDelays(DeltaTime);
                if (bCrossState.HasValue)
                {
                    bPowerState = bCrossState.Value;
                }
            }

            if (!bRecentPowerState && bPowerState)
            {
                Program.Log(Name + " launched", ELogType.Info);
            }
            if (bRecentPowerState && !bPowerState)
            {
                Program.Log(Name + " stopped", ELogType.Info);
            }

            if (Buffer != null)
            {
                Buffer.bEngineOn = bPowerState;
                var CurrentMin = CalibrationMode == EFCCalibrationMode.Scaling ?
                    CalibrationMinFrequency : 0;
                var CurrentMax = CalibrationMode == EFCCalibrationMode.Scaling ?
                    CalibrationMaxFrequency : 100;
                if (bPowerState)
                {
                    ConvertedOutput = (int)Program.BilinearConversion(CurrentFrequency,
                        CurrentMin, CurrentMax,
                        0, 0x4000, true);
                    Buffer.SW1 = (ushort)ConvertedOutput;
                }
            }
            if (PowerStateSignal != null) PowerStateSignal.SetBit(bPowerState);
            bRecentPowerState = bPowerState;
        }

        protected bool? CountDelays(decimal DeltaTime)
        {
            if (bInitialState)
            {
                StopDelayCounter = 0;
                if (StartDelayCounter <= StartDelay)
                {
                    StartDelayCounter += DeltaTime;
                }
                if (StartDelayCounter > StartDelay) 
                {
                    return true;
                }
            }
            else
            {
                StartDelayCounter = 0;
                if (StopDelayCounter <= StopDelay)
                {
                    StopDelayCounter += DeltaTime;
                }
                if (StopDelayCounter > StopDelay)
                {
                    return false;
                }
            }
            return null;
        }
    }
}
