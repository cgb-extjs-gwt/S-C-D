using Gdc.Scd.MigrationTool.Interfaces;
using System;
using System.Linq;

namespace Gdc.Scd.MigrationTool.Impl
{
    public abstract class AutoNumberMigrationAction : IMigrationAction
    {
        private const char DateSeparator = '_';

        private const int StartYear = 2020;

        private const int StartMonth = 1;

        private const int StartDay = 1;

        public abstract string Description { get; }

        public abstract void Execute();

        public int Number { get; }

        protected AutoNumberMigrationAction()
        {
            this.Number = this.GetNumber();
        }

        private int GetNumber()
        {
            const int DateItemCount = 5;

            var type = this.GetType();
            var rawDate = type.Name.Split(DateSeparator).Reverse().Take(DateItemCount).Reverse().ToArray();

            if (rawDate.Length < DateItemCount)
            {
                ThrowNameClassFormatException();
            }

            DateTime migarationDate;

            try
            {
                migarationDate = new DateTime(
                    int.Parse(rawDate[0]), 
                    int.Parse(rawDate[1]), 
                    int.Parse(rawDate[2]),
                    int.Parse(rawDate[3]),
                    int.Parse(rawDate[4]),
                    0);

                
            }
            catch (FormatException ex)
            {
                ThrowNameClassFormatException(ex);
            }

            var startDate = new DateTime(StartYear, StartMonth, StartDay);
            if (migarationDate < startDate)
            {
                throw new Exception($"Migration date must be less {startDate}");
            }

            return (int)(migarationDate - startDate).TotalSeconds;

            void ThrowNameClassFormatException(Exception innerException = null)
            {
                throw new Exception("Migratoin must have name '{name}_{year}_{month}_{day}_{hour}_{minute}'", innerException);
            }
        }
    }
}
