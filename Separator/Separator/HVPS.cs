/*
 * Created by SharpDevelop.
 * User: Pavillion G-6
 * Date: 20.11.2016
 * Time: 11:19
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Runtime.Remoting.Contexts;

namespace Separator
{
	/// <summary>
	/// Description of HVPS.
	/// </summary>
	public class HVPS: Engine, IDelayable
	{
		public bool bEmergencyReset {get; protected set;}
		public decimal VoltageOut {get; set;}
		public decimal VoltageIn {get; set;}
		public decimal CurrentIn {get; set;}
		public decimal StartDelay {get; set;}
		public decimal StopDelay {get; set;}
		protected decimal StartDelayCounter {get; set;}
		protected decimal StopDelayCounter {get; set;}
		protected decimal EmergencyReleaseCounter {get; set;}
		public bool bPolarity {get; set;}
		public decimal MinOutput {get; set;}
        public decimal MaxOutput {get; set;}
        public decimal ReverseMinOutput { get; set; }
        public decimal ReverseMaxOutput { get; set; }
        public decimal RealInputAt0kV {get; set;}
		public decimal RealInputAt30kV {get; set;}
        public decimal ReverseRealInputAt0kV { get; set; }
        public decimal ReverseRealInputAt30kV { get; set; }
        public decimal RealInputAt0mA {get; set;}
		public decimal RealInputAtMaxmA {get; set;}
		public decimal MaxmA {get; set;}
        public decimal ReverseRealInputAt0mA { get; set; }
        public decimal ReverseRealInputAtMaxmA { get; set; }
        public decimal ReverseMaxmA { get; set; }
        protected bool bFormerPowerState {get; set;}
        public decimal StartRampLength { get; set; }
        protected decimal StartRampCounter { get; set; }
        protected decimal ActualVoltageOut { get; set; }
        public int MaxScaleOut { get; set; } = 2000;
		
		
		public DataPoint EmergencyResetSignal;
		public DataPoint VoltageOutSignal;
		public DataPoint VoltageInSignal;
		public DataPoint CurrentInSignal;
		public DataPoint EmergencySignal;
        public DataPoint PolaritySignal;
		
		public HVPS(string Name): base(Name)
		{
			StartDelayCounter = 0;
			StopDelayCounter = 0;
		}
		
		public HVPS(): this("HVPS")
		{}
		
		public void SwitchState()
		{
			bInitialState = !bInitialState;
		}
		
		public void EmergencyResetPush()
		{
			bEmergencyReset |= bEmergency;
		}
		
		public void EmergencyResetRelease()
		{
			bEmergencyReset = false;
			bEmergency = false;
		}
		
		public override void Tick(decimal DeltaTime)
		{
            var bRecentEmergency = bEmergency;
			if(!bEnabled) return;

            var SelectedRealInputAt0kV = !bPolarity ? RealInputAt0kV : ReverseRealInputAt0kV;
            var SelectedRealInputAt30kV = !bPolarity ? RealInputAt30kV : ReverseRealInputAt30kV;
            var SelectedRealInputAt0mA = !bPolarity ? RealInputAt0mA : ReverseRealInputAt0mA;
            var SelectedRealInputAtMaxmA = !bPolarity ? RealInputAtMaxmA : ReverseRealInputAtMaxmA;
            var SelectedMaxmA = !bPolarity ? MaxmA : ReverseMaxmA;
            if (VoltageInSignal != null) VoltageIn = 
				Program.BilinearConversion
				(unchecked((short)(VoltageInSignal.GetData())), SelectedRealInputAt0kV * 200, 
                SelectedRealInputAt30kV * 200, 0, 30);
			if(CurrentInSignal != null) CurrentIn = 
				Program.BilinearConversion
				(unchecked((short)(CurrentInSignal.GetData())), SelectedRealInputAt0mA * 200, 
                SelectedRealInputAtMaxmA * 200, 0, SelectedMaxmA);
			bEmergency |= (EmergencySignal != null) && EmergencySignal.GetBit();

            if(!bRecentEmergency && bEmergency)
            {
                Program.Log("Emergency signal on " + Name, ELogType.Error);
            }
			
			if(bEmergency)
			{
				bInitialState = false;
				bPowerState = false;
				// VoltageOut = 0m;
				StartDelayCounter = 0;
				StopDelayCounter = 0;
				if(bEmergencyReset)
				{
					EmergencyReleaseCounter += DeltaTime;
					if(EmergencyReleaseCounter > 1)
					{
						EmergencyResetRelease();
						EmergencyReleaseCounter = 0;
					}
				}
			}
			else
			{
				CountDelays(DeltaTime);
				if(!bPowerState)
				{
					bEmergencyReset = false;
				}
                if(!bFormerPowerState && bPowerState)
                {
                    Program.Log(Name + " launched", ELogType.Info);
                }
                if(bFormerPowerState && !bPowerState)
                {
                    Program.Log(Name + " stopped", ELogType.Info);
                }
			}
			bFormerPowerState = bPowerState;

            if(StartRampLength > 0)
            {
                if (bPowerState)
                {
                    StartRampCounter = this.Min(StartRampCounter + DeltaTime, StartRampLength);
                }
                else
                {
                    StartRampCounter = 0;
                }
                ActualVoltageOut = VoltageOut / StartRampLength * StartRampCounter;
            }
            else
            {
                ActualVoltageOut = VoltageOut;
            }

            var SelectedMinOutput = !bPolarity ? MinOutput : ReverseMinOutput;
            var SelectedMaxOutput = !bPolarity ? MaxOutput : ReverseMaxOutput;

            if (PowerStateSignal != null) PowerStateSignal.SetBit(bPowerState);
			if(EmergencyResetSignal != null) EmergencyResetSignal.SetBit(bEmergencyReset);
            if (VoltageOutSignal != null) VoltageOutSignal.SetData
                    ((ushort)Program.BilinearConversion
                    (bPowerState ? ActualVoltageOut : 0, SelectedMinOutput, SelectedMaxOutput, 0,
                    MaxScaleOut, true));
            if(PolaritySignal != null)
            {
                PolaritySignal.SetBit(!bPolarity);
            }
		}
		
		public void ForceState()
		{
			bPowerState = bInitialState;
			if(bPowerState)
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

        protected void CountDelays(decimal DeltaTime)
		{
			if(bInitialState)
			{
				StopDelayCounter = 0;
				if(StartDelayCounter <= StartDelay)
				{
					StartDelayCounter += DeltaTime;
				}
				bPowerState |= (StartDelayCounter > StartDelay);
			}
			else
			{
				StartDelayCounter = 0;
				if(StopDelayCounter <= StopDelay)
				{
					StopDelayCounter += DeltaTime;
				}
				bPowerState &= (StopDelayCounter <= StopDelay);
			}
		}
	}
}
