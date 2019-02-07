using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.Core.Dto;
using Gdc.Scd.Core.Entities.Approval;

namespace Gdc.Scd.BusinessLogicLayer.Helpers
{
    public static class BundleDetailExtensions
    {
        public static IEnumerable<BundleDetailGroupDto> ToBundleDetailGroups(this IEnumerable<BundleDetail> bundleDetails)
        {
            var groups = bundleDetails.GroupBy(bundleDetail => new
            {
                bundleDetail.HistoryValueId,
                LastInputLevelId = bundleDetail.LastInputLevel.Id,
                bundleDetail.NewValue,
                bundleDetail.OldValue,
                bundleDetail.CountryGroupAvgValue,
                bundleDetail.IsPeriodError,
                bundleDetail.IsRegionError,
            });

            return
                groups.Select(bundleDetailGroup => new BundleDetailGroupDto
                {
                    HistoryValueId = bundleDetailGroup.Key.HistoryValueId,
                    NewValue = bundleDetailGroup.Key.NewValue,
                    OldValue = bundleDetailGroup.Key.OldValue,
                    CountryGroupAvgValue = bundleDetailGroup.Key.CountryGroupAvgValue,
                    IsPeriodError = bundleDetailGroup.Key.IsPeriodError,
                    IsRegionError = bundleDetailGroup.Key.IsRegionError,
                    LastInputLevel = bundleDetailGroup.Select(bundleDetail => bundleDetail.LastInputLevel).First(),
                    Coordinates =
                        bundleDetailGroup.SelectMany(bundleDetail => bundleDetail.InputLevels.Where(inputLevel => inputLevel.Value != bundleDetail.LastInputLevel))
                                         .Concat(bundleDetailGroup.SelectMany(bundleDetail => bundleDetail.Dependencies))
                                         .GroupBy(keyValue => keyValue.Key, keyValue => keyValue.Value)
                                         .ToDictionary(
                                            coordIdGroup => coordIdGroup.Key,
                                            coordIdGroup => 
                                                coordIdGroup.GroupBy(item => item.Id)
                                                            .Select(group => group.First())
                                                            .OrderBy(item => item.Name)
                                                            .ToArray())
                });
        }
    }
}
