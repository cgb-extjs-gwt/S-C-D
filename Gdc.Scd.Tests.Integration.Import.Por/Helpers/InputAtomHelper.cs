using Gdc.Scd.Core.Entities;
using System;
using System.Collections.Generic;

namespace Gdc.Scd.Tests.Integration.Import.Por.Helpers
{
    public class InputAtomHelper
    {
        public static Wg[] CreateWg(params string[] names)
        {
            if (names.Length == 0)
            {
                throw new System.ArgumentException();
            }

            var arr = new Wg[names.Length];
            for (var i = 0; i < names.Length; i++)
            {
                arr[i] = new Wg { Name = names[i] };
            }
            return arr;
        }

        public static SwDigit[] CreateDigit(params string[] names)
        {
            if (names.Length == 0)
            {
                throw new System.ArgumentException();
            }

            var arr = new SwDigit[names.Length];
            for (var i = 0; i < names.Length; i++)
            {
                arr[i] = new SwDigit { Name = names[i] };
            }
            return arr;
        }

        public static SwDigit CreateSwDigit(string[] swDigit, ICollection<SwDigitLicense> swDigitLicense)
        {

            bool convert = (long.TryParse(swDigit[2], out long sogId));
            if (swDigit.Length == 0)
            {
                throw new System.ArgumentException();
            }
            if (convert)
            {
                return new SwDigit { Name = swDigit[0], Description = swDigit[1], ModifiedDateTime = DateTime.Now, SogId=sogId, SwDigitLicenses= swDigitLicense};
            }
            else throw new System.ArgumentException();
        }
    }
}
