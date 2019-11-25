using Gdc.Scd.DataAccessLayer.Interfaces;

namespace Gdc.Scd.BusinessLogicLayer.Procedures
{
    public class CopyHwFspCodeTranslations
    {
        private const string PROC = "Temp.CopyHwFspCodeTranslations";

        private readonly IRepositorySet _repo;

        public CopyHwFspCodeTranslations(IRepositorySet repo)
        {
            _repo = repo;
        }

        public void Execute()
        {
            _repo.ExecuteProc(PROC);
        }
    }
}
