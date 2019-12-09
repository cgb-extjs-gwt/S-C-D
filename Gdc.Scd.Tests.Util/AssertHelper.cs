using NUnit.Framework;

namespace Gdc.Scd.Tests.Util
{
    public static class AssertHelper
    {
        public static void Contains(string expected, string src, string message, params object[] args)
        {
            Assert.True(src.Contains(expected), message, args);
        }

        public static void Contains(string expected, string src)
        {
            Assert.True(src.Contains(expected));
        }

        public static void Has(this string src, string expected)
        {
            Assert.True(src.Contains(expected));
        }

        public static void Has(this string src, string expected, string message, params object[] args)
        {
            Assert.True(src.Contains(expected), message, args);
        }

        public static void HasNot(this string src, string expected)
        {
            Assert.True(!src.Contains(expected));
        }
    }
}
