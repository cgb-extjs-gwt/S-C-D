using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Export.Sap
{
    class SapLogConstants
    {
        public const string UNEXPECTED_ERROR = "SapJob completed unsuccessfully. Please find details below.";
        public const string INITIALIZATION_END = "SapJob: Initialization ended...";
        public const string START_PROCESS = "SapJob: Process started...";
        public const string SAPLOG_NOTSENT = "SapJob: Last Sap file wasn't send to SAP. Please check file No: ";
        public const string SAPLOG_PREVNOTSENT = "SapJob: Previous Sap file wasn't send to SAP. Please check file No: ";
        public const string SAPLOG_RECEIVED = "SapJob: Get last SapLog info. Received.";
        public const string NODATA_FORUPLOAD = "SapJob: No locapMergedData for upload.";
        public const string END_PROCESS = "SapJob: Process has been finished";
        public const string HWMANUALCOSTS_CANTUPDATE = "SapJob: Unable to set SapUploadDate in hwManualCosts.";
    }
}
