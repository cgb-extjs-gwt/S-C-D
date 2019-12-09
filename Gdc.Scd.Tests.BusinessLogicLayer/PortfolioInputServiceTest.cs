using Gdc.Scd.BusinessLogicLayer.Impl;
using Gdc.Scd.Tests.Util;
using NUnit.Framework;

namespace Gdc.Scd.Tests.BusinessLogicLayer
{
    public class PortfolioInputServiceTest : PortfolioInputService
    {
        [TestCase]
        public void GetDurationItemsQueryTest()
        {
            this.dependency = "Duration";
            this.country = 41;
            string query = base.BuildQuery();

            query.Has("with cte as (");
            query.Has("SELECT p.DurationId");
            query.Has("FROM Portfolio.LocalPortfolio p");
            query.Has("where p.CountryId = 41");
            query.Has("group by p.DurationId");

            query.Has("union");

            query.Has("select std.DurationId");
            query.Has("from Fsp.HwStandardWarranty std");
            query.Has("where std.Country = 41");
            query.Has("group by std.DurationId");

            query.Has("select t.Id, t.Name");
            query.Has("from Dependencies.Duration t");
            query.Has("where exists(select * from cte where DurationId = t.Id)");
            query.HasNot("t.IsDisabled = 0");
            query.Has("order by t.Name");
        }

        [TestCase]
        public void GetReactionTimeReactionTypeItemsQueryTest()
        {
            this.dependency = "ReactionTimeType";
            this.country = 117;
            string query = base.BuildQuery();

            query.Has("with cte as (");
            query.Has("SELECT p.ReactionTime_ReactionType");
            query.Has("FROM Portfolio.LocalPortfolio p");
            query.Has("where p.CountryId = 117");
            query.Has("group by p.ReactionTime_ReactionType");
            query.Has("union");
            query.Has("select std.ReactionTime_ReactionType");
            query.Has("from Fsp.HwStandardWarranty std");
            query.Has("where std.Country = 117");
            query.Has("group by std.ReactionTime_ReactionType");
            query.Has("select t.Id, t.Name");
            query.Has("from Dependencies.ReactionTimeType t");
            query.Has("where exists(select * from cte where ReactionTime_ReactionType = t.Id)");
            query.Has("and t.IsDisabled = 0");
            query.Has("order by t.Name");

        }

        [TestCase]
        public void GetServiceLocationItemsQueryTest()
        {
            this.dependency = "ServiceLocation";
            this.country = 112;
            string query = base.BuildQuery();

            query.Has("with cte as (");
            query.Has("SELECT p.ServiceLocationId");
            query.Has("FROM Portfolio.LocalPortfolio p");
            query.Has("where p.CountryId = 112");
            query.Has("group by p.ServiceLocationId");

            query.Has("union");

            query.Has("select std.ServiceLocationId");
            query.Has("from Fsp.HwStandardWarranty std");
            query.Has("where std.Country = 112");
            query.Has("group by std.ServiceLocationId");
            query.Has(")");
            query.Has("select t.Id, t.Name");
            query.Has("from Dependencies.ServiceLocation t");
            query.Has("where exists(select * from cte where ServiceLocationId = t.Id)");
            query.Has("order by t.Name");
            query.HasNot("t.IsDisabled = 0");

        }

        [TestCase]
        public void GetReactionTimeTypeAvailabilityItemsQueryTest()
        {
            this.dependency = "ReactionTimeTypeAvailability";
            this.country = 41;
            string query = base.BuildQuery();

            query.Has("with cte as (");
            query.Has("SELECT p.ReactionTime_ReactionType_Avalability");
            query.Has("FROM Portfolio.LocalPortfolio p");
            query.Has("where p.CountryId = 41");
            query.Has("group by p.ReactionTime_ReactionType_Avalability");

            query.Has("union ");

            query.Has("select std.ReactionTime_ReactionType_Avalability");
            query.Has("from Fsp.HwStandardWarranty std");
            query.Has("where std.Country = 41");
            query.Has("group by std.ReactionTime_ReactionType_Avalability");
            query.Has(")");
            query.Has("select t.Id, t.Name");
            query.Has("from Dependencies.ReactionTimeTypeAvailability t ");
            query.Has("where exists(select * from cte where ReactionTime_ReactionType_Avalability = t.Id)");
            query.Has("and t.IsDisabled = 0");
            query.Has("order by t.Name");

        }
    }
}
