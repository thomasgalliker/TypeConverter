using System;

using Guards;

namespace TypeConverter.Utils
{
    internal class CastResult // TODO: make internal
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
