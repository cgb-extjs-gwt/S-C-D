using System;
using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Entities;
using Gdc.Scd.DataAccessLayer.SqlBuilders.Interfaces;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Impl
{
    public class CaseSqlBuilder : ISqlBuilder
    {
        public ISqlBuilder Input { get; set; }

        public IEnumerable<CaseItem> Cases { get; set; }

        public ISqlBuilder Else { get; set; }

        public string Build(SqlBuilderContext context)
        {
            var input = this.Input.Build(context);
            var cases = string.Join(
                Environment.NewLine, 
                this.Cases.Select(caseItem => this.BuildCaseItem(caseItem, context)));

            return $"CASE {input}{Environment.NewLine}{cases}{Environment.NewLine}END";
        }

        public IEnumerable<ISqlBuilder> GetChildrenBuilders()
        {
            foreach (var caseItem in this.Cases)
            {
                yield return caseItem.When;
                yield return caseItem.Then;
            }

            if (this.Else != null)
            {
                yield return this.Else;
            }
        }

        private string BuildCaseItem(CaseItem caseItem, SqlBuilderContext context)
        {
            var when = caseItem.When.Build(context);
            var then = caseItem.Then.Build(context);

            return $"WHEN {when} THEN {then}";
        }
    }
}
