using Gdc.Scd.DataAccessLayer.Interfaces;
using Gdc.Scd.Export.Archive.Procedures;
using Microsoft.SharePoint.Client;
using System.IO;
using System.Net;

namespace Gdc.Scd.Export.Archive.Impl
{
    public class ArchiveRepository : IArchiveRepository
    {
        private readonly IRepositorySet _repo;

        public ArchiveRepository(IRepositorySet repo)
        {
            _repo = repo;
        }

        public virtual CostBlockDto[] GetCostBlocks()
        {
            throw new System.NotImplementedException();
        }

        public virtual Stream GetData(CostBlockDto costBlock)
        {
            return new GetExcelArchive(_repo).ExecuteExcelAsync(costBlock.TableName, costBlock.Procedure, null);
        }

        public virtual void Save(CostBlockDto dto, string path, Stream stream)
        {
            var url = "";
            var cred = new NetworkCredential();

            //path = string.Format("{0}/{1} {2}", config.FileFolderUrl, country, Config.CalculatiolToolFileName)

            using (var ctx = new ClientContext(url))
            {
                ctx.Credentials = cred;

                Microsoft.SharePoint.Client.File.SaveBinaryDirect(ctx, path, stream, true);
            }
        }
    }
}
