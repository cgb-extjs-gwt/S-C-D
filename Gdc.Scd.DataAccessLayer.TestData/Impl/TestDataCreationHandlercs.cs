using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Gdc.Scd.Core.Constants;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Report;
using Gdc.Scd.Core.Enums;
using Gdc.Scd.Core.Interfaces;
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
        private const string ClusterRegionId = "ClusterRegion";

        private readonly DomainEnitiesMeta entityMetas;

        private readonly EntityFrameworkRepositorySet repositorySet;

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
            this.CreateUserAndRoles();
            this.CreateReactionTimeTypeAvalability();
            this.CreateRegions();
            this.CreateCurrenciesAndExchangeRates();
            this.CreateCountries();
            this.CreateDurations();
            this.CreateYearAvailability();
            this.CreateProActiveSla();
            this.CreateImportConfiguration();
            this.CreateRolecodes();
            this.CreateSoftwereInputLevels();

            //report
            this.CreateReportColumnTypes();
            this.CreateReportFilterTypes();
            this.CreateCdCsConfiguration();

            var queries = new List<SqlHelper>();
            queries.AddRange(this.BuildInsertCostBlockSql());
            queries.AddRange(this.BuildFromFile(@"Scripts.availabilityFee.sql"));

            queries.AddRange(this.BuildFromFile(@"Scripts.matrix.sql"));

            queries.AddRange(this.BuildFromFile(@"Scripts.calculation-hw.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.calculation-sw.sql"));

            queries.AddRange(this.BuildFromFile(@"Scripts.Report.reports.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-list.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-calc-output-new-vs-old.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-calc-output-vs-FREEZE.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-calc-parameter-hw.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-calc-parameter-proactive.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-contract.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-flat-fee.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-hdd-retention-central.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-hdd-retention-country.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-hdd-retention-parameter.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-local-detailed.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-locap.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-Logistic-cost-calc-central.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-logistic-cost-calc-country.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-logistic-cost-central.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-logistic-cost-country.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-Logistic-cost-input-central.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-Logistic-cost-input-country.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-po-standard-warranty.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-proactive.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-solution-pack-price-list-detail.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-solution-pack-price-list.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-solutionpack-proactive-costing.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-SW-Service-Price-List-detail.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-SW-Service-Price-List.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.CD_CS.split-string.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.CD_CS.cd-cs-hdd-retention.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.CD_CS.cd-cs-proactive.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.CD_CS.cd-cs-servicecosts.sql"));
            foreach (var query in queries)
            {
                this.repositorySet.ExecuteSql(query);
            }
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

        private void CreateUserAndRoles()
        {
            var costEditorPermission = new Permission { Name = PermissionConstants.CostEditor };
            var tableViewPermission = new Permission { Name = PermissionConstants.TableView };
            var approvalPermission = new Permission { Name = PermissionConstants.Approval };
            var ownApprovalPermission = new Permission { Name = PermissionConstants.OwnApproval };
            var portfolioPermission = new Permission { Name = PermissionConstants.Portfolio };
            var reviewProcessPermission = new Permission { Name = PermissionConstants.ReviewProcess };
            var reportPermission = new Permission { Name = PermissionConstants.Report };
            var adminPermission = new Permission { Name = PermissionConstants.Admin };

            var allPermissions = new List<Permission>
            {
                costEditorPermission,
                tableViewPermission,
                approvalPermission,
                ownApprovalPermission,
                portfolioPermission,
                reviewProcessPermission,
                reportPermission,
                adminPermission
            };

            var allRolePermissions = allPermissions.Select(permission => new RolePermission { Permission = permission });

            var adminRole = new Role
            {
                Name = "SCD Admin",
                IsGlobal = true,
                RolePermissions = allRolePermissions.ToList()
            };

            var users = new List<User> {
                new User
                {
                    Name = "Test user 1",
                    Login ="g02\\testUser1",
                    Email ="testuser1@fujitsu.com",
                    UserRoles = new List<UserRole>
                    {
                        new UserRole
                        {
                            Role = adminRole
                        }
                    }
                },
                new User
                {
                    Name = "Test user 2",
                    Login ="g03\\testUser2",
                    Email ="testuser2@fujitsu.com"
                },
                new User
                {
                    Name = "Test user 3",
                    Login ="g04\\testUser3",
                    Email ="testuser3@fujitsu.com"
                }
            };

            var roles = new List<Role>
            {
                adminRole,
                new Role
                {
                    Name = "PRS PSM",
                    IsGlobal = true,
                    RolePermissions = new List<RolePermission>
                    {
                        new RolePermission { Permission = tableViewPermission },
                        new RolePermission { Permission = costEditorPermission },
                        new RolePermission { Permission = reportPermission },
                        new RolePermission { Permission = approvalPermission },
                        new RolePermission { Permission = ownApprovalPermission },
                    }
                },
                new Role
                {
                    Name = "Country key user",
                    IsGlobal = false,
                    RolePermissions = new List<RolePermission>
                    {
                        new RolePermission { Permission = costEditorPermission },
                        new RolePermission { Permission = reportPermission },
                        new RolePermission { Permission = approvalPermission },
                        new RolePermission { Permission = ownApprovalPermission },
                        new RolePermission { Permission = reviewProcessPermission },
                    }
                },
                new Role
                {
                    Name = "Country Finance Director",
                    IsGlobal = false,
                    RolePermissions = new List<RolePermission>
                    {
                        new RolePermission { Permission = reportPermission },
                        new RolePermission { Permission = approvalPermission },
                        new RolePermission { Permission = reviewProcessPermission },
                    }
                },
                new Role
                {
                    Name = "PRS Finance",
                    IsGlobal = true,
                    RolePermissions = new List<RolePermission>
                    {
                        new RolePermission { Permission = costEditorPermission },
                        new RolePermission { Permission = tableViewPermission },
                        new RolePermission { Permission = reportPermission },
                        new RolePermission { Permission = approvalPermission },
                        new RolePermission { Permission = ownApprovalPermission },
                        new RolePermission { Permission = reviewProcessPermission },
                    }
                },
                new Role
                {
                    Name = "Spares Logistics",
                    IsGlobal = true,
                    RolePermissions = new List<RolePermission>
                    {
                        new RolePermission { Permission = tableViewPermission },
                        new RolePermission { Permission = reportPermission },
                        new RolePermission { Permission = reviewProcessPermission },
                    }
                },
                new Role
                {
                    Name = "GTS user",
                    IsGlobal = true,
                    RolePermissions = new List<RolePermission>
                    {
                        new RolePermission { Permission = tableViewPermission },
                        new RolePermission { Permission = reportPermission },
                        new RolePermission { Permission = reviewProcessPermission },
                    }
                },
                new Role
                {
                    Name = "Guest",
                    IsGlobal = true,
                    RolePermissions = new List<RolePermission>
                    {
                        new RolePermission { Permission = reportPermission },
                    }
                },
                new Role
                {
                    Name = "Opportunity Center",
                    IsGlobal = true,
                    RolePermissions = new List<RolePermission>
                    {
                        new RolePermission { Permission = reportPermission },
                    }
                }
            };

            this.repositorySet.GetRepository<Role>().Save(roles);
            this.repositorySet.GetRepository<User>().Save(users);
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
            var items = this.BuildTestItems<T>(count);

            this.repositorySet.GetRepository<T>().Save(items);
            this.repositorySet.Sync();
        }

        private void CreateSoftwereInputLevels()
        {
            var count = 5;
            var sogs = this.BuildDeactivatableTestItems<Sog>(count).ToArray();
            var plas = this.repositorySet.GetRepository<Pla>().GetAll().Take(count).ToArray();

            for (var i = 0; i < count; i++)
            {
                sogs[i].PlaId = plas[i].Id;
            }

            this.repositorySet.GetRepository<Sog>().Save(sogs);
            this.repositorySet.Sync();

            var swDigit = this.BuildDeactivatableTestItems<SwDigit>(count).ToArray();
            var sfabs = this.BuildDeactivatableTestItems<SFab>(count).ToArray();

            for (var i = 0; i < count; i++)
            {
                swDigit[i].SogId = sogs[i].Id;
                sfabs[i].PlaId = plas[i].Id;
            }

            this.repositorySet.GetRepository<SwDigit>().Save(swDigit);
            this.repositorySet.GetRepository<SFab>().Save(sfabs);

            var swLicences = this.BuildDeactivatableTestItems<SwLicense>();

            this.repositorySet.GetRepository<SwLicense>().Save(swLicences);
            this.repositorySet.Sync();
        }

        private IEnumerable<T> BuildTestItems<T>(int count = 5) where T : NamedId, new()
        {
            var typeName = typeof(T).Name;

            for (var i = 0; i < count; i++)
            {
                var item = new T
                {
                    Name = $"{typeName}_{i}"
                };

                yield return item;
            }
        }

        private IEnumerable<T> BuildDeactivatableTestItems<T>(int count = 5) where T : NamedId, IDeactivatable, new()
        {
            var nowTime = DateTime.UtcNow;

            foreach (var item in this.BuildTestItems<T>(count))
            {
                item.CreatedDateTime = nowTime;
                item.ModifiedDateTime = nowTime;

                yield return item;
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
            foreach (var costBlockMeta in this.entityMetas.CostBlocks)
            {
                var referenceFields = costBlockMeta.CoordinateFields.ToList();
                var selectColumns =
                    referenceFields.Select(field => new ColumnInfo(field.ReferenceValueField, field.ReferenceMeta.Name, field.Name))
                                   .ToList()
                                   .AsEnumerable();

                var insertFields = referenceFields.Select(field => field.Name).ToArray();

                var wgField = costBlockMeta.InputLevelFields[MetaConstants.WgInputLevelName];
                var plaField = costBlockMeta.InputLevelFields[MetaConstants.PlaInputLevelName];

                if (plaField != null && wgField != null)
                {
                    selectColumns =
                        selectColumns.Select(
                            column => column.TableName == plaField.Name
                                ? new ColumnInfo($"{nameof(Pla)}{nameof(Wg.Id)}", MetaConstants.WgInputLevelName, plaField.Name)
                                : column);

                    referenceFields.Remove(plaField);
                }

                var clusterRegionField = costBlockMeta.InputLevelFields[ClusterRegionId];
                var countryField = costBlockMeta.InputLevelFields[MetaConstants.CountryInputLevelName];

                ReferenceFieldMeta fromField = null;

                if (countryField == null)
                {
                    fromField = referenceFields[0];

                    referenceFields.RemoveAt(0);
                }
                else
                {
                    if (clusterRegionField != null)
                    {
                        selectColumns =
                            selectColumns.Select(
                                column => column.TableName == clusterRegionField.Name
                                    ? new ColumnInfo(nameof(Country.ClusterRegionId), MetaConstants.CountryInputLevelName, ClusterRegionId)
                                    : column);

                        referenceFields.Remove(clusterRegionField);
                    }

                    fromField = countryField;

                    referenceFields.Remove(countryField);
                }

                var joinQuery = Sql.Select(selectColumns.ToArray()).From(fromField.ReferenceMeta);

                foreach (var field in referenceFields)
                {
                    var referenceMeta = field.ReferenceMeta;

                    joinQuery = joinQuery.Join(referenceMeta.Schema, referenceMeta.Name, null, JoinType.Cross);
                }

                SqlHelper query;

                if (countryField == null)
                {
                    query = joinQuery;
                }
                else
                {
                    query = joinQuery.Where(
                        SqlOperators.Equals(nameof(Country.IsMaster), "isMaster", true, MetaConstants.CountryInputLevelName));
                }

                yield return Sql.Insert(costBlockMeta, insertFields).Query(query);
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
                    CodingPattern = "SME",
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
                    CodingPattern = "PSBM",
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
                    CodingPattern = "PSMO",
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
                    CodingPattern = "STOR",
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
                    CodingPattern = "SSHI",
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
                new Pla { Name = "RETAIL PRODUCTS", CodingPattern = "RETA"},
                new Pla { Name = "UNIX SERVER", CodingPattern = "UNIX" }
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
            var roleCodes = new RoleCode[] {
                new RoleCode { Name = "SEFS05" },
                new RoleCode { Name = "SEFS06" },
                new RoleCode { Name = "SEFS04" },
                new RoleCode { Name = "SEIE07" },
                new RoleCode { Name = "SEIE08" }
            };

            var repository = this.repositorySet.GetRepository<RoleCode>();

            repository.Save(roleCodes);
            this.repositorySet.Sync();
        }

        private void CreateReportColumnTypes()
        {
            var items = new ReportColumnType[] {
                new ReportColumnType { Name = "text" },
                new ReportColumnType { Name = "number" },
                new ReportColumnType { Name = "boolean" },
                new ReportColumnType { Name = "euro" },
                new ReportColumnType { Name = "percent" },
            };

            var repository = this.repositorySet.GetRepository<ReportColumnType>();
            repository.Save(items);
            this.repositorySet.Sync();
        }

        private void CreateReportFilterTypes()
        {
            var items = new ReportFilterType[] {
                new ReportFilterType { Name = "text" },
                new ReportFilterType { Name = "number" },
                new ReportFilterType { Name = "boolean" },
                new ReportFilterType { Name = "wg" , MultiSelect = true },
                new ReportFilterType { Name = "sog" , MultiSelect = true },
                new ReportFilterType { Name = "countrygroup" , MultiSelect = true },
                new ReportFilterType { Name = "country" , MultiSelect = true },
                new ReportFilterType { Name = "availability" , MultiSelect = true },
                new ReportFilterType { Name = "duration" , MultiSelect = true },
                new ReportFilterType { Name = "reactiontime" , MultiSelect = true },
                new ReportFilterType { Name = "reactiontype" , MultiSelect = true },
                new ReportFilterType { Name = "servicelocation" , MultiSelect = true },
                new ReportFilterType { Name = "year" , MultiSelect = true }
            };

            var repository = this.repositorySet.GetRepository<ReportFilterType>();
            repository.Save(items);
            this.repositorySet.Sync();
        }

        private void CreateServiceLocations()
        {
            var repository = this.repositorySet.GetRepository<ServiceLocation>();

            repository.Save(new List<ServiceLocation>
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

            });

            this.repositorySet.Sync();
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

        private string ReadText(string fn)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{fn}");
            var streamReader = new StreamReader(stream);

            return streamReader.ReadToEnd();
        }

        private void CreateImportConfiguration()
        {
            var taxAndDuties = new ImportConfiguration
            {
                Name = ImportSystems.AMBERROAD,
                FilePath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\Amber road",
                FileName = "SCD_Duties_Taxes.csv",
                ImportMode = Core.Enums.ImportMode.ManualyAutomaticly,
                ProcessedDateTime = null,
                Occurancy = Core.Enums.Occurancy.PerMonth,
                ProcessedFilesPath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\Amber road\processed",
                Delimeter = ";",
                HasHeader = true,
                Culture = "de-DE"
            };

            var logistic = new ImportConfiguration
            {
                Name = ImportSystems.LOGISTICS,
                FilePath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\Logistics",
                FileName = "FeeCalculator-Upload_*.txt",
                ImportMode = Core.Enums.ImportMode.Automatic,
                ProcessedDateTime = null,
                Occurancy = Core.Enums.Occurancy.PerMonth,
                ProcessedFilesPath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\Logistics\processed",
                Delimeter = "|",
                HasHeader = true,
                Culture = "de-DE"
            };

            var ebis_afr = new ImportConfiguration
            {
                Name = ImportSystems.EBIS_AFR,
                FilePath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\EBIS",
                FileName = "SCD_FR_LOAD.csv",
                ImportMode = Core.Enums.ImportMode.ManualyAutomaticly,
                ProcessedDateTime = null,
                Occurancy = Core.Enums.Occurancy.PerMonth,
                ProcessedFilesPath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\EBIS\processed",
                Delimeter = ";",
                HasHeader = true,
                Culture = "en-US"
            };

            var ebis_material_cost = new ImportConfiguration
            {
                Name = ImportSystems.EBIS_MATERIAL_COST,
                FilePath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\EBIS",
                FileName = "SCD_MATCO_LOAD.csv",
                ImportMode = Core.Enums.ImportMode.ManualyAutomaticly,
                ProcessedDateTime = null,
                Occurancy = Core.Enums.Occurancy.PerMonth,
                ProcessedFilesPath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\EBIS\processed",
                Delimeter = ";",
                HasHeader = true,
                Culture = "en-US"
            };

            var ebis_install_base = new ImportConfiguration
            {
                Name = ImportSystems.EBIS_INSTALL_BASE,
                FilePath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\EBIS",
                FileName = "SCD_FQR_LOAD.csv",
                ImportMode = Core.Enums.ImportMode.ManualyAutomaticly,
                ProcessedDateTime = null,
                Occurancy = Core.Enums.Occurancy.PerMonth,
                ProcessedFilesPath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\EBIS\processed",
                Delimeter = ";",
                HasHeader = true,
                Culture = "en-US"
            };

            this.repositorySet.GetRepository<ImportConfiguration>().Save(new List<ImportConfiguration>()
            {
                taxAndDuties,
                logistic,
                ebis_afr,
                ebis_install_base,
                ebis_material_cost
            });

            this.repositorySet.Sync();
        }

        private void CreateRegions()
        {
            var crAsia = new ClusterRegion { Name = "Asia" };
            var crEmeia = new ClusterRegion { Name = "EMEIA" };
            var crJapan = new ClusterRegion { Name = "Japan" };
            var crLatinAmerica = new ClusterRegion { Name = "Latin America" };
            var crUnitedStates = new ClusterRegion { Name = "United States" };
            var crOceania = new ClusterRegion { Name = "Oceania" };

            this.repositorySet.GetRepository<Region>().Save(new List<Region>()
            {
                new Region { Name = "Asia", ClusterRegion = crAsia },
                new Region {Name = "Central Europe", ClusterRegion = crEmeia },
                new Region { Name = "Japan", ClusterRegion = crJapan },
                new Region { Name = "Latin America", ClusterRegion = crLatinAmerica },
                new Region { Name = "Nordic", ClusterRegion = crEmeia },
                new Region { Name = "Oceania", ClusterRegion = crOceania },
                new Region { Name = "UK and Ireland", ClusterRegion = crEmeia },
                new Region { Name = "United States", ClusterRegion = crUnitedStates },
                new Region { Name = "WEMEI", ClusterRegion = crEmeia },
                new Region { Name = "EERA", ClusterRegion = crEmeia}
            });

            this.repositorySet.Sync();
        }

        private void CreateCountries()
        {
            var regionRepo = this.repositorySet.GetRepository<Region>();
            var clusterRepo = this.repositorySet.GetRepository<ClusterRegion>();

            var asiaClusterId = clusterRepo.GetAll().Where(r => r.Name == "Asia").First().Id;
            var emeiaClusterId = clusterRepo.GetAll().Where(r => r.Name == "EMEIA").First().Id;
            var japanCluserId = clusterRepo.GetAll().Where(r => r.Name == "Japan").First().Id;
            var laClusterId = clusterRepo.GetAll().Where(r => r.Name == "Latin America").First().Id;
            var oceaniaClusterId = clusterRepo.GetAll().Where(r => r.Name == "Oceania").First().Id;
            var usClusterId = clusterRepo.GetAll().Where(r => r.Name == "United States").First().Id;


            var asiaRegion = regionRepo.GetAll().Where(r => r.Name == "Asia").First();
            var ceRegion = regionRepo.GetAll().Where(r => r.Name == "Central Europe").First();
            var japanRegion = regionRepo.GetAll().Where(r => r.Name == "Japan").First();
            var laRegion = regionRepo.GetAll().Where(r => r.Name == "Latin America").First();
            var nordicRegion = regionRepo.GetAll().Where(r => r.Name == "Nordic").First();
            var oceaniaRegion = regionRepo.GetAll().Where(r => r.Name == "Oceania").First();
            var ukRegion = regionRepo.GetAll().Where(r => r.Name == "UK and Ireland").First();
            var usRegion = regionRepo.GetAll().Where(r => r.Name == "United States").First();
            var wemeiaRegion = regionRepo.GetAll().Where(r => r.Name == "WEMEI").First();
            var eeraRegion = regionRepo.GetAll().Where(r => r.Name == "EERA").First();

            #region Country Groups
            var argentinaCG = new CountryGroup
            {
                Name = "Argentina",
                RegionId = laRegion.Id,
                LUTCode = "FUJ"
            };
            var chinaCG = new CountryGroup
            {
                Name = "China",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "CN"
            };
            var hongKongCG = new CountryGroup
            {
                Name = "Hong Kong",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "HK"
            };
            var indonesiaCG = new CountryGroup
            {
                Name = "Indonesia",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "IO"
            };
            var koreaCG = new CountryGroup
            {
                Name = "Korea, South",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "KR"
            };
            var malaysiaCG = new CountryGroup
            {
                Name = "Malaysia",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "MY"
            };
            var pilippinesCG = new CountryGroup
            {
                Name = "Philippines",
                RegionId = asiaRegion.Id,
                LUTCode = "ASP"
            };
            var singaporeCG = new CountryGroup
            {
                Name = "Singapore",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "SG"
            };
            var taiwanCG = new CountryGroup
            {
                Name = "Taiwan",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "TW"
            };
            var thailandCG = new CountryGroup
            {
                Name = "Thailand",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "TH"
            };
            var vietnamCG = new CountryGroup
            {
                Name = "Vietnam",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "VN"
            };
            var austriaCG = new CountryGroup
            {
                Name = "Austria",
                RegionId = ceRegion.Id,
                LUTCode = "OES",
                CountryDigit = "AT"
            };
            var germanyCG = new CountryGroup
            {
                Name = "Germany",
                RegionId = ceRegion.Id,
                LUTCode = "D",
                CountryDigit = "DE"
            };
            var suisseCG = new CountryGroup
            {
                Name = "Suisse",
                RegionId = ceRegion.Id,
                LUTCode = "SWZ",
                CountryDigit = "CH"
            };
            var japanCG = new CountryGroup
            {
                Name = "Japan",
                RegionId = japanRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "JP"
            };
            var brazilCG = new CountryGroup
            {
                Name = "Brazil",
                RegionId = laRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "BR"
            };
            var chileCG = new CountryGroup
            {
                Name = "Chile",
                RegionId = laRegion.Id,
                LUTCode = "FUJ"
            };
            var colombiaCG = new CountryGroup
            {
                Name = "Colombia",
                RegionId = laRegion.Id,
                LUTCode = "FUJ"
            };
            var denmarkCG = new CountryGroup
            {
                Name = "Denmark",
                RegionId = nordicRegion.Id,
                LUTCode = "DAN",
                CountryDigit = "ND;DK"
            };
            var finlandCG = new CountryGroup
            {
                Name = "Finland",
                RegionId = nordicRegion.Id,
                LUTCode = "FIN",
                CountryDigit = "ND;FI"
            };
            var norwayCG = new CountryGroup
            {
                Name = "Norway",
                RegionId = nordicRegion.Id,
                LUTCode = "NOR",
                CountryDigit = "ND;NO",
            };
            var swedenCG = new CountryGroup
            {
                Name = "Sweden",
                RegionId = nordicRegion.Id,
                LUTCode = "SWD",
                CountryDigit = "ND;SE",
            };
            var australiaCG = new CountryGroup
            {
                Name = "Australia",
                RegionId = oceaniaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "AU"
            };
            var newZealnadCG = new CountryGroup
            {
                Name = "New Zealand",
                RegionId = oceaniaRegion.Id,
                LUTCode = "FUJ",
            };
            var ukCG = new CountryGroup
            {
                Name = "UK",
                RegionId = ukRegion.Id,
                LUTCode = "GBR",
                CountryDigit = "GB"
            };
            var mexicoCG = new CountryGroup
            {
                Name = "Mexico",
                RegionId = usRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "MX",
            };
            var usCG = new CountryGroup
            {
                Name = "United States",
                RegionId = usRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "US"
            };
            var belgiumCG = new CountryGroup
            {
                Name = "Belgium",
                RegionId = wemeiaRegion.Id,
                LUTCode = "BEL",
                CountryDigit = "BE"
            };
            var czechCG = new CountryGroup
            {
                Name = "Czech Republic",
                RegionId = eeraRegion.Id,
                LUTCode = "CRE",
                CountryDigit = "CZ"

            };
            var franceCG = new CountryGroup
            {
                Name = "France",
                RegionId = wemeiaRegion.Id,
                LUTCode = "FKR",
                CountryDigit = "FR"

            };
            var greeceCG = new CountryGroup
            {
                Name = "Greece",
                RegionId = wemeiaRegion.Id,
                LUTCode = "GRI",
                CountryDigit = "GR"

            };
            var hungaryCG = new CountryGroup
            {
                Name = "Hungary",
                RegionId = wemeiaRegion.Id,
                LUTCode = "HU"

            };
            var indiaCG = new CountryGroup
            {
                Name = "India",
                RegionId = wemeiaRegion.Id,
                LUTCode = "IND",
                CountryDigit = "ID"

            };
            var israelCG = new CountryGroup
            {
                Name = "Israel",
                RegionId = wemeiaRegion.Id,
                LUTCode = "ISR",
                CountryDigit = "IL"

            };
            var italyCG = new CountryGroup
            {
                Name = "Italy",
                RegionId = wemeiaRegion.Id,
                LUTCode = "ITL",
                CountryDigit = "IT"

            };
            var luxembourgCG = new CountryGroup
            {
                Name = "Luxembourg",
                RegionId = wemeiaRegion.Id,
                LUTCode = "LUX",
                CountryDigit = "LU"

            };
            var mdeCG = new CountryGroup
            {
                Name = "MDE",
                RegionId = wemeiaRegion.Id,
                LUTCode = "MDE",
                CountryDigit = "ME"

            };
            var netherlandsCG = new CountryGroup
            {
                Name = "Netherlands",
                RegionId = wemeiaRegion.Id,
                LUTCode = "NDL",
                CountryDigit = "NL"

            };
            var noaCG = new CountryGroup
            {
                Name = "NOA",
                RegionId = wemeiaRegion.Id,
                LUTCode = "NOA",
                CountryDigit = "NA"

            };
            var polandCG = new CountryGroup
            {
                Name = "Poland",
                RegionId = wemeiaRegion.Id,
                LUTCode = "POL",
                CountryDigit = "PL",

            };
            var portugalCG = new CountryGroup
            {
                Name = "Portugal",
                RegionId = wemeiaRegion.Id,
                LUTCode = "POR",
                CountryDigit = "PT"

            };
            var russiaCG = new CountryGroup
            {
                Name = "Russia",
                RegionId = wemeiaRegion.Id,
                LUTCode = "RUS",
                CountryDigit = "RU"

            };
            var seeCG = new CountryGroup
            {
                Name = "SEE",
                RegionId = wemeiaRegion.Id,
                LUTCode = "SEE",
                CountryDigit = "EE"

            };
            var southAfricaCG = new CountryGroup
            {
                Name = "South Africa",
                RegionId = wemeiaRegion.Id,
                LUTCode = "RSA",
                CountryDigit = "ZA"

            };
            var spainCG = new CountryGroup
            {
                Name = "Spain",
                RegionId = wemeiaRegion.Id,
                LUTCode = "SPA",
                CountryDigit = "ES"

            };
            var turkeyCG = new CountryGroup
            {
                Name = "Turkey",
                RegionId = wemeiaRegion.Id,
                LUTCode = "TRK",
                CountryDigit = "TR"
            };

            #endregion

            var countries = new Country[]
            {
                new Country { Name = "China",  SAPCountryCode = "CHN", ISO3CountryCode = "CHN", CountryGroup = chinaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = chinaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Hong Kong", SAPCountryCode = "HGK", ISO3CountryCode = "HKG", CountryGroup = hongKongCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = hongKongCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Indonesia", SAPCountryCode = "IDS", ISO3CountryCode = "IDN", CountryGroup = indonesiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = indonesiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Korea, South", SAPCountryCode = "KOR", ISO3CountryCode = "KOR", CountryGroup = koreaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = koreaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Malaysia", SAPCountryCode = "MAL", ISO3CountryCode = "MYS", CountryGroup = malaysiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = malaysiaCG.RegionId, AssignedToMultiVendor = false},
                new Country { Name = "Philippines", SAPCountryCode = "PHI", ISO3CountryCode = "PHL", CountryGroup = pilippinesCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = asiaClusterId, RegionId = pilippinesCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Singapore", SAPCountryCode = "SIN", ISO3CountryCode = "SGP", CountryGroup = singaporeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = singaporeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Taiwan", SAPCountryCode = "TAI", ISO3CountryCode = "TWN", CountryGroup = taiwanCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = taiwanCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Thailand", SAPCountryCode = "THA", ISO3CountryCode = "THA", CountryGroup = thailandCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = thailandCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Vietnam", SAPCountryCode = "VIT", ISO3CountryCode = "VNM", CountryGroup = vietnamCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = vietnamCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Austria", SAPCountryCode = "OES", ISO3CountryCode = "AUT", CountryGroup = austriaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = austriaCG.RegionId, AssignedToMultiVendor = true },
                new Country { Name = "Germany", SAPCountryCode = "D", ISO3CountryCode = "DEU", CountryGroup = germanyCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = germanyCG.RegionId, AssignedToMultiVendor = true },
                new Country { Name = "Liechtenstein", SAPCountryCode = "LIC", ISO3CountryCode = "LIE", CountryGroup = suisseCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = suisseCG.RegionId, AssignedToMultiVendor = true },
                new Country { Name = "Switzerland", SAPCountryCode = "SWZ", ISO3CountryCode = "CHE", CountryGroup = suisseCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = suisseCG.RegionId, AssignedToMultiVendor = true },
                new Country { Name = "Japan", SAPCountryCode = "FUJ", ISO3CountryCode = "JPN", CountryGroup = japanCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = japanCluserId, RegionId = japanCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Argentina", SAPCountryCode = "ARG", ISO3CountryCode = "ARG", CountryGroup = argentinaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = laClusterId, RegionId = argentinaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Brazil", SAPCountryCode = "FUJ", ISO3CountryCode = "BRA", CountryGroup = brazilCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = laClusterId, RegionId = brazilCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Chile", SAPCountryCode = "CHL", ISO3CountryCode = "CHL", CountryGroup = chileCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = laClusterId, RegionId = chileCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Colombia", SAPCountryCode = "KOL", ISO3CountryCode = "COL", CountryGroup = colombiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = laClusterId, RegionId = colombiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Denmark", SAPCountryCode = "DAN", ISO3CountryCode = "DNK", CountryGroup = denmarkCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = denmarkCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Faroe Islands", SAPCountryCode = "FAR", ISO3CountryCode = "FRO", CountryGroup = denmarkCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = denmarkCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Greenland", SAPCountryCode = "GRO", ISO3CountryCode = "GRL", CountryGroup = denmarkCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = denmarkCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Iceland", SAPCountryCode = "ISL", ISO3CountryCode = "ISL", CountryGroup = denmarkCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = denmarkCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Estonia", SAPCountryCode = "EST", ISO3CountryCode = "EST", CountryGroup = finlandCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = finlandCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Finland", SAPCountryCode = "FIN", ISO3CountryCode = "FIN", CountryGroup = finlandCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = finlandCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Latvia", SAPCountryCode = "LET", ISO3CountryCode = "LVA", CountryGroup = finlandCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = finlandCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Lithuania", SAPCountryCode = "LIT", ISO3CountryCode = "LTU", CountryGroup = finlandCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = finlandCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Norway", SAPCountryCode = "NOR", ISO3CountryCode = "NOR", CountryGroup = norwayCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = norwayCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Sweden", SAPCountryCode = "SWD", ISO3CountryCode = "SWE", CountryGroup = swedenCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = swedenCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Australia", SAPCountryCode = "AUS", ISO3CountryCode = "AUS", CountryGroup = australiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = oceaniaClusterId, RegionId = australiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "New Zealand", SAPCountryCode = "NSL", ISO3CountryCode = "NZL", CountryGroup = newZealnadCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = oceaniaClusterId, RegionId = newZealnadCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Guernsey", SAPCountryCode = "", ISO3CountryCode = "GGY", CountryGroup = ukCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = ukCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Ireland", SAPCountryCode = "GBR", ISO3CountryCode = "IRL", CountryGroup = ukCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = ukCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Jersey", SAPCountryCode = "", ISO3CountryCode = "JEY", CountryGroup = ukCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = ukCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Man, Isle of", SAPCountryCode = "", ISO3CountryCode = "", CountryGroup = ukCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = ukCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Great Britain", SAPCountryCode = "GBR", ISO3CountryCode = "GBR", CountryGroup = ukCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = ukCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Northern Ireland", SAPCountryCode = "", ISO3CountryCode = "", CountryGroup = ukCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = ukCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Mexico", SAPCountryCode = "MEX", ISO3CountryCode = "MEX", CountryGroup = mexicoCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = usClusterId, RegionId = mexicoCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "United States", SAPCountryCode = "FUJ", ISO3CountryCode = "USA", CountryGroup = usCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = usClusterId, RegionId = usCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Belgium", SAPCountryCode = "BEL", ISO3CountryCode = "BEL", CountryGroup = belgiumCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = belgiumCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Czech Republic", SAPCountryCode = "CRE", ISO3CountryCode = "CZE", CountryGroup = czechCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = czechCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Slovakia", SAPCountryCode = "SRE", ISO3CountryCode = "SVK", CountryGroup = czechCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = czechCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "France", SAPCountryCode = "FKR", ISO3CountryCode = "FRA", CountryGroup = franceCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = franceCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "French Guiana", SAPCountryCode = "FGU", ISO3CountryCode = "GUF", CountryGroup = franceCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = franceCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "French Polynesia", SAPCountryCode = "PYF", ISO3CountryCode = "PYF", CountryGroup = franceCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = franceCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Guadeloupe", SAPCountryCode = "GLP", ISO3CountryCode = "Guadeloupe", CountryGroup = franceCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = franceCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Martinique", SAPCountryCode = "MTQ", ISO3CountryCode = "Martinique", CountryGroup = franceCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = franceCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Monaco", SAPCountryCode = "MON", ISO3CountryCode = "Monaco", CountryGroup = franceCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = franceCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "New Caledonia", SAPCountryCode = "NKA", ISO3CountryCode = "New Caledonia", CountryGroup = franceCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = franceCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Reunion", SAPCountryCode = "REU", ISO3CountryCode = "REU", CountryGroup = franceCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = franceCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Saint Pierre and Miquelon", SAPCountryCode = "SPM", ISO3CountryCode = "", CountryGroup = franceCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = franceCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Cyprus", SAPCountryCode = "CYP", ISO3CountryCode = "CYP", CountryGroup = greeceCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = greeceCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Greece", SAPCountryCode = "GRI", ISO3CountryCode = "GRC", CountryGroup = greeceCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = greeceCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Hungary", SAPCountryCode = "UNG", ISO3CountryCode = "HUN", CountryGroup = hungaryCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = hungaryCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "India", SAPCountryCode = "IND", ISO3CountryCode = "IND", CountryGroup = indiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = indiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Israel", SAPCountryCode = "ISR", ISO3CountryCode = "ISR", CountryGroup = israelCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = israelCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Italy", SAPCountryCode = "ITL", ISO3CountryCode = "ITA", CountryGroup = italyCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = italyCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "San Marino", SAPCountryCode = "SMA", ISO3CountryCode = "SMR", CountryGroup = italyCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = italyCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Luxembourg", SAPCountryCode = "LUX", ISO3CountryCode = "LUX", CountryGroup = luxembourgCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = luxembourgCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Egypt", SAPCountryCode = "EGY", ISO3CountryCode = "EGY", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Afghanistan", SAPCountryCode = "AFG", ISO3CountryCode = "AFG", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Bahrain", SAPCountryCode = "BAH", ISO3CountryCode = "BHR", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Gaza Strip", SAPCountryCode = "", ISO3CountryCode = "", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Iran", SAPCountryCode = "IRN", ISO3CountryCode = "IRN", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Iraq", SAPCountryCode = "IRK", ISO3CountryCode = "IRQ", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Jordan", SAPCountryCode = "JOR", ISO3CountryCode = "JOR", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Kuwait", SAPCountryCode = "KUW", ISO3CountryCode = "KWT", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Lebanon", SAPCountryCode = "LIB", ISO3CountryCode = "LBN", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Malta", SAPCountryCode = "MTA", ISO3CountryCode = "MLT", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Oman", SAPCountryCode = "OMA", ISO3CountryCode = "OMN", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Pakistan", SAPCountryCode = "PAK", ISO3CountryCode = "PAK", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Qatar", SAPCountryCode = "KAT", ISO3CountryCode = "QAT", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Saudi Arabia", SAPCountryCode = "SAR", ISO3CountryCode = "SAU", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Syria", SAPCountryCode = "SYR", ISO3CountryCode = "SYR", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "United Arab Emirates", SAPCountryCode = "UAE", ISO3CountryCode = "ARE", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "West Bank", SAPCountryCode = "", ISO3CountryCode = "", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Yemen", SAPCountryCode = "", ISO3CountryCode = "YEM", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Palestine", SAPCountryCode = "", ISO3CountryCode = "PSE", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Netherlands", SAPCountryCode = "NDL", ISO3CountryCode = "NLD", CountryGroup = netherlandsCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = netherlandsCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Burkina Faso", SAPCountryCode = "BUF", ISO3CountryCode = "BFA", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Cameroon", SAPCountryCode = "KAM", ISO3CountryCode = "CMR", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Cape Verde", SAPCountryCode = "KAP", ISO3CountryCode = "CPV", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Central African Republic", SAPCountryCode = "ZAR", ISO3CountryCode = "CAF", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Chad", SAPCountryCode = "TSD", ISO3CountryCode = "TCD", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Congo, Democratic Republic of the", SAPCountryCode = "ZAI", ISO3CountryCode = "COD", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Congo, Republic of the", SAPCountryCode = "KGO", ISO3CountryCode = "COD", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Djibouti", SAPCountryCode = "DIB", ISO3CountryCode = "DJI", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Equatorial Guinea", SAPCountryCode = "AGU", ISO3CountryCode = "GNQ", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Eritrea", SAPCountryCode = "ERI", ISO3CountryCode = "ERI", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Ethiopia", SAPCountryCode = "ETH", ISO3CountryCode = "ETH", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Algeria", SAPCountryCode = "ALG", ISO3CountryCode = "DZA", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Benin", SAPCountryCode = "BEN", ISO3CountryCode = "BEN", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Gabon", SAPCountryCode = "GAB", ISO3CountryCode = "GAB", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Gambia, The", SAPCountryCode = "GBA", ISO3CountryCode = "GMB", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Ghana", SAPCountryCode = "GHA", ISO3CountryCode = "GHA", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Guinea", SAPCountryCode = "GUI", ISO3CountryCode = "GIN", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Guinea-Bissau", SAPCountryCode = "BIS", ISO3CountryCode = "GNB", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Liberia", SAPCountryCode = "LBA", ISO3CountryCode = "LBR", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Libya", SAPCountryCode = "LBY", ISO3CountryCode = "LBY", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Mali", SAPCountryCode = "MLI", ISO3CountryCode = "MLI", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Mauritania", SAPCountryCode = "MTN", ISO3CountryCode = "MRT", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Morocco", SAPCountryCode = "NOA", ISO3CountryCode = "MAR", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Niger", SAPCountryCode = "NGR", ISO3CountryCode = "NER", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Sao Tome and Principe", SAPCountryCode = "STP", ISO3CountryCode = "", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Senegal", SAPCountryCode = "SEN", ISO3CountryCode = "SEN", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Seychelles", SAPCountryCode = "SEY", ISO3CountryCode = "SYC", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Sierra Leone", SAPCountryCode = "LEO", ISO3CountryCode = "SLE", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Somalia", SAPCountryCode = "SOM", ISO3CountryCode = "SOM", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Sudan", SAPCountryCode = "SUD", ISO3CountryCode = "SDN", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Togo", SAPCountryCode = "TGO", ISO3CountryCode = "TGO", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Tunisia", SAPCountryCode = "TUN", ISO3CountryCode = "TUN", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Western Sahara", SAPCountryCode = "", ISO3CountryCode = "ESH", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Poland", SAPCountryCode = "POL", ISO3CountryCode = "POL", CountryGroup = polandCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = polandCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Portugal", SAPCountryCode = "POR", ISO3CountryCode = "PRT", CountryGroup = portugalCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = portugalCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Armenia", SAPCountryCode = "ARM", ISO3CountryCode = "ARM", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Azerbaijan", SAPCountryCode = "ASE", ISO3CountryCode = "AZE", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Belarus", SAPCountryCode = "WEI", ISO3CountryCode = "BLR", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Georgia", SAPCountryCode = "GEO", ISO3CountryCode = "GEO", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Kazakhstan", SAPCountryCode = "KAS", ISO3CountryCode = "KAZ", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Kyrgyzstan", SAPCountryCode = "KGI", ISO3CountryCode = "KGZ", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Moldova", SAPCountryCode = "MOL", ISO3CountryCode = "MDA", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Russia", SAPCountryCode = "RUS", ISO3CountryCode = "RUS", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Tajikistan", SAPCountryCode = "TAD", ISO3CountryCode = "TJK", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Turkmenistan", SAPCountryCode = "TUR", ISO3CountryCode = "TKM", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Ukraine", SAPCountryCode = "UKR", ISO3CountryCode = "UKR", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Uzbekistan", SAPCountryCode = "USB", ISO3CountryCode = "UZB", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Bulgaria", SAPCountryCode = "BUL", ISO3CountryCode = "BGR", CountryGroup = seeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = seeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Croatia", SAPCountryCode = "KRO", ISO3CountryCode = "HRV", CountryGroup = seeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = seeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Albania", SAPCountryCode = "ALB", ISO3CountryCode = "ALB", CountryGroup = seeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = seeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Bosnia and Herzegovina", SAPCountryCode = "BOH", ISO3CountryCode = "BIH", CountryGroup = seeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = seeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Macedonia", SAPCountryCode = "MAZ", ISO3CountryCode = "MKD", CountryGroup = seeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = seeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Romania", SAPCountryCode = "RUM", ISO3CountryCode = "ROU", CountryGroup = seeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = seeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Serbia", SAPCountryCode = "SRB", ISO3CountryCode = "SRB", CountryGroup = seeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = seeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Slovenia", SAPCountryCode = "SLO", ISO3CountryCode = "SVN", CountryGroup = seeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = seeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Montenegro", SAPCountryCode = "MNE", ISO3CountryCode = "MNE", CountryGroup = seeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = seeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Kosovo", SAPCountryCode = "", ISO3CountryCode = "", CountryGroup = seeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = seeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Burundi", SAPCountryCode = "BUD", ISO3CountryCode = "BDI", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Comoros", SAPCountryCode = "KOM", ISO3CountryCode = "COM", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Angola", SAPCountryCode = "ANG", ISO3CountryCode = "AGO", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Botswana", SAPCountryCode = "BTS", ISO3CountryCode = "BWA", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Kenya", SAPCountryCode = "KEN", ISO3CountryCode = "KEN", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Lesotho", SAPCountryCode = "LES", ISO3CountryCode = "LSO", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Madagascar", SAPCountryCode = "MAD", ISO3CountryCode = "MDG", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Malawi", SAPCountryCode = "MWI", ISO3CountryCode = "MWI", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Maldives", SAPCountryCode = "MLD", ISO3CountryCode = "MDV", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Mauritius", SAPCountryCode = "MAU", ISO3CountryCode = "MUS", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Mayotte", SAPCountryCode = "MAY", ISO3CountryCode = "MYT", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Mozambique", SAPCountryCode = "MOS", ISO3CountryCode = "MOZ", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Namibia", SAPCountryCode = "NAM", ISO3CountryCode = "NAM", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Nigeria", SAPCountryCode = "NIA", ISO3CountryCode = "NGA", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Rwanda", SAPCountryCode = "RWA", ISO3CountryCode = "RWA", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Saint Helena", SAPCountryCode = "STH", ISO3CountryCode = "", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "South Africa", SAPCountryCode = "RSA", ISO3CountryCode = "ZAF", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Swaziland", SAPCountryCode = "SWL", ISO3CountryCode = "SWZ", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Tanzania", SAPCountryCode = "TAN", ISO3CountryCode = "TZA", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Uganda", SAPCountryCode = "UGA", ISO3CountryCode = "UGA", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Zambia", SAPCountryCode = "SAM", ISO3CountryCode = "ZMB", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Zimbabwe", SAPCountryCode = "SIM", ISO3CountryCode = "ZWE", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Andorra", SAPCountryCode = "AND", ISO3CountryCode = "AND", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Spain", SAPCountryCode = "SPA", ISO3CountryCode = "ESP", CountryGroup = spainCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = spainCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Turkey", SAPCountryCode = "TRK", ISO3CountryCode = "TUR", CountryGroup = turkeyCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = turkeyCG.RegionId, AssignedToMultiVendor = false }
            };

            var currencyId = this.repositorySet.GetRepository<Currency>().GetAll().First(c => c.Name == "EUR").Id;
            foreach (var country in countries)
            {
                country.CurrencyId = currencyId;
            }

            this.repositorySet.GetRepository<Country>().Save(countries);
            this.repositorySet.Sync();
        }

        private void CreateCdCsConfiguration()
        {
            var russiaConfig = new CdCsConfiguration()
            {
                CountryId = 41,
                FileWebUrl = "http://emeia.fujitsu.local/02/sites/p/Migration-GDC",
                FileFolderUrl = "/02/sites/p/Migration-GDC/Shared Documents/CD_CS calculation tool interface/Russia"
            };
            var germanyConfig = new CdCsConfiguration()
            {
                CountryId = 113,
                FileWebUrl = "http://emeia.fujitsu.local/02/sites/p/Migration-GDC",
                FileFolderUrl = "/02/sites/p/Migration-GDC/Shared Documents/CD_CS calculation tool interface/Germany"
            };

            this.repositorySet.GetRepository<CdCsConfiguration>().Save(new List<CdCsConfiguration>()
            {
                russiaConfig,
                germanyConfig
            });
            this.repositorySet.Sync();
        }
    }
}
