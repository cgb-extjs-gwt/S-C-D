using Gdc.Scd.BusinessLogicLayer.Scripts;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class PortfolioInputService
    {
        private IRepositorySet repo;

        protected string dependency;

        protected long country;

        private bool prepared;

        private int iID;
        private int iNAME;

        public PortfolioInputService(IRepositorySet repositorySet)
        {
            this.repo = repositorySet;
        }

        protected PortfolioInputService() { }

        public Task<IEnumerable<NamedId>> GetCoordinateItemsByPorfolio(long cnt, string dep)
        {
            this.dependency = dep;
            this.country = cnt;
            this.prepared = false;

            return repo.ReadBySqlAsync(BuildQuery(), Read);
        }

        protected string BuildQuery()
        {
            DependencyByPortfolio tpl;

            switch (dependency)
            {

                case "ReactionTimeType":
                    tpl = new DependencyByPortfolio(country, dependency, "ReactionTime_ReactionType", true);
                    break;

                case "Duration":
                    tpl = new DependencyByPortfolio(country, dependency, "DurationId");
                    break;

                case "ServiceLocation":
                    tpl = new DependencyByPortfolio(country, dependency, "ServiceLocationId");
                    break;

                case "ReactionTimeTypeAvailability":
                    tpl = new DependencyByPortfolio(country, dependency, "ReactionTime_ReactionType_Avalability", true);
                    break;

                default:
                    //TODO: realize other deps
                    throw new System.NotImplementedException("dependency");
            }

            return tpl.TransformText();
        }

        private NamedId Read(IDataReader reader)
        {
            if (!prepared)
            {
                Prepare(reader);
            }
            return new NamedId
            {
                Id = reader.GetInt64(iID),
                Name = reader.GetString(iNAME)
            };
        }

        private void Prepare(IDataReader reader)
        {
            iID = reader.GetOrdinal("Id");
            iNAME = reader.GetOrdinal("Name");

            prepared = true;
        }
    }
}
