using System.IO;
using System.Reflection;

namespace Gdc.Scd.Tests.Util
{
    public class StreamUtil
    {
        public static Stream ReadBin(string path, string fn)
        {
            path = Path.Combine(Location(), path, fn);
            return File.OpenRead(path);
        }

        public static string ReadText(string path, string fn)
        {
            path = Path.Combine(Location(), path, fn);
            var streamReader = new StreamReader(path);

            return streamReader.ReadToEnd();
        }

        private static string Location()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}
