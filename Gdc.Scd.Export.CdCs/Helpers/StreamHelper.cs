using System.IO;

namespace Gdc.Scd.Export.CdCs.Helpers
{
    public static class StreamHelper
    {
        private const int BUFFER_SIZE = 32768;

        public static MemoryStream Copy(this Stream source)
        {
            var ms = new MemoryStream();
            source.CopyTo(ms, BUFFER_SIZE);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}
