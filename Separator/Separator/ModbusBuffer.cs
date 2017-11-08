/*
 * Created by SharpDevelop.
 * User: Pavillion G-6
 * Date: 20.11.2016
 * Time: 11:35
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO.Ports;
using System.Runtime.InteropServices.ComTypes;

namespace Separator
{
	/// <summary>
	/// Буффер, выступающий посредником между программой и связанными с контроллером устройствами.
	/// </summary>
	
	public enum EDataDirection
	{
		Input,
		Output
	}
	
	public class ModbusBuffer: ICommunicationBuffer
	{
		public ushort[] Data {get; set;}
		public ushort ControllerNum {get; protected set;}
		public ushort AddressLBound {get; protected set;}
		public ushort AddressUBound {get; protected set;}
		public EDataDirection DataDirection {get; protected set;}
		public PortHandler Handler { get; set; }
		public delegate void TimeoutDelegate();
        public string LastSentCommand { get; protected set; }
        public bool bForceWorkIndicationBit { get; set; } = false;
		
		public ModbusBuffer(PortHandler Handler, 
		                  EDataDirection DataDirection, 
		                  ushort ControllerNum, 
		                  ushort AddressLBound, 
		                  ushort AddressUBound)
		{
			this.Handler = Handler;
//			Port.Encoding = System.Text.Encoding.GetEncoding();
			this.ControllerNum = ControllerNum;
			this.DataDirection = DataDirection;
			this.AddressLBound = AddressLBound;
			this.AddressUBound = AddressUBound;
			Data = new ushort[AddressUBound - AddressLBound + 1];
		}
		
		public ushort GetData(ushort Address)
		{
			if(Address < AddressLBound || Address > AddressUBound)
			{
				throw new ArgumentOutOfRangeException();
			}
			return Data[Address - AddressLBound];
		}
		
		public void SetData(ushort Value, ushort Address)
		{
			if(Address < AddressLBound || Address > AddressUBound)
			{
				throw new ArgumentOutOfRangeException();
			}
			Data[Address - AddressLBound] = Value;
		}
		
		static public void CRC16(ref byte[] Message)
		{
			ushort PreRes = 65535;
			bool bOdd;
			for(int i = 0; i < Message.Length - 2; i++)
			{
				PreRes ^= Message[i];
				for(int j = 0; j < 8; j++)
				{
					bOdd = (PreRes & 0x0001) != 0;
					PreRes >>= 1;
					if(bOdd)
					{
						PreRes ^= 0xA001;
					}
				}
			}
			Message[Message.Length - 1] = (byte)(PreRes >> 8);
			Message[Message.Length - 2] = (byte)(PreRes & 255);
		}
		
		public void SendCommand()
		{
			byte[] Res;
			if(DataDirection == EDataDirection.Output)
			{
                var DataCopy = new ushort[Data.Length];
                Data.CopyTo(DataCopy, 0);
                if(bForceWorkIndicationBit)
                {
                    DataCopy[1] |= 32;
                }
				Res = new byte[(AddressUBound - AddressLBound + 1) * 2 + 9];
				Res[0] = (byte)ControllerNum;
				Res[1] = 16;
				Res[2] = (byte)(AddressLBound >> 8);
				Res[3] = (byte)(AddressLBound & 255);
				Res[4] = (byte)((AddressUBound - AddressLBound + 1) >> 8);
				Res[5] = (byte)((AddressUBound - AddressLBound + 1) & 255);
				Res[6] = (byte)(Res.Length - 9);
				for(int i = 0; i < (AddressUBound - AddressLBound + 1); i++)
				{
					Res[i * 2 + 7] = (byte)(DataCopy[i] >> 8);
					Res[i * 2 + 8] = (byte)(DataCopy[i] & 255);
				}
				CRC16(ref Res);
			}
			else
			{
				Res = new byte[8];
				Res[0] = (byte)ControllerNum;
				Res[1] = 3;
				Res[2] = (byte)(AddressLBound >> 8);
				Res[3] = (byte)(AddressLBound & 255);
				Res[4] = (byte)((AddressUBound - AddressLBound + 1) >> 8);
				Res[5] = (byte)((AddressUBound - AddressLBound + 1) & 255);
				CRC16(ref Res);
			}
            LastSentCommand = "";
            foreach (byte R in Res)
            {
                LastSentCommand += R.ToString() + " ";
            }
            Handler.SendCommand(this, Res);
            if(CommunicationLoop.CommDebugMode == ECommDebugMode.Modbus
                || CommunicationLoop.CommDebugMode == ECommDebugMode.Both)
            {
                CommunicationLoop.SentFramesModbus++;
            }
		}

        public void ReceiveData(byte[] Data)
        {
            ushort ReceivedCRC, TrueCRC;
            if (Data[0] != (byte)ControllerNum) return;
            if ((DataDirection == EDataDirection.Input && Data[1] != 3) ||
               (DataDirection == EDataDirection.Output && Data[1] != 16))
                return;
            ReceivedCRC = (ushort)(Data[Data.Length - 1] << 8 | Data[Data.Length - 2]);
            CRC16(ref Data);
            TrueCRC = (ushort)(Data[Data.Length - 1] << 8 | Data[Data.Length - 2]);
            if (ReceivedCRC != TrueCRC) return;
            if (DataDirection == EDataDirection.Input)
            {
                ushort Res;
                if ((Data.Length - 5) / 2 > Data.Length) return;
                for (int i = 0; i < (Data.Length - 5) / 2; i++)
                {
                    Res = (ushort)(Data[i * 2 + 3] << 8);
                    Res |= (ushort)(Data[i * 2 + 4] & 255);
                    this.Data[i] = Res;
                }
            }
            if(CommunicationLoop.CommDebugMode == ECommDebugMode.Modbus ||
                CommunicationLoop.CommDebugMode == ECommDebugMode.Both)
            {
                CommunicationLoop.SuccessfullyReceivedFramesModbus++;
            }
        }
	}
}
