using Gdc.Scd.Core.Helpers;
using Ninject;
using System;

namespace Gdc.Scd.CopyDataTool
{
    public class Program
    {
        static void Main(string[] args)
        {
            NinjectExt.IsConsoleApplication = true;
            var kernel = CreateKernel();
            
            try
            {
                var dataCopyService = new DataCopyService(kernel);
                dataCopyService.CopyData();

                Console.WriteLine();

                var manualCopyService = new ManualDataCopyService(kernel);
                manualCopyService.CopyData();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:");
                Console.WriteLine(ex.Message);
            }
            
            Console.ReadKey();
        }


        private static StandardKernel CreateKernel()
        {
            return new StandardKernel(
                new Core.Module(),
                new DataAccessLayer.Module(),
                new BusinessLogicLayer.Module(),
                new Module());
        }
    }
}
