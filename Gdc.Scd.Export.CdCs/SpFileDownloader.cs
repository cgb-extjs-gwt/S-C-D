using Gdc.Scd.Export.CdCs.Dto;
using Gdc.Scd.Export.CdCs.Helpers;
using Microsoft.SharePoint.Client;
using System.IO;
using System.Net;

namespace Gdc.Scd.Export.CdCs
{
    public class SpFileDownloader
    {
        private readonly NetworkCredential _networkCredential;

        public SpFileDownloader(NetworkCredential networkCredential)
        {
            _networkCredential = networkCredential;
        }

        public virtual MemoryStream DownloadData(SpFileDto cfg)
        {
            using (var ctx = new ClientContext(cfg.WebUrl))
            {
                ctx.Credentials = _networkCredential;

                var item = CheckFile(ctx, cfg);
                var fileInformation = Microsoft.SharePoint.Client.File.OpenBinaryDirect(ctx, (string)item["FileRef"]);

                return fileInformation.Stream.Copy();
            }
        }

        //public virtual void Send(
        //        Stream masterFileStream, 
        //        Core.Entities.CdCsConfiguration config, 
        //        string country
        //    )
        //{
        //    masterFileStream.Seek(0, SeekOrigin.Begin);
        //    using (var ctx = new ClientContext(config.FileWebUrl))
        //    {
        //        ctx.Credentials = NetworkCredential;

        //        SharePointFile.SaveBinaryDirect(
        //                ctx,
        //                $"{config.FileFolderUrl}/{country} {Config.CalculationToolFileName}",
        //                masterFileStream,
        //                true
        //            );
        //    }
        //}

        protected virtual ListItem CheckFile(ClientContext ctx, SpFileDto cfg)
        {
            var list = ctx.Web.Lists.GetByTitle(cfg.ListName);
            var camlQuery = new CamlQuery
            {
                ViewXml = $@"<View><Query><Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>{cfg.FileName}</Value></Eq></Where><ViewFields><FieldRef Name='FileRef' /></ViewFields><RowLimit>1</RowLimit></Query></View>",
                FolderServerRelativeUrl = cfg.FolderServerRelativeUrl,
            };

            var listItems = list.GetItems(camlQuery);
            ctx.Load(list);
            ctx.Load(listItems);
            ctx.ExecuteQuery();

            if (listItems.Count == 0)
            {
                throw new FileNotFoundException("File cannot be found", cfg.FileName);
            }

            return listItems[0];
        }
    }
}
