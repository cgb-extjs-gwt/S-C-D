using System.Data;
using System.Linq;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Export.ArchiveJob.Dto;

namespace Gdc.Scd.Export.ArchiveJob.Procedures
{
    public class GetCountries
    {
        private readonly IRepositorySet _repo;

        private bool prepared;

        private int ID;
        private int NAME;
        private int ISO;

        public GetCountries(IRepositorySet repo)
        {
            _repo = repo;
        }

        public CountryDto[] Execute()
        {
            var sql = @"select c.Id, c.Name, c.ISO3CountryCode as ISO
                        from InputAtoms.Country c 
                        where exists(select * from Portfolio.LocalPortfolio where CountryId = c.Id)";

            return _repo.ReadBySqlAsync(sql, Read).Result.ToArray();
        }

        private CountryDto Read(IDataReader r)
        {
            if (!prepared)
            {
                Prepare(r);
            }
            return new CountryDto
            {
                Id = r.GetInt64(ID),
                Name = r.GetString(NAME),
                ISO = r.GetString(ISO)
            };
        }

        private void Prepare(IDataReader r)
        {
            ID = r.GetOrdinal("Id");
            NAME = r.GetOrdinal("Name");
            ISO = r.GetOrdinal("ISO");

            prepared = true;
        }
    }
}
