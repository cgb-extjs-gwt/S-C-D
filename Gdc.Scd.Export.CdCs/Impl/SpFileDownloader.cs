using System.IO;
using System.Net;
using Gdc.Scd.Export.CdCs.Dto;
using Microsoft.SharePoint.Client;

namespace Gdc.Scd.Export.CdCs.Impl
{
    public class SpFileDownloader
    {
        readonly NetworkCredential _networkCredential;

        public SpFileDownloader(NetworkCredential networkCredential)
        {
            _networkCredential = networkCredential;   
        }
        
        public Stream DownloadData(SpFileDto fileDto)
        {
            using (var ctx = new ClientContext(fileDto.WebUrl))
            {
                ctx.Credentials = _networkCredential;

                var item = CheckFile(ctx, fileDto);
                var fileInformation = Microsoft.SharePoint.Client.File.OpenBinaryDirect(ctx,
                    (string)item["FileRef"]);

                return fileInformation.Stream;
            }            
        }

        private ListItem CheckFile(ClientContext ctx, SpFileDto fileDto)
        {
            var list = ctx.Web.Lists.GetByTitle(fileDto.ListName);
            var camlQuery = new CamlQuery
            {
                ViewXml = $@"<View><Query><Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>{fileDto.FileName}</Value></Eq></Where><ViewFields><FieldRef Name='FileRef' /></ViewFields><RowLimit>1</RowLimit></Query></View>",
                FolderServerRelativeUrl = fileDto.FolderServerRelativeUrl,
            };
            var listItems = list.GetItems(camlQuery);
            ctx.Load(list);
            ctx.Load(listItems);
            ctx.ExecuteQuery();
            if (listItems.Count == 0)
                throw new FileNotFoundException("File cannot be found", fileDto.FileName);
            return listItems[0];
        }
    }
}
