using Gdc.Scd.BusinessLogicLayer.Dto.Report;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.BusinessLogicLayer.Procedures;
using Gdc.Scd.Core.Entities.Report;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class ReportService : IReportService
    {
        private static readonly object syncRoot = new object();

        private static ReportSchemaCollection cache; //static report and schemas cache

        private readonly IRepositorySet repositorySet;

        private readonly IRepository<Report> reportRepo;

        private readonly IRepository<ReportColumn> columnRepo;

        private readonly IRepository<ReportFilter> filterRepo;

        public ReportService(
                IRepositorySet repositorySet,
                IRepository<Report> reportRepo,
                IRepository<ReportColumn> columnRepo,
                IRepository<ReportFilter> filterRepo
            )
        {
            this.repositorySet = repositorySet;
            this.reportRepo = reportRepo;
            this.columnRepo = columnRepo;
            this.filterRepo = filterRepo;
        }

        public Task<FileStreamDto> Excel(long reportId, ReportFilterCollection filter)
        {
            throw new NotImplementedException();
        }

        public Task<DataTable> GetData(long reportId, ReportFilterCollection filter)
        {
            throw new NotImplementedException();
        }

        public Task<DataTableDto> GetData(long reportId, ReportFilterCollection filter, int start, int limit)
        {
            throw new NotImplementedException();
        }

        public Task<JsonArrayDto> GetJsonArrayData(long reportId, ReportFilterCollection filter, int start, int limit)
        {
            var r = GetSchemas().GetSchema(reportId);
            var func = r.Report.SqlFunc;
            //var parameters = r.AsDbParameters();



            return new GetReport(repositorySet).ExecuteJsonAsync(func, start, limit);
        }

        public ReportDto[] GetReports()
        {
            return GetSchemas().GetReportDto();
        }

        public ReportSchemaDto GetSchema(long reportId)
        {
            return GetSchemas().GetSchemaDto(reportId);
        }

        private ReportSchemaCollection GetSchemas()
        {
            //double check lock

            if (cache == null)
            {
                lock (syncRoot)
                {
                    if (cache == null)
                    {
                        cache = LoadSchemas();
                    }
                }
            }
            return cache;
        }

        private ReportSchemaCollection LoadSchemas()
        {
            var collection = new ReportSchemaCollection();

            var reports = reportRepo.GetAll().ToArray();

            var columns = columnRepo.GetAll()
                                    .Select(x => new ReportColumn
                                    {
                                        Id = x.Id,
                                        Index = x.Index,
                                        Name = x.Name,
                                        Text = x.Text,
                                        Report = new Report { Id = x.Report.Id },
                                        AllowNull = x.AllowNull,
                                        Flex = x.Flex,
                                        Type = new ReportColumnType
                                        {
                                            Id = x.Type.Id,
                                            Name = x.Type.Name
                                        }
                                    })
                                    .ToLookup(x => x.Report.Id);

            var filters = filterRepo.GetAll()
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

            var EMPTY_COLUMNS = new ReportColumn[0];
            var EMPTY_FILTERS = new ReportFilter[0];

            for (var i = 0; i < reports.Length; i++)
            {
                var r = reports[i];
                var cols = columns.Contains(r.Id) ? columns[r.Id].OrderBy(x => x.Index).ToArray() : EMPTY_COLUMNS;
                var fils = filters.Contains(r.Id) ? filters[r.Id].OrderBy(x => x.Index).ToArray() : EMPTY_FILTERS;

                collection.Add(r.Id, new ReportSchema(r, cols, fils));
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

        public ReportSchemaDto GetSchemaDto(long reportId)
        {
            return GetSchema(reportId).AsSchemaDto();
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

        public ReportSchema(
                Report report,
                ReportColumn[] columns,
                ReportFilter[] filters
            )
        {
            this.report = report;
            this.columns = columns;
            this.filters = filters;
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
                    Flex = x.Flex
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

        public DbParameter[] AsDbParameters()
        {
            int len = filters.Length;

            var result = new DbParameter[len];
            for (var i = 0; i < len; i++)
            {

            }
            return result;
        }
    }
}
