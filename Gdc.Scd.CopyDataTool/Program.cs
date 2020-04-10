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
                var helperService = kernel.Get<CopyDataToolHelperService>();
                var exchangeRateCalculator = helperService.GetExcangeRateCalculator();

                var dataCopyService = new DataCopyService(kernel, exchangeRateCalculator);
                dataCopyService.CopyData();

                Console.WriteLine();

                var manualCopyService = new ManualDataCopyService(kernel, exchangeRateCalculator);
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
