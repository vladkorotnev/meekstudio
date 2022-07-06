using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MikuASM
{
    /// <summary>
    /// Allows inputting booleans more freely
    /// </summary>
    class BoolTransform : ArgCompileTransform
    {
        public BoolTransform() : base() { }
        public override object TransformInput(object input)
        {
            if (input is string)
            {
                string inStr = (string)input;
                if(inStr.Equals("1"))
                {
                    return true.ToString();
                } 
                else if (inStr.Equals("0"))
                {
                    return false.ToString();
                }
            }
            return input;
        }
    }

    /// <summary>
    /// Allows inputting the value as percentage or decimal
    /// </summary>
    class PercentTransform : ArgCompileTransform
    {
        public int Min { get; private set; }
        public int Max { get; private set; }

        public bool HookDecimals { get; private set; }

        /// <summary>
        /// Regex for decimal definitions
        /// </summary>
        Regex floatRegex = new Regex(@"^(\d+\.\d+)$", RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromMilliseconds(300));

        /// <summary>
        /// Regex for frame definitions
        /// </summary>
        Regex percentRegex = new Regex(@"^(\d+)%$", RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromMilliseconds(300));

        public PercentTransform(int min, int max) : base()
        {
            Min = min;
            Max = max;
        }

        public PercentTransform(int min, int max, bool decimals) : this(min, max)
        {
            HookDecimals = decimals;
        }

        private int TransformToDecimal(double decVal)
        {
            return Min + Convert.ToInt32(((double)(Max - Min)) * decVal);
        }

        public override object TransformInput(object input)
        {
            if(input is string)
            {
                string inStr = (string)input;
                Match percentMatch = percentRegex.Match(inStr);
                if (percentMatch.Success)
                {
                    return TransformToDecimal(Convert.ToDouble(percentMatch.Groups[1].Value) / 100.0).ToString();
                }
                else if(HookDecimals)
                {
                    Match floatMatch = floatRegex.Match(inStr);
                    if (floatMatch.Success)
                    {
                        return TransformToDecimal(Convert.ToDouble(floatMatch.Groups[1].Value)).ToString();
                    }
                }
            }
            return input;
        }
    }


    /// <summary>
    /// Transforms a time string into a ms integer string.
    /// Valid inputs:
    ///     * 6431 (ms)
    ///     * 6.431 (s.ds)
    ///     * 01:30.431 (m:s.ds)
    ///     * F5002 (approximate frames)
    /// </summary>
    class TimeTransform : ArgCompileTransform
    {
        const int MS_PER_FRAME = 17; // 16.66... rounded up

        /// <summary>
        /// Regex for formats such as 01:30, 4:20.690
        /// </summary>
        static Regex minSecDecimalRegex = new Regex(@"^(\d+):(\d\d)\.?(\d*)$", RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromMilliseconds(300));

        /// <summary>
        /// Regex for decimal second definitions
        /// </summary>
        static Regex floatRegex = new Regex(@"^(\d+\.\d+)$", RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromMilliseconds(300));

        /// <summary>
        /// Regex for frame definitions
        /// </summary>
        static Regex frameRegex = new Regex(@"^F(\d+)$", RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromMilliseconds(300));

        private static int ConvertDecimalPart(string dec)
        {
            return Convert.ToInt32(dec.Substring(0, Math.Min(dec.Length, 3)).PadRight(3, '0'));
        }

        public static string UserStringToMsec(string input)
        {
            Match minSecDecimalMatch = minSecDecimalRegex.Match(input);
            if (minSecDecimalMatch.Success)
            {
                // ?M:SS
                int min = Convert.ToInt32(minSecDecimalMatch.Groups[1].Value);
                int sec = Convert.ToInt32(minSecDecimalMatch.Groups[2].Value);
                int millis = 0;
                if (minSecDecimalMatch.Groups[3].Value.Length > 0)
                {
                    millis = ConvertDecimalPart(minSecDecimalMatch.Groups[3].Value);
                }

                int total = millis + (sec * 1000) + (min * 60000);
                return total.ToString();
            }
            else
            {
                Match decimalSec = floatRegex.Match(input);
                if (decimalSec.Success)
                {
                    return Convert.ToInt32(Convert.ToDouble(decimalSec.Groups[1].Value) * 1000.0).ToString();
                }
                else
                {
                    Match frameMatch = frameRegex.Match(input);
                    if (frameMatch.Success)
                    {
                        return (Convert.ToInt32(frameMatch.Groups[1].Value) * MS_PER_FRAME).ToString();
                    }
                }
            }
            return input;
        }

        public override object TransformInput(object inVal)
        {
            if(inVal is string)
            {
                var input = (string)inVal;
                return UserStringToMsec(input);
            }
            return inVal;
        }
    }
}
