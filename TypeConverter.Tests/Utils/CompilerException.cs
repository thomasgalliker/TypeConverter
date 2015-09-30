using System;
using System.Diagnostics;

namespace TypeConverter.Tests.Utils
{
    [DebuggerDisplay("Line = {Line}, Column = {Column}, ErrorText = {ErrorText}")]
    internal class CompilerException : Exception
    {
        public CompilerException(int line, int column, string errorText)
        {
            this.Line = line;
            this.Column = column;
            this.ErrorText = errorText;
        }

        public int Line { get; private set; }

        public int Column { get; private set; }

        public string ErrorText { get; private set; }
    }
}
