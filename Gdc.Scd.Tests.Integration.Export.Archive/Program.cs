using Gdc.Scd.Export.Archive;
using Gdc.Scd.Export.ArchiveJob;

namespace Gdc.Scd.Tests.Integration.Export.Archive
{
    class Program
    {
        static void Main(string[] args)
        {
            new ArchiveJob().Output();
        }
    }
}
