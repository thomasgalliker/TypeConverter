using System;

namespace TypeConverter.Tests.Stubs
{
    public class MyEnumConverter : IConvertable<string, MyEnum>, IConvertable<MyEnum, string>
    {
        public MyEnum Convert(string value)
        {
            return (MyEnum)Enum.Parse(typeof(MyEnum), value);
        }

        public string Convert(MyEnum value)
        {
            return value.ToString();
        }
    }
}