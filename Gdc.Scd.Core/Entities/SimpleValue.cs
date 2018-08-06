using System;
using System.Collections.Generic;
using System.Text;
using Gdc.Scd.Core.Interfaces;

namespace Gdc.Scd.Core.Entities
{
    public class SimpleValue<T> : IIdentifiable
    {
        public long Id { get; set; }

        public T Value { get; set; }
    }
}
