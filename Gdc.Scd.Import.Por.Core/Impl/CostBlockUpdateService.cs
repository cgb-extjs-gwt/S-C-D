using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Por.Core.Interfaces;
using Gdc.Scd.Import.Por.Core.Scripts;
using System.IO;
using System.Reflection;

namespace Gdc.Scd.Import.Por.Core.Impl
{
    public class CostBlockUpdateService : ICostBlockUpdateService
    {
        private readonly IRepositorySet _repo;

        public CostBlockUpdateService(IRepositorySet repo)
        {
            _repo = repo;
        }

        public void UpdateByPla(Wg[] wgs)
        {
            if (wgs == null || wgs.Length == 0)
            {
                return;
            }

            UpdateFieldServiceCost(wgs);
            UpdateLogisticsCost(wgs);
            UpdateMarkupOtherCosts(wgs);
            UpdateMarkupStandardWaranty(wgs);
            UpdateProactive(wgs);
        }

        public void UpdateBySog(SwDigit[] digits)
        {
            if (digits == null || digits.Length == 0)
            {
                return;
            }

            UpdateSwProactive(digits);
            UpdateSwSpMaintenance(digits);
        }

        public virtual void UpdateFieldServiceCost(Wg[] wgs)
        {
            var tpl = new UpdateFieldServiceCost(wgs);
            _repo.ExecuteSql(tpl.ByCentralContractGroup());
            _repo.ExecuteSql(tpl.ByPla());
            _repo.ExecuteSql(ReadText("UpdateFieldServiceCost.sql"));
        }

        public virtual void UpdateLogisticsCost(Wg[] wgs)
        {
            var tpl = new UpdateLogisticCost(wgs);
            _repo.ExecuteSql(tpl.ByCentralContractGroup());
            _repo.ExecuteSql(tpl.ByPla());
        }

        public virtual void UpdateMarkupOtherCosts(Wg[] wgs)
        {
            var tpl = new UpdateMarkupOtherCosts(wgs);
            _repo.ExecuteSql(tpl.ByCentralContractGroup());
            _repo.ExecuteSql(tpl.ByPla());
        }

        public virtual void UpdateMarkupStandardWaranty(Wg[] wgs)
        {
            var tpl = new UpdateMarkupStandardWaranty(wgs);
            _repo.ExecuteSql(tpl.ByCentralContractGroup());
            _repo.ExecuteSql(tpl.ByPla());
        }

        public virtual void UpdateProactive(Wg[] wgs)
        {
            var tpl = new UpdateProactive(wgs);
            _repo.ExecuteSql(tpl.ByCentralContractGroup());
            _repo.ExecuteSql(tpl.ByPla());
        }

        public virtual void UpdateSwSpMaintenance(SwDigit[] digits)
        {
            var tpl = new UpdateSwSpMaintenance(digits);
            _repo.ExecuteSql(tpl.BySog());
        }

        public virtual void UpdateSwProactive(SwDigit[] digits)
        {
            var tpl = new UpdateSwProactive(digits);
            _repo.ExecuteSql(tpl.BySog());
        }

        protected virtual string ReadText(string fn)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Scripts.{fn}");
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}
