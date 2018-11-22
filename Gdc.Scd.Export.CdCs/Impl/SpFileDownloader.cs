using Gdc.Scd.Import.Core.Dto;
using Gdc.Scd.Import.Core.Interfaces;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Export.CdCs
{
    public class SpFileDownloader
    {
        NetworkCredential _networkCredential = null;

        public SpFileDownloader(NetworkCredential networkCredential)
        {
            _networkCredential = networkCredential;   
        }
        
        public Stream DownloadData(SpFileDto fileDto)
        {
            using (ClientContext ctx = new ClientContext(fileDto.WebUrl))
            {
                ctx.Credentials = _networkCredential;

                var item = CheckFile(ctx, fileDto);
                FileInformation fileInformation = Microsoft.SharePoint.Client.File.OpenBinaryDirect(ctx,
                    (string)item["FileRef"]);

                return fileInformation.Stream;
            }            
        }

        private ListItem CheckFile(ClientContext ctx, SpFileDto fileDto)
        {
            var list = ctx.Web.Lists.GetByTitle(fileDto.ListName);
            var camlQuery = new CamlQuery
            {
                ViewXml = String.Format(
                    @"<View><Query><Where><Eq><FieldRef Name='FileLeafRef'/><Value Type='Text'>{0}</Value></Eq></Where><ViewFields><FieldRef Name='FileRef' /></ViewFields><RowLimit>1</RowLimit></Query></View>",
                    fileDto.FileName),              
            };
            camlQuery.FolderServerRelativeUrl = fileDto.FolderServerRelativeUrl;
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
