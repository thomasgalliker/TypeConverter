using System;
using System.Diagnostics;

using Guards;

namespace TypeConverter.Utils
{
    [DebuggerDisplay("Value = {Value}, IsSuccessful = {IsSuccessful}, CastFlag = {CastFlag}")]
    internal class CastResult // : IEquatable<CastResult>
    {
        public CastResult(object value, CastFlag castFlag)
        {
            this.Value = value;
            this.CastFlag = castFlag;
        }

        public CastResult(Exception exception, CastFlag castFlag)
        {
            Guard.ArgumentNotNull(() => exception);

            this.Exception = exception;
            this.CastFlag = castFlag;
        }

        public bool IsSuccessful
        {
            get
            {
                return this.Exception == null;
            }
        }

        public object Value { get; private set; }

        public Exception Exception { get; private set; }

        public CastFlag CastFlag { get; private set; }

        ////public override bool Equals(object obj)
        ////{
        ////    return this.Equals(obj as CastResult);
        ////}

        ////public override int GetHashCode()
        ////{
        ////    unchecked
        ////    {
        ////        int hash = 17;
        ////        hash = hash * 23 + this.Value.GetHashCode();
        ////        hash = hash * 23 + this.IsSuccessful.GetHashCode();
        ////        hash = hash * 23 + this.CastFlag.GetHashCode();
        ////        return hash;
        ////    }
        ////}

        ////public bool Equals(CastResult other)
        ////{
        ////    return other != null &&
        ////        this.Value == other.Value &&
        ////        this.IsSuccessful == other.IsSuccessful &&
        ////        this.CastFlag == other.CastFlag;
        ////}
    }
}
