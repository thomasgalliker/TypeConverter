using System;

namespace TypeConverter.Tests
{
    public class DerivedOperators : Operators
    {
        public static explicit operator DateTime(DerivedOperators o)
        {
            return DateTime.Now;
        }

        public static explicit operator Byte(DerivedOperators o)
        {
            return new byte();
        }

        public static explicit operator Char(DerivedOperators o)
        {
            return new char();
        }
    }
}