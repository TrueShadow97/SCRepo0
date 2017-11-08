/*
 * Created by SharpDevelop.
 * User: Pavillion G-6
 * Date: 21.11.2016
 * Time: 11:54
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Separator
{
	/// <summary>
	/// Description of Contactor.
	/// </summary>
	public class Contactor: Engine, IDelayable, IPWMable
	{
		public decimal StartDelay {get; set;}
		public decimal StopDelay {get; set;}
		protected decimal StartDelayCounter {get; set;}
		protected decimal StopDelayCounter {get; set;}
		public decimal Impulse {get; set;}
		public decimal Pause {get; set;}
		protected decimal PWMCounter {get; set;}
		protected bool bBCState {get; set;}
		public decimal BCDelay {get; set;}
		protected decimal BCDelayCounter {get; set;}
		protected bool bBCError {get; set;}
		protected bool bCrossState {get; set;}
		protected bool bFormerPowerState {get; set;}
		
		public DataPoint BCStateSignal;
		
		public void SwitchState()
		{
			bInitialState = !bInitialState;
		}
		
		public Contactor(string Name): base(Name)
		{
			PWMCounter = 0;
			StartDelayCounter = 0;
			StopDelayCounter = 0;
			BCDelay = 1;
		}
		
		public Contactor(): this("Unknown contactor")
		{}
		
		public override void Tick(decimal DeltaTime)
		{
			if(!bEnabled) return;
			
			bBCState = BCStateSignal != null ? BCStateSignal.GetBit() : bPowerState;

            if (Program.bComEnabled)
            {
                CountBC(DeltaTime);
            }
			
			bEmergency |= bBCError;
			
			if(bEmergency)
			{
				bInitialState = false;
				bPowerState = false;
				StartDelayCounter = 0;
				StopDelayCounter = 0;
				BCDelayCounter = 0;
			}
			else
			{
				CountDelays(DeltaTime);
				if(bCrossState) 
					CountImpulsePause(DeltaTime);
				else
					bPowerState = false;
			}
			bFormerPowerState = bPowerState;
			
			if(PowerStateSignal != null) PowerStateSignal.SetBit(bPowerState);
		}

        public override void ForcePowerOff()
        {
            bInitialState = false;
            ForceState();
        }

        public void ResetEmergency()
		{
			bBCError = false;
			bEmergency = false;
		}
		
		protected void CountBC(decimal DeltaTime)
		{
			if(bPowerState ^ bBCState)
			{
                var RecentError = bBCError;
				BCDelayCounter += DeltaTime;
				bBCError |= BCDelayCounter > BCDelay;
                if(!RecentError && bBCError)
                {
                    Program.Log("BC Error on " + Name, ELogType.Error);
                }
			}
			else
			{
				BCDelayCounter = 0;
			}
		}
		
		protected void CountImpulsePause(decimal DeltaTime)
		{
			if (Impulse >= 0 && Pause == 0)
			{
				bPowerState = true;
				return;
			}
			if (Impulse == 0 && Pause >= 0)
			{
				bPowerState = false;
				return;
			}
			PWMCounter += DeltaTime;
			if(PWMCounter > (Impulse + Pause))
			{
				PWMCounter -= Impulse + Pause;
			}
			bPowerState = (PWMCounter <= Impulse);
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
		
		protected void CountDelays(decimal DeltaTime)
		{
            var bRecentCS = bCrossState;
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
            if(!bRecentCS && bCrossState)
            {
                Program.Log(Name + " launched", ELogType.Info);
            }
            if(bRecentCS && !bCrossState)
            {
                Program.Log(Name + " stopped", ELogType.Info);
            }
		}
	}
}
