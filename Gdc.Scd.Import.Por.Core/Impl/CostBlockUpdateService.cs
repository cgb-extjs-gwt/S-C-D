using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Import.Por.Core.Interfaces;
using System.IO;
using System.Reflection;
using System.Text;

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
            UpdateFieldServiceCost(wgs);
            UpdateLogisticsCost(wgs);
            UpdateMarkupOtherCosts(wgs);
            UpdateMarkupStandardWaranty(wgs);
            UpdateProactive(wgs);
        }

        public virtual void UpdateFieldServiceCost(Wg[] wgs)
        {
            ExecuteFromFile("UpdateFieldServiceCost.sql", wgs);
        }

        public virtual void UpdateLogisticsCost(Wg[] wgs)
        {
            ExecuteFromFile("UpdateLogisticsCost.sql", wgs);
        }

        public virtual void UpdateMarkupOtherCosts(Wg[] wgs)
        {
            ExecuteFromFile("UpdateMarkupOtherCosts.sql", wgs);
        }

        public virtual void UpdateMarkupStandardWaranty(Wg[] wgs)
        {
            ExecuteFromFile("UpdateMarkupStandardWaranty.sql", wgs);
        }

        public virtual void UpdateProactive(Wg[] wgs)
        {
            ExecuteFromFile("UpdateProactive.sql", wgs);
        }

       public virtual void UpdateSwSpMaintenance(SwDigit[] digits)
        {
            ExecuteFromFile("UpdateSwSpMaintenance.sql", digits);
        }

        protected virtual void ExecuteFromFile(string fn, Wg[] wgs)
        {
            var sb = new StringBuilder(2048);
            sb.Append(@"
                declare @wg dbo.ListID;
                insert into @wg(id) select id from InputAtoms.Wg where Deactivated = 0 and UPPER(name) in (");
            for (var i = 0; i < wgs.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append("'").Append(wgs[i].Name.ToUpper()).Append("'");
            }
            sb.Append(");");
            sb.AppendLine();
            sb.AppendLine();
            sb.Append(ReadText(fn));

            _repo.ExecuteSql(sb.ToString());
        }

        protected virtual void ExecuteFromFile(string fn, SwDigit[] wgs)
        {
            var sb = new StringBuilder(2048);
            sb.Append(@"
                declare @dig dbo.ListID;
                insert into @dig select id from InputAtoms.SwDigit where Deactivated = 0 and UPPER(name) in (");
            for (var i = 0; i < wgs.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append("'").Append(wgs[i].Name.ToUpper()).Append("'");
            }
            sb.Append(");");
            sb.AppendLine();
            sb.AppendLine();
            sb.Append(ReadText(fn));

            _repo.ExecuteSql(sb.ToString());
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
