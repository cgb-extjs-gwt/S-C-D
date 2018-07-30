using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.BusinessLogicLayer.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Impl;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.TestData.Impl
{
    public class TestDataCreationHandlercs : IConfigureDatabaseHandler
    {
        private const string CountryLevelId = "Country";

        private const string PlaLevelId = "Pla";

        private const string WgLevelId = "Wg";

        private const string RoleCodeKey = "RoleCodeCode";

        private const string ServiceLocationKey = "ServiceLocation";

        private const string YearKey = "Year";

        private const string ReactionTimeKey = "ReactionTime";

        private const string ReactionTypeKey = "ReactionType";

        private const string AvailabilityKey = "Availability";

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
            this.CreatePlas();

            var countryInputLevelMeta = (NamedEntityMeta)this.entityMetas.GetEntityMeta(CountryLevelId, MetaConstants.InputLevelSchema);
            var countryRepository = repositorySet.GetRepository<Country>();

            var countries = this.GetCountrieNames().Select(c => new Country
            {
                Name = c,
                CanOverrideListAndDealerPrices = GenerateRandomBool(),
                CanOverrideTransferCostAndPrice = GenerateRandomBool(),
                ShowDealerPrice = GenerateRandomBool()
            });

            countryRepository.Save(countries);
            repositorySet.Sync();


            var plaInputLevelMeta = (NamedEntityMeta)this.entityMetas.GetEntityMeta(PlaLevelId, MetaConstants.InputLevelSchema);
            var wgInputLevelMeta = (NamedEntityMeta)this.entityMetas.GetEntityMeta(WgLevelId, MetaConstants.InputLevelSchema);


            var queries = new List<SqlHelper>
            {
                //this.BuildInsertSql(plaInputLevelMeta, this.GetPlaNames()),
                //this.BuildInsertSql(wgInputLevelMeta, this.GetWarrantyGroupNames()),

                //this.BuildInsertSql(MetaConstants.DependencySchema, RoleCodeKey, this.GetRoleCodeNames()),
                this.BuildInsertSql(MetaConstants.DependencySchema, ServiceLocationKey, this.GetServiceLocationCodeNames()),
                this.BuildInsertSql(new NamedEntityMeta(ReactionTimeKey, MetaConstants.DependencySchema), this.GetReactionTimeCodeNames()),
                this.BuildInsertSql(new NamedEntityMeta(ReactionTypeKey, MetaConstants.DependencySchema), this.GetReactionTypeNames()),
                this.BuildInsertSql(MetaConstants.DependencySchema, YearKey, this.GetYearNames()),
                this.BuildInsertSql("References", "Currency", this.GetCurrenciesNames()),
                this.BuildInsertSql(MetaConstants.DependencySchema, AvailabilityKey, this.GetAvailabilityNames()),
                this.BuildInsertReactionTimeTypeSql(),
                this.BuildInsertReactionTimeAvailabilitySql()
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

        //private IEnumerable<SqlHelper> BuildInsertCostBlockSql()
        //{
        //    var countries = this.GetCountrieNames();
        //    var plas = this.GetPlaNames();
        //    var warrantyGroups = this.GetWarrantyGroupNames();
        //    //var roleCodes = this.GetRoleCodeNames();
        //    var serviceLocations = this.GetServiceLocationCodeNames();
        //    var reactionTimes = this.GetReactionTimeCodeNames();
        //    var map = new Dictionary<string, string[]>
        //    {
        //        [CountryLevelId] = countries,
        //        [PlaLevelId] = plas,
        //        [WgLevelId] = warrantyGroups,
        //        //[RoleCodeKey] = roleCodes,
        //        [ServiceLocationKey] = serviceLocations,
        //        [ReactionTimeKey] = reactionTimes
        //    };

        //    var countryLevelMeta = (NamedEntityMeta)this.entityMetas.GetEntityMeta(CountryLevelId, MetaConstants.InputLevelSchema);
        //    var firtsCountryQuery = this.BuildSelectIdByNameQuery(countryLevelMeta, countries[0], "Country_0");

        //    var inputLevels = new HashSet<string> { CountryLevelId, PlaLevelId, WgLevelId };
        //    var costBlocks = 
        //        this.entityMetas.CostBlocks.Where(
        //            costBlock => costBlock.InputLevelFields.All(field => inputLevels.Contains(field.Name)));

        //    foreach (var costBlockMeta in costBlocks)
        //    {
        //        var fieldNames = 
        //            map.Keys.Where(fieldName => costBlockMeta.AllFields.Any(costBlockField => costBlockField.Name == fieldName))
        //                    .ToArray();

        //        var insertValues = new ISqlBuilder[warrantyGroups.Length, fieldNames.Length];

        //        for (var warrantyGroupIndex = 0; warrantyGroupIndex < warrantyGroups.Length; warrantyGroupIndex++)
        //        {
        //            insertValues[warrantyGroupIndex, 0] = firtsCountryQuery;

        //            for (var fieldIndex = 1; fieldIndex < fieldNames.Length; fieldIndex++)
        //            {
        //                var fieldName = fieldNames[fieldIndex];
        //                var refNames = map[fieldName];
        //                var refNameIndex = warrantyGroupIndex - warrantyGroupIndex / refNames.Length * refNames.Length;
        //                var refName = refNames[refNameIndex];

        //                var refField = costBlockMeta.InputLevelFields[fieldName] ?? (ReferenceFieldMeta)costBlockMeta.DependencyFields[fieldName];

        //                insertValues[warrantyGroupIndex, fieldIndex] = 
        //                    this.BuildSelectIdByNameQuery((NamedEntityMeta)refField.ReferenceMeta, refName, $"{fieldName}_{warrantyGroupIndex}");
        //            }
        //        }

        //        yield return Sql.Insert(costBlockMeta, fieldNames).Values(insertValues);

        //        var columns = new List<ColumnInfo>
        //        {
        //            new ColumnInfo(countryLevelMeta.IdField.Name, countryLevelMeta.Name)
        //        };
        //        columns.AddRange(
        //            fieldNames.Where(field => field != CountryLevelId)
        //                      .Select(fieldName => new ColumnInfo(fieldName, costBlockMeta.Name)));

        //        yield return
        //            Sql.Insert(costBlockMeta, fieldNames)
        //               .Query(
        //                    Sql.Select(columns.ToArray())
        //                       .From(countryLevelMeta)
        //                       .Join(
        //                            costBlockMeta,
        //                            SqlOperators.Equals(
        //                                new ColumnSqlBuilder(new ColumnInfo(CountryLevelId, costBlockMeta.Name)),
        //                                firtsCountryQuery))
        //                       .Where(SqlOperators.NotEquals(countryLevelMeta.NameField.Name, "firstCountry", countries[0], countryLevelMeta.Name)));
        //    }
        //}

        //private ISqlBuilder BuildSelectIdByNameQuery(NamedEntityMeta meta, string name, string paramName)
        //{
        //    return new BracketsSqlBuilder
        //    {
        //        SqlBuilder = Sql.Select(new ColumnInfo { Name = meta.IdField.Name, TableName = meta.Name })
        //                        .From(meta)
        //                        .Where(SqlOperators.Equals(meta.NameField.Name, paramName, name, meta.Name))
        //                        .ToSqlBuilder()
        //    };
        //}

        private IEnumerable<SqlHelper> BuildInsertCostBlockSql()
        {
            foreach (var costBlockMeta in this.entityMetas.CostBlocks)
            {
                var referenceFields = costBlockMeta.AllFields.OfType<ReferenceFieldMeta>().ToList();
                var selectColumns =
                    referenceFields.Select(field => new ColumnInfo(field.ReferenceValueField, field.ReferenceMeta.Name, field.Name))
                                   .ToList();

                var insertFields = referenceFields.Select(field => field.Name).ToList();

                var wgField = costBlockMeta.InputLevelFields[WgLevelId];
                var plaField = costBlockMeta.InputLevelFields[PlaLevelId];

                if (plaField != null && wgField != null)
                {
                    selectColumns = 
                        selectColumns.Select(
                            field => field.TableName == plaField.Name 
                                ? new ColumnInfo("PlaId", WgLevelId, plaField.Name)
                                : field)
                                    .ToList();

                    referenceFields.Remove(plaField);
                }
                
                IJoinSqlHelper<SelectJoinSqlHelper> selectQuery = Sql.Select(selectColumns.ToArray()).From(referenceFields[0].ReferenceMeta);

                for (var i = 1; i < referenceFields.Count; i++)
                {
                    var referenceMeta = referenceFields[i].ReferenceMeta;

                    selectQuery = selectQuery.Join(referenceMeta.Schema, referenceMeta.Name, null, JoinType.Cross);
                }

                yield return Sql.Insert(costBlockMeta, insertFields.ToArray()).Query((SqlHelper)selectQuery);
            }
        }

        private SqlHelper BuildInsertReactionTimeTypeSql()
        {
            //2nd Business Day response
            //NBD response
            //4h response
            //NBD recovery
            //24h recovery
            //8h recovery
            //4h recovey

            var twoBdQuery = this.BuildSelectIdByNameQuery(ReactionTimeKey, "2nd Business Day");
            var nbdQuery = this.BuildSelectIdByNameQuery(ReactionTimeKey, "NBD");
            var fourHourQuery = this.BuildSelectIdByNameQuery(ReactionTimeKey, "4h");
            var twentyFourHourQuery = this.BuildSelectIdByNameQuery(ReactionTimeKey, "24h");
            var eightHourQuery = this.BuildSelectIdByNameQuery(ReactionTimeKey, "8h");

            var responseQuery = this.BuildSelectIdByNameQuery(ReactionTypeKey, "response");
            var recoveryQuery = this.BuildSelectIdByNameQuery(ReactionTypeKey, "recovery");

            return
                Sql.Insert(MetaConstants.DependencySchema, $"{ReactionTimeKey}_{ReactionTypeKey}", ReactionTimeKey, ReactionTypeKey)
                   .Values(new ISqlBuilder[,]
                   {
                       { twoBdQuery, responseQuery },
                       { nbdQuery, responseQuery },
                       { fourHourQuery, responseQuery },
                       { nbdQuery, recoveryQuery },
                       { twentyFourHourQuery, recoveryQuery },
                       { eightHourQuery, recoveryQuery },
                       { fourHourQuery, recoveryQuery },
                   });
        }

        private SqlHelper BuildInsertReactionTimeAvailabilitySql()
        {
            //NBD 9x5
            //4h 9x5
            //4h 24x7

            var nbdQuery = this.BuildSelectIdByNameQuery(ReactionTimeKey, "NBD");
            var fourHourQuery = this.BuildSelectIdByNameQuery(ReactionTimeKey, "4h");

            var nineByFive = this.BuildSelectIdByNameQuery(AvailabilityKey, "9x5");
            var twentyFourBySeven = this.BuildSelectIdByNameQuery(AvailabilityKey, "24x7"); 

            return
               Sql.Insert(MetaConstants.DependencySchema, $"{ReactionTimeKey}_{AvailabilityKey}", ReactionTimeKey, AvailabilityKey)
                  .Values(new ISqlBuilder[,]
                  {
                       { nbdQuery, nineByFive },
                       { fourHourQuery, nineByFive },
                       { fourHourQuery, twentyFourBySeven },
                  });
        }

        private ISqlBuilder BuildSelectIdByNameQuery(string table, string name)
        {
            var paramName = name.Replace(" ", string.Empty);

            return
                new BracketsSqlBuilder
                {
                    SqlBuilder =
                        Sql.Select(IdFieldMeta.DefaultId)
                           .From(table, MetaConstants.DependencySchema)
                           .Where(SqlOperators.Equals(MetaConstants.NameFieldKey, paramName, name))
                           .ToSqlBuilder()
                };
        }

        private Pla[] GetPlas()
        {
            var plaRepository = this.repositorySet.GetRepository<Pla>();

            return new Pla[]
            {
                new Pla
                {
                    Name = "Desktops",
                    WarrantyGroups = new List<Wg>
                    {
                        new Wg
                        {
                            Name = "TC4"
                        },
                        new Wg
                        {
                            Name = "TC5"
                        },
                        new Wg
                        {
                            Name = "TC6"
                        },
                        new Wg
                        {
                            Name = "TC8"
                        },
                        new Wg
                        {
                            Name = "TC7"
                        },
                        new Wg
                        {
                            Name = "U05"
                        },
                        new Wg
                        {
                            Name = "U11"
                        },
                        new Wg
                        {
                            Name = "U13"
                        },
                        new Wg
                        {
                            Name = "WSJ"
                        },
                        new Wg
                        {
                            Name = "WSN"
                        },
                        new Wg
                        {
                            Name = "WSS"
                        },
                        new Wg
                        {
                            Name = "WSW"
                        },
                        new Wg
                        {
                            Name = "U02"
                        },
                        new Wg
                        {
                            Name = "U06"
                        },
                        new Wg
                        {
                            Name = "U07"
                        },
                        new Wg
                        {
                            Name = "U12"
                        },
                        new Wg
                        {
                            Name = "U14"
                        },
                        new Wg
                        {
                            Name = "WRC"
                        },
                    }
                },
                new Pla
                {
                    Name = "Mobiles",
                    WarrantyGroups = new List<Wg>
                    {
                        new Wg
                        {
                            Name = "HMD"
                        },
                        new Wg
                        {
                            Name = "NB6"
                        },
                        new Wg
                        {
                            Name = "NB1"
                        },
                        new Wg
                        {
                            Name = "NB2"
                        },
                        new Wg
                        {
                            Name = "NB5"
                        },
                        new Wg
                        {
                            Name = "ND3"
                        },
                        new Wg
                        {
                            Name = "NC1"
                        },
                        new Wg
                        {
                            Name = "NC3"
                        },
                        new Wg
                        {
                            Name = "NC9"
                        },
                        new Wg
                        {
                            Name = "TR7"
                        },
                    }
                },
                new Pla
                {
                    Name = "Peripherals",
                    WarrantyGroups = new List<Wg>
                    {
                        new Wg
                        {
                            Name = "DPE"
                        },
                        new Wg
                        {
                            Name = "DPH"
                        },
                        new Wg
                        {
                            Name = "DPM"
                        },
                        new Wg
                        {
                            Name = "DPX"
                        },
                        new Wg
                        {
                            Name = "IOA"
                        },
                        new Wg
                        {
                            Name = "IOB"
                        },
                        new Wg
                        {
                            Name = "IOC"
                        },
                        new Wg
                        {
                            Name = "MD1"
                        },
                        new Wg
                        {
                            Name = "PSN"
                        },
                        new Wg
                        {
                            Name = "SB2"
                        },
                        new Wg
                        {
                            Name = "SB3"
                        },
                    }
                },
                new Pla
                {
                    Name = "Storage Products",
                    WarrantyGroups = new List<Wg>
                    {
                        new Wg
                        {
                            Name = "CD1"
                        },
                        new Wg
                        {
                            Name = "CD2"
                        },
                        new Wg
                        {
                            Name = "CE1"
                        },
                        new Wg
                        {
                            Name = "CE2"
                        },
                        new Wg
                        {
                            Name = "CD4"
                        },
                        new Wg
                        {
                            Name = "CD5"
                        },
                        new Wg
                        {
                            Name = "CD6"
                        },
                        new Wg
                        {
                            Name = "CD7"
                        },
                        new Wg
                        {
                            Name = "CDD"
                        },
                        new Wg
                        {
                            Name = "CD8"
                        },
                        new Wg
                        {
                            Name = "CD9"
                        },
                        new Wg
                        {
                            Name = "C70"
                        },
                        new Wg
                        {
                            Name = "CS8"
                        },
                        new Wg
                        {
                            Name = "C74"
                        },
                        new Wg
                        {
                            Name = "C75"
                        },
                        new Wg
                        {
                            Name = "CS7"
                        },
                        new Wg
                        {
                            Name = "CS1"
                        },
                        new Wg
                        {
                            Name = "CS2"
                        },
                        new Wg
                        {
                            Name = "CS3"
                        },
                        new Wg
                        {
                            Name = "C16"
                        },
                        new Wg
                        {
                            Name = "C18"
                        },
                        new Wg
                        {
                            Name = "C33"
                        },
                        new Wg
                        {
                            Name = "CS5"
                        },
                        new Wg
                        {
                            Name = "CS4"
                        },
                        new Wg
                        {
                            Name = "CS6"
                        },
                        new Wg
                        {
                            Name = "CS9"
                        },
                        new Wg
                        {
                            Name = "C96"
                        },
                        new Wg
                        {
                            Name = "C97"
                        },
                        new Wg
                        {
                            Name = "C98"
                        },
                        new Wg
                        {
                            Name = "C71"
                        },
                        new Wg
                        {
                            Name = "C73"
                        },
                        new Wg
                        {
                            Name = "C80"
                        },
                        new Wg
                        {
                            Name = "C84"
                        },
                        new Wg
                        {
                            Name = "F58"
                        },
                        new Wg
                        {
                            Name = "F40"
                        },
                        new Wg
                        {
                            Name = "F48"
                        },
                        new Wg
                        {
                            Name = "F53"
                        },
                        new Wg
                        {
                            Name = "F54"
                        },
                        new Wg
                        {
                            Name = "F57"
                        },
                        new Wg
                        {
                            Name = "F41"
                        },
                        new Wg
                        {
                            Name = "F49"
                        },
                        new Wg
                        {
                            Name = "F42"
                        },
                        new Wg
                        {
                            Name = "F43"
                        },
                        new Wg
                        {
                            Name = "F44"
                        },
                        new Wg
                        {
                            Name = "F45"
                        },
                        new Wg
                        {
                            Name = "F50"
                        },
                        new Wg
                        {
                            Name = "F51"
                        },
                        new Wg
                        {
                            Name = "F52"
                        },
                        new Wg
                        {
                            Name = "F36"
                        },
                        new Wg
                        {
                            Name = "F46"
                        },
                        new Wg
                        {
                            Name = "F47"
                        },
                        new Wg
                        {
                            Name = "F56"
                        },
                        new Wg
                        {
                            Name = "F28"
                        },
                        new Wg
                        {
                            Name = "F29"
                        },
                        new Wg
                        {
                            Name = "F35"
                        },
                        new Wg
                        {
                            Name = "F55"
                        },
                        new Wg
                        {
                            Name = "S14"
                        },
                        new Wg
                        {
                            Name = "S17"
                        },
                        new Wg
                        {
                            Name = "S15"
                        },
                        new Wg
                        {
                            Name = "S16"
                        },
                        new Wg
                        {
                            Name = "S50"
                        },
                        new Wg
                        {
                            Name = "S51"
                        },
                        new Wg
                        {
                            Name = "S18"
                        },
                        new Wg
                        {
                            Name = "S35"
                        },
                        new Wg
                        {
                            Name = "S36"
                        },
                        new Wg
                        {
                            Name = "S37"
                        },
                        new Wg
                        {
                            Name = "S39"
                        },
                        new Wg
                        {
                            Name = "S40"
                        },
                        new Wg
                        {
                            Name = "S55"
                        },
                        new Wg
                        {
                            Name = "VSH"
                        },
                    }
                },
                new Pla
                {
                    Name = "x86/IA Servers",
                    WarrantyGroups = new List<Wg>
                    {
                        new Wg
                        {
                            Name = "MN1"
                        },
                        new Wg
                        {
                            Name = "MN4"
                        },
                        new Wg
                        {
                            Name = "PQ8"
                        },
                        new Wg
                        {
                            Name = "Y01"
                        },
                        new Wg
                        {
                            Name = "Y15"
                        },
                        new Wg
                        {
                            Name = "PX1"
                        },
                        new Wg
                        {
                            Name = "PY1"
                        },
                        new Wg
                        {
                            Name = "PY4"
                        },
                        new Wg
                        {
                            Name = "Y09"
                        },
                        new Wg
                        {
                            Name = "Y12"
                        },
                        new Wg
                        {
                            Name = "MN2"
                        },
                        new Wg
                        {
                            Name = "PX2"
                        },
                        new Wg
                        {
                            Name = "PX3"
                        },
                        new Wg
                        {
                            Name = "PXS"
                        },
                        new Wg
                        {
                            Name = "PY2"
                        },
                        new Wg
                        {
                            Name = "PY3"
                        },
                        new Wg
                        {
                            Name = "SD2"
                        },
                        new Wg
                        {
                            Name = "Y03"
                        },
                        new Wg
                        {
                            Name = "Y17"
                        },
                        new Wg
                        {
                            Name = "Y21"
                        },
                        new Wg
                        {
                            Name = "Y32"
                        },
                        new Wg
                        {
                            Name = "Y06"
                        },
                        new Wg
                        {
                            Name = "Y13"
                        },
                        new Wg
                        {
                            Name = "Y28"
                        },
                        new Wg
                        {
                            Name = "Y30"
                        },
                        new Wg
                        {
                            Name = "Y31"
                        },
                        new Wg
                        {
                            Name = "Y37"
                        },
                        new Wg
                        {
                            Name = "Y38"
                        },
                        new Wg
                        {
                            Name = "Y39"
                        },
                        new Wg
                        {
                            Name = "Y40"
                        },
                        new Wg
                        {
                            Name = "PX6"
                        },
                        new Wg
                        {
                            Name = "PX8"
                        },
                        new Wg
                        {
                            Name = "PRC"
                        },
                        new Wg
                        {
                            Name = "RTE"
                        },
                        new Wg
                        {
                            Name = "Y07"
                        },
                        new Wg
                        {
                            Name = "Y16"
                        },
                        new Wg
                        {
                            Name = "Y18"
                        },
                        new Wg
                        {
                            Name = "Y25"
                        },
                        new Wg
                        {
                            Name = "Y26"
                        },
                        new Wg
                        {
                            Name = "Y27"
                        },
                        new Wg
                        {
                            Name = "Y33"
                        },
                        new Wg
                        {
                            Name = "Y36"
                        },
                        new Wg
                        {
                            Name = "S41"
                        },
                        new Wg
                        {
                            Name = "S42"
                        },
                        new Wg
                        {
                            Name = "S43"
                        },
                        new Wg
                        {
                            Name = "S44"
                        },
                        new Wg
                        {
                            Name = "S45"
                        },
                        new Wg
                        {
                            Name = "S46"
                        },
                        new Wg
                        {
                            Name = "S47"
                        },
                        new Wg
                        {
                            Name = "S48"
                        },
                        new Wg
                        {
                            Name = "S49"
                        },
                        new Wg
                        {
                            Name = "S52"
                        },
                        new Wg
                        {
                            Name = "S53"
                        },
                        new Wg
                        {
                            Name = "S54"
                        },
                        new Wg
                        {
                            Name = "PQ0"
                        },
                        new Wg
                        {
                            Name = "PQ5"
                        },
                        new Wg
                        {
                            Name = "PQ9"
                        },
                    }
                },
            };
        }

        private void CreatePlas()
        {
            var plas = this.GetPlas();
            var repository = this.repositorySet.GetRepository<Pla>();

            repository.Save(plas);
            this.repositorySet.Sync();
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
                "Japan",
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

        //private string[] GetPlaNames()
        //{
        //    return new[]
        //    {
        //        "Desktops",
        //        "Mobiles",
        //        "Peripherals",
        //        "Storage Products",
        //        "x86/IA Servers"
        //    };
        //}

        //private string[] GetWarrantyGroupNames()
        //{
        //    return new string[]
        //    {
        //        "TC4",
        //        "TC5",
        //        "TC6",
        //        "TC8",
        //        "TC7",
        //        "TCL",
        //        "U05",
        //        "U11",
        //        "U13",
        //        "WSJ",
        //        "WSN",
        //        "WSS",
        //        "WSW",
        //        "U02",
        //        "U06",
        //        "U07",
        //        "U12",
        //        "U14",
        //        "WRC",
        //        "HMD",
        //        "NB6",
        //        "NB1",
        //        "NB2",
        //        "NB5",
        //        "ND3",
        //        "NC1",
        //        "NC3",
        //        "NC9",
        //        "TR7",
        //        "DPE",
        //        "DPH",
        //        "DPM",
        //        "DPX",
        //        "IOA",
        //        "IOB",
        //        "IOC",
        //        "MD1",
        //        "PSN",
        //        "SB2",
        //        "SB3",
        //        "CD1",
        //        "CD2",
        //        "CE1",
        //        "CE2",
        //        "CD4",
        //        "CD5",
        //        "CD6",
        //        "CD7",
        //        "CDD",
        //        "CD8",
        //        "CD9",
        //        "C70",
        //        "CS8",
        //        "C74",
        //        "C75",
        //        "CS7",
        //        "CS1",
        //        "CS2",
        //        "CS3",
        //        "C16",
        //        "C18",
        //        "C33",
        //        "CS5",
        //        "CS4",
        //        "CS6",
        //        "CS9",
        //        "C96",
        //        "C97",
        //        "C98",
        //        "C71",
        //        "C73",
        //        "C80",
        //        "C84",
        //        "F58",
        //        "F40",
        //        "F48",
        //        "F53",
        //        "F54",
        //        "F57",
        //        "F41",
        //        "F49",
        //        "F42",
        //        "F43",
        //        "F44",
        //        "F45",
        //        "F50",
        //        "F51",
        //        "F52",
        //        "F36",
        //        "F46",
        //        "F47",
        //        "F56",
        //        "F28",
        //        "F29",
        //        "F35",
        //        "F55",
        //        "S14",
        //        "S17",
        //        "S15",
        //        "S16",
        //        "S50",
        //        "S51",
        //        "S18",
        //        "S35",
        //        "S36",
        //        "S37",
        //        "S39",
        //        "S40",
        //        "S55",
        //        "VSH",
        //        "MN1",
        //        "MN4",
        //        "PQ8",
        //        "Y01",
        //        "Y15",
        //        "PX1",
        //        "PY1",
        //        "PY4",
        //        "Y09",
        //        "Y12",
        //        "MN2",
        //        "MN3",
        //        "PX2",
        //        "PX3",
        //        "PXS",
        //        "PY2",
        //        "PY3",
        //        "SD2",
        //        "Y03",
        //        "Y17",
        //        "Y21",
        //        "Y32",
        //        "Y06",
        //        "Y13",
        //        "Y28",
        //        "Y30",
        //        "Y31",
        //        "Y37",
        //        "Y38",
        //        "Y39",
        //        "Y40",
        //        "PX6",
        //        "PX8",
        //        "PRC",
        //        "RTE",
        //        "Y07",
        //        "Y16",
        //        "Y18",
        //        "Y25",
        //        "Y26",
        //        "Y27",
        //        "Y33",
        //        "Y36",
        //        "S41",
        //        "S42",
        //        "S43",
        //        "S44",
        //        "S45",
        //        "S46",
        //        "S47",
        //        "S48",
        //        "S49",
        //        "S52",
        //        "S53",
        //        "S54",
        //        "PQ0",
        //        "PQ5",
        //        "PQ9"
        //    };
        //}

        //private string[] GetRoleCodeNames()
        //{
        //    return new string[]
        //    {
        //        "SEFS05",
        //        "SEFS06",
        //        "SEFS04",
        //        "SEIE07",
        //        "SEIE08"
        //    };
        //}

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
                "2nd Business Day",
                "NBD",
                "24h",
                "8h",
                "4h"
            };
        }

        private string[] GetReactionTypeNames()
        {
            return new[]
            {
                "response",
                "recovery"
            };
        }

        private string[] GetYearNames()
        {
            return new[]
            {
                "1st year",
                "2nd year",
                "3rd year",
                "4th year",
                "5th year",
                "1 year prolongation"
            };
        }

        private string[] GetCurrenciesNames()
        {
            return new[]
            {
                "EUR",
                "USD"
            };
        }

        private string[] GetAvailabilityNames()
        {
            return new string[]
            {
                "9x5",
                "24x7"
            };
        }

        private bool GenerateRandomBool()
        {
            Random gen = new Random();
            int prob = gen.Next(100);
            return prob <= 70;
        }
    }
}
