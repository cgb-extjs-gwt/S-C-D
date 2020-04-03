namespace Gdc.Scd.Spooler.Core.Interfaces
{
    public interface IOperationResult
    {
        bool IsSuccess { get; set; }

        string ErrorMessage { get; set; }
    }
}
