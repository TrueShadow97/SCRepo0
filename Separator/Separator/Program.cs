using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Globalization;

namespace Separator
{
    public static partial class Program
    {
        public static bool bComEnabled;
        public static bool bUnlimitedManual = false;

        public static decimal Min(this object AnyObject, decimal A, decimal B)
        {
            if (A < B) return A;
            return B;
        }

        public static decimal Max(this object AnyObject, decimal A, decimal B)
        {
            if (A > B) return A;
            return B;
        }

        public static void AddRange(this ItemCollection Other, IEnumerable<object> Range)
        {
            foreach (object Item in Range)
            {
                Other.Add(Item);
            }
        }

        /// <summary>
        /// Целая часть десятичного числа.
        /// </summary>
        public static long IntPart(this decimal Other)
        {
            return (long)Other;
        }

        /// <summary>
        /// Дробная часть десятичного числа.
        /// </summary>
        public static decimal DecimalPart(this decimal Other)
        {
            return Other - Other.IntPart();
        }

        public static void Init()
        {
            bComEnabled = false;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            MUI.LoadLibs();
            if(CommunicationLoop.Init())
            {
                CommunicationLoop.Start();
                bComEnabled = true;
            }
            Technology.InitRoster();
            Settings.LoadAll();
            Settings.Init();
            Technology.StartLoop();
        }

        public static void Finish()
        {
            Technology.EndLoop();
            if(bComEnabled)
            {
                CommunicationLoop.Stop();
            }
        }

        public static decimal BilinearConversion(decimal Value, decimal L1, 
            decimal U1, decimal L2, decimal U2, bool bStrict = false)
        {
            decimal Mul, Add;
            if (bStrict)
            {
                if (Value < L1) Value = L1;
                if (Value > U1) Value = U1;
            }
            if (U1 == L1) return 0;
            Mul = (U2 - L2) / (U1 - L1);
            Add = L2 - L1 * Mul;
            return Value * Mul + Add;
        }
    }
}
