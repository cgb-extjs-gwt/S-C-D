using Gdc.Scd.Core.Interfaces;
using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Export.Archive;
using Gdc.Scd.Export.ArchiveJob;
using Gdc.Scd.Export.ArchiveJob.Dto;
using Ninject;
using NUnit.Framework;

namespace Gdc.Scd.Tests.Integration.Export.Archive
{
    public class ArchiveServiceTest2 : ArchiveService
    {
        public ArchiveServiceTest2() : base(null, null) { }

        [TestCase]
        public void ProcessCountryArchiveTest()
        {
            var kernel = Module.CreateKernel();
            var fileRepo = new FileArchiveRepository(kernel.Get<IRepositorySet>());
            this.repo = fileRepo;
            this.logger = kernel.Get<ILogger>();

            fileRepo.SetPath(FileArchiveRepository.PathToBin());

            Process(new CountryDto { Id = 113, Name = "Germany" });
        }

        protected override void Process(ArchiveDto b) { }
    }
}
