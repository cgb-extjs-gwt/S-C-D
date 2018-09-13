using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Helpers;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Impl;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Gdc.Scd.DataAccessLayer.TestData.Impl
{
    public class TestDataCreationHandlercs : IConfigureDatabaseHandler
    {
        private const string CountryLevelId = "Country";

        private const string PlaLevelId = "Pla";

        private const string WgLevelId = "Wg";

        private const string RoleCodeKey = "RoleCode";

        private const string ServiceLocationKey = "ServiceLocation";

        private const string YearKey = "Year";

        private const string ReactionTimeKey = "ReactionTime";

        private const string ReactionTypeKey = "ReactionType";

        private const string AvailabilityKey = "Availability";

        private const string DurationKey = "Duration";

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
            this.CreateClusterPlas();
            this.CreateUsers();
            this.CreateReactionTimeTypeAvalability();
            this.CreateClusterRegions();
            this.CreateCurrenciesAndExchangeRates();
            this.CreateCountries();
            this.CreateDurations();
            this.CreateYearAvailability();
            this.CreateProActiveSla();
            this.CreateTestItems<SwDigit>();
            this.CreateTestItems<Sog>();

            var plaInputLevelMeta = (NamedEntityMeta)this.entityMetas.GetEntityMeta(PlaLevelId, MetaConstants.InputLevelSchema);
            var wgInputLevelMeta = (NamedEntityMeta)this.entityMetas.GetEntityMeta(WgLevelId, MetaConstants.InputLevelSchema);

            var queries = new List<SqlHelper>
            {
                this.BuildInsertSql(MetaConstants.InputLevelSchema, RoleCodeKey, this.GetRoleCodeNames()),
                this.BuildInsertSql(MetaConstants.DependencySchema, ServiceLocationKey, this.GetServiceLocationCodeNames()),
            };
            queries.AddRange(this.BuildInsertCostBlockSql());
            queries.AddRange(this.BuildFromFile(@"Scripts\matrix.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts\availabilityFee.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts\calculation-hw.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts\calculation-sw.sql"));

            foreach (var query in queries)
            {
                this.repositorySet.ExecuteSql(query);
            }
        }

        private void CreateCountries()
        {
            var countryGroups = new List<CountryGroup>();

            CountryGroup countryGroup = null;

            foreach (var country in this.GetCountries())
            {
                if (countryGroup == null || countryGroup.Countries.Count % 5 == 0)
                {
                    countryGroup = new CountryGroup
                    {
                        Name = $"CountryGroup_{countryGroups.Count}",
                        Countries = new List<Country>()
                    };

                    countryGroups.Add(countryGroup);
                }

                countryGroup.Countries.Add(country);
            }

            this.repositorySet.GetRepository<CountryGroup>().Save(countryGroups);
            this.repositorySet.Sync();
        }

        private void CreateYearAvailability()
        {
            var years = this.GetYears();
            var availabilities = this.repositorySet.GetRepository<Availability>().GetAll().ToArray();
            var yearAvailabilityRepository = this.repositorySet.GetRepository<YearAvailability>();

            foreach (var availability in availabilities)
            {
                foreach (var year in years)
                {
                    yearAvailabilityRepository.Save(new YearAvailability
                    {
                        Availability = availability,
                        Year = year
                    });
                }
            }

            repositorySet.Sync();
        }

        private void CreateDurations()
        {
            //Insert Durations
            var durationRepository = repositorySet.GetRepository<Duration>();
            durationRepository.Save(GetDurations());
            repositorySet.Sync();
        }

        private void CreateClusterRegions()
        {
            //Insert Cluster Regions
            var clusterRegionsRepository = repositorySet.GetRepository<ClusterRegion>();
            clusterRegionsRepository.Save(GetClusterRegions());
            repositorySet.Sync();
        }

        private void CreateUsers()
        {
            var repository = this.repositorySet.GetRepository<User>();
            var user = new User { Name = "Test user" };

            repository.Save(user);
            this.repositorySet.Sync();
        }

        private void CreateProActiveSla()
        {
            this.repositorySet.GetRepository<ProActiveSla>().Save(new ProActiveSla[]
            {
                new ProActiveSla { Name = "0", Value = 0 },
                new ProActiveSla { Name = "2", Value = 2 },
                new ProActiveSla { Name = "3", Value = 3 },
                new ProActiveSla { Name = "4", Value = 4 },
                new ProActiveSla { Name = "6", Value = 6 },
                new ProActiveSla { Name = "7", Value = 7 }
            });

            this.repositorySet.Sync();
        }

        private void CreateTestItems<T>(int count = 5) where T : NamedId, new()
        {
            var items = new List<T>();
            var typeName = typeof(T).Name;


            for (var i = 0; i < count; i++)
            {
                items.Add(new T
                {
                    Name = $"{typeName}_{i}"
                });
            }

            this.repositorySet.GetRepository<T>().Save(items);
            this.repositorySet.Sync();
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
            foreach (var costBlockMeta in this.entityMetas.CostBlocks)
            {
                var referenceFields = costBlockMeta.InputLevelFields.Concat(costBlockMeta.DependencyFields).ToList();
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

                var selectQuery = Sql.Select(selectColumns.ToArray()).From(referenceFields[0].ReferenceMeta);

                for (var i = 1; i < referenceFields.Count; i++)
                {
                    var referenceMeta = referenceFields[i].ReferenceMeta;

                    selectQuery = selectQuery.Join(referenceMeta.Schema, referenceMeta.Name, null, JoinType.Cross);
                }

                yield return Sql.Insert(costBlockMeta, insertFields.ToArray()).Query(selectQuery);
            }
        }

        private void CreateReactionTimeTypeAvalability()
        {
            var twoBusinessDay = new ReactionTime { Name = "2nd Business Day" };
            var nbd = new ReactionTime { Name = "NBD" };
            var fourHour = new ReactionTime { Name = "4h" };
            var twentyFourHour = new ReactionTime { Name = "24h" };
            var eightHour = new ReactionTime { Name = "8h" };

            var response = new ReactionType { Name = "response" };
            var recovery = new ReactionType { Name = "recovery" };

            this.repositorySet.GetRepository<ReactionTimeType>().Save(new List<ReactionTimeType>
            {
                new ReactionTimeType { ReactionTime = twoBusinessDay, ReactionType = response },
                new ReactionTimeType { ReactionTime = nbd, ReactionType = response },
                new ReactionTimeType { ReactionTime = fourHour, ReactionType = response },
                new ReactionTimeType { ReactionTime = nbd, ReactionType = recovery },
                new ReactionTimeType { ReactionTime = twentyFourHour, ReactionType = recovery },
                new ReactionTimeType { ReactionTime = eightHour, ReactionType = recovery },
                new ReactionTimeType { ReactionTime = fourHour, ReactionType = recovery },
            });

            var nineByFive = new Availability { Name = "9x5" };
            var twentyFourBySeven = new Availability { Name = "24x7" };

            var reactionTimeAvalabilities = new List<ReactionTimeAvalability>
            {
                new ReactionTimeAvalability { ReactionTime = nbd, Availability = nineByFive },
                new ReactionTimeAvalability { ReactionTime = fourHour, Availability = nineByFive },
                new ReactionTimeAvalability { ReactionTime = fourHour, Availability = twentyFourBySeven },
            };

            this.repositorySet.GetRepository<ReactionTimeAvalability>().Save(reactionTimeAvalabilities);

            var reactionTypes = new List<ReactionType> { response, recovery };
            var reactionTimeTypeAvalabilities = new List<ReactionTimeTypeAvalability>();

            foreach (var reactionType in reactionTypes)
            {
                foreach (var reactionTimeAvalability in reactionTimeAvalabilities)
                {
                    reactionTimeTypeAvalabilities.Add(new ReactionTimeTypeAvalability
                    {
                        ReactionType = reactionType,
                        ReactionTime = reactionTimeAvalability.ReactionTime,
                        Availability = reactionTimeAvalability.Availability
                    });
                }
            }

            this.repositorySet.GetRepository<ReactionTimeTypeAvalability>().Save(reactionTimeTypeAvalabilities);

            this.repositorySet.Sync();
        }

        private void CreateCurrenciesAndExchangeRates()
        {
            var curs = GetCurrencies();

            var curRepo = repositorySet.GetRepository<Currency>();
            curRepo.Save(curs);
            repositorySet.Sync();

            var eur = Array.Find(curs, x => string.Equals(x.Name, "EUR", StringComparison.InvariantCultureIgnoreCase));
            var usd = Array.Find(curs, x => string.Equals(x.Name, "USD", StringComparison.InvariantCultureIgnoreCase));

            var exRepo = repositorySet.GetRepository<ExchangeRate>();

            exRepo.Save(new ExchangeRate { Currency = eur, Value = 1 });
            exRepo.Save(new ExchangeRate { Currency = usd, Value = 1.2 });
            repositorySet.Sync();
        }

        private ISqlBuilder BuildSelectIdByNameQuery(string table, string name)
        {
            var paramName = name.Replace(" ", string.Empty);

            return
                new BracketsSqlBuilder
                {
                    Query =
                        Sql.Select(IdFieldMeta.DefaultId)
                           .From(table, MetaConstants.DependencySchema)
                           .Where(SqlOperators.Equals(MetaConstants.NameFieldKey, paramName, name))
                           .ToSqlBuilder()
                };
        }

        private IEnumerable<SqlHelper> BuildFromFile(string fn)
        {
            return Regex.Split(ReadText(fn), "go", RegexOptions.IgnoreCase)
                               .Where(x => !string.IsNullOrWhiteSpace(x))
                               .Select(x => new SqlHelper(new RawSqlBuilder() { RawSql = x }));
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

        private void CreateClusterPlas()
        {
            var plas = this.GetPlas();
            var clusterPlas = new List<ClusterPla>();

            ClusterPla clusterPla = null;

            for (var i = 0; i < plas.Length; i++)
            {
                if (i % 2 == 0)
                {
                    clusterPla = new ClusterPla
                    {
                        Name = $"ClusterPla_{i}",
                        Plas = new List<Pla>()
                    };

                    clusterPlas.Add(clusterPla);
                }

                clusterPla.Plas.Add(plas[i]);
            }

            this.repositorySet.GetRepository<ClusterPla>().Save(clusterPlas);
            this.repositorySet.Sync();
        }

        private RoleCode[] GetRoleCodes()
        {
            return new RoleCode[]
            {
                new RoleCode
                {
                    Name = "SEFS05"

                }
            };
        }

        private void CreateRolecodes()
        {
            var roleCodes = this.GetRoleCodes();
            var repository = this.repositorySet.GetRepository<RoleCode>();

            repository.Save(roleCodes);
            this.repositorySet.Sync();
        }

        private Country[] GetCountries()
        {
            var names = new[]
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

            var len = names.Length;
            var result = new Country[len];

            var eur = repositorySet.GetRepository<Currency>()
                                       .GetAll()
                                       .First(x => x.Name.ToUpper() == "EUR");

            for (var i = 0; i < len; i++)
            {
                result[i] = new Country
                {
                    Name = names[i],
                    CanOverrideListAndDealerPrices = GenerateRandomBool(),
                    CanOverrideTransferCostAndPrice = GenerateRandomBool(),
                    ShowDealerPrice = GenerateRandomBool(),
                    ClusterRegionId = 2,
                    Currency = eur
                };
            }

            return result;
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

        private Year[] GetYears()
        {
            return new Year[]
            {
                new Year { Name = "1st year", Value = 1, IsProlongation = false },
                new Year { Name = "2nd year", Value = 2, IsProlongation = false },
                new Year { Name = "3rd year", Value = 3, IsProlongation = false },
                new Year { Name = "4th year", Value = 4, IsProlongation = false },
                new Year { Name = "5th year", Value = 5, IsProlongation = false },
                new Year { Name = "1 year prolongation", Value = 1, IsProlongation = true }
            };
        }

        private ClusterRegion[] GetClusterRegions()
        {
            return new ClusterRegion[]
            {
                new ClusterRegion { Name = "Asia" },
                new ClusterRegion { Name = "EMEIA" },
                new ClusterRegion { Name = "Japan" },
                new ClusterRegion { Name = "Latin America" },
                new ClusterRegion { Name = "Oceania" },
                new ClusterRegion { Name = "United States" }
            };
        }

        private Currency[] GetCurrencies()
        {
            return new Currency[]
            {
                new Currency { Name =  "EUR" },
                new Currency { Name =  "USD" }
            };
        }

        private Duration[] GetDurations()
        {
            return new Duration[]
            {
                new Duration { Name = "3 Years", Value = 3, IsProlongation = false },
                new Duration { Name = "4 Years", Value = 4, IsProlongation = false },
                new Duration { Name = "5 Years", Value = 5, IsProlongation = false },
                new Duration { Name = "Prolongation", Value = 1, IsProlongation = true }
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
                "SEIE08",
            };
        }

        private bool GenerateRandomBool()
        {
            Random gen = new Random();
            int prob = gen.Next(100);
            return prob <= 70;
        }

        private string ReadText(string fn)
        {
            string root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            fn = Path.Combine(root, fn);
            return File.ReadAllText(fn);
        }
    }
}
