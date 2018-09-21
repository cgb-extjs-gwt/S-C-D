using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Meta.Constants;
using Gdc.Scd.Core.Meta.Entities;
using Gdc.Scd.DataAccessLayer.Impl;
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

        private const string ClusterRegionId = "ClusterRegion";

        private const string RoleCodeKey = "RoleCode";

        private const string ServiceLocationKey = "ServiceLocation";

        private const string YearKey = "Year";

        private const string ReactionTimeKey = "ReactionTime";

        private const string ReactionTypeKey = "ReactionType";

        private const string AvailabilityKey = "Availability";

        private const string DurationKey = "Duration";

        private readonly EntityFrameworkRepositorySet repositorySet;

        private const string ProActiveKey = "ProActive";

        private const string ProActiveSlaKey = "ProActiveSla";

        private readonly DomainEnitiesMeta entityMetas;

        public TestDataCreationHandlercs(
                DomainEnitiesMeta entityMetas,
                EntityFrameworkRepositorySet repositorySet
            )
        {
            this.entityMetas = entityMetas;
            this.repositorySet = repositorySet;
        }

        public void Handle()
        {
            this.CreateClusterPlas();
            this.CreateServiceLocations();
            this.CreateUsers();
            this.CreateRoles();
            this.CreateReactionTimeTypeAvalability();
            this.CreateClusterRegions();
            this.CreateCurrenciesAndExchangeRates();
            this.CreateDurations();
            this.CreateYearAvailability();
            this.CreateProActiveSla();
            this.CreateRolecodes();

            var queries = new List<SqlHelper>();
            queries.AddRange(this.BuildInsertCostBlockSql());
            queries.AddRange(this.BuildFromFile(@"Scripts.insert-countries.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.matrix.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.availabilityFee.sql"));
            //queries.AddRange(this.BuildFromFile(@"Scripts.calculation-hw.sql"));
            //queries.AddRange(this.BuildFromFile(@"Scripts.calculation-sw.sql"));
        }

        private void CreateServiceLocations()
        {
            var repo = repositorySet.GetRepository<ServiceLocation>();
            repo.Save(GetServiceLocations());
            repositorySet.Sync();
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
            var users = new List<User> {
                new User { Name = "Test user 1", Login="g02\testUser1", Email="testuser1@fujitsu.com" },
                new User { Name = "Test user 2", Login="g03\testUser2", Email="testuser2@fujitsu.com" },
                new User { Name = "Test user 3", Login="g04\testUser3", Email="testuser3@fujitsu.com" }
            };

            repository.Save(users);
            this.repositorySet.Sync();
        }

        private void CreateRoles()
        {
            var repository = this.repositorySet.GetRepository<Role>();
            var roles = new List<Role> {
                new Role {Name = "Test Role 1", IsGlobal=true},
                new Role {Name = "Test Role 2", IsGlobal=true },
                new Role {Name = "Test Role 3", IsGlobal=false },
                new Role {Name = "Test Role 4", IsGlobal=false },
                new Role {Name = "Test Role 5", IsGlobal=false }
            };
            repository.Save(roles);
            this.repositorySet.Sync();
        }

        private void CreateProActiveSla()
        {
            this.repositorySet.GetRepository<ProActiveSla>().Save(new ProActiveSla[]
            {
                new ProActiveSla { Name = "0", ExternalName = "none" },
                new ProActiveSla { Name = "1", ExternalName = "with autocall" },
                new ProActiveSla { Name = "2", ExternalName = "with 1x System Health Check & Patch Information incl. remote Technical Account Management (per year)" },
                new ProActiveSla { Name = "3", ExternalName = "with 2x System Health Check & Patch Information incl. remote Technical Account Management (per year)" },
                new ProActiveSla { Name = "4", ExternalName = "with 4x System Health Check & Patch Information incl. remote Technical Account Management (per year)" },
                new ProActiveSla { Name = "6", ExternalName = "with 2x System Health Check & Patch Information incl. onsite Technical Account Management (per year)" },
                new ProActiveSla { Name = "7", ExternalName = "with 4x System Health Check & Patch Information incl. onsite Technical Account Management (per year)" },
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

                var wgField = costBlockMeta.InputLevelFields[MetaConstants.WgInputLevelName];
                var plaField = costBlockMeta.InputLevelFields[PlaLevelId];

                if (plaField != null && wgField != null)
                {
                    selectColumns =
                        selectColumns.Select(
                            field => field.TableName == plaField.Name
                                ? new ColumnInfo($"{nameof(Pla)}{nameof(Wg.Id)}", MetaConstants.WgInputLevelName, plaField.Name)
                                : field)
                                    .ToList();

                    referenceFields.Remove(plaField);
                }

                var clusterRegionField = costBlockMeta.InputLevelFields[ClusterRegionId];
                var countryField = costBlockMeta.InputLevelFields[MetaConstants.CountryInputLevelName];

                if (clusterRegionField != null && countryField != null)
                {
                    selectColumns =
                        selectColumns.Select(
                            field => field.TableName == clusterRegionField.Name
                                ? new ColumnInfo(nameof(Country.ClusterRegionId), MetaConstants.CountryInputLevelName, ClusterRegionId)
                                : field)
                                    .ToList();

                    referenceFields.Remove(clusterRegionField);
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
            var twoBusinessDay = new ReactionTime { Name = "2nd Business Day", ExternalName = "SBD" };
            var nbd = new ReactionTime { Name = "NBD", ExternalName = "NBD" };
            var fourHour = new ReactionTime { Name = "4h", ExternalName = "4h" };
            var twentyFourHour = new ReactionTime { Name = "24h", ExternalName = "24h" };
            var eightHour = new ReactionTime { Name = "8h", ExternalName = "8h" };
            var noneTime = new ReactionTime { Name = "none", ExternalName = "none" };

            var response = new ReactionType { Name = "response", ExternalName = "response" };
            var recovery = new ReactionType { Name = "recovery", ExternalName = "recovery" };
            var noneType = new ReactionType { Name = "none", ExternalName = "none" };

            this.repositorySet.GetRepository<ReactionType>().Save(noneType);
            this.repositorySet.GetRepository<ReactionTime>().Save(noneTime);

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

            var nineByFive = new Availability { Name = "9x5", ExternalName = "9x5 (local business hours);9x5 (08:00-17:00)" };
            var twentyFourBySeven = new Availability { Name = "24x7", ExternalName = "24x7" };

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
            return Regex.Split(ReadText(fn), @"[\r\n]+go[\s]*[\r\n]*", RegexOptions.IgnoreCase)
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
                    Name = "DESKTOP AND WORKSTATION",
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
                    Name = "NOTEBOOK AND TABLET",
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
                    Name = "PERIPHERALS",
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
                    Name = "STORAGE PRODUCTS",
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
                    Name = "X86 / IA SERVER",
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
                new Pla { Name = "EPS MAINFRAME PRODUCTS"},
                new Pla { Name = "RETAIL PRODUCTS"},
                new Pla { Name = "UNIX SERVER" }
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

        private void CreateRolecodes()
        {
            var roleCodes = new RoleCode[] { new RoleCode { Name = "SEFS05" } };

            var repository = this.repositorySet.GetRepository<RoleCode>();

            repository.Save(roleCodes);
            this.repositorySet.Sync();
        }

        private List<ServiceLocation> GetServiceLocations()
        {
            return new List<ServiceLocation>
            {
                new ServiceLocation {Name = "Material/Spares Service", ExternalName = "Material/Spares" },
                new ServiceLocation {Name = "Bring-In Service", ExternalName = "Bring-In" },
                new ServiceLocation {Name = "Send-In / Return-to-Base Service", ExternalName = "Send-In/Return-to-Base Service" },
                new ServiceLocation {Name = "Collect & Return Service", ExternalName = "Collect & Return" },
                new ServiceLocation {Name = "Collect & Return-Display Service", ExternalName = "Collect & Return-Display Service" },
                new ServiceLocation {Name = "Door-to-Door Exchange Service", ExternalName = "Door-to-Door Exchange" },
                new ServiceLocation {Name = "Desk-to-Desk Exchange Service", ExternalName = "Desk-to-Desk Exchange" },
                new ServiceLocation {Name = "On-Site Service", ExternalName = "On-Site Service" },
                new ServiceLocation {Name = "On-Site Exchange Service", ExternalName = "On-Site Exchange" },
                new ServiceLocation {Name = "Remote", ExternalName = "Remote Service" },

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
                new Duration { Name = "1 Year", Value = 1, IsProlongation = false, ExternalName = "1 year" },
                new Duration { Name = "2 Years", Value = 2, IsProlongation = false, ExternalName = "2 years"},
                new Duration { Name = "3 Years", Value = 3, IsProlongation = false, ExternalName = "3 years" },
                new Duration { Name = "4 Years", Value = 4, IsProlongation = false, ExternalName = "4 years" },
                new Duration { Name = "5 Years", Value = 5, IsProlongation = false, ExternalName = "5 years" },
                new Duration { Name = "Prolongation", Value = 1, IsProlongation = true, ExternalName = "1 year (P)" }
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

        private string ReadText(string fn)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{fn}");
            var streamReader = new StreamReader(stream);

            return streamReader.ReadToEnd();
        }
    }
}
