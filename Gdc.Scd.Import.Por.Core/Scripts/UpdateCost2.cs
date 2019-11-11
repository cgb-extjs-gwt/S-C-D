using Gdc.Scd.Core.Entities;
using System;

namespace Gdc.Scd.Import.Por.Core.Scripts
{
    public partial class UpdateCost
    {
        protected NamedId[] items;

        protected string[] deps;

        protected string[] updateFields;

        protected string table;

        protected string field;

        public UpdateCost(NamedId[] items)
        {
            if (items == null || items.Length == 0)
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

        protected void WriteNames()
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

        protected void WriteDeps()
        {
            for (var i = 0; i < deps.Length; i++)
            {
                if (i > 0)
                {
                    Write(", ");
                }
                Write("[");
                Write(deps[i]);
                Write("]");
            }
        }

        protected void WriteJoinByDeps()
        {
            for (var i = 0; i < deps.Length; i++)
            {
                if (i > 0)
                {
                    Write(" and ");
                }
                Write("t.["); Write(deps[i]); Write("] = c.["); Write(deps[i]); Write("]");
            }
        }

        protected void WriteSelectField(string f)
        {
            Write(", case when min([");Write(f);Write("]) = max(["); Write(f); Write("]) then min(["); Write(f); Write("]) else null end as ["); Write(f); Write("]");
        }

        protected void WriteUpdateFields()
        {
            throw new NotImplementedException();
        }

        protected void WriteSetField(string f)
        {
            Write("["); Write(f); Write("] = coalesce(t.["); Write(f); Write("], c.["); Write(f); Write("])");
        }

        private string For(string f)
        {
            this.field = f;
            this.GenerationEnvironment.Clear();
            return this.TransformText();
        }

        protected UpdateCost Print(string v)
        {
            Write(v);
            return this;
        }
    }
}
