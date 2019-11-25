using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Gdc.Scd.Tests.Util
{
    public static class StreamUtil
    {
        private const int BUFFER_SIZE = 32768;

        public static Stream ReadBin(string path, string fn)
        {
            path = Path.Combine(Location(), path, fn);
            return File.OpenRead(path);
        }

        public static string ReadText(string path, string fn)
        {
            return GetReader(path, fn).ReadToEnd();
        }

        public static T ReadJson<T>(string path, string fn)
        {
            return ReadText(path, fn).AsObject<T>();
        }

        public static List<string[]> ReadCsv(string path, string fn, char sep = ';')
        {
            var result = new List<string[]>(25);

            using (var rdr = GetReader(path, fn))
            {
                string line;
                while ((line = rdr.ReadLine()) != null)
                {
                    result.Add(line.Split(sep));
                }

                rdr.Close();
            }

            return result;
        }

        public static StreamReader GetReader(string path, string fn)
        {
            path = Path.Combine(Location(), path, fn);
            return new StreamReader(path);
        }

        public static void Save(string path, string fn, Stream stream)
        {
            path = Path.Combine(Location(), path);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, fn);

            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }

        public static void Save(string path, string fn, string text)
        {
            path = Path.Combine(Location(), path);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, fn);

            File.WriteAllText(path, text);
        }

        public static MemoryStream Copy(this Stream source)
        {
            var ms = new MemoryStream();
            source.CopyTo(ms, BUFFER_SIZE);
            return ms;
        }

        private static string Location()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}
