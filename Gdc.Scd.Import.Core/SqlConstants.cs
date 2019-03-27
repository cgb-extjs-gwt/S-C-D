using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core
{
    public static class SqlConstants
    {
        public static string GET_ALL_CENTRAL_CONTRACT_GROUPS = @"SELECT ZZWTY_WTY_GRP, ZZWTY_CONTR_GR, 
                                                                        ZZWTY_CONTR_GR_D 
                                                                 FROM [Partner_New].[dbo].[vw_EXT_zwty_wgcg_v]
                                                                 WHERE [VKORG] = ''";
    }
}
