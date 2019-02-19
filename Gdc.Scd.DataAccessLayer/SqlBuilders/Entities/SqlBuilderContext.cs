using System.Collections.Generic;
using System.Linq;
using Gdc.Scd.DataAccessLayer.Entities;

namespace Gdc.Scd.DataAccessLayer.SqlBuilders.Entities
{
    public class SqlBuilderContext
    {
        private int parameterIndex;

        private string nullParameterName;

        private readonly Dictionary<object, string> parameterNames = new Dictionary<object, string>();

        public string GetParameterName(object parameterValue)
        {
            string parameterName;

            if (parameterValue == null)
            {
                if (this.nullParameterName == null)
                {
                    this.nullParameterName = this.GetNewParamName();
                }

                parameterName = this.nullParameterName;
            }
            else
            {
                if (!this.parameterNames.TryGetValue(parameterValue, out parameterName))
                {
                    parameterName = this.GetNewParamName();

                    this.parameterNames[parameterValue] = parameterName;
                }
            }

            return parameterName;
        }

        public CommandParameterInfo[] GetParameters()
        {
            var parameters =
                this.parameterNames.Keys.Select(value => new CommandParameterInfo(this.GetParameterName(value), value))
                                        .ToList();

            if (this.nullParameterName != null)
            {
                parameters.Add(new CommandParameterInfo(this.nullParameterName, null));
            }

            return parameters.ToArray();
        }

        private string GetNewParamName()
        {
            return $"@param_{this.parameterIndex++}";
        }
    }
}
