using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace Separator
{
    public class PortHandler
    {
        public SerialPort Port { get; set; }
        protected ICommunicationBuffer CurrentBuffer { get; set; }
        public int CommunicationTimeout { get; set; }
        protected Timer TimeoutTimer { get; set; }
        public bool bUseOwnPause { get; set; } = false;
        public bool bPauseCommunication { get; set; } = false;

        public PortHandler(SerialPort Port, int Timeout)
        {
            this.Port = Port;
            if(!Port.IsOpen)
            {
                Port.Open();
            }
            Port.Encoding = Encoding.UTF8;
            //Port.DataReceived += Port_DataReceived;
            CommunicationTimeout = Timeout;
        }

        protected void ChangeCommunicationPauseState(bool bNewValue)
        {
            lock (CommunicationLoop.CommPauseLock)
            {
                if (bUseOwnPause)
                {
                    bPauseCommunication = bNewValue;
                }
                else
                {
                    CommunicationLoop.bPauseCommunication = bNewValue;
                }
            }
        }

        public void Port_DataReceived()
        {
            var Data = new byte[Port.BytesToRead];
            try
            {
                Port.Read(Data, 0, Port.BytesToRead);
                if (TimeoutTimer != null)
                {
                    TimeoutTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    ChangeCommunicationPauseState(false);
                }
                if (CurrentBuffer != null)
                {
                    CurrentBuffer.ReceiveData(Data);
                    CurrentBuffer = null;
                }
            }
            catch
            {
                Program.Log("Unable to read port data", ELogType.Error);
                // clear input data
                Port.ReadExisting();
            }
        }

        private void CommenceTimeout(object state)
        {
            ChangeCommunicationPauseState(false);
        }

        public void SendCommand(ICommunicationBuffer Sender, byte[] Data)
        {
            if(!Port.IsOpen)
            {
                return;
            }
            CurrentBuffer = Sender;
            Task.Factory.StartNew(() =>
            {
                Port.Write(Data, 0, Data.Length);
            });
            ChangeCommunicationPauseState(true);
            TimeoutTimer = new Timer(CommenceTimeout, null, CommunicationTimeout,
                Timeout.Infinite);
        }
    }
}
