using System.Text;

namespace Gdc.Scd.BusinessLogicLayer.Helpers
{
    public class SqlStringBuilder
    {
        private StringBuilder sb;

        public SqlStringBuilder()
        {
            sb = new StringBuilder(512);
        }

        public SqlStringBuilder Append(string v)
        {
            sb.Append(v);
            return this;
        }

        public SqlStringBuilder AppendEqualsOrNull(long? v)
        {
            if (v.HasValue)
            {
                sb.Append('=').Append(v.Value);
            }
            else
            {
                sb.Append("IS NULL");
            }
            return this;
        }

        public SqlStringBuilder AppendEquals(bool v)
        {
            if (v)
            {
                sb.Append("= 1");
            }
            else
            {
                sb.Append("= 0");
            }
            return this;
        }

        public SqlStringBuilder AppendInOrNull(long[] items)
        {
            if (items == null || items.Length == 0)
            {
                sb.Append("IS NULL");
            }
            else
            {
                sb.Append("IN (").AppendJoin(',', items).Append(")");
            }
            return this;
        }

        public string AsSql()
        {
            return sb.ToString();
        }
    }
}
