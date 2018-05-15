using System;

namespace Gdc.Scd.DataAccessLayer.Interfaces
{
    public interface ITransaction : IDisposable
    {
        void Commit();

        void Rollback();
    }
}
