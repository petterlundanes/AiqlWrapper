using System;

namespace AiqlWrapper.Helper
{
    /// <summary>
    /// Disabled any local evaluation of the target method. This can only be used on method with a specified AIQL translation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NoLocalEvaluationAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TranslateFuncAttribute : Attribute
    {
        internal bool IsAggregation { get; }

        public TranslateFuncAttribute()
        {
            IsAggregation = false;
        }

        public TranslateFuncAttribute(bool isAggregation)
        {
            IsAggregation = isAggregation;
        }
    }

    /// <summary>
    /// Attribute to specify a direct AIQL translation for a method with two arguments
    /// (someObject.Somefunction("foobar") and staticFunction("foo", "bar"), but have two arguments).
    /// </summary>
    /// <remarks>If a the target methods implemention is left empty/non-functional, the <seealso cref="NoLocalEvaluationAttribute"/> should also be used.</remarks>
    /// <example>
    /// <code>
    /// [DirectBinaryTranslationAttribute("aiqlFuncName")]
    /// static string FuncName(string foo, string bar){ throw new NotImplementedException(); }
    /// </code>
    /// Will translate to "[bar] aiqlFuncName [bar]"
    /// with foo and bar either referencing a field in the query or a direct representation of the locally evaluated variable.
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TranslateBinaryMethodAttribute : Attribute
    {

        internal string Func { get; }
        internal bool Negatable { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="func">The AIQL translation of the function</param>
        /// <param name="negatable">If the function can be negated by prefixing "!". If false, a negated call will result in not([call])</param>
        public TranslateBinaryMethodAttribute(string func, bool negatable = false)
        {
            Func = func;
            Negatable = negatable;
        }
    }

    public class FieldNameAttribute : Attribute
    {
        public string Name { get; }

        public FieldNameAttribute(string name)
        {
            Name = name;
        }
    }
}
