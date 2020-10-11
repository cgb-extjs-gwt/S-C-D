using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
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
        private readonly ICostBlockService costBlockService;
        private readonly DomainEnitiesMeta meta;

        public CostBlockUpdateService(IRepositorySet repo, ICostBlockService costBlockService, DomainEnitiesMeta meta)
        {
            _repo = repo;
            this.costBlockService = costBlockService;
            this.meta = meta;
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
        public void ActivateBySog(SwDigit[]digits)
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
            var tpl = new UpdateFieldServiceAvailability(wgs);
            _repo.ExecuteSql(tpl.ByCentralContractGroup());
            _repo.ExecuteSql(tpl.ByPla());

            var tpl1 = new UpdateFieldServiceLocation(wgs);
            _repo.ExecuteSql(tpl1.ByCentralContractGroup());
            _repo.ExecuteSql(tpl1.ByPla());

            var tpl2 = new UpdateFieldServiceReactionTimeType(wgs);
            _repo.ExecuteSql(tpl2.ByCentralContractGroup());
            _repo.ExecuteSql(tpl2.ByPla());

            var tpl3 = new UpdateFieldServiceWg(wgs);
            _repo.ExecuteSql(tpl3.ByCentralContractGroup());
            _repo.ExecuteSql(tpl3.ByPla());

            //_repo.ExecuteSql(ReadText("UpdateFieldServiceCost.sql"));

            //var costBlocks = this.meta.CostBlocks.GetSome(MetaConstants.HardwareSchema, "FieldServiceCost");

            //this.costBlockService.UpdateByCoordinates(costBlocks);
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
            //var tpl = new UpdateProactive(wgs);
            //_repo.ExecuteSql(tpl.ByCentralContractGroup());
            //_repo.ExecuteSql(tpl.ByPla());

            var costBlocks = this.meta.CostBlocks.GetSome(MetaConstants.HardwareSchema, "ProActive");

            this.costBlockService.UpdateByCoordinates(costBlocks);
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
        public virtual void ActivateSwProactive(SwDigit[] digits)
        {
            var tpl = new ActivateSwProactive(digits);
            _repo.ExecuteSql(tpl.BySog());
            //todo come back here
        }
        public virtual void ActivateSwSpMaintenance(SwDigit[] digits)
        {
            var tpl = new ActiateSwSpMaintenance(digits);
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
