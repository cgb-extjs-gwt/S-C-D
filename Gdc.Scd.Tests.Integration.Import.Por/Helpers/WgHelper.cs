using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Tests.Integration.Import.Por.Helpers
{
    public class WgHelper
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
    }
}
