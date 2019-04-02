using System;

namespace Gdc.Scd.Export.Archive
{
    class Program
    {
        static void Main(string[] args)
        {
            var job = new ArchiveJob();
            Console.WriteLine(job);
            job.Output();
        }
    }
}
