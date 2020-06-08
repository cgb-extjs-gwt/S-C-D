using Gdc.Scd.Export.Sap.Impl;

namespace Gdc.Scd.Tests.Integration.Export.Sap
{
    class Program
    {
        static void Main(string[] args)
        {
            new SapJob().Output();
        }
    }
}
