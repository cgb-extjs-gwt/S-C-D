namespace Gdc.Scd.OperationResult
{
    public class OperationResult<T> : IOperationResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public T Result { get; set; }
    }
}