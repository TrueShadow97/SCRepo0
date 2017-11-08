/*
 * Created by SharpDevelop.
 * User: Pavillion G-6
 * Date: 20.11.2016
 * Time: 16:21
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Separator
{
	/// <summary>
	/// Очиститель электрода.
	/// </summary>
	public class Cleaner: Engine, IDelayable
	{
		public long CleanPause;
		public long MaxCleanTimeout;
		protected short Position;
		protected bool bServoState;
		public bool bDesiredLocation {get; protected set;}
		protected bool bCrossState;
		protected bool bMoveOnce {get; set;}
		protected decimal CleanPauseCounter;
		protected decimal CleanTimeoutCounter;
		public decimal StartDelay {get; set;}
		public decimal StopDelay {get; set;}
		protected decimal StartDelayCounter {get; set;}
		protected decimal StopDelayCounter {get; set;}
		
		public DataPoint LocationSignal;
		public DataPoint ServoSignal;
		public DataPoint Edge1Signal;
		public DataPoint Edge2Signal;
		
		public Cleaner(string Name): base(Name)
		{
			StartDelayCounter = 0;
			StopDelayCounter = 0;
		}
		
		public Cleaner(): this("Cleaner")
		{}
		
		public void ResetEmergency()
		{
			bEmergency = false;
		}
		
		public void SwitchState()
		{
			bInitialState = !bInitialState;
		}
		
		public void LaunchOnce(ushort Direction)
		{
			StartDelayCounter = StartDelay;
			CleanPauseCounter = CleanPause;
			bInitialState = true;
			bPowerState = true;
			bMoveOnce = true;
			bDesiredLocation = (Direction == 1);
		}
		
		/// <summary>
		/// Don't call it. It does nothing. Absolutely nothing.
		/// </summary>
		public void ForceState()
		{}

        public override void ForcePowerOff()
        {
            bInitialState = false;
            bCrossState = false;
            StartDelayCounter = 0;
            StopDelayCounter = 0;
        }

        public override void Tick(decimal DeltaTime)
		{
			if(!bEnabled) return;
			
			if(Edge1Signal != null && Edge2Signal != null)
				Position = (short)(Edge2Signal.GetBit() ? 2 : (Edge1Signal.GetBit() ? 1 : 0));
			
			if(bEmergency)
			{
				bInitialState = false;
				bCrossState = false;
				StartDelayCounter = 0;
				StopDelayCounter = 0;
			}
			else
			{
				CountDelays(DeltaTime);
			}
			
			if(bCrossState)
			{
				if(bPowerState)
				{
					if((!bDesiredLocation && Position == 2) || (bDesiredLocation && Position == 1))
					{
						bPowerState = false;
						bServoState = false;
						bDesiredLocation = false;
						bInitialState = !bMoveOnce;
						bMoveOnce = false;
					}
					else
					{
						CleanTimeoutCounter += DeltaTime;
						if(CleanTimeoutCounter > MaxCleanTimeout)
						{
							bPowerState = false;
							bServoState = false;
							bDesiredLocation = false;
							bEmergency = true;
							CleanTimeoutCounter = 0;
						}
					}
				}
				else
				{
					CleanPauseCounter += DeltaTime;
					if(CleanPauseCounter > CleanPause)
					{
						CleanPauseCounter -= CleanPause;
						bPowerState = true;
						bServoState = true;
						bDesiredLocation = (Position == 2);
					}
				}
			}
			else
			{
				bPowerState = false;
				bServoState = false;
				bDesiredLocation = false;
				CleanTimeoutCounter = 0;
			}
			
			if(PowerStateSignal != null) PowerStateSignal.SetBit(bPowerState && bInitialState);
			if(ServoSignal != null) ServoSignal.SetBit(bServoState && bInitialState);
			if(LocationSignal != null) LocationSignal.SetBit(bDesiredLocation && bInitialState);
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
				bCrossState |= (StartDelayCounter > StartDelay);
			}
			else
			{
				StartDelayCounter = 0;
				if(StopDelayCounter <= StopDelay)
				{
					StopDelayCounter += DeltaTime;
				}
				bCrossState &= (StopDelayCounter <= StopDelay);
			}
		}
	}
}
