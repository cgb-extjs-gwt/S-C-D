using Microsoft.SharePoint.Client;
using System.IO;
using System.Net;

namespace Gdc.Scd.Export.Archive.Impl
{
    public class SharePointRepository : IArchiveRepository
    {
        public void Save(string path, Stream stream)
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
