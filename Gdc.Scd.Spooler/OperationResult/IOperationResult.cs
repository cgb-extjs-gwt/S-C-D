using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.OperationResult
{
    public interface IOperationResult
    {
        bool IsSuccess { get; set; }
        string ErrorMessage { get; set; }
    }
}
