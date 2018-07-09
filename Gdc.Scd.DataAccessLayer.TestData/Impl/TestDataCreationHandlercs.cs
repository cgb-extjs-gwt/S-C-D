﻿using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.TestData.Impl
{
    public class TestDataCreationHandlercs : IConfigureDatabaseHandler
    {
        public const string CountryLevelId = "Country";

        public const string PlaLevelId = "PLA";

        public const string WgLevelId = "WG";

        private readonly IRepositorySet repositorySet;

        private readonly DomainEnitiesMeta entityMetas;

        public TestDataCreationHandlercs(
            DomainEnitiesMeta entityMetas,
            IRepositorySet repositorySet)
        {
            this.entityMetas = entityMetas;
            this.repositorySet = repositorySet;
        }

        public void Handle()
        {
            var countryInputLevelMeta = (NamedEntityMeta)this.entityMetas.GetEntityMeta(CountryLevelId, MetaConstants.InputLevelSchema);
            var plaInputLevelMeta = (NamedEntityMeta)this.entityMetas.GetEntityMeta(PlaLevelId, MetaConstants.InputLevelSchema);
            var wgInputLevelMeta = (NamedEntityMeta)this.entityMetas.GetEntityMeta(WgLevelId, MetaConstants.InputLevelSchema);

            var queries = new List<SqlHelper>
            {
                this.BuildInsertSql(countryInputLevelMeta, this.GetCountrieNames()),
                this.BuildInsertSql(plaInputLevelMeta, this.GetPlaNames()),
                this.BuildInsertSql(wgInputLevelMeta, this.GetWarrantyGroupNames()),
                this.BuildInsertSql(MetaConstants.DependencySchema, "RoleCodeCode", this.GetRoleCodeNames()),
                this.BuildInsertSql(MetaConstants.DependencySchema, "ServiceLocationCode", this.GetServiceLocationCodeNames()),
                this.BuildInsertSql(MetaConstants.DependencySchema, "ReactionTimeCode", this.GetReactionTimeCodeNames())
            };
            queries.AddRange(this.BuildInsertCostBlockSql());

            foreach (var query in queries)
            {
                this.repositorySet.ExecuteSql(query);
            }
        }

        private SqlHelper BuildInsertSql(NamedEntityMeta entityMeta, string[] names)
        {
            var rows = new object[names.Length, 1];

            for (var index = 0; index < names.Length; index++)
            {
                rows[index, 0] = names[index];
            }

            return Sql.Insert(entityMeta, entityMeta.NameField.Name).Values(rows);
        }

        private SqlHelper BuildInsertSql(string schema, string entityName, string[] names)
        {
            var entityMeta = (NamedEntityMeta)this.entityMetas.GetEntityMeta(entityName, schema);

            return this.BuildInsertSql(entityMeta, names);
        }

        private IEnumerable<SqlHelper> BuildInsertCostBlockSql()
        {
            var countries = this.GetCountrieNames();
            var plas = this.GetPlaNames();
            var warrantyGroups = this.GetWarrantyGroupNames();
            var roleCodes = this.GetRoleCodeNames();
            var serviceLocations = this.GetServiceLocationCodeNames();
            var reactionTimes = this.GetReactionTimeCodeNames();
            var map = new Dictionary<string, string[]>
            {
                [CountryLevelId] = countries,
                [PlaLevelId] = plas,
                [WgLevelId] = warrantyGroups,
                ["RoleCodeCode"] = roleCodes,
                ["ServiceLocationCode"] = serviceLocations,
                ["ReactionTimeCode"] = reactionTimes
            };

            var countryLevelMeta = (NamedEntityMeta)this.entityMetas.GetEntityMeta(CountryLevelId, MetaConstants.InputLevelSchema);
            var firtsCountryQuery = this.BuildSelectIdByNameQuery(countryLevelMeta, countries[0], "Country_0");

            foreach (var costBlockMeta in this.entityMetas.CostBlocks)
            {
                var fieldNames = 
                    map.Keys.Where(fieldName => costBlockMeta.AllFields.Any(costBlockField => costBlockField.Name == fieldName))
                            .ToArray();

                var insertValues = new ISqlBuilder[warrantyGroups.Length, fieldNames.Length];

                for (var warrantyGroupIndex = 0; warrantyGroupIndex < warrantyGroups.Length; warrantyGroupIndex++)
                {
                    insertValues[warrantyGroupIndex, 0] = firtsCountryQuery;

                    for (var fieldIndex = 1; fieldIndex < fieldNames.Length; fieldIndex++)
                    {
                        var fieldName = fieldNames[fieldIndex];
                        var refNames = map[fieldName];
                        var refNameIndex = warrantyGroupIndex - warrantyGroupIndex / refNames.Length * refNames.Length;
                        var refName = refNames[refNameIndex];

                        var refField = costBlockMeta.InputLevelFields[fieldName] ?? (ReferenceFieldMeta)costBlockMeta.DependencyFields[fieldName];

                        insertValues[warrantyGroupIndex, fieldIndex] = 
                            this.BuildSelectIdByNameQuery((NamedEntityMeta)refField.ReferenceMeta, refName, $"{fieldName}_{warrantyGroupIndex}");
                    }
                }

                yield return Sql.Insert(costBlockMeta, fieldNames).Values(insertValues);

                var columns = new List<ColumnInfo>
                {
                    new ColumnInfo(countryLevelMeta.IdField.Name, countryLevelMeta.Name)
                };
                columns.AddRange(
                    fieldNames.Where(field => field != CountryLevelId)
                              .Select(fieldName => new ColumnInfo(fieldName, costBlockMeta.Name)));

                yield return
                    Sql.Insert(costBlockMeta, fieldNames)
                       .Query(
                            Sql.Select(columns.ToArray())
                               .From(countryLevelMeta)
                               .Join(
                                    costBlockMeta,
                                    SqlOperators.Equals(
                                        new ColumnSqlBuilder(new ColumnInfo(CountryLevelId, costBlockMeta.Name)),
                                        firtsCountryQuery))
                               .Where(SqlOperators.NotEquals(countryLevelMeta.NameField.Name, "firstCountry", countries[0], countryLevelMeta.Name)));
            }
        }

        private ISqlBuilder BuildSelectIdByNameQuery(NamedEntityMeta meta, string name, string paramName)
        {
            return new BracketsSqlBuilder
            {
                SqlBuilder = Sql.Select(new ColumnInfo { Name = meta.IdField.Name, TableName = meta.Name })
                                .From(meta)
                                .Where(SqlOperators.Equals(meta.NameField.Name, paramName, name, meta.Name))
                                .ToSqlBuilder()
            };
        }

        private string[] GetCountrieNames()
        {
            return new[]
            {
                "Algeria",
                "Austria",
                "Balkans",
                "Belgium",
                "CIS & Russia",
                "Czech Republic",
                "Denmark",
                "Egypt",
                "Finland",
                "France",
                "Germany",
                "Greece",
                "Hungary",
                "India",
                "Italy",
                "Luxembourg",
                "Middle East",
                "Morocco",
                "Netherlands",
                "Norway",
                "Poland",
                "Portugal",
                "South Africa",
                "Spain",
                "Sweden",
                "Switzerland",
                "Tunisia",
                "Turkey",
                "UK & Ireland"
            };
        }

        private string[] GetPlaNames()
        {
            return new[]
            {
                "Desktops",
                "Mobiles",
                "Peripherals",
                "Storage Products",
                "x86/IA Servers"
            };
        }

        private string[] GetWarrantyGroupNames()
        {
            return new string[]
            {
                "TC4",
                "TC5",
                "TC6",
                "TC8",
                "TC7",
                "TCL",
                "U05",
                "U11",
                "U13",
                "WSJ",
                "WSN",
                "WSS",
                "WSW",
                "U02",
                "U06",
                "U07",
                "U12",
                "U14",
                "WRC",
                "HMD",
                "NB6",
                "NB1",
                "NB2",
                "NB5",
                "ND3",
                "NC1",
                "NC3",
                "NC9",
                "TR7",
                "DPE",
                "DPH",
                "DPM",
                "DPX",
                "IOA",
                "IOB",
                "IOC",
                "MD1",
                "PSN",
                "SB2",
                "SB3",
                "CD1",
                "CD2",
                "CE1",
                "CE2",
                "CD4",
                "CD5",
                "CD6",
                "CD7",
                "CDD",
                "CD8",
                "CD9",
                "C70",
                "CS8",
                "C74",
                "C75",
                "CS7",
                "CS1",
                "CS2",
                "CS3",
                "C16",
                "C18",
                "C33",
                "CS5",
                "CS4",
                "CS6",
                "CS9",
                "C96",
                "C97",
                "C98",
                "C71",
                "C73",
                "C80",
                "C84",
                "F58",
                "F40",
                "F48",
                "F53",
                "F54",
                "F57",
                "F41",
                "F49",
                "F42",
                "F43",
                "F44",
                "F45",
                "F50",
                "F51",
                "F52",
                "F36",
                "F46",
                "F47",
                "F56",
                "F28",
                "F29",
                "F35",
                "F55",
                "S14",
                "S17",
                "S15",
                "S16",
                "S50",
                "S51",
                "S18",
                "S35",
                "S36",
                "S37",
                "S39",
                "S40",
                "S55",
                "VSH",
                "MN1",
                "MN4",
                "PQ8",
                "Y01",
                "Y15",
                "PX1",
                "PY1",
                "PY4",
                "Y09",
                "Y12",
                "MN2",
                "MN3",
                "PX2",
                "PX3",
                "PXS",
                "PY2",
                "PY3",
                "SD2",
                "Y03",
                "Y17",
                "Y21",
                "Y32",
                "Y06",
                "Y13",
                "Y28",
                "Y30",
                "Y31",
                "Y37",
                "Y38",
                "Y39",
                "Y40",
                "PX6",
                "PX8",
                "PRC",
                "RTE",
                "Y07",
                "Y16",
                "Y18",
                "Y25",
                "Y26",
                "Y27",
                "Y33",
                "Y36",
                "S41",
                "S42",
                "S43",
                "S44",
                "S45",
                "S46",
                "S47",
                "S48",
                "S49",
                "S52",
                "S53",
                "S54",
                "PQ0",
                "PQ5",
                "PQ9"
            };
        }

        private string[] GetRoleCodeNames()
        {
            return new string[]
            {
                "SEFS05",
                "SEFS06",
                "SEFS04",
                "SEIE07",
                "SEIE08"
            };
        }

        private string[] GetServiceLocationCodeNames()
        {
            return new string[]
            {
                "Material",
                "Bring-In",
                "Send-In",
                "Collect & Return",
                "Collect & Return (Displays)",
                "Door-to-Door (SWAP)",
                "Desk-to-Desk (SWAP)",
                "On-Site",
                "On-Site (Exchange)"
            };
        }

        private string[] GetReactionTimeCodeNames()
        {
            return new string[]
            {
                "best effort",
                "SBD response",
                "NBD response",
                "4h response",
                "NBD recovery",
                "8h recovery",
                "4h recovery",
                "24h recovery"
            };
        }
    }
}
