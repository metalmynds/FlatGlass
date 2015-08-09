using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MetalMynds.Utilities
{
    public class ConversionHelper
    {
        public enum Numbers
        {
            One = 1,
            Two = 2,
            Three = 3,
            Four = 4,
            Five = 5,
            Six = 6,
            Seven = 7,
            Eight = 8,
            Nine = 9,
            Ten = 10,
            Eleven = 11,
            Twelve = 12,
            Thirteen = 13,
            Fourteen = 14,
            Fifteen = 15,
            Sixteen = 16,
            Seventeen = 17,
            Eighteen = 18,
            Nineteen = 19,
            Twenty = 20,
            TwentyOne = 21,
            TwentyTwo = 22,
            TwentyThree = 23,
            TwentyFour = 24,
            TwentyFive = 25,
            TwentySix = 26,
            TwentySeven = 27
        }

        public static Numbers NumberToWords(int Number)
        {
            return (Numbers)Number;
        }

        public static String NumberToWords(Numbers Number)
        {
            return Number.ToString();
        }

        public static SByte[] ToSByteArray(Byte[] Source)
        {
            return (SByte[])Array.ConvertAll(Source, (a) => (sbyte)a);
        }

        public static Boolean TryBase64Encode(byte[] Data, out String Output, out String Error)
        {
            try
            {
                Output = Convert.ToBase64String(Data);
                Error = String.Empty;
                return true;
            }
            catch (Exception e)
            {
                Output = null;
                Error = String.Format("Base64 Encode Data Failed!\nError: {0}\nStack Trace:\n{1}",e.Message,e.StackTrace);
                return false;
            }
        }

        public static String ToBase26(Int32 Number)
        {
            Number = Math.Abs(Number);
            String converted = "";
            // Repeatedly divide the number by 26 and convert the
            // remainder into the appropriate letter.
            do
            {
                int remainder = Number % 26;
                converted = (char)(remainder + 'A') + converted;
                Number = Number / 26;
            } while (Number > 0);

            return converted;
        }

        public static String ToBase36(long Number)
        {
            return Base36.NumberToBase36(Number);
        }

        public static String ToBase36(String Text)
        {
            return new Base36(Text).Value;
        }

        public static Boolean TryGetEnum(Type EnumType, String Value, Boolean IgnoreCase, out Object EnumObject, out String Error)
        {

            try
            {

                EnumObject = Enum.Parse(EnumType, Value, IgnoreCase);
                Error = string.Empty;
                return true;

            }
            catch (Exception ex)
            {
                Error = String.Format("Get Enum Value [{0}] From Enum Type [{1}] Failed!\nError: {2}\nStack Trace:\n{3}", Value, EnumType.FullName, ex.Message, ex.StackTrace);
                EnumObject = null;
                return false;
                
            }
            
        }

        public static string ConvertToUnixLines(string Value)
        {
            return Value.Replace("\r\n", "\n");
        }

        public static string ConvertToWindowsLines(string Value)
        {
            Value = ConvertToUnixLines(Value);
            Value = Value.Replace("\n", "\r\n");
            return Value;
        }
    }

    /// <summary>
    /// Class representing a Base36 number
    /// </summary>
    public struct Base36
    {
        #region Constants (and pseudo-constants)

        /// <summary>
        /// Base36 containing the maximum supported value for this type
        /// </summary>
        public static readonly Base36 MaxValue = new Base36(long.MaxValue);
        /// <summary>
        /// Base36 containing the minimum supported value for this type
        /// </summary>
        public static readonly Base36 MinValue = new Base36(long.MinValue + 1);

        #endregion

        #region Fields

        private long numericValue;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiate a Base36 number from a long value
        /// </summary>
        /// <param name="NumericValue">The long value to give to the Base36 number</param>
        public Base36(long NumericValue)
        {
            numericValue = 0; //required by the struct.
            this.NumericValue = NumericValue;
        }


        /// <summary>
        /// Instantiate a Base36 number from a Base36 string
        /// </summary>
        /// <param name="Value">The value to give to the Base36 number</param>
        public Base36(string Value)
        {
            numericValue = 0; //required by the struct.
            this.Value = Value;
        }


        #endregion

        #region Properties

        /// <summary>
        /// Get or set the value of the type using a base-10 long integer
        /// </summary>
        public long NumericValue
        {
            get
            {
                return numericValue;
            }
            set
            {
                //Make sure value is between allowed ranges
                if (value <= long.MinValue || value > long.MaxValue)
                {
                    throw new InvalidBase36ValueException(value);
                }

                numericValue = value;
            }
        }


        /// <summary>
        /// Get or set the value of the type using a Base36 string
        /// </summary>
        public string Value
        {
            get
            {
                return Base36.NumberToBase36(numericValue);
            }
            set
            {
                try
                {
                    numericValue = Base36.Base36ToNumber(value);
                }
                catch
                {
                    //Catch potential errors
                    throw new InvalidBase36NumberException(value);
                }
            }
        }


        #endregion

        #region Public Static Methods

        /// <summary>
        /// Static method to convert a Base36 string to a long integer (base-10)
        /// </summary>
        /// <param name="Base36Value">The number to convert from</param>
        /// <returns>The long integer</returns>
        public static long Base36ToNumber(string Base36Value)
        {
            //Make sure we have passed something
            if (Base36Value == "")
            {
                throw new InvalidBase36NumberException(Base36Value);
            }

            //Make sure the number is in upper case:
            Base36Value = Base36Value.ToUpper();

            //Account for negative values:
            bool isNegative = false;

            if (Base36Value[0] == '-')
            {
                Base36Value = Base36Value.Substring(1);
                isNegative = true;
            }

            //Loop through our string and calculate its value
            try
            {
                //Keep a running total of the value
                long returnValue = Base36DigitToNumber(Base36Value[Base36Value.Length - 1]);

                //Loop through the character in the string (right to left) and add
                //up increasing powers as we go.
                for (int i = 1; i < Base36Value.Length; i++)
                {
                    returnValue += ((long)Math.Pow(36, i) * Base36DigitToNumber(Base36Value[Base36Value.Length - (i + 1)]));
                }

                //Do negative correction if required:
                if (isNegative)
                {
                    return returnValue * -1;
                }
                else
                {
                    return returnValue;
                }
            }
            catch
            {
                //If something goes wrong, this is not a valid number
                throw new InvalidBase36NumberException(Base36Value);
            }
        }


        /// <summary>
        /// Public static method to convert a long integer (base-10) to a Base36 number
        /// </summary>
        /// <param name="NumericValue">The base-10 long integer</param>
        /// <returns>A Base36 representation</returns>
        public static string NumberToBase36(long NumericValue)
        {
            try
            {
                //Handle negative values:
                if (NumericValue < 0)
                {
                    return string.Concat("-", PositiveNumberToBase36(Math.Abs(NumericValue)));
                }
                else
                {
                    return PositiveNumberToBase36(NumericValue);
                }
            }
            catch
            {
                throw new InvalidBase36ValueException(NumericValue);
            }
        }


        #endregion

        #region Private Static Methods

        private static string PositiveNumberToBase36(long NumericValue)
        {
            //This is a clever recursively called function that builds
            //the base-36 string representation of the long base-10 value
            if (NumericValue < 36)
            {
                //The get out clause; fires when we reach a number less than 
                //36 - this means we can add the last digit.
                return NumberToBase36Digit((byte)NumericValue).ToString();
            }
            else
            {
                //Add digits from left to right in powers of 36 
                //(recursive)
                return string.Concat(PositiveNumberToBase36(NumericValue / 36), NumberToBase36Digit((byte)(NumericValue % 36)).ToString());
            }
        }


        private static byte Base36DigitToNumber(char Base36Digit)
        {
            //Converts one base-36 digit to it's base-10 value
            if (!char.IsLetterOrDigit(Base36Digit))
            {
                throw new InvalidBase36DigitException(Base36Digit);
            }

            if (char.IsDigit(Base36Digit))
            {
                //Handles 0 - 9
                return byte.Parse(Base36Digit.ToString());
            }
            else
            {
                //Handles A - Z
                return (byte)((int)Base36Digit - 55);
            }
        }


        private static char NumberToBase36Digit(byte NumericValue)
        {
            //Converts a number to it's base-36 value.
            //Only works for numbers <= 35.
            if (NumericValue > 35)
            {
                throw new InvalidBase36DigitValueException(NumericValue);
            }

            //Numbers:
            if (NumericValue <= 9)
            {
                return NumericValue.ToString()[0];
            }
            else
            {
                //Note that A is code 65, and in this
                //scheme, A = 10.
                return (char)(NumericValue + 55);
            }
        }


        #endregion

        #region Operator Overloads

        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static bool operator >(Base36 LHS, Base36 RHS)
        {
            return LHS.numericValue > RHS.numericValue;
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static bool operator <(Base36 LHS, Base36 RHS)
        {
            return LHS.numericValue < RHS.numericValue;
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static bool operator >=(Base36 LHS, Base36 RHS)
        {
            return LHS.numericValue >= RHS.numericValue;
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static bool operator <=(Base36 LHS, Base36 RHS)
        {
            return LHS.numericValue <= RHS.numericValue;
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static bool operator ==(Base36 LHS, Base36 RHS)
        {
            return LHS.numericValue == RHS.numericValue;
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static bool operator !=(Base36 LHS, Base36 RHS)
        {
            return !(LHS == RHS);
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static Base36 operator +(Base36 LHS, Base36 RHS)
        {
            return new Base36(LHS.numericValue + RHS.numericValue);
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static Base36 operator -(Base36 LHS, Base36 RHS)
        {
            return new Base36(LHS.numericValue - RHS.numericValue);
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static Base36 operator ++(Base36 Value)
        {
            return new Base36(Value.numericValue++);
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static Base36 operator --(Base36 Value)
        {
            return new Base36(Value.numericValue--);
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static Base36 operator *(Base36 LHS, Base36 RHS)
        {
            return new Base36(LHS.numericValue * RHS.numericValue);
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static Base36 operator /(Base36 LHS, Base36 RHS)
        {
            return new Base36(LHS.numericValue / RHS.numericValue);
        }


        /// <summary>
        /// Operator overload
        /// </summary>
        /// <param name="LHS"></param>
        /// <param name="RHS"></param>
        /// <returns></returns>
        public static Base36 operator %(Base36 LHS, Base36 RHS)
        {
            return new Base36(LHS.numericValue % RHS.numericValue);
        }


        /// <summary>
        /// Converts type Base36 to a base-10 long
        /// </summary>
        /// <param name="Value">The Base36 object</param>
        /// <returns>The base-10 long integer</returns>
        public static implicit operator long(Base36 Value)
        {
            return Value.numericValue;
        }


        /// <summary>
        /// Converts type Base36 to a base-10 integer
        /// </summary>
        /// <param name="Value">The Base36 object</param>
        /// <returns>The base-10 integer</returns>
        public static implicit operator int(Base36 Value)
        {
            try
            {
                return (int)Value.numericValue;
            }
            catch
            {
                throw new OverflowException("Overflow: Value too large to return as an integer");
            }
        }


        /// <summary>
        /// Converts type Base36 to a base-10 short
        /// </summary>
        /// <param name="Value">The Base36 object</param>
        /// <returns>The base-10 short</returns>
        public static implicit operator short(Base36 Value)
        {
            try
            {
                return (short)Value.numericValue;
            }
            catch
            {
                throw new OverflowException("Overflow: Value too large to return as a short");
            }
        }


        /// <summary>
        /// Converts a long (base-10) to a Base36 type
        /// </summary>
        /// <param name="Value">The long to convert</param>
        /// <returns>The Base36 object</returns>
        public static implicit operator Base36(long Value)
        {
            return new Base36(Value);
        }


        /// <summary>
        /// Converts type Base36 to a string; must be explicit, since
        /// Base36 > string is dangerous!
        /// </summary>
        /// <param name="Value">The Base36 type</param>
        /// <returns>The string representation</returns>
        public static explicit operator string(Base36 Value)
        {
            return Value.Value;
        }


        /// <summary>
        /// Converts a string to a Base36
        /// </summary>
        /// <param name="Value">The string (must be a Base36 string)</param>
        /// <returns>A Base36 type</returns>
        public static implicit operator Base36(string Value)
        {
            return new Base36(Value);
        }


        #endregion

        #region Public Override Methods

        /// <summary>
        /// Returns a string representation of the Base36 number
        /// </summary>
        /// <returns>A string representation</returns>
        public override string ToString()
        {
            return Base36.NumberToBase36(numericValue);
        }


        /// <summary>
        /// A unique value representing the value of the number
        /// </summary>
        /// <returns>The unique number</returns>
        public override int GetHashCode()
        {
            return numericValue.GetHashCode();
        }


        /// <summary>
        /// Determines if an object has the same value as the instance
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if the values are the same</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Base36))
            {
                return false;
            }
            else
            {
                return this == (Base36)obj;
            }
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a string representation padding the leading edge with
        /// zeros if necessary to make up the number of characters
        /// </summary>
        /// <param name="MinimumDigits">The minimum number of digits that the string must contain</param>
        /// <returns>The padded string representation</returns>
        public string ToString(int MinimumDigits)
        {
            string base36Value = Base36.NumberToBase36(numericValue);

            if (base36Value.Length >= MinimumDigits)
            {
                return base36Value;
            }
            else
            {
                string padding = new string('0', (MinimumDigits - base36Value.Length));
                return string.Format("{0}{1}", padding, base36Value);
            }
        }


        #endregion

    }

    public class InvalidBase36ValueException : Exception
    {

        public InvalidBase36ValueException(char Invalid)
            : base(String.Format("Invalid Base 36 Value [{0}]", Invalid))
        {
        }

        public InvalidBase36ValueException(long Invalid)
            : base(String.Format("Invalid Base 36 Value [{0}]", Invalid))
        {
        }

    }

    public class InvalidBase36NumberException : Exception
    {
        public InvalidBase36NumberException(string Invalid)
            : base(String.Format("Invalid Base 36 Number [{0}]", Invalid))
        {
        }

        public InvalidBase36NumberException(int Invalid)
            : base(String.Format("Invalid Base 36 Number [{0}]", Invalid))
        {
        }

    }

    public class InvalidBase36DigitException : Exception
    {
        public InvalidBase36DigitException(string Invalid)
            : base(String.Format("Invalid Base 36 Digit [{0}]", Invalid))
        {
        }

        public InvalidBase36DigitException(int Invalid)
            : base(String.Format("Invalid Base 36 Digit [{0}]", Invalid))
        {
        }

    }

    public class InvalidBase36DigitValueException : Exception
    {

        public InvalidBase36DigitValueException(byte Invalid)
            : base(String.Format("Invalid Base 36 Digit Value [{0}]", Invalid))
        {
        }


    }

}




