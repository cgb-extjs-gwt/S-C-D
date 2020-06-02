using Gdc.Scd.BusinessLogicLayer.Dto;
using Gdc.Scd.BusinessLogicLayer.Interfaces;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities;
using Gdc.Scd.DataAccessLayer.Helpers;
using Gdc.Scd.DataAccessLayer.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace Gdc.Scd.BusinessLogicLayer.Impl
{
    public class CountryAdminService : ICountryAdminService
    {
        private readonly IRepositorySet _repositorySet;

        private readonly IRepository<Country> _countryRepo;

        private readonly IRepository<Currency> _currencyRepo;

        public CountryAdminService(
                IRepositorySet repositorySet,
                IRepository<Country> countryRepo,
                IRepository<Currency> currencyRepo
            )
        {
            _repositorySet = repositorySet;
            _countryRepo = countryRepo;
            _currencyRepo = currencyRepo;
        }

        public Stream ExportToExcel(AdminCountryFilterDto filter = null)
        {
            var records = GetAll(null, null, out int totalCount, filter);
            const int START_ROW = 1;
            const int START_COLUMN = 1;
            const string COUNTRY_SHEET = "Country Management";

            MemoryStream stream;
            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add(COUNTRY_SHEET);
                var headerInfo = WriteHeaders(sheet, START_ROW, START_COLUMN);
                var dataInfo = WriteData(sheet, headerInfo.EndRow + 1, START_COLUMN);

                for (var column = START_COLUMN; column <= 5; column++)
                {
                    sheet.Column(column).AdjustToContents();
                }

                sheet.Range(headerInfo.EndRow, START_COLUMN, dataInfo.EndRow, dataInfo.EndColumn).SetAutoFilter();

                workbook.SaveAs(stream = new MemoryStream());

                stream.Position = 0;
            }

            return stream;

            (int EndRow, int EndColumn) WriteHeaders(IXLWorksheet sheet, int row, int column)
            {
                var endRow = row + 1;
                sheet.Range(row, column, endRow, column++).Merge().Value = "Country";
                sheet.Range(row, column, endRow, column++).Merge().Value = "Country Group";
                sheet.Range(row, column, endRow, column++).Merge().Value = "Region";
                sheet.Range(row, column, endRow, column++).Merge().Value = "LUT";
                sheet.Range(row, column, endRow, column++).Merge().Value = "Digit";
                sheet.Range(row, column, endRow, column++).Merge().Value = "ISO Code";
                sheet.Range(row, column, endRow, column++).Merge().Value = "Currency Code";
                sheet.Range(row, column, endRow, column++).Merge().Value = "Is Master";
                sheet.Range(row, column, endRow, column++).Merge().Value = "Store List and Dealer Prices";
                sheet.Range(row, column, endRow, column++).Merge().Value = "Override TC and TP";
                sheet.Range(row, column, endRow, column++).Merge().Value = "Override 2nd Level Support local";
                sheet.Range(row, column, endRow, column++).Merge().Value = "Quality Group";

                column--;

                var range = sheet.Range(row, 1, endRow, column);

                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                range.Style.Alignment.WrapText = true;
                range.Style.Font.Bold = true;
                range.Style.Fill.BackgroundColor = XLColor.FromHtml("#8064A2");
                range.Style.Font.FontColor = XLColor.White;

                return (endRow, column);
            }

            (int EndRow, int EndColumn) WriteData(IXLWorksheet sheet, int row, int startColumn)
            {
                var column = startColumn;
                foreach (var record in records)
                {
                    column = startColumn;
                    sheet.Cell(row, column++).Value = record.CountryName;
                    sheet.Cell(row, column++).Value = record.CountryGroup;
                    sheet.Cell(row, column++).Value = record.Region;
                    sheet.Cell(row, column++).Value = record.LUTCode;
                    sheet.Cell(row, column++).Value = record.CountryDigit;
                    sheet.Cell(row, column++).Value = record.ISO3Code;
                    sheet.Cell(row, column++).Value = record.Currency;
                    sheet.Cell(row, column++).Value = FormatBoolField(record.IsMaster);
                    sheet.Cell(row, column++).Value = FormatBoolField(record.CanStoreListAndDealerPrices);
                    sheet.Cell(row, column++).Value = FormatBoolField(record.CanOverrideTransferCostAndPrice);
                    sheet.Cell(row, column++).Value = FormatBoolField(record.CanOverride2ndLevelSupportLocal);
                    sheet.Cell(row, column++).Value = record.QualityGroup;

                    row++;

                }

                return (row - 1, column - 1);
            }

            string FormatBoolField(bool value) => value ? "YES" : "NO";
        }

        public List<CountryDto> GetAll(int? pageNumber, int? limit, out int totalCount, AdminCountryFilterDto filter = null)
        {
            var countries = _countryRepo.GetAll();

            if (filter != null)
            {
                countries = countries.WhereIf(filter.Country != null, x => x.Name == filter.Country)
                                     .WhereIf(filter.Group.HasValue, x => x.CountryGroupId == filter.Group.Value)
                                     .WhereIf(filter.Region.HasValue, x => x.RegionId == filter.Region.Value)
                                     .WhereIf(filter.Lut != null, x => x.CountryGroup.LUTCode == filter.Lut)
                                     .WhereIf(filter.Digit != null, x => x.CountryGroup.CountryDigit == filter.Digit)
                                     .WhereIf(filter.Iso != null, x => x.ISO3CountryCode == filter.Iso)
                                     .WhereIf(filter.QualityGroup != null, x => x.QualityGateGroup == filter.QualityGroup)
                                     .WhereIf(filter.IsMaster.HasValue, x => x.IsMaster == filter.IsMaster.Value)
                                     .WhereIf(filter.StoreListAndDealer.HasValue, x => x.CanStoreListAndDealerPrices == filter.StoreListAndDealer.Value)
                                     .WhereIf(filter.OverrideTCandTP.HasValue, x => x.CanOverrideTransferCostAndPrice == filter.OverrideTCandTP.Value)
                                     .WhereIf(filter.Override2ndLevelSupportLocal.HasValue, x => x.CanOverride2ndLevelSupportLocal == filter.Override2ndLevelSupportLocal.Value);
            }

            totalCount = countries.Count();

            countries = (pageNumber.HasValue && limit.HasValue) ? 
                countries.OrderBy(c => c.Name).Skip((pageNumber.Value - 1) * limit.Value) :
                countries.OrderBy(c => c.Name);

            return countries.Select(c => new CountryDto
            {
                CanOverrideTransferCostAndPrice = c.CanOverrideTransferCostAndPrice,
                CanOverride2ndLevelSupportLocal = c.CanOverride2ndLevelSupportLocal,
                CanStoreListAndDealerPrices = c.CanStoreListAndDealerPrices,
                CountryDigit = c.CountryGroup.CountryDigit ?? string.Empty,
                CountryGroup = c.CountryGroup.Name,
                CountryName = c.Name,
                LUTCode = c.CountryGroup.LUTCode ?? string.Empty,
                ISO3Code = c.ISO3CountryCode ?? string.Empty,
                IsMaster = c.IsMaster,
                QualityGroup = c.QualityGateGroup ?? string.Empty,
                Currency = c.Currency.Name,
                CountryId = c.Id,
                Region = c.Region.Name
            }).OrderBy(x => x.CountryName).ToList();
        }

        public void Save(IEnumerable<CountryDto> countries)
        {
            var countryDict = countries.ToDictionary(c => c.CountryId);
            var keys = countryDict.Select(d => d.Key).ToList();
            var countriesToUpdate = _countryRepo.GetAll().Where(c => keys.Contains(c.Id));
            var currencies = SelectCurrencies(countries);

            foreach (var country in countriesToUpdate)
            {
                var dto = countryDict[country.Id];
                //
                country.CanOverrideTransferCostAndPrice = dto.CanOverrideTransferCostAndPrice;
                country.CanStoreListAndDealerPrices = dto.CanStoreListAndDealerPrices;
                country.CanOverride2ndLevelSupportLocal = dto.CanOverride2ndLevelSupportLocal;
                country.QualityGateGroup = string.IsNullOrEmpty(dto.QualityGroup?.Trim()) ? null : dto.QualityGroup.Trim();
                country.CurrencyId = currencies[dto.Currency].Id;
            }

            _countryRepo.Save(countriesToUpdate);
            _repositorySet.Sync();
        }

        private Dictionary<string, Currency> SelectCurrencies(IEnumerable<CountryDto> countries)
        {
            var keys = countries.Select(x => x.Currency);
            return _currencyRepo.GetAll().Where(c => keys.Contains(c.Name)).ToDictionary(x => x.Name);
        }
    }
}
