using System;
using System.Linq.Expressions;
using AiqlWrapper.Helper;

namespace AiqlWrapper
{
    public static class AiqlFuncs
    {
        private const string ExceptionMessage = "Method should not be run directly. Only passed to an application insights LINQ";

        [TranslateFunc]
        [NoLocalEvaluation]
        public static DateTime Ago(TimeSpan ts)
        {
            throw new Exception(ExceptionMessage);
        }

        [TranslateFunc]
        [NoLocalEvaluation]
        public static T Iff<T>(bool predicate, T valTrue, T valFalse)
        {
            throw new Exception(ExceptionMessage);
        }

        #region Numerical
        [TranslateFunc]
        [NoLocalEvaluation]
        public static DateTime Bin(DateTime d, TimeSpan binSize)
        {
            throw new Exception(ExceptionMessage);
        }
        [TranslateFunc]
        [NoLocalEvaluation]
        public static long Bin(long field, long binSize)
        {
            throw new Exception(ExceptionMessage);
        }

        [TranslateFunc]
        [NoLocalEvaluation]
        public static double Abs(double field)
        {
            throw new Exception(ExceptionMessage);
        }
        [TranslateFunc]
        [NoLocalEvaluation]
        public static long Abs(long field)
        {
            throw new Exception(ExceptionMessage);
        }
        [TranslateFunc]
        [NoLocalEvaluation]
        public static int Abs(int field)
        {
            throw new Exception(ExceptionMessage);
        }
        [TranslateFunc]
        [NoLocalEvaluation]
        public static TimeSpan Abs(TimeSpan field)
        {
            throw new Exception(ExceptionMessage);
        }
        #endregion

        #region Conversions

        [TranslateFunc]
        [NoLocalEvaluation]
        public static long ToLong(int i)
        {
            throw new NotImplementedException();
        }
        [TranslateFunc]
        [NoLocalEvaluation]
        public static long ToLong(string s)
        {
            throw new NotImplementedException();
        }
        [TranslateFunc]
        [NoLocalEvaluation]
        public static long ToLong(double d)
        {
            throw new NotImplementedException();
        }

        [TranslateFunc]
        [NoLocalEvaluation]
        public static int ToInt(long l)
        {
            throw new NotImplementedException();
        }
        [TranslateFunc]
        [NoLocalEvaluation]
        public static int ToInt(string s)
        {
            throw new NotImplementedException();
        }
        [TranslateFunc]
        [NoLocalEvaluation]
        public static int ToInt(double d)
        {
            throw new NotImplementedException();
        }

        [TranslateFunc]
        [NoLocalEvaluation]
        public static double ToDouble(string s)
        {
            throw new NotImplementedException();
        }
        [TranslateFunc]
        [NoLocalEvaluation]
        public static double ToDouble(int i)
        {
            throw new NotImplementedException();
        }
        [TranslateFunc]
        [NoLocalEvaluation]
        public static double ToDouble(long l)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Aggregators
        [TranslateFunc(true)]
        [NoLocalEvaluation]
        public static long Count(bool predicate)
        {
            throw new Exception(ExceptionMessage);
        }
        [TranslateFunc(true)]
        [NoLocalEvaluation]
        public static long Count()
        {
            throw new Exception(ExceptionMessage);
        }

        [TranslateFunc(true)]
        [NoLocalEvaluation]
        public static long Sum(long l)
        {
            throw new Exception(ExceptionMessage);
        }
        [TranslateFunc(true)]
        [NoLocalEvaluation]
        public static double Sum(double d)
        {
            throw new Exception(ExceptionMessage);
        }
        [TranslateFunc(true)]
        [NoLocalEvaluation]
        public static int Sum(int l)
        {
            throw new Exception(ExceptionMessage);
        }

        [TranslateFunc(true)]
        [NoLocalEvaluation]
        public static double Avg(int i)
        {
            throw new Exception(ExceptionMessage);
        }
        [TranslateFunc(true)]
        [NoLocalEvaluation]
        public static double Avg(long l)
        {
            throw new Exception(ExceptionMessage);
        }
        [TranslateFunc(true)]
        [NoLocalEvaluation]
        public static double Avg(double d)
        {
            throw new Exception(ExceptionMessage);
        }

        [TranslateFunc(true)]
        [NoLocalEvaluation]
        public static double Percentile(int field, int percentile)
        {
            throw new Exception();
        }
        [TranslateFunc(true)]
        [NoLocalEvaluation]
        public static double Percentile(long field, int percentile)
        {
            throw new Exception();
        }
        [TranslateFunc(true)]
        [NoLocalEvaluation]
        public static double Percentile(TimeSpan field, int percentile)
        {
            throw new Exception();
        }
        [TranslateFunc(true)]
        [NoLocalEvaluation]
        public static double Percentile(double field, int percentile)
        {
            throw new Exception();
        }

        [TranslateFunc(true)]
        [NoLocalEvaluation]
        public static double Percentilew(int field, long weight, int percentile)
        {
            throw new Exception();
        }
        [TranslateFunc(true)]
        [NoLocalEvaluation]
        public static double Percentilew(long field, long weight, int percentile)
        {
            throw new Exception();
        }
        [TranslateFunc(true)]
        [NoLocalEvaluation]
        public static double Percentilew(TimeSpan field, long weight, int percentile)
        {
            throw new Exception();
        }
        [TranslateFunc(true)]
        [NoLocalEvaluation]
        public static double Percentilew(double field, long weight, int percentile)
        {
            throw new Exception();
        }

        #endregion

        #region strings
        /// <summary>
        /// RHS occurs as a substring of LHS.
        /// </summary>
        /// <param name="source">LHS</param>
        /// <param name="substring">RHS</param>
        /// <returns></returns>
        [TranslateBinaryMethod("contains", true)]
        [NoLocalEvaluation]
        public static bool ContainsCaseInsensitive(this string source, string substring)
        {
            throw new Exception(ExceptionMessage);
        }

        /// <summary>
        /// Right-hand-side (RHS) is a whole term in left-hand-side (LHS). Case insensitive
        /// </summary>
        /// <param name="source">LHS</param>
        /// <param name="substring">RHS</param>
        /// <returns></returns>
        [TranslateBinaryMethod("has", true)]
        [NoLocalEvaluation]
        public static bool Has(this string source, string substring)
        {
            throw new Exception(ExceptionMessage);
        }

        /// <summary>
        /// RHS is a prefix of a term in LHS. Case insensitive
        /// </summary>
        /// <param name="source">LHS</param>
        /// <param name="substring">RHS</param>
        /// <returns></returns>
        [TranslateBinaryMethod("hasprefix", true)]
        [NoLocalEvaluation]
        public static bool HasPrefix(this string source, string substring)
        {
            throw new Exception(ExceptionMessage);
        }

        /// <summary>
        /// RHS is a suffix of a term in LHS. Case insensitive
        /// </summary>
        /// <param name="source">LHS</param>
        /// <param name="substring">RHS</param>
        /// <returns></returns>
        [TranslateBinaryMethod("hassuffix", true)]
        [NoLocalEvaluation]
        public static bool HasSuffix(this string source, string substring)
        {
            throw new Exception(ExceptionMessage);
        }

        /// <summary>
        /// LHS contains a match for RHS.
        /// </summary>
        /// <param name="source">LHS</param>
        /// <param name="substring">RHS</param>
        /// <returns></returns>
        [TranslateBinaryMethod("matches regex")]
        [NoLocalEvaluation]
        public static bool MatchesRegex(this string source, string substring)
        {
            throw new Exception(ExceptionMessage);
        }
        #endregion
    }
    
}
