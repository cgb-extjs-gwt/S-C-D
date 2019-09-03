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
            var dataCopyService = new DataCopyService();
            dataCopyService.CopyData();
        }
    }
}
