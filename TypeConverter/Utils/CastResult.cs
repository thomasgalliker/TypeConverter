using System;

using Guards;

namespace TypeConverter.Utils
{
    public class CastResult
    {
        public CastResult(object value)
        {
            this.Value = value;
        }

        public CastResult(Exception exception)
        {
            Guard.ArgumentNotNull(() => exception);

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
