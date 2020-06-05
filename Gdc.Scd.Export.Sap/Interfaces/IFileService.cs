using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Export.Sap.Enitities;

namespace Gdc.Scd.Export.Sap.Interfaces
{
    public interface IFileService
    {
        string CreateFileOnServer(List<ReleasedData> sapUploadData, int fileNumber);
        bool SendFileToSap(string filename);
    }
}
