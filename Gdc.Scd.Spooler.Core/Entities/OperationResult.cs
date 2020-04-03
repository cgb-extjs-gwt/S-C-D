using Gdc.Scd.Spooler.Core.Interfaces;

namespace Gdc.Scd.Spooler.Core.Entities
{
    public class OperationResult<T> : IOperationResult
    {
        public bool IsSuccess { get; set; }

        public string ErrorMessage { get; set; }

        public T Result { get; set; }
    }
}