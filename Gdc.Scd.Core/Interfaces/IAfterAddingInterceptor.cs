namespace Gdc.Scd.Core.Interfaces
{
    public interface IAfterAddingInterceptor<T> where T : class, IIdentifiable
    {
        void Handle(T[] items);
    }
}
