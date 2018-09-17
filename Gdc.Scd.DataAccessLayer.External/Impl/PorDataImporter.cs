using Gdc.Scd.DataAccessLayer.External.Interfaces;
using Gdc.Scd.DataAccessLayer.External.Por;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.DataAccessLayer.External.Impl
{
    public class PorDataImporter<T> : IDataImporter<T> where T : class 
                                                             
    {
        private readonly FrieseEntities _frieseEntities;
        private IDbSet<T> _entities;

        public PorDataImporter(FrieseEntities frieseEntities)
        {
            _frieseEntities = frieseEntities;
        }

        public IEnumerable<T> ImportData()
        {
            return Entities;
        }

        private IDbSet<T> Entities
        {
            get
            {
                if (_entities == null)
                    _entities = _frieseEntities.Set<T>();

                return _entities;
            }
        }
    }
}
