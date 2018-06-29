using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gdc.Scd.Core.Meta.Entities
{
    public class CostBlockEntityMeta : BaseEntityMeta
    {
        //private readonly List<string> inputLevelFieldNames = new List<string>();

        //private readonly List<string> dependencyFieldNames = new List<string>();

        //private readonly List<string> costElementFieldNames = new List<string>();

        //public IdFieldMeta IdField { get; } = new IdFieldMeta();

        //public IEnumerable<ReferenceFieldMeta> InputLevelFields => this.GetFields<ReferenceFieldMeta>(this.inputLevelFieldNames);

        //public IEnumerable<FieldMeta> DependencyFields => this.dependencyFieldNames.Select(name => this.Fields[name]);

        //public IEnumerable<SimpleFieldMeta> CostElementsFields => this.GetFields<SimpleFieldMeta>(this.costElementFieldNames);

        //public CostBlockEntityMeta(string name, string shema = null) 
        //    : base(name, shema)
        //{
        //    this.Fields.Add(this.IdField);
        //}

        //public void AddInputLevelField(ReferenceFieldMeta field)
        //{
        //    this.inputLevelFieldNames.Add(field.Name);
        //    this.Fields.Add(field);
        //}

        //public void AddDependencyField(FieldMeta field)
        //{
        //    this.dependencyFieldNames.Add(field.Name);
        //    this.Fields.Add(field);
        //}

        //public void AddCostElementField(SimpleFieldMeta field)
        //{
        //    this.costElementFieldNames.Add(field.Name);
        //    this.Fields.Add(field);
        //}

        //private  IEnumerable<T> GetFields<T>(IEnumerable<string> fieldNames)
        //    where T : FieldMeta
        //{
        //    return fieldNames.Select(name => this.Fields[name] as T).Where(field => field != null);
        //}

        public IdFieldMeta IdField { get; } = new IdFieldMeta();

        public MetaCollection<ReferenceFieldMeta> InputLevelFields { get; } = new MetaCollection<ReferenceFieldMeta>();

        public MetaCollection<FieldMeta> DependencyFields { get; } = new MetaCollection<FieldMeta>();

        public MetaCollection<SimpleFieldMeta> CostElementsFields { get; } = new MetaCollection<SimpleFieldMeta>();

        public override IEnumerable<FieldMeta> AllFields
        {
            get
            {
                yield return 
            }
        }
    }
}
