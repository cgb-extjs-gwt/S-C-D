﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 15.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace Gdc.Scd.Import.Por.Core.Scripts
{
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "C:\Dev\SCD\Gdc.Scd.Import.Por.Core\Scripts\UpdateSwCost.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "15.0.0.0")]
    public partial class UpdateSwCost : UpdateCost
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public override string TransformText()
        {
            this.Write("\r\ndeclare @dig dbo.ListID;\r\ninsert into @dig(id) select id from InputAtoms.SwDigi" +
                    "t where Deactivated = 0 and UPPER(name) in (");
            
            #line 4 "C:\Dev\SCD\Gdc.Scd.Import.Por.Core\Scripts\UpdateSwCost.tt"
 WriteNames(); 
            
            #line default
            #line hidden
            this.Write(")\r\n\r\nIF OBJECT_ID(\'tempdb..#tmp\') IS NOT NULL DROP TABLE #tmp;\r\nIF OBJECT_ID(\'tem" +
                    "pdb..#tmpMin\') IS NOT NULL DROP TABLE #tmpMin;\r\n\r\nselect c.* into #tmp\r\nfrom ");
            
            #line 10 "C:\Dev\SCD\Gdc.Scd.Import.Por.Core\Scripts\UpdateSwCost.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(table));
            
            #line default
            #line hidden
            this.Write(" c\r\nwhere c.Deactivated = 0 and not exists(select * from @dig where Id = c.SwDigi" +
                    "t);\r\n\r\ncreate index ix_tmp_Country_SLA on #tmp(");
            
            #line 13 "C:\Dev\SCD\Gdc.Scd.Import.Por.Core\Scripts\UpdateSwCost.tt"
 WriteDeps(); 
            
            #line default
            #line hidden
            this.Write(");\r\n\r\nselect  ");
            
            #line 15 "C:\Dev\SCD\Gdc.Scd.Import.Por.Core\Scripts\UpdateSwCost.tt"
 WriteDeps(); 
            
            #line default
            #line hidden
            this.Write(" \r\n      , ");
            
            #line 16 "C:\Dev\SCD\Gdc.Scd.Import.Por.Core\Scripts\UpdateSwCost.tt"
 WriteSelectFields(); 
            
            #line default
            #line hidden
            this.Write("\r\ninto #tmpMin\r\nfrom #tmp \r\ngroup by ");
            
            #line 20 "C:\Dev\SCD\Gdc.Scd.Import.Por.Core\Scripts\UpdateSwCost.tt"
 WriteDeps(); 
            
            #line default
            #line hidden
            this.Write("\r\ncreate index ix_tmpmin_Country_SLA on #tmpMin(");
            
            #line 22 "C:\Dev\SCD\Gdc.Scd.Import.Por.Core\Scripts\UpdateSwCost.tt"
 WriteDeps(); 
            
            #line default
            #line hidden
            this.Write(");\r\n\r\nupdate c set \r\n        ");
            
            #line 25 "C:\Dev\SCD\Gdc.Scd.Import.Por.Core\Scripts\UpdateSwCost.tt"
 WriteSetFields(); 
            
            #line default
            #line hidden
            this.Write("\r\nfrom ");
            
            #line 27 "C:\Dev\SCD\Gdc.Scd.Import.Por.Core\Scripts\UpdateSwCost.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(table));
            
            #line default
            #line hidden
            this.Write(" c\r\ninner join #tmpMin t on ");
            
            #line 28 "C:\Dev\SCD\Gdc.Scd.Import.Por.Core\Scripts\UpdateSwCost.tt"
 WriteJoinByDeps(); 
            
            #line default
            #line hidden
            this.Write(" \r\nwhere c.Deactivated = 0 and exists(select * from @dig where Id = c.SwDigit);\r\n" +
                    "\r\nIF OBJECT_ID(\'tempdb..#tmp\') IS NOT NULL DROP TABLE #tmp\r\nIF OBJECT_ID(\'tempdb" +
                    "..#tmpMin\') IS NOT NULL DROP TABLE #tmpMin;\r\n\r\n");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
}
