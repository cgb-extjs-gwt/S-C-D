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
using Gdc.Scd.Core.Meta.Helpers;
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

        private readonly ICostBlockRepository costBlockRepository;

        public TestDataCreationHandlercs(
            DomainEnitiesMeta entityMetas,
            EntityFrameworkRepositorySet repositorySet,
            ICostBlockRepository costBlockRepository)
        {
            this.entityMetas = entityMetas;
            this.repositorySet = repositorySet;
            this.costBlockRepository = costBlockRepository;
        }

        public void Handle()
        {
            this.CreateCentralContractGroup();
            this.CreatePlas();
            this.CreateServiceLocations();
            this.CreateUserAndRoles();
            this.CreateReactionTimeTypeAvalability();
            this.CreateRegions();
            this.CreateCurrenciesAndExchangeRates();
            this.CreateCountries();
            this.CreateYears();
            this.CreateDurationAvailability();
            this.CreateProActiveSla();
            this.CreateImportConfiguration();
            this.CreateRolecodes();
            this.CreateSoftwereInputLevels();
            this.CreateDisabledRows();

            //report
            this.CreateReportColumnTypes();
            this.CreateReportFilterTypes();
            this.CreateCdCsConfiguration();

            this.FillCostBlocks();

            var queries = new List<SqlHelper>();
            queries.AddRange(this.BuildFromFile(@"Scripts.availabilityFee.sql"));

            queries.AddRange(this.BuildFromFile(@"Scripts.country.sql"));

            queries.AddRange(this.BuildFromFile(@"Scripts.portfolio-func.sql"));

            queries.AddRange(this.BuildFromFile(@"Scripts.calculation-hw.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.calculation-sw.sql"));

            queries.AddRange(this.BuildFromFile(@"Scripts.Report.reports.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-list.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-calc-output-new-vs-old.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-calc-output-vs-FREEZE.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-calc-parameter-hw-not-approved.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-calc-parameter-hw.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-calc-parameter-proactive.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-contract.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-flat-fee.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-hdd-retention-central.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-hdd-retention-country.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-hdd-retention-parameter.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-hdd-retention-calc-result.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-locap.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-locap-detailed.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-locap-support-pack.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-Logistic-cost-calc-central.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-logistic-cost-calc-country.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-logistic-cost-central.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-logistic-cost-country.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-Logistic-cost-input-central.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-Logistic-cost-input-country.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-po-standard-warranty.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-proactive.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-solution-pack-price-list.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-solution-pack-price-list-detail.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-solutionpack-proactive-costing.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-SW-Service-Price-List.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-SW-Service-Price-List-detail.sql"));

            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-hw-calc-result.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-SW-calc-result.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-SW-proactive-calc-result.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.Report.report-SW-param-overview.sql"));

            queries.AddRange(this.BuildFromFile(@"Scripts.CD_CS.split-string.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.CD_CS.cd-cs-hdd-retention.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.CD_CS.cd-cs-proactive.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.CD_CS.cd-cs-servicecosts.sql"));
            queries.AddRange(this.BuildFromFile(@"Scripts.triggers.sql"));
            foreach (var query in queries)
            {
                this.repositorySet.ExecuteSql(query);
            }
        }

        private void CreateDurationAvailability()
        {
            var years = this.GetDurations();
            var availabilities = this.repositorySet.GetRepository<Availability>().GetAll().ToArray();
            var durationAvailabilityRepository = this.repositorySet.GetRepository<DurationAvailability>();

            foreach (var availability in availabilities)
            {
                foreach (var year in years)
                {
                    durationAvailabilityRepository.Save(new DurationAvailability
                    {
                        Availability = availability,
                        Year = year
                    });
                }
            }

            repositorySet.Sync();
        }

        private void CreateDisabledRows()
        {
            var baseDisabledType = typeof(BaseDisabledEntity);
            var queries =
                this.repositorySet.GetRegisteredEntities()
                                  .Where(type => baseDisabledType.IsAssignableFrom(type))
                                  .Select(type => new
                                  {
                                      EntityInfo = MetaHelper.GetEntityInfo(type),
                                      ReferenceInfos =
                                        type.GetProperties()
                                            .Where(prop => !prop.PropertyType.IsPrimitive && prop.CanRead && prop.CanWrite)
                                            .Select(prop => new
                                            {
                                                Column = $"{prop.Name}Id",
                                                EntityInfo = MetaHelper.GetEntityInfo(prop.PropertyType)
                                            })
                                            .ToArray()
                                  })
                                  .Select(info =>
                                  {
                                      var selectExceptColumns = new List<BaseColumnInfo>(info.ReferenceInfos.Select(refInfo => new ColumnInfo(refInfo.Column, null, refInfo.Column)))
                                      {
                                          new QueryColumnInfo(new RawSqlBuilder{ RawSql = "1" }, nameof(BaseDisabledEntity.IsDisabled))
                                      };

                                      var insertColumns = selectExceptColumns.Select(column => column.Alias).ToArray();

                                      var firstRefInfo = info.ReferenceInfos[0].EntityInfo;
                                      var crossJoinColumns = info.ReferenceInfos.Select(refInfo => new ColumnInfo(MetaConstants.IdFieldKey, refInfo.EntityInfo.Name, refInfo.Column)).ToArray();
                                      var crossJoinQuery =
                                          info.ReferenceInfos.Skip(1)
                                                             .Aggregate(
                                                                 Sql.Select(crossJoinColumns).From(firstRefInfo.Name, firstRefInfo.Schema),
                                                                 (acc, refInfo) => acc.Join(refInfo.EntityInfo.Schema, refInfo.EntityInfo.Name, null, JoinType.Cross));

                                      var currentDataQuery =
                                          Sql.Select(info.ReferenceInfos.Select(refInfo => refInfo.Column).ToArray())
                                             .From(info.EntityInfo.Name, info.EntityInfo.Schema);

                                      return
                                          Sql.Insert(info.EntityInfo.Schema, info.EntityInfo.Name, insertColumns)
                                             .Query(
                                                Sql.Select(selectExceptColumns.ToArray())
                                                   .FromQuery(
                                                       Sql.Except(crossJoinQuery, currentDataQuery),
                                                       "t"));
                                  });

            this.repositorySet.ExecuteSql(Sql.Queries(queries));
        }

        private void CreateYears()
        {
            //Insert Years
            var yearRepository = repositorySet.GetRepository<Year>();
            yearRepository.Save(GetYears());
            repositorySet.Sync();
        }

        private void CreateUserAndRoles()
        {
            var costEditorPermission = new Permission { Name = PermissionConstants.CostEditor };
            var tableViewPermission = new Permission { Name = PermissionConstants.TableView };
            var costImportPermission = new Permission { Name = PermissionConstants.CostImport };
            var approvalPermission = new Permission { Name = PermissionConstants.Approval };
            var ownApprovalPermission = new Permission { Name = PermissionConstants.OwnApproval };
            var portfolioPermission = new Permission { Name = PermissionConstants.Portfolio };
            var reviewProcessPermission = new Permission { Name = PermissionConstants.ReviewProcess };
            var reportPermission = new Permission { Name = PermissionConstants.Report };
            var adminPermission = new Permission { Name = PermissionConstants.Admin };
            var calcResultHddServiceCostNotApprovedPermission = new Permission { Name = PermissionConstants.CalcResultHddServiceCostNotApproved };
            var calcResultSoftwareSolutionServiceCostNotApprovedPermission = new Permission { Name = PermissionConstants.CalcResultSoftwareServiceCostNotApproved };

            var allPermissions = new List<Permission>
            {
                costEditorPermission,
                tableViewPermission,
                approvalPermission,
                costImportPermission,
                ownApprovalPermission,
                portfolioPermission,
                reviewProcessPermission,
                reportPermission,
                adminPermission,
                calcResultHddServiceCostNotApprovedPermission,
                calcResultSoftwareSolutionServiceCostNotApprovedPermission
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
                        new RolePermission { Permission = costImportPermission },
                        new RolePermission { Permission = calcResultSoftwareSolutionServiceCostNotApprovedPermission },
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
                        new RolePermission { Permission = portfolioPermission },
                        new RolePermission { Permission = ownApprovalPermission },
                        new RolePermission { Permission = reviewProcessPermission },
                        new RolePermission { Permission = costImportPermission },
                        new RolePermission { Permission = costImportPermission },
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
                        new RolePermission { Permission = costImportPermission },
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
                        new RolePermission { Permission = costImportPermission },
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
                        new RolePermission { Permission = costImportPermission },
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
                new ProActiveSla { Name = "0", ExternalName = MetaConstants.NoneValue },
                new ProActiveSla { Name = "1", ExternalName = "with autocall" },
                new ProActiveSla { Name = "2", ExternalName = "with 1x System Health Check & Patch Information incl. remote Technical Account Management (per year)" },
                new ProActiveSla { Name = "3", ExternalName = "with 2x System Health Check & Patch Information incl. remote Technical Account Management (per year)",
                    LocalPreparationShcRepetition =1, LocalRegularUpdateReadyRepetition=1, CentralExecutionShcReportRepetition=2, LocalRemoteShcCustomerBriefingRepetition=2
                },
                new ProActiveSla { Name = "4", ExternalName = "with 4x System Health Check & Patch Information incl. remote Technical Account Management (per year)",
                    LocalPreparationShcRepetition =1, LocalRegularUpdateReadyRepetition=1, CentralExecutionShcReportRepetition=4, LocalRemoteShcCustomerBriefingRepetition=4
                },
                new ProActiveSla { Name = "6", ExternalName = "with 2x System Health Check & Patch Information incl. onsite Technical Account Management (per year)",
                    LocalPreparationShcRepetition =1, LocalRegularUpdateReadyRepetition=1, CentralExecutionShcReportRepetition=2, LocalRemoteShcCustomerBriefingRepetition=0,
                    TravellingTimeRepetition=2, LocalOnsiteShcCustomerBriefingRepetition=4
                },
                new ProActiveSla { Name = "7", ExternalName = "with 4x System Health Check & Patch Information incl. onsite Technical Account Management (per year)",
                    LocalPreparationShcRepetition =1, LocalRegularUpdateReadyRepetition=1, CentralExecutionShcReportRepetition=4, LocalRemoteShcCustomerBriefingRepetition=0,
                    TravellingTimeRepetition=2, LocalOnsiteShcCustomerBriefingRepetition=4
                }
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
            var sfabs = this.BuildDeactivatableTestItems<SFab>(count).ToArray();

            for (var i = 0; i < count; i++)
            {
                sfabs[i].PlaId = plas[i].Id;
            }

            this.repositorySet.GetRepository<SFab>().Save(sfabs);
            this.repositorySet.Sync();

            for (var i = 0; i < count; i++)
            {
                sogs[i].PlaId = plas[i].Id;
                sogs[i].SFabId = sfabs[i].Id;
            }

            this.repositorySet.GetRepository<Sog>().Save(sogs);
            this.repositorySet.Sync();

            var swDigit = this.BuildDeactivatableTestItems<SwDigit>(count).ToArray();

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

        private void FillCostBlocks()
        {
            foreach (var costBlock in this.entityMetas.CostBlocks)
            {
                this.costBlockRepository.UpdateByCoordinates(costBlock);
            }
        }

        private void CreateReactionTimeTypeAvalability()
        {
            var twoBusinessDay = new ReactionTime { Name = "2nd Business Day", ExternalName = "SBD" };
            var nbd = new ReactionTime { Name = "NBD", ExternalName = "NBD" };
            var fourHour = new ReactionTime { Name = "4h", ExternalName = "4h" };
            var twentyFourHour = new ReactionTime { Name = "24h", ExternalName = "24h" };
            var eightHour = new ReactionTime { Name = "8h", ExternalName = "8h" };
            var noneTime = new ReactionTime { Name = MetaConstants.NoneValue, ExternalName = MetaConstants.NoneValue };

            var response = new ReactionType { Name = "response", ExternalName = "response" };
            var recovery = new ReactionType { Name = "recovery", ExternalName = "recovery" };
            var noneType = new ReactionType { Name = MetaConstants.NoneValue, ExternalName = MetaConstants.NoneValue };

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
                new ReactionTimeType { ReactionTime = noneTime, ReactionType = noneType },
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
            return
                new BracketsSqlBuilder
                {
                    Query =
                        Sql.Select(IdFieldMeta.DefaultId)
                           .From(table, MetaConstants.DependencySchema)
                           .Where(SqlOperators.Equals(MetaConstants.NameFieldKey, name))
                           .ToSqlBuilder()
                };
        }

        private IEnumerable<SqlHelper> BuildFromFile(string fn)
        {
            return Regex.Split(ReadText(fn), @"[\r\n]+go[\s]*[\r\n]*", RegexOptions.IgnoreCase)
                               .Where(x => !string.IsNullOrWhiteSpace(x))
                               .Select(x => new SqlHelper(new RawSqlBuilder() { RawSql = x }));
        }

        private void CreateCentralContractGroup()
        {
            var repository = this.repositorySet.GetRepository<CentralContractGroup>();

            repository.Save(new List<CentralContractGroup>
            {
                new CentralContractGroup {Code = "NA", Name = "UNASSIGNED" },
                new CentralContractGroup {Code = "CG350", Name = "CENTRICSTOR" },
                new CentralContractGroup {Code = "CG110", Name = "CLIENTS ENTRY" },
                new CentralContractGroup {Code = "CG130", Name = "CLIENTS HIGHEND" },
                new CentralContractGroup {Code = "CG120", Name = "CLIENTS MIDRANGE" },
                new CentralContractGroup {Code = "CG100", Name = "CLIENTS SUBENTRY/ SWAP / EXCHANGE" },
                new CentralContractGroup {Code = "CG041", Name = "DISPLAY W/O ODM" },
                new CentralContractGroup {Code = "CG270", Name = "ENTERPRISE SERVER HIGHEND" },
                new CentralContractGroup {Code = "CG260", Name = "ENTERPRISE SERVER MIDRANGE" },
                new CentralContractGroup {Code = "CG050", Name = "PERIPHERALS" },
                new CentralContractGroup {Code = "CG070", Name = "PRINTER" },
                new CentralContractGroup {Code = "CG510", Name = "RETAIL PRODUCTS ENTRY" },
                new CentralContractGroup {Code = "CG040", Name = "DISPLAYS" },
                new CentralContractGroup {Code = "CG500", Name = "RETAIL SUBENTRY/ SWAP / EXCHANGE" },
                new CentralContractGroup {Code = "CG060", Name = "SECURITY DEVICES" },
                new CentralContractGroup {Code = "CG200", Name = "SERVER  SUBENTRY/ SWAP / EXCHANGE" },
                new CentralContractGroup {Code = "CG210", Name = "SERVER ENTRY" },
                new CentralContractGroup {Code = "CG220", Name = "SERVER MIDRANGE" },
                new CentralContractGroup {Code = "CG230", Name = "SERVER HIGHEND" },
                new CentralContractGroup {Code = "CG320", Name = "STORAGE MIDRANGE" },
                new CentralContractGroup {Code = "CG310", Name = "STORAGE ENTRY" },
                new CentralContractGroup {Code = "CG330", Name = "STORAGE HIGHEND" },
                new CentralContractGroup {Code = "CG540", Name = "THIRD PARTY VENDORS" },

            });

            this.repositorySet.Sync();
        }

        private void CreatePlas()
        {
            var repository = this.repositorySet.GetRepository<CentralContractGroup>();

            var na = repository.GetAll().First(c => c.Code == "NA").Id;
            var centricStor = repository.GetAll().First(c => c.Code == "CG350").Id;
            var clientsEntry = repository.GetAll().First(c => c.Code == "CG110").Id;
            var clientsHighend = repository.GetAll().First(c => c.Code == "CG130").Id;
            var clientsMidrange = repository.GetAll().First(c => c.Code == "CG120").Id;
            var clientsSubentry = repository.GetAll().First(c => c.Code == "CG100").Id;
            var displayODM = repository.GetAll().First(c => c.Code == "CG041").Id;
            var enterpriseServerHighend = repository.GetAll().First(c => c.Code == "CG270").Id;
            var enterpriseServerMidrange = repository.GetAll().First(c => c.Code == "CG260").Id;
            var peripherals = repository.GetAll().First(c => c.Code == "CG050").Id;
            var pribter = repository.GetAll().First(c => c.Code == "CG070").Id;
            var retailProducts = repository.GetAll().First(c => c.Code == "CG510").Id;
            var displays = repository.GetAll().First(c => c.Code == "CG040").Id;
            var retailSubentry = repository.GetAll().First(c => c.Code == "CG500").Id;
            var securityDevices = repository.GetAll().First(c => c.Code == "CG060").Id;
            var serverSubentry = repository.GetAll().First(c => c.Code == "CG200").Id;
            var serverEntry = repository.GetAll().First(c => c.Code == "CG210").Id;
            var serverMidrange = repository.GetAll().First(c => c.Code == "CG220").Id;
            var serverHighend = repository.GetAll().First(c => c.Code == "CG230").Id;
            var storageMidrange = repository.GetAll().First(c => c.Code == "CG320").Id;
            var storageEntry = repository.GetAll().First(c => c.Code == "CG310").Id;
            var storageHighend = repository.GetAll().First(c => c.Code == "CG330").Id;
            var thirdPartyVendors = repository.GetAll().First(c => c.Code == "CG540").Id;

            var clusterPlas = new List<ClusterPla>
            {
                new ClusterPla { Name = "CCD", Plas = new List<Pla>
                {
                    new Pla
                {
                    Name = "DESKTOP AND WORKSTATION",
                    CodingPattern = "SME",

                    WarrantyGroups = new List<Wg>
                    {
                        new Wg
                        {
                            Name = "TC4",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsEntry
                        },
                        new Wg
                        {
                            Name = "TC5",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsHighend
                        },
                        new Wg
                        {
                            Name = "TC6",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsHighend
                        },
                        new Wg
                        {
                            Name = "TC8",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsEntry
                        },
                        new Wg
                        {
                            Name = "TC7",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsEntry
                        },
                        new Wg
                        {
                            Name = "U05",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsEntry
                        },
                        new Wg
                        {
                            Name = "U11",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsHighend
                        },
                        new Wg
                        {
                            Name = "U13",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsHighend
                        },
                        new Wg
                        {
                            Name = "WSJ",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsMidrange
                        },
                        new Wg
                        {
                            Name = "WSN",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsHighend
                        },
                        new Wg
                        {
                            Name = "WSS",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsHighend
                        },
                        new Wg
                        {
                            Name = "WSW",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsMidrange
                        },
                        new Wg
                        {
                            Name = "U02",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsMidrange
                        },
                        new Wg
                        {
                            Name = "U06",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsEntry
                        },
                        new Wg
                        {
                            Name = "U07",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsMidrange
                        },
                        new Wg
                        {
                            Name = "U12",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsMidrange
                        },
                        new Wg
                        {
                            Name = "U14",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsEntry
                        },
                        new Wg
                        {
                            Name = "WRC",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsHighend
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
                            Name = "HMD",
                            WgType = WgType.Por,
                            CentralContractGroupId = peripherals
                        },
                        new Wg
                        {
                            Name = "NB6",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsMidrange
                        },
                        new Wg
                        {
                            Name = "NB1",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsMidrange
                        },
                        new Wg
                        {
                            Name = "NB2",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsMidrange
                        },
                        new Wg
                        {
                            Name = "NB5",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsMidrange
                        },
                        new Wg
                        {
                            Name = "ND3",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsMidrange
                        },
                        new Wg
                        {
                            Name = "NC1",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsHighend
                        },
                        new Wg
                        {
                            Name = "NC3",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsHighend
                        },
                        new Wg
                        {
                            Name = "NC9",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsMidrange
                        },
                        new Wg
                        {
                            Name = "TR7",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsMidrange
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
                            Name = "DPE",
                            WgType = WgType.Por,
                            CentralContractGroupId = displays
                        },
                        new Wg
                        {
                            Name = "DPH",
                            WgType = WgType.Por,
                            CentralContractGroupId = displays
                        },
                        new Wg
                        {
                            Name = "DPM",
                            WgType = WgType.Por,
                            CentralContractGroupId = displays
                        },
                        new Wg
                        {
                            Name = "DPX",
                            WgType = WgType.Por,
                            CentralContractGroupId = displayODM
                        },
                        new Wg
                        {
                            Name = "IOA",
                            WgType = WgType.Por,
                            CentralContractGroupId = peripherals
                        },
                        new Wg
                        {
                            Name = "IOB",
                            WgType = WgType.Por,
                            CentralContractGroupId = peripherals
                        },
                        new Wg
                        {
                            Name = "IOC",
                            WgType = WgType.Por,
                            CentralContractGroupId = peripherals
                        },
                        new Wg
                        {
                            Name = "MD1",
                            WgType = WgType.Por,
                            CentralContractGroupId = peripherals
                        },
                        new Wg
                        {
                            Name = "PSN",
                            WgType = WgType.Por,
                            CentralContractGroupId = securityDevices
                        },
                        new Wg
                        {
                            Name = "SB2",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsEntry
                        },
                        new Wg
                        {
                            Name = "SB3",
                            WgType = WgType.Por,
                            CentralContractGroupId = clientsEntry
                        },
                    }
                },
                    new Pla { Name = "RETAIL PRODUCTS", CodingPattern = "RETA"}
                }},
                new ClusterPla { Name = "STORAGE", Plas = new List<Pla>
                {
                    new Pla
                {
                    Name = "STORAGE PRODUCTS",
                    CodingPattern = "STOR",
                    WarrantyGroups = new List<Wg>
                    {
                        new Wg
                        {
                            Name = "CD1",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "CD2",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "CE1",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "CE2",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "CD4",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "CD5",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "CD6",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "CD7",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "CDD",
                            WgType = WgType.Por,
                            CentralContractGroupId = na
                        },
                        new Wg
                        {
                            Name = "CD8",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "CD9",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "C70",
                            WgType = WgType.Por,
                            CentralContractGroupId = centricStor
                        },
                        new Wg
                        {
                            Name = "CS8",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "C74",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "C75",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "CS7",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageMidrange
                        },
                        new Wg
                        {
                            Name = "CS1",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "CS2",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "CS3",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "C16",
                            WgType = WgType.Por,
                            CentralContractGroupId = centricStor
                        },
                        new Wg
                        {
                            Name = "C18",
                            WgType = WgType.Por,
                            CentralContractGroupId = centricStor
                        },
                        new Wg
                        {
                            Name = "C33",
                            WgType = WgType.Por,
                            CentralContractGroupId = centricStor
                        },
                        new Wg
                        {
                            Name = "CS5",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "CS4",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "CS6",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageMidrange
                        },
                        new Wg
                        {
                            Name = "CS9",
                            WgType = WgType.Por,
                            CentralContractGroupId = na
                        },
                        new Wg
                        {
                            Name = "C96",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "C97",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "C98",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "C71",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageMidrange
                        },
                        new Wg
                        {
                            Name = "C73",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageMidrange
                        },
                        new Wg
                        {
                            Name = "C80",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageMidrange
                        },
                        new Wg
                        {
                            Name = "C84",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageMidrange
                        },
                        new Wg
                        {
                            Name = "F58",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "F40",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "F48",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "F53",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "F54",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "F57",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "F41",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "F49",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "F42",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "F43",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "F44",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "F45",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "F50",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "F51",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "F52",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "F36",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "F46",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "F47",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "F56",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "F28",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageMidrange
                        },
                        new Wg
                        {
                            Name = "F29",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "F35",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "F55",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageHighend
                        },
                        new Wg
                        {
                            Name = "S14",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "S17",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "S15",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "S16",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "S50",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "S51",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "S18",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "S35",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "S36",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageMidrange
                        },
                        new Wg
                        {
                            Name = "S37",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageMidrange
                        },
                        new Wg
                        {
                            Name = "S39",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "S40",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "S55",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "VSH",
                            WgType = WgType.Por,
                            CentralContractGroupId = na
                        },
                    }
                },
                } },
                new ClusterPla { Name = "SERVER", Plas = new List<Pla>{
                    new Pla
                {
                    Name = "X86 / IA SERVER",
                    CodingPattern = "SSHI",
                    WarrantyGroups = new List<Wg>
                    {
                        new Wg
                        {
                            Name = "MN1",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "MN4",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "PQ8",
                            WgType = WgType.Por,
                            CentralContractGroupId = enterpriseServerHighend
                        },
                        new Wg
                        {
                            Name = "Y01",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverHighend
                        },
                        new Wg
                        {
                            Name = "Y15",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "PX1",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "PY1",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "PY4",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "Y09",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "Y12",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "MN2",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "PX2",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "PX3",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "PXS",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "PY2",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "PY3",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "SD2",
                            WgType = WgType.Por,
                            CentralContractGroupId = na
                        },
                        new Wg
                        {
                            Name = "Y03",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y17",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y21",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y32",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y06",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y13",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y28",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y30",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y31",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y37",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y38",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y39",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y40",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "PX6",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "PX8",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverHighend
                        },
                        new Wg
                        {
                            Name = "PRC",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "RTE",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y07",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y16",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "Y18",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y25",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y26",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y27",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y33",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "Y36",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverMidrange
                        },
                        new Wg
                        {
                            Name = "S41",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "S42",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "S43",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "S44",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "S45",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "S46",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "S47",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "S48",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "S49",
                            WgType = WgType.Por,
                            CentralContractGroupId = serverEntry
                        },
                        new Wg
                        {
                            Name = "S52",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "S53",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "S54",
                            WgType = WgType.Por,
                            CentralContractGroupId = storageEntry
                        },
                        new Wg
                        {
                            Name = "PQ0",
                            WgType = WgType.Por,
                            CentralContractGroupId = enterpriseServerHighend
                        },
                        new Wg
                        {
                            Name = "PQ5",
                            WgType = WgType.Por,
                            CentralContractGroupId = enterpriseServerHighend
                        },
                        new Wg
                        {
                            Name = "PQ9",
                            WgType = WgType.Por,
                            CentralContractGroupId = enterpriseServerHighend
                        },
                    }
                },
                    new Pla { Name = "UNIX SERVER", CodingPattern = "UNIX" }
                } }
            };

            this.repositorySet.GetRepository<ClusterPla>().Save(clusterPlas);
            this.repositorySet.Sync();

            var plas = new Pla[]
            {
                new Pla { Name = "EPS MAINFRAME PRODUCTS", CodingPattern = "EPSM"},
                new Pla { Name = "UNASSIGNED", CodingPattern = "ZZZZ"}
            };
            this.repositorySet.GetRepository<Pla>().Save(plas);
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
                new ReportColumnType { Name = "money" }
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
                new ReportFilterType { Name = "wg" , MultiSelect = false },
                new ReportFilterType { Name = "sog" , MultiSelect = false },
                new ReportFilterType { Name = "countrygroup" , MultiSelect = false },
                new ReportFilterType { Name = "country" , MultiSelect = false },
                new ReportFilterType { Name = "availability" , MultiSelect = false },
                new ReportFilterType { Name = "duration" , MultiSelect = false },
                new ReportFilterType { Name = "reactiontime" , MultiSelect = false },
                new ReportFilterType { Name = "reactiontype" , MultiSelect = false },
                new ReportFilterType { Name = "servicelocation" , MultiSelect = false },
                new ReportFilterType { Name = "year" , MultiSelect = false },
                new ReportFilterType { Name = "proactive" , MultiSelect = false },
                new ReportFilterType { Name = "usercountry" , MultiSelect = false },
                new ReportFilterType { Name = "swdigit" , MultiSelect = false },
                new ReportFilterType { Name = "swdigitsog" , MultiSelect = false },
                new ReportFilterType { Name = "wgall" , MultiSelect = false },
                new ReportFilterType { Name = "wgstandard" , MultiSelect = false },

                new ReportFilterType { Name = "wg" , MultiSelect = true },
                new ReportFilterType { Name = "country" , MultiSelect = true },
                new ReportFilterType { Name = "availability" , MultiSelect = true },
                new ReportFilterType { Name = "duration" , MultiSelect = true },
                new ReportFilterType { Name = "reactiontime" , MultiSelect = true },
                new ReportFilterType { Name = "reactiontype" , MultiSelect = true },
                new ReportFilterType { Name = "servicelocation" , MultiSelect = true },
                new ReportFilterType { Name = "year" , MultiSelect = true },
                new ReportFilterType { Name = "proactive" , MultiSelect = true },
                new ReportFilterType { Name = "swdigit" , MultiSelect = true },
                new ReportFilterType { Name = "swdigitsog" , MultiSelect = true },
                new ReportFilterType { Name = "wgstandard" , MultiSelect = true }
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
                new ServiceLocation {Name = "Material/Spares Service", ExternalName = "Material/Spares", Order = 1 },
                new ServiceLocation {Name = "Bring-In Service", ExternalName = "Bring-In", Order = 2 },
                new ServiceLocation {Name = "Send-In / Return-to-Base Service", ExternalName = "Send-In/Return-to-Base Service", Order = 3 },
                new ServiceLocation {Name = "Collect & Return Service", ExternalName = "Collect & Return", Order = 4 },
                new ServiceLocation {Name = "Collect & Return-Display Service", ExternalName = "Collect & Return-Display Service", Order = 5 },
                new ServiceLocation {Name = "Door-to-Door Exchange Service", ExternalName = "Door-to-Door Exchange", Order = 6 },
                new ServiceLocation {Name = "Desk-to-Desk Exchange Service", ExternalName = "Desk-to-Desk Exchange", Order = 7 },
                new ServiceLocation {Name = "On-Site Service", ExternalName = "On-Site Service", Order = 8 },
                new ServiceLocation {Name = "On-Site Exchange Service", ExternalName = "On-Site Exchange", Order = 9 },
                new ServiceLocation {Name = "Remote", ExternalName = "Remote Service", Order = 10 },

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
                new Currency { Name =  "DZD" },
                new Currency { Name =  "AUD" },
                new Currency { Name =  "BHD" },
                new Currency { Name =  "BDT" },
                new Currency { Name =  "BBD" },
                new Currency { Name =  "BRL" },
                new Currency { Name =  "BGN" },
                new Currency { Name =  "CAD" },
                new Currency { Name =  "CNY" },
                new Currency { Name =  "CRC" },
                new Currency { Name =  "HRK" },
                new Currency { Name =  "CZK" },
                new Currency { Name =  "DKK" },
                new Currency { Name =  "AED" },
                new Currency { Name =  "EGP" },
                new Currency { Name =  "HKD" },
                new Currency { Name =  "HUF" },
                new Currency { Name =  "ISK" },
                new Currency { Name =  "INR" },
                new Currency { Name =  "IDR" },
                new Currency { Name =  "JMD" },
                new Currency { Name =  "JPY" },
                new Currency { Name =  "KZT" },
                new Currency { Name =  "MKD" },
                new Currency { Name =  "MYR" },
                new Currency { Name =  "MXN" },
                new Currency { Name =  "MAD" },
                new Currency { Name =  "NZD" },
                new Currency { Name =  "NOK" },
                new Currency { Name =  "OMR" },
                new Currency { Name =  "PKR" },
                new Currency { Name =  "PHP" },
                new Currency { Name =  "PLN" },
                new Currency { Name =  "QAR" },
                new Currency { Name =  "RON" },
                new Currency { Name =  "RUB" },
                new Currency { Name =  "SAR" },
                new Currency { Name =  "RSD" },
                new Currency { Name =  "SGD" },
                new Currency { Name =  "ZAR" },
                new Currency { Name =  "KRW" },
                new Currency { Name =  "LKR" },
                new Currency { Name =  "SEK" },
                new Currency { Name =  "CHF" },
                new Currency { Name =  "TWD" },
                new Currency { Name =  "THB" },
                new Currency { Name =  "TTD" },
                new Currency { Name =  "TND" },
                new Currency { Name =  "TRY" },
                new Currency { Name =  "GBP" },
                new Currency { Name =  "UAH" },
                new Currency { Name =  "USD" },
                new Currency { Name =  "UZS" },
                new Currency { Name =  "VND" }
            };
        }

        private Duration[] GetDurations()
        {
            var years = repositorySet.GetRepository<Year>().GetAll().ToArray();
            var durs = new Duration[]
            {
                new Duration { Name = "1 Year", Value = 1, IsProlongation = false, ExternalName = "1 year" },
                new Duration { Name = "2 Years", Value = 2, IsProlongation = false, ExternalName = "2 years"},
                new Duration { Name = "3 Years", Value = 3, IsProlongation = false, ExternalName = "3 years" },
                new Duration { Name = "4 Years", Value = 4, IsProlongation = false, ExternalName = "4 years" },
                new Duration { Name = "5 Years", Value = 5, IsProlongation = false, ExternalName = "5 years" },
                new Duration { Name = "Prolongation", Value = 1, IsProlongation = true, ExternalName = "1 year (P)" }
            };

            for (var i = 0; i < durs.Length; i++)
            {
                var dur = durs[i];
                dur.Year = years.First(x => x.IsProlongation == dur.IsProlongation && x.Value == dur.Value);
            }

            return durs;
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
                ImportMode = Core.Enums.ImportMode.Automatic,
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
                FilePath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\LogisticsCost",
                FileName = "FeeCalculator-Upload_*.txt",
                ImportMode = Core.Enums.ImportMode.Automatic,
                ProcessedDateTime = null,
                Occurancy = Core.Enums.Occurancy.PerMonth,
                ProcessedFilesPath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\LogisticsCost\processed",
                Delimeter = "|",
                HasHeader = true,
                Culture = "de-DE"
            };

            var ebis_afr = new ImportConfiguration
            {
                Name = ImportSystems.EBIS_AFR,
                FilePath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\EBIS",
                FileName = "SCD_FR_LOAD.csv",
                ImportMode = Core.Enums.ImportMode.Automatic,
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
                ImportMode = Core.Enums.ImportMode.Automatic,
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
                ImportMode = Core.Enums.ImportMode.Automatic,
                ProcessedDateTime = null,
                Occurancy = Core.Enums.Occurancy.PerMonth,
                ProcessedFilesPath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\EBIS\processed",
                Delimeter = ";",
                HasHeader = true,
                Culture = "en-US"
            };

            var sfabs = new ImportConfiguration
            {
                Name = ImportSystems.SFABS,
                FilePath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\Software_Solution",
                FileName = "SWSolution_WG to SFAB mapping.csv",
                ImportMode = Core.Enums.ImportMode.Automatic,
                ProcessedDateTime = null,
                Occurancy = Core.Enums.Occurancy.PerWeek,
                ProcessedFilesPath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\Software_Solution\processed",
                Delimeter = ";",
                HasHeader = true,
                Culture = "de-DE"
            };

            var exRates = new ImportConfiguration
            {
                Name = ImportSystems.EXRATES,
                FilePath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\ExchangeRates",
                FileName = "Exchange Rate Import.csv",
                ImportMode = Core.Enums.ImportMode.Automatic,
                ProcessedDateTime = null,
                Occurancy = Core.Enums.Occurancy.PerMonth,
                ProcessedFilesPath = @"\\fsc.net\DFSRoot\PDB\Groups\Service_cost_db\ExchangeRates\processed",
                Delimeter = ";",
                HasHeader = true,
                Culture = "de-DE"
            };

            this.repositorySet.GetRepository<ImportConfiguration>().Save(new List<ImportConfiguration>()
            {
                taxAndDuties,
                logistic,
                ebis_afr,
                ebis_install_base,
                ebis_material_cost,
                sfabs,
                exRates
            });

            this.repositorySet.Sync();
        }

        private void CreateRegions()
        {
            var crAsia = new ClusterRegion { Name = "Asia" };
            var crEmeia = new ClusterRegion { Name = "EMEIA", IsEmeia = true };
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
                LUTCode = "FUJ",
                AutoUploadInstallBase = false
            };
            var chinaCG = new CountryGroup
            {
                Name = "China",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "CN",
                AutoUploadInstallBase = false
            };
            var hongKongCG = new CountryGroup
            {
                Name = "Hong Kong",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "HK",
                AutoUploadInstallBase = false
            };
            var indonesiaCG = new CountryGroup
            {
                Name = "Indonesia",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "IO",
                AutoUploadInstallBase = false
            };
            var koreaCG = new CountryGroup
            {
                Name = "Korea, South",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "KR",
                AutoUploadInstallBase = false
            };
            var malaysiaCG = new CountryGroup
            {
                Name = "Malaysia",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "MY",
                AutoUploadInstallBase = false
            };
            var pilippinesCG = new CountryGroup
            {
                Name = "Philippines",
                RegionId = asiaRegion.Id,
                LUTCode = "ASP",
                AutoUploadInstallBase = false
            };
            var singaporeCG = new CountryGroup
            {
                Name = "Singapore",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "SG",
                AutoUploadInstallBase = false
            };
            var taiwanCG = new CountryGroup
            {
                Name = "Taiwan",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "TW",
                AutoUploadInstallBase = false
            };
            var thailandCG = new CountryGroup
            {
                Name = "Thailand",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "TH",
                AutoUploadInstallBase = false
            };
            var vietnamCG = new CountryGroup
            {
                Name = "Vietnam",
                RegionId = asiaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "VN",
                AutoUploadInstallBase = false
            };
            var austriaCG = new CountryGroup
            {
                Name = "Austria",
                RegionId = ceRegion.Id,
                LUTCode = "OES",
                CountryDigit = "AT",
                AutoUploadInstallBase = false
            };
            var germanyCG = new CountryGroup
            {
                Name = "Germany",
                RegionId = ceRegion.Id,
                LUTCode = "D",
                CountryDigit = "DE",
                AutoUploadInstallBase = false
            };
            var suisseCG = new CountryGroup
            {
                Name = "Suisse",
                RegionId = ceRegion.Id,
                LUTCode = "SWZ",
                CountryDigit = "CH",
                AutoUploadInstallBase = false
            };
            var japanCG = new CountryGroup
            {
                Name = "Japan",
                RegionId = japanRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "JP",
                AutoUploadInstallBase = false
            };
            var brazilCG = new CountryGroup
            {
                Name = "Brazil",
                RegionId = laRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "BR",
                AutoUploadInstallBase = false
            };
            var chileCG = new CountryGroup
            {
                Name = "Chile",
                RegionId = laRegion.Id,
                LUTCode = "FUJ",
                AutoUploadInstallBase = false
            };
            var colombiaCG = new CountryGroup
            {
                Name = "Colombia",
                RegionId = laRegion.Id,
                LUTCode = "FUJ",
                AutoUploadInstallBase = false
            };
            var denmarkCG = new CountryGroup
            {
                Name = "Denmark",
                RegionId = nordicRegion.Id,
                LUTCode = "DAN",
                CountryDigit = "ND;DK",
                AutoUploadInstallBase = true
            };
            var finlandCG = new CountryGroup
            {
                Name = "Finland",
                RegionId = nordicRegion.Id,
                LUTCode = "FIN",
                CountryDigit = "ND;FI",
                AutoUploadInstallBase = true
            };
            var norwayCG = new CountryGroup
            {
                Name = "Norway",
                RegionId = nordicRegion.Id,
                LUTCode = "NOR",
                CountryDigit = "ND;NO",
                AutoUploadInstallBase = true
            };
            var swedenCG = new CountryGroup
            {
                Name = "Sweden",
                RegionId = nordicRegion.Id,
                LUTCode = "SWD",
                CountryDigit = "ND;SE",
                AutoUploadInstallBase = true
            };
            var australiaCG = new CountryGroup
            {
                Name = "Australia",
                RegionId = oceaniaRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "AU",
                AutoUploadInstallBase = false
            };
            var newZealnadCG = new CountryGroup
            {
                Name = "New Zealand",
                RegionId = oceaniaRegion.Id,
                LUTCode = "FUJ",
                AutoUploadInstallBase = false
            };
            var ukCG = new CountryGroup
            {
                Name = "UK",
                RegionId = ukRegion.Id,
                LUTCode = "GBR",
                CountryDigit = "GB",
                AutoUploadInstallBase = true
            };
            var mexicoCG = new CountryGroup
            {
                Name = "Mexico",
                RegionId = usRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "MX",
                AutoUploadInstallBase = false
            };
            var usCG = new CountryGroup
            {
                Name = "United States",
                RegionId = usRegion.Id,
                LUTCode = "FUJ",
                CountryDigit = "US",
                AutoUploadInstallBase = false
            };
            var belgiumCG = new CountryGroup
            {
                Name = "Belgium",
                RegionId = wemeiaRegion.Id,
                LUTCode = "BEL",
                CountryDigit = "BE",
                AutoUploadInstallBase = true
            };
            var czechCG = new CountryGroup
            {
                Name = "Czech Republic",
                RegionId = eeraRegion.Id,
                LUTCode = "CRE",
                CountryDigit = "CZ",
                AutoUploadInstallBase = true
            };
            var franceCG = new CountryGroup
            {
                Name = "France",
                RegionId = wemeiaRegion.Id,
                LUTCode = "FKR",
                CountryDigit = "FR",
                AutoUploadInstallBase = true
            };
            var greeceCG = new CountryGroup
            {
                Name = "Greece",
                RegionId = wemeiaRegion.Id,
                LUTCode = "GRI",
                CountryDigit = "GR",
                AutoUploadInstallBase = true
            };
            var hungaryCG = new CountryGroup
            {
                Name = "Hungary",
                RegionId = wemeiaRegion.Id,
                LUTCode = "HU",
                AutoUploadInstallBase = true
            };
            var indiaCG = new CountryGroup
            {
                Name = "India",
                RegionId = wemeiaRegion.Id,
                LUTCode = "IND",
                CountryDigit = "ID",
                AutoUploadInstallBase = true
            };
            var israelCG = new CountryGroup
            {
                Name = "Israel",
                RegionId = wemeiaRegion.Id,
                LUTCode = "ISR",
                CountryDigit = "IL",
                AutoUploadInstallBase = true
            };
            var italyCG = new CountryGroup
            {
                Name = "Italy",
                RegionId = wemeiaRegion.Id,
                LUTCode = "ITL",
                CountryDigit = "IT",
                AutoUploadInstallBase = true
            };
            var luxembourgCG = new CountryGroup
            {
                Name = "Luxembourg",
                RegionId = wemeiaRegion.Id,
                LUTCode = "LUX",
                CountryDigit = "LU",
                AutoUploadInstallBase = true
            };
            var mdeCG = new CountryGroup
            {
                Name = "MDE",
                RegionId = wemeiaRegion.Id,
                LUTCode = "MDE",
                CountryDigit = "ME",
                AutoUploadInstallBase = true
            };
            var netherlandsCG = new CountryGroup
            {
                Name = "Netherlands",
                RegionId = wemeiaRegion.Id,
                LUTCode = "NDL",
                CountryDigit = "NL",
                AutoUploadInstallBase = true
            };
            var noaCG = new CountryGroup
            {
                Name = "NOA",
                RegionId = wemeiaRegion.Id,
                LUTCode = "NOA",
                CountryDigit = "NA",
                AutoUploadInstallBase = true
            };
            var polandCG = new CountryGroup
            {
                Name = "Poland",
                RegionId = wemeiaRegion.Id,
                LUTCode = "POL",
                CountryDigit = "PL",
                AutoUploadInstallBase = true
            };
            var portugalCG = new CountryGroup
            {
                Name = "Portugal",
                RegionId = wemeiaRegion.Id,
                LUTCode = "POR",
                CountryDigit = "PT",
                AutoUploadInstallBase = true
            };
            var russiaCG = new CountryGroup
            {
                Name = "Russia",
                RegionId = wemeiaRegion.Id,
                LUTCode = "RUS",
                CountryDigit = "RU",
                AutoUploadInstallBase = true
            };
            var seeCG = new CountryGroup
            {
                Name = "SEE",
                RegionId = wemeiaRegion.Id,
                LUTCode = "SEE",
                CountryDigit = "EE",
                AutoUploadInstallBase = true
            };
            var southAfricaCG = new CountryGroup
            {
                Name = "South Africa",
                RegionId = wemeiaRegion.Id,
                LUTCode = "RSA",
                CountryDigit = "ZA",
                AutoUploadInstallBase = true
            };
            var spainCG = new CountryGroup
            {
                Name = "Spain",
                RegionId = wemeiaRegion.Id,
                LUTCode = "SPA",
                CountryDigit = "ES",
                AutoUploadInstallBase = true
            };
            var turkeyCG = new CountryGroup
            {
                Name = "Turkey",
                RegionId = wemeiaRegion.Id,
                LUTCode = "TRK",
                CountryDigit = "TR",
                AutoUploadInstallBase = true
            };

            #endregion

            var currencies = this.repositorySet.GetRepository<Currency>().GetAll().ToList();

            var eur = currencies.First(c => c.Name.Equals("EUR", StringComparison.OrdinalIgnoreCase)).Id;
            var dzd = currencies.First(c => c.Name.Equals("DZD", StringComparison.OrdinalIgnoreCase)).Id;
            var aud = currencies.First(c => c.Name.Equals("AUD", StringComparison.OrdinalIgnoreCase)).Id;
            var bhd = currencies.First(c => c.Name.Equals("BHD", StringComparison.OrdinalIgnoreCase)).Id;
            var bdt = currencies.First(c => c.Name.Equals("BDT", StringComparison.OrdinalIgnoreCase)).Id;
            var bbd = currencies.First(c => c.Name.Equals("BBD", StringComparison.OrdinalIgnoreCase)).Id;
            var brl = currencies.First(c => c.Name.Equals("BRL", StringComparison.OrdinalIgnoreCase)).Id;
            var bgn = currencies.First(c => c.Name.Equals("BGN", StringComparison.OrdinalIgnoreCase)).Id;
            var cad = currencies.First(c => c.Name.Equals("CAD", StringComparison.OrdinalIgnoreCase)).Id;
            var cny = currencies.First(c => c.Name.Equals("CNY", StringComparison.OrdinalIgnoreCase)).Id;
            var crc = currencies.First(c => c.Name.Equals("CRC", StringComparison.OrdinalIgnoreCase)).Id;
            var hrk = currencies.First(c => c.Name.Equals("HRK", StringComparison.OrdinalIgnoreCase)).Id;
            var czk = currencies.First(c => c.Name.Equals("CZK", StringComparison.OrdinalIgnoreCase)).Id;
            var dkk = currencies.First(c => c.Name.Equals("DKK", StringComparison.OrdinalIgnoreCase)).Id;
            var aed = currencies.First(c => c.Name.Equals("AED", StringComparison.OrdinalIgnoreCase)).Id;
            var egp = currencies.First(c => c.Name.Equals("EGP", StringComparison.OrdinalIgnoreCase)).Id;
            var hkd = currencies.First(c => c.Name.Equals("HKD", StringComparison.OrdinalIgnoreCase)).Id;
            var huf = currencies.First(c => c.Name.Equals("HUF", StringComparison.OrdinalIgnoreCase)).Id;
            var isk = currencies.First(c => c.Name.Equals("ISK", StringComparison.OrdinalIgnoreCase)).Id;
            var inr = currencies.First(c => c.Name.Equals("INR", StringComparison.OrdinalIgnoreCase)).Id;
            var idr = currencies.First(c => c.Name.Equals("IDR", StringComparison.OrdinalIgnoreCase)).Id;
            var jmd = currencies.First(c => c.Name.Equals("JMD", StringComparison.OrdinalIgnoreCase)).Id;
            var jpy = currencies.First(c => c.Name.Equals("JPY", StringComparison.OrdinalIgnoreCase)).Id;
            var kzt = currencies.First(c => c.Name.Equals("KZT", StringComparison.OrdinalIgnoreCase)).Id;
            var mkd = currencies.First(c => c.Name.Equals("MKD", StringComparison.OrdinalIgnoreCase)).Id;
            var myr = currencies.First(c => c.Name.Equals("MYR", StringComparison.OrdinalIgnoreCase)).Id;
            var mxn = currencies.First(c => c.Name.Equals("MXN", StringComparison.OrdinalIgnoreCase)).Id;
            var mad = currencies.First(c => c.Name.Equals("MAD", StringComparison.OrdinalIgnoreCase)).Id;
            var nzd = currencies.First(c => c.Name.Equals("NZD", StringComparison.OrdinalIgnoreCase)).Id;
            var nok = currencies.First(c => c.Name.Equals("NOK", StringComparison.OrdinalIgnoreCase)).Id;
            var omr = currencies.First(c => c.Name.Equals("OMR", StringComparison.OrdinalIgnoreCase)).Id;
            var pkr = currencies.First(c => c.Name.Equals("PKR", StringComparison.OrdinalIgnoreCase)).Id;
            var php = currencies.First(c => c.Name.Equals("PHP", StringComparison.OrdinalIgnoreCase)).Id;
            var pln = currencies.First(c => c.Name.Equals("PLN", StringComparison.OrdinalIgnoreCase)).Id;
            var qar = currencies.First(c => c.Name.Equals("QAR", StringComparison.OrdinalIgnoreCase)).Id;
            var ron = currencies.First(c => c.Name.Equals("RON", StringComparison.OrdinalIgnoreCase)).Id;
            var rub = currencies.First(c => c.Name.Equals("RUB", StringComparison.OrdinalIgnoreCase)).Id;
            var sar = currencies.First(c => c.Name.Equals("SAR", StringComparison.OrdinalIgnoreCase)).Id;
            var rsd = currencies.First(c => c.Name.Equals("RSD", StringComparison.OrdinalIgnoreCase)).Id;
            var sgd = currencies.First(c => c.Name.Equals("SGD", StringComparison.OrdinalIgnoreCase)).Id;
            var zar = currencies.First(c => c.Name.Equals("ZAR", StringComparison.OrdinalIgnoreCase)).Id;
            var krw = currencies.First(c => c.Name.Equals("KRW", StringComparison.OrdinalIgnoreCase)).Id;
            var lkr = currencies.First(c => c.Name.Equals("LKR", StringComparison.OrdinalIgnoreCase)).Id;
            var sek = currencies.First(c => c.Name.Equals("SEK", StringComparison.OrdinalIgnoreCase)).Id;
            var chf = currencies.First(c => c.Name.Equals("CHF", StringComparison.OrdinalIgnoreCase)).Id;
            var twd = currencies.First(c => c.Name.Equals("TWD", StringComparison.OrdinalIgnoreCase)).Id;
            var thb = currencies.First(c => c.Name.Equals("THB", StringComparison.OrdinalIgnoreCase)).Id;
            var ttd = currencies.First(c => c.Name.Equals("TTD", StringComparison.OrdinalIgnoreCase)).Id;
            var tnd = currencies.First(c => c.Name.Equals("TND", StringComparison.OrdinalIgnoreCase)).Id;
            var @try = currencies.First(c => c.Name.Equals("TRY", StringComparison.OrdinalIgnoreCase)).Id;
            var gbp = currencies.First(c => c.Name.Equals("GBP", StringComparison.OrdinalIgnoreCase)).Id;
            var uah = currencies.First(c => c.Name.Equals("UAH", StringComparison.OrdinalIgnoreCase)).Id;
            var usd = currencies.First(c => c.Name.Equals("USD", StringComparison.OrdinalIgnoreCase)).Id;
            var uzs = currencies.First(c => c.Name.Equals("UZS", StringComparison.OrdinalIgnoreCase)).Id;
            var vnd = currencies.First(c => c.Name.Equals("VND", StringComparison.OrdinalIgnoreCase)).Id;

            var countries = new Country[]
            {
                new Country { Name = "China", CurrencyId = cny,  SAPCountryCode = "CHN", ISO3CountryCode = "CHN", CountryGroup = chinaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = chinaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Hong Kong", CurrencyId = hkd, SAPCountryCode = "HGK", ISO3CountryCode = "HKG", CountryGroup = hongKongCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = hongKongCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Indonesia", CurrencyId = idr, SAPCountryCode = "IDS", ISO3CountryCode = "IDN", CountryGroup = indonesiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = indonesiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Korea, South", CurrencyId = krw, SAPCountryCode = "KOR", ISO3CountryCode = "KOR", CountryGroup = koreaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = koreaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Malaysia", CurrencyId = myr, SAPCountryCode = "MAL", ISO3CountryCode = "MYS", CountryGroup = malaysiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = malaysiaCG.RegionId, AssignedToMultiVendor = false},
                new Country { Name = "Philippines", CurrencyId = php, SAPCountryCode = "PHI", ISO3CountryCode = "PHL", CountryGroup = pilippinesCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = asiaClusterId, RegionId = pilippinesCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Singapore", CurrencyId = sgd, SAPCountryCode = "SIN", ISO3CountryCode = "SGP", CountryGroup = singaporeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = singaporeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Taiwan", CurrencyId = twd, SAPCountryCode = "TAI", ISO3CountryCode = "TWN", CountryGroup = taiwanCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = taiwanCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Thailand", CurrencyId = thb, SAPCountryCode = "THA", ISO3CountryCode = "THA", CountryGroup = thailandCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = thailandCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Vietnam", CurrencyId = vnd, SAPCountryCode = "VIT", ISO3CountryCode = "VNM", CountryGroup = vietnamCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = asiaClusterId, RegionId = vietnamCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Austria", SAPCountryCode = "OES", ISO3CountryCode = "AUT", CountryGroup = austriaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = austriaCG.RegionId, AssignedToMultiVendor = true },
                new Country { Name = "Germany", SAPCountryCode = "D", ISO3CountryCode = "DEU", CountryGroup = germanyCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = germanyCG.RegionId, AssignedToMultiVendor = true },
                new Country { Name = "Liechtenstein", SAPCountryCode = "LIC", ISO3CountryCode = "LIE", CountryGroup = suisseCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = suisseCG.RegionId, AssignedToMultiVendor = true },
                new Country { Name = "Switzerland", CurrencyId = chf, SAPCountryCode = "SWZ", ISO3CountryCode = "CHE", CountryGroup = suisseCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = suisseCG.RegionId, AssignedToMultiVendor = true },
                new Country { Name = "Japan", CurrencyId = jpy, SAPCountryCode = "FUJ", ISO3CountryCode = "JPN", CountryGroup = japanCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = japanCluserId, RegionId = japanCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Argentina", SAPCountryCode = "ARG", ISO3CountryCode = "ARG", CountryGroup = argentinaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = laClusterId, RegionId = argentinaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Brazil", CurrencyId = brl, SAPCountryCode = "FUJ", ISO3CountryCode = "BRA", CountryGroup = brazilCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = laClusterId, RegionId = brazilCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Chile", SAPCountryCode = "CHL", ISO3CountryCode = "CHL", CountryGroup = chileCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = laClusterId, RegionId = chileCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Colombia", SAPCountryCode = "KOL", ISO3CountryCode = "COL", CountryGroup = colombiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = laClusterId, RegionId = colombiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Denmark", CurrencyId = dkk, SAPCountryCode = "DAN", ISO3CountryCode = "DNK", CountryGroup = denmarkCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = denmarkCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Faroe Islands", SAPCountryCode = "FAR", ISO3CountryCode = "FRO", CountryGroup = denmarkCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = denmarkCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Greenland", SAPCountryCode = "GRO", ISO3CountryCode = "GRL", CountryGroup = denmarkCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = denmarkCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Iceland", CurrencyId = isk, SAPCountryCode = "ISL", ISO3CountryCode = "ISL", CountryGroup = denmarkCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = denmarkCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Estonia", SAPCountryCode = "EST", ISO3CountryCode = "EST", CountryGroup = finlandCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = finlandCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Finland", SAPCountryCode = "FIN", ISO3CountryCode = "FIN", CountryGroup = finlandCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = finlandCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Latvia", SAPCountryCode = "LET", ISO3CountryCode = "LVA", CountryGroup = finlandCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = finlandCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Lithuania", SAPCountryCode = "LIT", ISO3CountryCode = "LTU", CountryGroup = finlandCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = finlandCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Norway", CurrencyId = nok, SAPCountryCode = "NOR", ISO3CountryCode = "NOR", CountryGroup = norwayCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = norwayCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Sweden", CurrencyId = sek, SAPCountryCode = "SWD", ISO3CountryCode = "SWE", CountryGroup = swedenCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = swedenCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Australia", CurrencyId = aud, SAPCountryCode = "AUS", ISO3CountryCode = "AUS", CountryGroup = australiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = oceaniaClusterId, RegionId = australiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "New Zealand", CurrencyId = nzd, SAPCountryCode = "NSL", ISO3CountryCode = "NZL", CountryGroup = newZealnadCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = oceaniaClusterId, RegionId = newZealnadCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Guernsey", SAPCountryCode = "", ISO3CountryCode = "GGY", CountryGroup = ukCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = ukCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Ireland", SAPCountryCode = "GBR", ISO3CountryCode = "IRL", CountryGroup = ukCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = ukCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Jersey", SAPCountryCode = "", ISO3CountryCode = "JEY", CountryGroup = ukCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = ukCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Man, Isle of", SAPCountryCode = "", ISO3CountryCode = "", CountryGroup = ukCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = ukCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Great Britain", CurrencyId = gbp, SAPCountryCode = "GBR", ISO3CountryCode = "GBR", CountryGroup = ukCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = ukCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Northern Ireland", SAPCountryCode = "", ISO3CountryCode = "", CountryGroup = ukCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = ukCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Mexico", CurrencyId = mxn, SAPCountryCode = "MEX", ISO3CountryCode = "MEX", CountryGroup = mexicoCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = usClusterId, RegionId = mexicoCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "United States", CurrencyId = usd, SAPCountryCode = "FUJ", ISO3CountryCode = "USA", CountryGroup = usCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = usClusterId, RegionId = usCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Belgium", SAPCountryCode = "BEL", ISO3CountryCode = "BEL", CountryGroup = belgiumCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = belgiumCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Czech Republic", CurrencyId = czk, SAPCountryCode = "CRE", ISO3CountryCode = "CZE", CountryGroup = czechCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = czechCG.RegionId, AssignedToMultiVendor = false },
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
                new Country { Name = "Hungary", CurrencyId = huf, SAPCountryCode = "UNG", ISO3CountryCode = "HUN", CountryGroup = hungaryCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = hungaryCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "India", CurrencyId = inr, SAPCountryCode = "IND", ISO3CountryCode = "IND", CountryGroup = indiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = indiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Israel", SAPCountryCode = "ISR", ISO3CountryCode = "ISR", CountryGroup = israelCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = israelCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Italy", SAPCountryCode = "ITL", ISO3CountryCode = "ITA", CountryGroup = italyCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = italyCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "San Marino", SAPCountryCode = "SMA", ISO3CountryCode = "SMR", CountryGroup = italyCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = italyCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Luxembourg", SAPCountryCode = "LUX", ISO3CountryCode = "LUX", CountryGroup = luxembourgCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = luxembourgCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Egypt", CurrencyId = egp, SAPCountryCode = "EGY", ISO3CountryCode = "EGY", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Afghanistan", SAPCountryCode = "AFG", ISO3CountryCode = "AFG", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Bahrain", CurrencyId = bhd, SAPCountryCode = "BAH", ISO3CountryCode = "BHR", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Gaza Strip", SAPCountryCode = "", ISO3CountryCode = "", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Iran", SAPCountryCode = "IRN", ISO3CountryCode = "IRN", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Iraq", SAPCountryCode = "IRK", ISO3CountryCode = "IRQ", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Jordan", SAPCountryCode = "JOR", ISO3CountryCode = "JOR", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Kuwait", SAPCountryCode = "KUW", ISO3CountryCode = "KWT", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Lebanon", SAPCountryCode = "LIB", ISO3CountryCode = "LBN", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Malta", SAPCountryCode = "MTA", ISO3CountryCode = "MLT", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Oman", CurrencyId = omr, SAPCountryCode = "OMA", ISO3CountryCode = "OMN", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Pakistan", CurrencyId = pkr, SAPCountryCode = "PAK", ISO3CountryCode = "PAK", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Qatar", CurrencyId = qar, SAPCountryCode = "KAT", ISO3CountryCode = "QAT", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Saudi Arabia", CurrencyId = sar, SAPCountryCode = "SAR", ISO3CountryCode = "SAU", CountryGroup = mdeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = mdeCG.RegionId, AssignedToMultiVendor = false },
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
                new Country { Name = "Algeria", CurrencyId = dzd, SAPCountryCode = "ALG", ISO3CountryCode = "DZA", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
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
                new Country { Name = "Morocco", CurrencyId = mad, SAPCountryCode = "NOA", ISO3CountryCode = "MAR", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Niger", SAPCountryCode = "NGR", ISO3CountryCode = "NER", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Sao Tome and Principe", SAPCountryCode = "STP", ISO3CountryCode = "", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Senegal", SAPCountryCode = "SEN", ISO3CountryCode = "SEN", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Seychelles", SAPCountryCode = "SEY", ISO3CountryCode = "SYC", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Sierra Leone", SAPCountryCode = "LEO", ISO3CountryCode = "SLE", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Somalia", SAPCountryCode = "SOM", ISO3CountryCode = "SOM", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Sudan", SAPCountryCode = "SUD", ISO3CountryCode = "SDN", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Togo", SAPCountryCode = "TGO", ISO3CountryCode = "TGO", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Tunisia", CurrencyId = eur, SAPCountryCode = "TUN", ISO3CountryCode = "TUN", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Western Sahara", SAPCountryCode = "", ISO3CountryCode = "ESH", CountryGroup = noaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = noaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Poland", CurrencyId = pln, SAPCountryCode = "POL", ISO3CountryCode = "POL", CountryGroup = polandCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = polandCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Portugal", SAPCountryCode = "POR", ISO3CountryCode = "PRT", CountryGroup = portugalCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = portugalCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Armenia", SAPCountryCode = "ARM", ISO3CountryCode = "ARM", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Azerbaijan", SAPCountryCode = "ASE", ISO3CountryCode = "AZE", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Belarus", SAPCountryCode = "WEI", ISO3CountryCode = "BLR", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Georgia", SAPCountryCode = "GEO", ISO3CountryCode = "GEO", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Kazakhstan", CurrencyId = kzt, SAPCountryCode = "KAS", ISO3CountryCode = "KAZ", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Kyrgyzstan", SAPCountryCode = "KGI", ISO3CountryCode = "KGZ", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Moldova", SAPCountryCode = "MOL", ISO3CountryCode = "MDA", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Russia", CurrencyId = rub, SAPCountryCode = "RUS", ISO3CountryCode = "RUS", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Tajikistan", SAPCountryCode = "TAD", ISO3CountryCode = "TJK", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Turkmenistan", SAPCountryCode = "TUR", ISO3CountryCode = "TKM", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Ukraine", CurrencyId = uah, SAPCountryCode = "UKR", ISO3CountryCode = "UKR", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Uzbekistan", CurrencyId = uzs, SAPCountryCode = "USB", ISO3CountryCode = "UZB", CountryGroup = russiaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = russiaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Bulgaria", CurrencyId = bgn, SAPCountryCode = "BUL", ISO3CountryCode = "BGR", CountryGroup = seeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = seeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Croatia", CurrencyId = hrk, SAPCountryCode = "KRO", ISO3CountryCode = "HRV", CountryGroup = seeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = seeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Albania", SAPCountryCode = "ALB", ISO3CountryCode = "ALB", CountryGroup = seeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = seeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Bosnia and Herzegovina", SAPCountryCode = "BOH", ISO3CountryCode = "BIH", CountryGroup = seeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = seeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Macedonia", CurrencyId = mkd, SAPCountryCode = "MAZ", ISO3CountryCode = "MKD", CountryGroup = seeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = seeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Romania", CurrencyId = ron, SAPCountryCode = "RUM", ISO3CountryCode = "ROU", CountryGroup = seeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = seeCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Serbia", CurrencyId = rsd, SAPCountryCode = "SRB", ISO3CountryCode = "SRB", CountryGroup = seeCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = seeCG.RegionId, AssignedToMultiVendor = false },
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
                new Country { Name = "South Africa", CurrencyId = eur, SAPCountryCode = "RSA", ISO3CountryCode = "ZAF", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Swaziland", SAPCountryCode = "SWL", ISO3CountryCode = "SWZ", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Tanzania", SAPCountryCode = "TAN", ISO3CountryCode = "TZA", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Uganda", SAPCountryCode = "UGA", ISO3CountryCode = "UGA", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Zambia", SAPCountryCode = "SAM", ISO3CountryCode = "ZMB", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Zimbabwe", SAPCountryCode = "SIM", ISO3CountryCode = "ZWE", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Andorra", SAPCountryCode = "AND", ISO3CountryCode = "AND", CountryGroup = southAfricaCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = false, ClusterRegionId = emeiaClusterId, RegionId = southAfricaCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Spain", SAPCountryCode = "SPA", ISO3CountryCode = "ESP", CountryGroup = spainCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = spainCG.RegionId, AssignedToMultiVendor = false },
                new Country { Name = "Turkey", CurrencyId = @try, SAPCountryCode = "TRK", ISO3CountryCode = "TUR", CountryGroup = turkeyCG, CanOverrideTransferCostAndPrice = false, CanStoreListAndDealerPrices = false, IsMaster = true, ClusterRegionId = emeiaClusterId, RegionId = turkeyCG.RegionId, AssignedToMultiVendor = false }
            };

            foreach (var country in countries)
            {
                if (country.CurrencyId == 0)
                    country.CurrencyId = eur;
            }

            this.repositorySet.GetRepository<Country>().Save(countries);
            this.repositorySet.Sync();
        }

        private void CreateCdCsConfiguration()
        {
            this.repositorySet.GetRepository<CdCsConfiguration>().Save(new List<CdCsConfiguration>()
            {
                new CdCsConfiguration()
                {
                    CountryId = 41,
                    FileWebUrl = "http://emeia.fujitsu.local/02/sites/p/Migration-GDC",
                    FileFolderUrl = "/02/sites/p/Migration-GDC/Shared Documents/CD_CS calculation tool interface/Russia"
                },
                new CdCsConfiguration()
                {
                    CountryId = 113,
                    FileWebUrl = "http://emeia.fujitsu.local/02/sites/p/Migration-GDC",
                    FileFolderUrl = "/02/sites/p/Migration-GDC/Shared Documents/CD_CS calculation tool interface/Germany"
                }
            });
            this.repositorySet.Sync();
        }
    }
}
