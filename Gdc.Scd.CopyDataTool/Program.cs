using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.CopyDataTool.Configuration;
using Gdc.Scd.Core.Helpers;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Ninject;

namespace Gdc.Scd.CopyDataTool
{
    public class Program
    {
        static void Main(string[] args)
        {
            NinjectExt.IsConsoleApplication = true;
            var kernel = CreateKernel();

            var dataCopyService = new DataCopyService(kernel);
            dataCopyService.CopyData();

            var manualCopyService = new ManualDataCopyService(kernel);
            manualCopyService.CopyData();
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
