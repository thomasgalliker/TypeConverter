using System;

namespace TypeConverter.Tests
{
    public class Operators
    {
        public static implicit operator string(Operators o)
        {
            throw new NotImplementedException();
        }

        public static implicit operator int(Operators o)
        {
            return 1;
        }

        public static explicit operator decimal?(Operators o)
        {
            throw new NotImplementedException();
        }

        public static explicit operator StringSplitOptions(Operators o)
        {
            return StringSplitOptions.RemoveEmptyEntries;
        }

        public static explicit operator Operators2(Operators o)
        {
            throw new NotImplementedException();
        }
    }
}