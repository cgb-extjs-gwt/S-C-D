using Gdc.Scd.Core.Entities;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using Gdc.Scd.Import.Por.Core.Dto;
using Gdc.Scd.Import.Por.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Por
{
    public static class FormatDataHelper
    {
        /// <summary>
        /// Construct Dictionary with SFab as key and PLA as value
        /// </summary>
        /// <param name="sogs"></param>
        /// <param name="wgs"></param>
        /// <returns></returns>
        public static Dictionary<string, string> FillSFabDictionary(
            IEnumerable<SCD2_ServiceOfferingGroups> sogs,
            IEnumerable<SCD2_WarrantyGroups> wgs)
        {
            var porFabsDictionary = new Dictionary<string, string>();

            foreach (var sog in sogs)
            {
                if (!porFabsDictionary.Keys.Contains(sog.FabGrp, StringComparer.OrdinalIgnoreCase))
                {
                    porFabsDictionary.Add(sog.FabGrp, sog.SOG_PLA);
                }
            }

            foreach (var wg in wgs)
            {
                if (!porFabsDictionary.Keys.Contains(wg.FabGrp, StringComparer.OrdinalIgnoreCase))
                {
                    porFabsDictionary.Add(wg.FabGrp, wg.Warranty_PLA);
                }
            }

            return porFabsDictionary;
        }

        /// <summary>
        /// Construct Dictionaries with keys Software Digits and Software License and SCD2_SW_Overview as value
        /// </summary>
        /// <param name="swInfo"></param>
        /// <returns></returns>
        public static SwHelperModel FillSwInfo(
            IEnumerable<SCD2_SW_Overview> swInfo)
        {
            var swDigitsDictionary = new Dictionary<string, SCD2_SW_Overview>();
            var swLicenseDictionary = new Dictionary<string, SCD2_SW_Overview>();

            foreach (var sw in swInfo)
            {
                if (!swDigitsDictionary.Keys.Contains(sw.Software_Lizenz_Digit, StringComparer.OrdinalIgnoreCase))
                    swDigitsDictionary.Add(sw.Software_Lizenz_Digit, sw);

                if (!swLicenseDictionary.Keys.Contains(sw.Software_Lizenz, StringComparer.OrdinalIgnoreCase))
                    swLicenseDictionary.Add(sw.Software_Lizenz, sw);
            }

            return new SwHelperModel(swDigitsDictionary, swLicenseDictionary);
        }

        /// <summary>
        /// Construct Dictionary with Country Digit and LUT codes as keys and Master CountryId as values
        /// </summary>
        /// <param name="countries"></param>
        /// <param name="countryGroups"></param>
        /// <returns></returns>
        public static Dictionary<string, List<long>> FillCountryDictionary(IEnumerable<Country> countries,
            IEnumerable<CountryGroup> countryGroups)
        {
            var result = new Dictionary<string, List<long>>();
            foreach (var countryGroup in countryGroups)
            {
                if (String.IsNullOrEmpty(countryGroup.CountryDigit) && String.IsNullOrEmpty(countryGroup.LUTCode))
                    continue;

                var countryDigits = new List<string>();

                if (!String.IsNullOrEmpty(countryGroup.LUTCode))
                    countryDigits.AddRange(countryGroup.LUTCode.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim()));

                if (!String.IsNullOrEmpty(countryGroup.CountryDigit))
                    countryDigits.AddRange(countryGroup.CountryDigit.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim()));


                var masterCountries = countries.Where(c => c.CountryGroupId == countryGroup.Id && c.IsMaster);
                if (masterCountries.Any())
                {
                    foreach (var digit in countryDigits)
                    {
                        if (result.Keys.Contains(digit))
                            result[digit].AddRange(masterCountries.Select(c => c.Id));
                        else
                            result.Add(digit, new List<long>(masterCountries.Select(c => c.Id)));

                    }
                }

            }

            return result;
        }

        /// <summary>
        /// Construct SLA Dictionaries with external names as keys and Id as values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="slas"></param>
        /// <returns></returns>
        public static Dictionary<string, long> FillSlaDictionary<T>(IEnumerable<T> slas) where T : ExternalEntity
        {
            var result = new Dictionary<string, long>();
            foreach (var sla in slas)
            {
                var values = sla.ExternalName.Split(';').Select(s => s.Trim());
                foreach (var val in values)
                {
                    result.Add(val, sla.Id);
                }
            }

            return result;
        }

        /// <summary>
        /// Fill SLA Dictionary
        /// </summary>
        /// <returns></returns>
        public static SlaDictsDto FillSlasDictionaries()
        {
            var locationServiceValues = PorService.LocationService.GetAll().ToList();
            var reactionTypeValues = PorService.ReactionTypeService.GetAll().ToList();
            var reactonTimeValues = PorService.ReactionTimeService.GetAll().ToList();
            var availabilityValues = PorService.AvailabilityService.GetAll().ToList();
            var durationValues = PorService.DurationService.GetAll().ToList();

            var locationDictionary = FillSlaDictionary(locationServiceValues);
            var reactionTimeDictionary = FillSlaDictionary(reactonTimeValues);
            var reactionTypeDictionary = FillSlaDictionary(reactionTypeValues);
            var availabilityDictionary = FillSlaDictionary(availabilityValues);
            var durationDictionary = FillSlaDictionary(durationValues);

            return new SlaDictsDto
            {
                Availability = availabilityDictionary,
                Duration = durationDictionary,
                Locations = locationDictionary,
                ReactionTime = reactionTimeDictionary,
                ReactionType = reactionTypeDictionary
            };
        }

    }
}
