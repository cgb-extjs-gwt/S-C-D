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

        public SqlStringBuilder AppendValue(long? v)
        {
            if (v.HasValue)
            {
                sb.Append(v.Value);
            }
            else
            {
                sb.Append("NULL");
            }
            return this;
        }

        public SqlStringBuilder AppendValue(bool v)
        {
            if (v)
            {
                sb.Append("1");
            }
            else
            {
                sb.Append("0");
            }
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

        public SqlStringBuilder AppendAndIfNotNull(string col, long[] items)
        {
            if (items != null && items.Length == 0)
            {
                sb.Append(" AND ").Append(col).Append(" ");
                AppendInOrNull(items);
            }
            return this;
        }

        public SqlStringBuilder AppendValues(long[] items)
        {
            if (items == null || items.Length == 0)
            {
                sb.Append("(VALUES (null))");
            }
            else
            {
                bool flag = false;
                sb.Append("(VALUES ");
                for(var i = 0; i < items.Length; i++)
                {
                    if (flag)
                    {
                        sb.Append(", ");
                    }
                    flag = true;
                    sb.Append("(").Append(items[i]).Append(")");
                }
                sb.Append(")");
            }
            return this;
        }

        public string AsSql()
        {
            return sb.ToString();
        }
    }
}
