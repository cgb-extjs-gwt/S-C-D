//using System.Linq;
//using Gdc.Scd.Core.Meta.Constants;

//namespace Gdc.Scd.Core.Meta.Entities
//{
//    public class LocalPortfolioEntityMeta : EntityMeta
//    {
//        public LocalPortfolioEntityMeta() 
//            : base(MetaConstants.LocalPortfolioTableName, MetaConstants.PortfolioSchema)
//        {
//        }

//        public ReferenceFieldMeta GetFieldByReferenceMeta(BaseEntityMeta referenceMeta)
//        {
//            return
//                this.Fields.OfType<ReferenceFieldMeta>()
//                           .FirstOrDefault(field => field.ReferenceMeta == referenceMeta);
//        }
//    }
//}
