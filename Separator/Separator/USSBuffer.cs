/*
 * Created by SharpDevelop.
 * User: Pavillion G-6
 * Date: 12/20/2016
 * Time: 16:13
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO.Ports;

namespace Separator
{
	/// <summary>
	/// Description of USSBuffer.
	/// </summary>
	public class USSBuffer: ICommunicationBuffer
	{
        public PortHandler Handler { get; set; }
		public byte Address {get; set;}
		public ushort PKE {get; set;}
		public ushort IND {get; set;}
		public ushort PWE1 {get; set;}
		public ushort PWE2 {get; set;}
		public ushort STW {get; set;}
		public ushort SW1 {get; set;}
		public ushort SW2 {get; set;}
		public ushort SW3 {get; set;}
		public ushort ZSW {get; set;}
		public ushort IW1 {get; set;}
		public ushort IW2 {get; set;}
		public ushort IW3 {get; set;}
		
		public USSBuffer()
		{
		}

        public USSBuffer(PortHandler Handler)
        {
            this.Handler = Handler;
        }
		
        public virtual void ReceiveData(byte[] Data)
        {
            try
            {
                byte TrueCRC = 0;
                if (Data[0] != 2) return;
                if (Data[1] != Data.Length - 2) return;
                if (Data[2] != Address) return;
                for (int i = 0; i < Data.Length - 1; i++)
                {
                    TrueCRC ^= Data[i];
                }
                if (TrueCRC != Data[Data.Length - 1])
                {
                    return;
                }
                switch (Data[1])
                {
                    case 6:
                        ZSW = (ushort)(Data[3] << 8 | Data[4]);
                        IW1 = (ushort)(Data[5] << 8 | Data[6]);
                        break;
                    case 10:
                        ZSW = (ushort)(Data[3] << 8 | Data[4]);
                        IW1 = (ushort)(Data[5] << 8 | Data[6]);
                        IW2 = (ushort)(Data[7] << 8 | Data[8]);
                        IW3 = (ushort)(Data[9] << 8 | Data[10]);
                        break;
                    case 12:
                        PKE = (ushort)(Data[3] << 8 | Data[4]);
                        IND = (ushort)(Data[5] << 8 | Data[6]);
                        PWE1 = (ushort)(Data[7] << 8 | Data[8]);
                        ZSW = (ushort)(Data[9] << 8 | Data[10]);
                        IW1 = (ushort)(Data[11] << 8 | Data[12]);
                        break;
                    case 14:
                        PKE = (ushort)(Data[3] << 8 | Data[4]);
                        IND = (ushort)(Data[5] << 8 | Data[6]);
                        PWE1 = (ushort)(Data[7] << 8 | Data[8]);
                        PWE2 = (ushort)(Data[9] << 8 | Data[10]);
                        ZSW = (ushort)(Data[11] << 8 | Data[12]);
                        IW1 = (ushort)(Data[13] << 8 | Data[14]);
                        break;
                    case 18:
                        PKE = (ushort)(Data[3] << 8 | Data[4]);
                        IND = (ushort)(Data[5] << 8 | Data[6]);
                        PWE1 = (ushort)(Data[7] << 8 | Data[8]);
                        PWE2 = (ushort)(Data[9] << 8 | Data[10]);
                        ZSW = (ushort)(Data[11] << 8 | Data[12]);
                        IW1 = (ushort)(Data[13] << 8 | Data[14]);
                        IW2 = (ushort)(Data[15] << 8 | Data[16]);
                        IW3 = (ushort)(Data[17] << 8 | Data[18]);
                        break;
                }
                if(CommunicationLoop.CommDebugMode == ECommDebugMode.USS ||
                    CommunicationLoop.CommDebugMode == ECommDebugMode.Both)
                {
                    CommunicationLoop.SuccessfullyReceivedFramesUSS++;
                }
            }
            catch
            {
                var FinalString = "USS buffer failed to process message:";
                foreach(byte Other in Data)
                {
                    FinalString += " " + Other.ToString();
                }
                Program.Log(FinalString, ELogType.Info);
            }
        }

        public virtual void SendCommand()
        {
            //int MessageType;
            //var Res = new byte[MessageType]
            //Handler.SendCommand(this, Res);
        }

		public void SendMessage(int MessageType)
		{
			byte Checksum = 0;
			int MessageLength = 4 * (MessageType + 1);
            if(MessageType == 5)
            {
                MessageLength = 14;
            }
			var SData = new byte[MessageLength];
			
			SData[0] = 2;
			SData[1] = (byte)(MessageLength - 2);
			SData[2] = Address;
			switch(MessageType)
			{
				case 1:
					SData[3] = (byte)(STW >> 8);
					SData[4] = (byte)(STW & 0xFF);
					SData[5] = (byte)(SW1 >> 8);
					SData[6] = (byte)(SW1 & 0xFF);
					break;
				case 2:
					SData[3] = (byte)(STW >> 8);
					SData[4] = (byte)(STW & 0xFF);
					SData[5] = (byte)(SW1 >> 8);
					SData[6] = (byte)(SW1 & 0xFF);
					SData[7] = (byte)(SW2 >> 8);
					SData[8] = (byte)(SW2 & 0xFF);
					SData[9] = (byte)(SW3 >> 8);
					SData[10] = (byte)(SW3 & 0xFF);
					break;
				case 3:
					SData[3] = (byte)(PKE >> 8);
					SData[4] = (byte)(PKE & 0xFF);
					SData[5] = (byte)(IND >> 8);
					SData[6] = (byte)(IND & 0xFF);
					SData[7] = (byte)(PWE1 >> 8);
					SData[8] = (byte)(PWE1 & 0xFF);
					SData[9] = (byte)(PWE2 >> 8);
					SData[10] = (byte)(PWE2 & 0xFF);
					SData[11] = (byte)(STW >> 8);
					SData[12] = (byte)(STW & 0xFF);
					SData[13] = (byte)(SW1 >> 8);
					SData[14] = (byte)(SW1 & 0xFF);
					break;
				case 4:
					SData[3] = (byte)(PKE >> 8);
					SData[4] = (byte)(PKE & 0xFF);
					SData[5] = (byte)(IND >> 8);
					SData[6] = (byte)(IND & 0xFF);
					SData[7] = (byte)(PWE1 >> 8);
					SData[8] = (byte)(PWE1 & 0xFF);
					SData[9] = (byte)(PWE2 >> 8);
					SData[10] = (byte)(PWE2 & 0xFF);
					SData[11] = (byte)(STW >> 8);
					SData[12] = (byte)(STW & 0xFF);
					SData[13] = (byte)(SW1 >> 8);
					SData[14] = (byte)(SW1 & 0xFF);
					SData[15] = (byte)(SW2 >> 8);
					SData[16] = (byte)(SW2 & 0xFF);
					SData[17] = (byte)(SW3 >> 8);
					SData[18] = (byte)(SW3 & 0xFF);
					break;
                case 5:
                    SData[3] = (byte)(PKE >> 8);
                    SData[4] = (byte)(PKE & 0xFF);
                    SData[5] = (byte)(IND >> 8);
                    SData[6] = (byte)(IND & 0xFF);
                    SData[7] = (byte)(PWE1 >> 8);
                    SData[8] = (byte)(PWE1 & 0xFF);
                    SData[9] = (byte)(STW >> 8);
                    SData[10] = (byte)(STW & 0xFF);
                    SData[11] = (byte)(SW1 >> 8);
                    SData[12] = (byte)(SW1 & 0xFF);
                    break;
            }
			for(int i = 0; i < MessageLength - 1; i++)
			{
				Checksum ^= SData[i];
			}
			SData[MessageLength - 1] = Checksum;
            Handler.SendCommand(this, SData);
            if (CommunicationLoop.CommDebugMode == ECommDebugMode.USS
                || CommunicationLoop.CommDebugMode == ECommDebugMode.Both)
            {
                CommunicationLoop.SentFramesUSS++;
            }
		}

		void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			
		}
	}
}