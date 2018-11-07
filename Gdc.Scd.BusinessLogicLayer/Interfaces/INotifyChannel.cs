namespace Gdc.Scd.BusinessLogicLayer.Interfaces
{
    public interface INotifyChannel
    {
        void Create(string username);

        object GetMessage(string userName);

        void RemoveMessage(string userName, object msg);

        void Send(object value);

        void Send(string userName, object value);
    }
}