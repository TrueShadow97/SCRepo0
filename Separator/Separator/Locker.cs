/*
 * Created by SharpDevelop.
 * User: Pavillion G-6
 * Date: 21.11.2016
 * Time: 15:05
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Runtime.Remoting.Contexts;

namespace Separator
{
	/// <summary>
	/// Description of Locker.
	/// </summary>
	public class Locker
	{
		public bool bEnabled;
		public bool bState {get; protected set;}
		public bool bEmergency {get; protected set;}
		public string Name {get; protected set;}
        public decimal FireTimeout { get; set; } = 0.5m;
        protected decimal FireTimeoutCounter { get; set; }
        public int Group { get; set; }
		
		public DataPoint StateSignal;
		
		public Locker(string Name)
		{
			this.Name = Name;
		}
		
		public Locker(): this("Unknown blocker")
		{}
		
        public void ResetEmergency()
        {
            bEmergency = false;
        }

		public void Tick(decimal DeltaTime)
		{
            var bRecentEmergency = bEmergency;
			if(!bEnabled || !Program.bComEnabled) 
			{
                bState = true;
				bEmergency = false;
				return;
			}
			
			bState = (StateSignal != null) && StateSignal.GetBit();
			
            if(Group == 1 && !bState)
            {
                if (!bEmergency)
                {
                    FireTimeoutCounter += DeltaTime;
                    if (FireTimeoutCounter >= FireTimeout)
                    {
                        bEmergency = true;
                    }
                }
            }
            else
            {
                FireTimeoutCounter = 0;
            }

			// bEmergency = Group == 1 ? !bState : false;

            if(!bRecentEmergency && bEmergency)
            {
                Program.Log(Name + " fired", ELogType.Error);
            }
		}
	}
}
