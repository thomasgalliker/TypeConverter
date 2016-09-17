using System;
using System.Diagnostics;

namespace TypeConverter
{
    [DebuggerDisplay("Value = {Value}, IsSuccessful = {IsSuccessful}, CastFlag = {CastFlag}")]
    internal class CastResult : ConversionResult
    {
        public CastResult(object value, CastFlag castFlag)
            : base(value)
        {
            this.CastFlag = castFlag;
        }

        public CastResult(Exception exception, CastFlag castFlag)
            : base(exception)
        {
            this.CastFlag = castFlag;
        }

        public CastFlag CastFlag { get; private set; }
    }
}