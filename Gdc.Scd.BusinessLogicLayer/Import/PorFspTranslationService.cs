using Gdc.Scd.Core.Entities;
using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Import
{
    public class PorFspTranslationService<T> where T : NamedId, new()
    {
        protected readonly IRepositorySet _repositorySet;
        protected readonly IRepository<T> _repository;
        private const int BATCH_NUMBER = 5000;

        public PorFspTranslationService(IRepositorySet repositorySet)
        {
            if (repositorySet == null)
                throw new ArgumentNullException(nameof(repositorySet));

            _repositorySet = repositorySet;
            _repository = _repositorySet.GetRepository<T>();
        }

        public void Save(IEnumerable<T> items)
        {
            int count = 0;
            foreach (var item in items)
            {
                count++;
                _repository.Save(item);
                if (count % BATCH_NUMBER == 0 && count > 0)
                {
                    this._repositorySet.Sync();
                }
            }
            this._repositorySet.Sync();
        }
    }
}
