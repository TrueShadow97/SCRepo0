using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;

namespace Separator
{
    public enum ECommDebugMode
    {
        None,
        Modbus,
        USS,
        Both
    }
    public static class CommunicationLoop
    {
        public static bool bPauseCommunication { get; set; }
        public static bool bPauseUSS { get; set; }
        public static object CommPauseLock { get; private set; } = new object();
        public static bool bLaunched { get; set; }
        public static object LaunchLock { get; private set; } = new object();
        public static List<ICommunicationBuffer> Buffers = new List<ICommunicationBuffer>();
        public static List<ModbusBuffer> ModbusBuffers = new List<ModbusBuffer>();
        public static List<FCBuffer> USSBuffers = new List<FCBuffer>();
        private static PortHandler Handler { get; set; }
        private static PortHandler Handler2 { get; set; }
        private static Timer ShutdownTimer { get; set; }
        private static bool bParallelCommunication { get; set; } = false;
        public static bool bDisableFCs { get; set; } = false;
        public static long SentFramesModbus { get; set; } = 0;
        public static long SentFramesUSS { get; set; } = 0;
        public static long SuccessfullyReceivedFramesModbus { get; set; } = 0;
        public static long SuccessfullyReceivedFramesUSS { get; set; } = 0;
        public static decimal ModbusCommHealth
        {
            get
            {
                try
                {
                    return (decimal)SuccessfullyReceivedFramesModbus / SentFramesModbus;
                }
                catch (DivideByZeroException)
                {
                    return 0;
                }
            }
        }
        public static decimal USSCommHealth
        {
            get
            {
                try
                {
                    return (decimal)SuccessfullyReceivedFramesUSS / SentFramesUSS;
                }
                catch (DivideByZeroException)
                {
                    return 0;
                }
            }
        }
        public static bool bModbusFellSick { get; set; }
        public static bool bUSSFellSick { get; set; }
        public static bool bReopenPortsOnFailure { get; set; } = false;
        public static ECommDebugMode CommDebugMode { get; set; } = ECommDebugMode.Both;

        public static bool Init()
        {
            bool Res;
            try
            {
                /*var PortNames = SerialPort.GetPortNames();
                string FirstPort = null, SecondPort = null;
                foreach(string PortName in PortNames)
                {
                    if(PortName != "COM1" && PortName != "COM2")
                    {
                        var TempPort = new SerialPort(PortName, 38400, Parity.None, 8, StopBits.One);
                        TempPort.Write(
                            new byte[] { 1, 3, 10, 100, 0, 1, 15, 0xC1}, 0, 8);
                        Thread.Sleep(100);
                        if(TempPort.BytesToRead != 0)
                        {
                            TempPort.ReadExisting();
                            TempPort.Close();
                            FirstPort = PortName;
                        }
                        else
                        {
                            SecondPort = PortName;
                        }
                    }
                }
                if(FirstPort != null && SecondPort != null)
                {
                    throw (new Exception());
                }*/
                Handler = new PortHandler(new SerialPort("COM1", 38400, Parity.None, 8, StopBits.One),
                    30);
                if (!bDisableFCs)
                {
                    Handler2 = new PortHandler(new SerialPort("COM2", 38400, Parity.Even, 8, StopBits.One),
                        30);
                    Handler2.bUseOwnPause = bParallelCommunication;
                }
                Handler.bUseOwnPause = bParallelCommunication;
                Res = true;
                ShutdownTimer = new Timer(CommenceShutdown, null, 10000, Timeout.Infinite);
            }
            catch
            {
                Res = false;
            }
            ModbusBuffers.Add(new ModbusBuffer(Handler, EDataDirection.Input, 1, 0x1000 + 100,
                0x1000 + 102));
            ModbusBuffers.Add(new ModbusBuffer(Handler, EDataDirection.Output, 1, 0x1000 + 200,
                0x1000 + 202));
            //ModbusBuffers[1].bForceWorkIndicationBit = true;
            ModbusBuffers.Add(new ModbusBuffer(Handler, EDataDirection.Input, 1, 0x1000 + 300,
                0x1000 + 303));
            ModbusBuffers.Add(new ModbusBuffer(Handler, EDataDirection.Output, 1, 0x1000 + 400,
                0x1000 + 401));
            ModbusBuffers.Add(new ModbusBuffer(Handler, EDataDirection.Input, 1, 0x1000 + 506,
                0x1000 + 506));
            int[] FCBufferAddresses =
            {
                1, 2, 3, 4, 5, 6, 7
            };
            Buffers.AddRange(ModbusBuffers);
            if (!bDisableFCs)
            {
                for (int i = 0; i < 7; i++)
                {
                    var NewUSSB = new FCBuffer(Handler2);
                    USSBuffers.Add(NewUSSB);
                    NewUSSB.Address = (byte)FCBufferAddresses[i];
                }
                Buffers.AddRange(USSBuffers);
            }
            return Res;
        }

        private static void CommenceShutdown(object Data)
        {
            var psi = new ProcessStartInfo("shutdown", "/s /t 0");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

        public static void Start()
        {
            bool bLocalLaunchParameter;
            lock(LaunchLock)
            {
                bLocalLaunchParameter = bLaunched;
            }
            if(!bLocalLaunchParameter)
            {
                lock (LaunchLock)
                {
                    bLaunched = true;
                }
                if (bParallelCommunication)
                {
                    var mList = new List<ICommunicationBuffer>();
                    var uList = new List<ICommunicationBuffer>();
                    mList.AddRange(ModbusBuffers);
                    uList.AddRange(USSBuffers);
                    new Thread(delegate ()
                    {
                        CommLoop(mList);
                    }).Start();
                    if (!bDisableFCs)
                    {
                        new Thread(delegate ()
                        {
                            CommLoop(uList);
                        }).Start();
                    }
                }
                else
                {
                    new Thread(delegate ()
                    {
                        CommLoop(Buffers);
                    }).Start();
                }
            }
        }

        public static void Stop()
        {
            bool bLocalLaunchParameter;
            lock (LaunchLock)
            {
                bLocalLaunchParameter = bLaunched;
            }
            if (bLocalLaunchParameter)
            {
                lock (LaunchLock)
                {
                    bLaunched = false;
                }
            }
        }

        private static void CommLoop(List<ICommunicationBuffer> UsedBuffers)
        {
			
        }
    }
}
