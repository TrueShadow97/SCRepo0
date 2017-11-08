/*
 * Created by SharpDevelop.
 * User: Pavillion G-6
 * Date: 20.11.2016
 * Time: 11:38
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Runtime.Remoting.Contexts;

namespace Separator
{
	public enum EDataMode
	{
		Discrete,
		Analog
	}
	
	/// <summary>
	/// Точка данных, позволяющая читать и писать данные в буффер.
	/// </summary>
	 
	public class DataPoint
	{
		protected ushort Data;
		public ushort Address;
		public ModbusBuffer DataSource;
		public EDataMode DataMode;
		public byte BitNum;
		
		public DataPoint(ModbusBuffer Source, EDataMode DataMode, ushort Address, byte BitNum)
		{
			DataSource = Source;
			this.DataMode = DataMode;
			this.Address = Address;
			this.BitNum = (byte)(BitNum % 16);
		}
		
		public DataPoint(ModbusBuffer Source, EDataMode DataMode, ushort Address)
			:this(Source, DataMode, Address, 0)
		{}
		
		public ushort GetData()
		{
			if(DataMode != EDataMode.Analog)
			{
				throw new DataModeException();
			}
			if(DataSource != null)
				return DataSource.GetData(Address);
			return 0;
		}
		
		public void SetData(ushort Value)
		{
			if(DataMode != EDataMode.Analog)
			{
				throw new DataModeException();
			}
			if(DataSource != null)
				DataSource.SetData(Value, Address);
		}
		
		public bool GetBit()
		{
			if(DataMode != EDataMode.Discrete)
			{
				throw new DataModeException();
			}
			if(BitNum < 0 || BitNum > 15)
			{
				throw new ArgumentOutOfRangeException();
			}
			if(DataSource != null)
				return (((ushort)Math.Pow(2, BitNum) & DataSource.GetData(Address)) != 0);
			return false;
		}
		
		public void SetBit(bool Value)
		{
			ushort NewData;
			if(DataMode != EDataMode.Discrete)
			{
				throw new DataModeException();
			}
			if(BitNum < 0 || BitNum > 15)
			{
				throw new ArgumentOutOfRangeException();
			}
			NewData = DataSource.GetData(Address);
			if(Value)
			{
				NewData |= (ushort)Math.Pow(2, BitNum);
			}
			else
			{
				long tmp = (long)Math.Pow(2, BitNum);
				tmp = ~tmp;
				NewData = (ushort)(NewData & tmp);
			}
			if(DataSource != null)
				DataSource.SetData(NewData, Address);
		}
	}
}
