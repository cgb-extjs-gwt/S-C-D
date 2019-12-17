using Gdc.Scd.Export.ArchiveResultSender;

namespace Gdc.Scd.Tests.Integration.Export.ArchiveResultSender
{
    class Program
    {
        static void Main(string[] args)
        {
            new ArchiveResultSenderJob().Output();
        }
    }
}
