using System;

namespace TypeConverter.Tests.Stubs
{
    public class DerivedOperators : Operators
    {
        public static explicit operator DateTime(DerivedOperators o)
        {
            return DateTime.Now;
        }

        public static explicit operator Byte(DerivedOperators o)
        {
            return (byte)0x08;
        }

        public static explicit operator Char(DerivedOperators o)
        {
            return 'X';
        }
    }
}