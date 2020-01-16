using Gdc.Scd.Core.Entities;
using Gdc.Scd.Export.CdCsJob.Dto;
using Gdc.Scd.Export.CdCsJob.Helpers;
using Microsoft.SharePoint.Client;
using System.IO;
using System.Net;
using SharePointFile = Microsoft.SharePoint.Client.File;

namespace Gdc.Scd.Export.CdCsJob
{
    public class SharePointClient
    {
        private readonly NetworkCredential credentials;

        public SharePointClient(NetworkCredential networkCredential)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            credentials = networkCredential;
        }

        public virtual MemoryStream Load(SpFileDto cfg)
        {
            using (var ctx = new ClientContext(cfg.WebUrl))
            {
                ctx.Credentials = credentials;

                var file = GetFile(ctx, cfg);
                var fileInfo = SharePointFile.OpenBinaryDirect(ctx, file);

                return fileInfo.Stream.Copy();
            }
        }

        public virtual void Send(Stream data, CdCsConfiguration config)
        {
            Send(data, config.FileWebUrl, FullName(config, Config.CalculationToolFileName));
        }

        public virtual void Send(Stream data, string host, string path)
        {
            data.Seek(0, SeekOrigin.Begin);
            using (var ctx = new ClientContext(host))
            {
                ctx.Credentials = credentials;
                SharePointFile.SaveBinaryDirect(ctx, path, data, true);
            }
        }

        protected virtual string GetFile(ClientContext ctx, SpFileDto cfg)
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

            var item = listItems[0];
            return (string)item["FileRef"];
        }

        public static string FullName(CdCsConfiguration config, string fn)
        {
            return string.Concat(config.FileFolderUrl, "/", config.Country.Name, " ", fn);
        }
    }
}
