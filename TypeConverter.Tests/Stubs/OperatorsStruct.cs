using System;

namespace TypeConverter.Tests
{
    public struct OperatorsStruct
    {
        public static implicit operator string(OperatorsStruct o)
        {
            throw new NotImplementedException();
        }

        public static implicit operator int(OperatorsStruct o)
        {
            return 1;
        }

        public static explicit operator decimal?(OperatorsStruct o)
        {
            throw new NotImplementedException();
        }

        public static explicit operator StringSplitOptions(OperatorsStruct o)
        {
            return StringSplitOptions.RemoveEmptyEntries;
        }
    }
}