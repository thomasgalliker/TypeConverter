using System;
using System.CodeDom.Compiler;
using System.Diagnostics;

using TypeConverter.Utils;

namespace TypeConverter.Tests.Utils
{
    [DebuggerDisplay("SourceType = {SourceType}, TargetType = {TargetType}, IsCompilable = {IsCompilable}")]
    internal class CompilerConversionTestCase
    {
        public CompilerConversionTestCase(Type sourceType, Type targetType, CastFlag castFlag, string codeline = null, CompilerError compilerError = null)
        {
            this.SourceType = sourceType;
            this.TargetType = targetType;
            this.Codeline = codeline;
            this.CompilerError = compilerError;
            this.CastFlag = castFlag;
        }

        public CastFlag CastFlag { get; private set; }

        public Type SourceType { get; private set; }

        public Type TargetType { get; private set; }

        public string Codeline { get; private set; }

        public CompilerError CompilerError { get; private set; }

        public bool IsCompilable
        {
            get
            {
                return this.CompilerError == null;
            }
        }
    }

}
