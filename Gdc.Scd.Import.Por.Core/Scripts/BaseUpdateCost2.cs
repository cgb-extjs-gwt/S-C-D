using Gdc.Scd.Core.Entities;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public partial class BaseUpdateCost
    {
        protected NamedId[] items;

        protected string field;

        public BaseUpdateCost(NamedId[] items)
        {
            if(items == null || items.Length == 0)
            {
                throw new System.ArgumentException("empty items");
            }
            this.items = items;
        }

        public string ByCentralContractGroup()
        {
            return For("CentralContractGroup");
        }

        public string ByPla()
        {
            return For("Pla");
        }

        protected void PrintNames()
        {
            for (var i = 0; i < items.Length; i++)
            {
                if (i > 0)
                {
                    Write(", ");
                }
                Write("'");
                Write(items[i].Name.ToUpper());
                Write("'");
            }
        }

        private string For(string f)
        {
            this.field = f;
            this.GenerationEnvironment.Clear();
            return this.TransformText();
        }
    }
}
