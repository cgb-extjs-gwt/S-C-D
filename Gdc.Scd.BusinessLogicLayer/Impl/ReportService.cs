using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Helpers;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.BusinessLogicLayer.Procedures;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Entities.Report;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Parameters;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class ReportService : IReportService
    {
        private static readonly object syncRoot = new object();

        private static readonly string SCHEMA_CACHE_KEY = typeof(ReportService).FullName + ".GetSchemas"; // report and schemas cache

        private readonly IRepositorySet repositorySet;
        private readonly IUserService userService;

        public ReportService(
                IRepositorySet repositorySet,
                IUserService userService
            )
        {
            this.repositorySet = repositorySet;
            this.userService = userService;
        }

        public Task<(Stream data, string fileName)> Excel(long reportId, ReportFilterCollection filter)
        {
            return Excel(GetSchemas().GetSchema(reportId), filter);
        }

        public Task<(Stream data, string fileName)> Excel(string reportName, ReportFilterCollection filter)
        {
            return Excel(GetSchemas().GetSchema(reportName), filter);
        }

        private async Task<(Stream data, string fileName)> Excel(ReportSchema r, ReportFilterCollection filter)
        {
            using (var multi = new GetReport.MultiSheetReport(repositorySet))
            {
                var func = r.Report.SqlFunc;
                var schema = r.AsSchemaDto();
                var fn = FileNameHelper.Excel(schema.Name);
                var parameters = r.FillParameters(filter, userService.GetCurrentUser());
                await multi.ExecuteExcelAsync(schema, func, parameters);

                var schemas = GetSchemas();
                foreach (var p in r.AsParts())
                {
                    r = schemas.GetSchema(p);
                    func = r.Report.SqlFunc;
                    schema = r.AsSchemaDto();
                    parameters = r.FillParameters(filter, userService.GetCurrentUser());
                    await multi.ExecuteExcelAsync(schema, func, parameters);
                }

                return (multi.GetData(), fn);
            }
        }

        public async Task<(string json, int total)> GetJsonArrayData(long reportId, ReportFilterCollection filter, int start, int limit)
        {
            var r = GetSchemas().GetSchema(reportId);
            var func = r.Report.SqlFunc;
            var parameters = r.FillParameters(filter, userService.GetCurrentUser());

            var d = await new GetReport(repositorySet).ExecuteJsonAsync(func, start, limit + 1, parameters);

            return (d.json, d.total < limit ? start + d.total : start + limit + 1);
        }

        public ReportDto[] GetReports()
        {
            return GetSchemas().GetReportDto();
        }

        public ReportSchemaDto GetSchema(long reportId)
        {
            return GetSchemas().GetSchemaDto(reportId);
        }

        public ReportSchemaDto GetSchema(string reportName)
        {
            return GetSchemas().GetSchemaDto(reportName);
        }

        private ReportSchemaCollection GetSchemas()
        {
            var cache = MemoryCache.Default;
            var schema = cache[SCHEMA_CACHE_KEY] as ReportSchemaCollection;

            if (schema == null)
            {
                //double check lock
                lock (syncRoot)
                {
                    if (schema == null)
                    {
                        schema = LoadSchemas();
                        cache.Set(SCHEMA_CACHE_KEY, schema, DateTime.Now.AddMinutes(15));
                    }
                }
            }

            return schema;
        }

        private ReportSchemaCollection LoadSchemas()
        {
            var collection = new ReportSchemaCollection();

            var reports = repositorySet.GetRepository<Report>().GetAll().ToArray();

            var columns = repositorySet.GetRepository<ReportColumn>()
                                    .GetAll()
                                    .Select(x => new ReportColumn
                                    {
                                        Id = x.Id,
                                        Index = x.Index,
                                        Name = x.Name,
                                        Text = x.Text,
                                        Report = new Report { Id = x.Report.Id },
                                        AllowNull = x.AllowNull,
                                        Flex = x.Flex,
                                        Format = x.Format,
                                        Type = new ReportColumnType
                                        {
                                            Id = x.Type.Id,
                                            Name = x.Type.Name
                                        }
                                    })
                                    .ToLookup(x => x.Report.Id);

            var filters = repositorySet.GetRepository<ReportFilter>()
                                    .GetAll()
                                    .Select(x => new ReportFilter
                                    {
                                        Id = x.Id,
                                        Index = x.Index,
                                        Name = x.Name,
                                        Text = x.Text,
                                        Report = new Report { Id = x.Report.Id },
                                        Value = x.Value,
                                        Type = new ReportFilterType
                                        {
                                            Id = x.Type.Id,
                                            Name = x.Type.Name,
                                            MultiSelect = x.Type.MultiSelect,
                                            ExecSql = x.Type.ExecSql
                                        }
                                    })
                                    .ToLookup(x => x.Report.Id);

            var parts = repositorySet.GetRepository<ReportPart>()
                                     .GetAll()
                                     .Select(x => new ReportPart
                                     {
                                         Id = x.Id,
                                         Report = new Report { Id = x.Report.Id },
                                         Part = new Report { Id = x.Part.Id },
                                         Index = x.Index
                                     })
                                     .ToLookup(x => x.Report.Id);

            var EMPTY_COLUMNS = new ReportColumn[0];
            var EMPTY_FILTERS = new ReportFilter[0];
            var EMPTY_PARTS = new ReportPart[0];

            for (var i = 0; i < reports.Length; i++)
            {
                var r = reports[i];
                var cols = columns.Contains(r.Id) ? columns[r.Id].OrderBy(x => x.Index).ToArray() : EMPTY_COLUMNS;
                var fils = filters.Contains(r.Id) ? filters[r.Id].OrderBy(x => x.Index).ToArray() : EMPTY_FILTERS;
                var ps = parts.Contains(r.Id) ? parts[r.Id].OrderBy(x => x.Index).ToArray() : EMPTY_PARTS;

                collection.Add(r.Id, new ReportSchema(r, cols, fils, ps));
            }

            return collection;
        }
    }

    internal class ReportSchemaCollection : Dictionary<long, ReportSchema>
    {
        public ReportSchema GetSchema(long reportId)
        {
            if (ContainsKey(reportId))
            {
                return this[reportId];
            }
            throw new ArgumentException("Schema not found");
        }

        public ReportSchema GetSchema(string name)
        {
            var result = Values.FirstOrDefault(x => string.Compare(x.Report.Name, name, true) == 0);
            if (result == null)
            {
                throw new ArgumentException("Schema not found");
            }
            return result;
        }

        public ReportSchemaDto GetSchemaDto(long reportId)
        {
            return GetSchema(reportId).AsSchemaDto();
        }

        public ReportSchemaDto GetSchemaDto(string name)
        {
            return GetSchema(name).AsSchemaDto();
        }

        public ReportDto[] GetReportDto()
        {
            var result = new ReportDto[this.Count];
            int i = 0;

            foreach (var x in this)
            {
                result[i++] = x.Value.AsReportDto();
            }

            return result;
        }
    }

    internal class ReportSchema
    {
        private Report report;

        private ReportColumn[] columns;

        private ReportFilter[] filters;

        private ReportPart[] parts;

        public ReportSchema(
                Report report,
                ReportColumn[] columns,
                ReportFilter[] filters,
                ReportPart[] parts
            )
        {
            this.report = report;
            this.columns = columns;
            this.filters = filters;
            this.parts = parts;
        }

        public Report Report { get { return report; } }

        public ReportSchemaDto AsSchemaDto()
        {
            return new ReportSchemaDto
            {
                Id = report.Id,
                Name = report.Name,
                Title = report.Title,
                Fields = AsFields(),
                Filter = AsFilter()
            };
        }

        public ReportDto AsReportDto()
        {
            return new ReportDto
            {
                Id = report.Id,
                Name = report.Name,
                Title = report.Title,
                CountrySpecific = report.CountrySpecific,
                HasFreesedVersion = report.HasFreesedVersion
            };
        }

        public ReportColumnDto[] AsFields()
        {
            int len = columns.Length;
            var result = new ReportColumnDto[len];

            for (var i = 0; i < len; i++)
            {
                var x = columns[i];
                result[i] = new ReportColumnDto
                {
                    TypeId = x.Type.Id,
                    Type = x.Type.Name,
                    Name = x.Name,
                    Text = x.Text,
                    AllowNull = x.AllowNull,
                    Flex = x.Flex,
                    Format = x.Format
                };
            }

            return result;
        }

        public ReportFilterDto[] AsFilter()
        {
            int len = filters.Length;
            var result = new ReportFilterDto[len];

            for (var i = 0; i < len; i++)
            {
                var x = filters[i];
                result[i] = new ReportFilterDto
                {
                    MultiSelect = x.Type.MultiSelect,
                    TypeId = x.Type.Id,
                    Type = x.Type.Name,
                    Name = x.Name,
                    Text = x.Text,
                    Value = x.Value
                };
            }

            return result;
        }

        public long[] AsParts()
        {
            int len = parts.Length;
            var result = new long[len];

            for (var i = 0; i < len; i++)
            {
                result[i] = parts[i].Part.Id;
            }

            return result;
        }

        public DbParameter[] FillParameters(ReportFilterCollection src, User user)
        {
            int len = filters.Length;
            var result = new DbParameter[len];

            for (var i = 0; i < len; i++)
            {
                result[i] = FillParameter(filters[i], src, user);
            }

            return result;
        }

        public DbParameter FillParameter(ReportFilter f, ReportFilterCollection src, User user)
        {
            var builder = new DbParameterBuilder();

            builder.WithName(f.Name);

            object value;

            if (f.Type.IsLogin())
            {
                //inject user login as parameter
                builder.WithValue(user.Login);
            }
            else if (f.Type.MultiSelect)
            {
                long[] ids;
                src.TryGetVal(f.Name, out ids);
                builder.WithListIdValue(ids);
            }
            else if (src.TryGetVal(f.Name, out value))
            {
                builder.WithValue(value);
            }
            else
            {
                builder.WithNull();
            }

            return builder.Build();
        }
    }
}
