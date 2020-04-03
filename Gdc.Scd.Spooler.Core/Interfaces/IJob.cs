namespace Gdc.Scd.Spooler.Core.Interfaces
{
    public interface IJob
    {
        IOperationResult Output();

        string WhoAmI();
    }
}
