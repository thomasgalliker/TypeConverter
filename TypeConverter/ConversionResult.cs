using System;
using System.Diagnostics;

using Guards;

namespace TypeConverter
{
    [DebuggerDisplay("Value = {Value}, IsSuccessful = {IsSuccessful}")]
    internal class ConversionResult
    {
        public ConversionResult(object value)
        {
            this.Value = value;
        }

        public ConversionResult(Exception exception)
        {
            Guard.ArgumentNotNull(exception, nameof(exception));

            this.Exception = exception;
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
    }
}