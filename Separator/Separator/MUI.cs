using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Separator
{
    public static class MUI
    {
        private const string MUI_LIB_FILENAME = @"langs.dat";
        private const string MUI_LIB_PREWRITEFILENAME = @"langs.da_";
        private const string MUI_LIB_BACKUPFILENAME = @"langs_backup.dat";

        public static Dictionary<string, string> EnglishLib = new Dictionary<string, string>();
        public static Dictionary<string, string> RussianLib = new Dictionary<string, string>();
        public static Dictionary<string, string> UkrainianLib = new Dictionary<string, string>();
        public static Dictionary<string, string> CustomLib = new Dictionary<string, string>();

        private static Dictionary<string, string>[] Libs =
        {
            EnglishLib,
            RussianLib,
            UkrainianLib,
            CustomLib
        };

        public static void LoadLibs()
        {
            try
            {
                using (var sr = new StreamReader(MUI_LIB_FILENAME))
                {
                    while(!sr.EndOfStream)
                    {
                        var NewLine = sr.ReadLine();
                        var Strings = NewLine.Split((char)31);
                        // Fallback option for manual edits. Clever!
                        if (Strings.Length == 1)
                        {
                            Strings = NewLine.Split('|'); 
                        }
                        int ArrayElementNumber = 1;
                        foreach (Dictionary<string, string> Lib in Libs)
                        {
                            if (Lib.ContainsKey(Strings[0]))
                            {
                                Lib[Strings[0]] = Strings[ArrayElementNumber];
                            }
                            else
                            {
                                Lib.Add(Strings[0], Strings[ArrayElementNumber]);
                            }
                            ArrayElementNumber++;
                        }
                    }
                }
            }
            catch
            { }
        }

        public static void SaveLibs()
        {
            try
            {
                using (var sw = new StreamWriter(MUI_LIB_PREWRITEFILENAME, false, Encoding.UTF8))
                {
                    foreach(string Key in EnglishLib.Keys)
                    {
                        sw.WriteLine(Key + (char)31 +
                            EnglishLib[Key] + (char)31 +
                            RussianLib[Key] + (char)31 +
                            UkrainianLib[Key] + (char)31 +
                            CustomLib[Key]);
                    }
                }
                File.Replace(MUI_LIB_PREWRITEFILENAME, MUI_LIB_FILENAME, MUI_LIB_BACKUPFILENAME);
            }
            catch
            { }
        }
    }
}
