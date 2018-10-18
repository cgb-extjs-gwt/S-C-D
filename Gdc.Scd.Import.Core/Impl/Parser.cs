using Gdc.Scd.Core.Comparators;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.Import.Core.Attributes;
using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Gdc.Scd.Import.Core.Impl
{
    public class Parser<T> : IParser<T> where T: new()
    {
        private readonly ILogger<LogLevel> _logger;

        public Parser(ILogger<LogLevel> logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _logger = logger;
        }

        public IEnumerable<T> Parse(ParseInfoDto info)
        {
            if (info.Content == null)
            {
                _logger.Log(LogLevel.Warn, ImportConstants.PARSE_EMPTY_FILE);
                return null;
            }

            List<T> entities = new List<T>();
            if (info.HasHeader)
                //Skip Header
                info.Content.ReadLine();
            
            while (!info.Content.EndOfStream)
            {
                var row = info.Content.ReadLine();
                if (!String.IsNullOrEmpty(row))
                {
                    var splittedRow = row.Split(info.Delimeter.ToCharArray());
                    var entity = ParseRow(splittedRow, info.Culture);
                    entities.Add(entity);
                }
            }

            return entities;
        }

        private T ParseRow(string[] values, string inpCulture = "de-DE")
        {
            var entity = new T();
            foreach (PropertyInfo pi in typeof(T).GetProperties())
            {
                foreach (Attribute attr in pi.GetCustomAttributes())
                {
                    if (attr is ParseInfoAttribute)
                    {
                        ParseInfoAttribute parseInfoAttr = (ParseInfoAttribute)attr;
                        var propValue = values[parseInfoAttr.ColumnNumber];
                        object value = pi.PropertyType.GetDefaultValue();
                        if (pi.PropertyType.IsNumericType())
                        {
                            var style = NumberStyles.Number | NumberStyles.AllowDecimalPoint;
                            var culture = CultureInfo.CreateSpecificCulture(inpCulture);
                            switch (parseInfoAttr.Format)
                            {
                                case Enums.Format.Percentage:
                                    propValue = propValue.Replace("%", "").Trim();
                                    double dblResult = 0.0;
                                    if (Double.TryParse(propValue, style, culture, out dblResult))
                                        value = dblResult;
                                    else
                                    {
                                        _logger.Log(LogLevel.Warn, ImportConstants.PARSE_CANNOT_PARSE, propValue, typeof(Double));
                                    }
                                    break;
                                case Enums.Format.Number:
                                    int intResult = 0;
                                    if (Int32.TryParse(propValue.Trim(), out intResult))
                                        value = intResult;
                                    else
                                        _logger.Log(LogLevel.Warn, ImportConstants.PARSE_CANNOT_PARSE, propValue, typeof(Int32));
                                    break;
                                case Enums.Format.None:
                                    dblResult = 0;
                                    if (Double.TryParse(propValue.Trim(), style, culture, out dblResult))
                                        value = dblResult;
                                    else
                                        _logger.Log(LogLevel.Warn, ImportConstants.PARSE_CANNOT_PARSE, propValue, typeof(Double));
                                    break;
                            }
                        }
                        else
                        {
                            value = propValue;
                        }

                        pi.SetValue(entity, value, null);
                    }
                }
            }

            return entity;
        }
    }
}
