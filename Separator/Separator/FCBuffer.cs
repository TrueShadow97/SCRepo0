using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Separator
{
    public enum EErrorProcessStatus
    {
        NoError,
        GotFlag,
        GotCode,
        AcknowledgedByUser
    }

    public class FCBuffer: USSBuffer
    {
        public bool bDebugMode { get; } = false;
        public string LastMessage { get; protected set; }
        public int State { get; protected set; }
        public bool bEngineOn { get; set; }
        public EErrorProcessStatus ErrorProcessStatus { get; set; }
        public int ErrorCode { get; protected set; }
        public decimal TimeSinceLastResponse { get; protected set; }

        public FCBuffer()
        {
        }

        public FCBuffer(PortHandler Handler)
            :base(Handler)
        {
        }

        public void Tick(decimal DeltaTime)
        {
            TimeSinceLastResponse += DeltaTime;
        }

        public override void ReceiveData(byte[] Data)
        {
            base.ReceiveData(Data);
            LastMessage = "";
            foreach (byte D in Data)
            {
                LastMessage += D.ToString() + " ";
            }
            TimeSinceLastResponse = 0;
            if((ZSW & 1) == 0)
            {
                State = 1;
            }
            else
            {
                State = 3;
            }
            if((ZSW & 64) == 64)
            {
                State = 2;
            }
            if((ZSW & 2) == 2)
            {
                State = 4;
            }
            if((ZSW & 4) == 4)
            {
                State = 5;
            }
            if ((ZSW & 32) == 0)
            {
                State = 6;
            }
            if ((ZSW & 8) == 8)
            {
                State = 8;
                ErrorCode = PWE1;
            }
            if (((ZSW & 4) == 4) && ((ZSW & 8) == 8))
            {
                State = 7;
            }
        }

        public override void SendCommand()
        {
            PKE = 0x12BC;
            IND = 0;
            PWE1 = 0;
            STW = 0x047E; // default state.
            switch(State)
            {
                case 5:
                // if the engine is on, continue running. Command remains 0x047F
                // else proceed to state 3. Command remains 0x047E
                case 3:
                case 4:
                    // if the engine is off, FC will go to state 3 and/or remain in it.
                    // else proceed to state 5.
                    if(bEngineOn)
                    {
                        STW = 0x047F;
                    }
                    break;
                case 1:
                // just wait 'till state 2.
                case 2:
                // proceed to state 3.
                // Bits 0, 1, 2 and 10 are set as needed by the command before.
                case 6:
                    ErrorProcessStatus = EErrorProcessStatus.NoError;
                    break;
                case 7:
                    // all we can do here is wait.
                    break;
                case 8:
                    if (ErrorProcessStatus == EErrorProcessStatus.NoError)
                    {
                        ErrorProcessStatus = EErrorProcessStatus.GotFlag;
                    }
                    if (ErrorProcessStatus == EErrorProcessStatus.GotFlag)
                    {
                        if (ErrorCode != 0)
                        {
                            ErrorProcessStatus = EErrorProcessStatus.GotCode;
                        }
                    }
                    if (ErrorProcessStatus == EErrorProcessStatus.GotCode)
                    {}
                    else if (ErrorProcessStatus == EErrorProcessStatus.AcknowledgedByUser)
                    {
                        STW = 0x04FE;
                    }
                    break;
            }
            SendMessage(5);
        }

        public void SetHandler(PortHandler Handler)
        {
            this.Handler = Handler;
        }
    }
}
