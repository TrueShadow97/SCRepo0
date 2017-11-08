/*
 * Created by SharpDevelop.
 * User: Pavillion G-6
 * Date: 20.11.2016
 * Time: 11:08
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Separator
{
	/// <summary>
	/// Базовый класс двигателя.
	/// </summary>
	public abstract class Engine
	{
		public bool bEnabled {get; set;}
		public string Name {get; set;}
		public bool bPowerState {get; protected set;}
		public bool bInitialState {get; protected set;}
        protected bool _bEmergency;
		public bool bEmergency
        {
            get
            {
                return _bEmergency;
            }
            set
            {
                if(!_bEmergency)
                {
                    _bEmergencyAcknowledged = false;
                }
                _bEmergency = value;
            }
        }
        protected bool _bEmergencyAcknowledged;
        public bool bEmergencyAcknowledged
        {
            get
            {
                var bRes = _bEmergencyAcknowledged;
                _bEmergencyAcknowledged = true;
                return bRes;
            }
        }
		
		public DataPoint PowerStateSignal;
		
		protected Engine(string Name)
		{
			this.Name = Name;
		}
		
		protected Engine(): this("Unknown engine")
		{}
		
		public abstract void Tick(decimal DeltaTime);
        public abstract void ForcePowerOff();
	}
}
