/*
 * Created by SharpDevelop.
 * User: Pavillion G-6
 * Date: 16.01.2017
 * Time: 14:38
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Remoting.Contexts;

namespace Separator
{
	public enum ELogType
	{
		Info,
		Operator,
		Automation,
		Error
	}
	
	public partial class Program
	{
        public static bool bLogToProgram = true;
		public const string LogDir = "Archive\\";
		
		private static void LogJob(string Message)
		{
        	string FileName;
			if(!Directory.Exists(LogDir))
			{
				Directory.CreateDirectory(LogDir);
			}
			FileName = "Prodecologia_" + 
				DateTime.Now.Day + "_" + 
				DateTime.Now.Month + "_" + 
				DateTime.Now.Year + ".log";
            while (true)
            {
                try
                {
                    using (var sw = new StreamWriter(LogDir + FileName, true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(Message);
                    }
                    break;
                }
                catch
                {
                    Thread.Sleep(2);
                    continue;
                }
            }
            System.Collections.Generic.List<FileInfo> FilesToDelete = 
                new System.Collections.Generic.List<FileInfo>();
            foreach(FileInfo Other in new DirectoryInfo(LogDir).EnumerateFiles())
            {
                if(DateTime.Now - Other.CreationTime > new TimeSpan(710, 0, 0, 0))
                {
                    FilesToDelete.Add(Other);
                }
            }
            for(int i = 0; i < FilesToDelete.Count; i++)
            {
                FilesToDelete[i].Delete();
            }
		}
		
		/// <summary>
		/// Записывает текст из аргумента в log-файл в асинхронном режиме.
		/// </summary>
		public static Task Log(string Message, ELogType LogType)
		{
			string LTString;
			switch(LogType)
			{
				case ELogType.Info:
					LTString = "[INFO]";
					break;
				case ELogType.Operator:
					LTString = "[OPERATOR]";
					break;
				case ELogType.Automation:
					LTString = "[AUTOMATION]";
					break;
				case ELogType.Error:
					LTString = "[ERROR]";
					break;
				default:
					LTString = "[UNDEFINED]";
					break;
			}
            var FinalString = LTString + " " + DateTime.Now.ToLongTimeString() + ": " + Message;
            if(bLogToProgram)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        MainWindow.Instance.Log(FinalString);
                    }
                    catch
                    {
                        MainWindow.Instance.LogQueue.Add(FinalString);
                    }
                });
            }
            return Task.Factory.StartNew(() => LogJob(FinalString));
		}
		
		public static Task LogException(Exception Ex)
		{
			return Task.Factory.StartNew(() => LogJob(Ex.StackTrace));
		}
	}
}
