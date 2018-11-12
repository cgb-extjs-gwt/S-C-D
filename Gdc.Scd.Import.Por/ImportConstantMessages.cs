using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Por
{
    public static class ImportConstantMessages
    {
        public static string UPLOAD_START = "STEP {0}: Uploading {1}s...";
        public static string UPLOAD_ENDS = "STEP {0} ends.";
        public static string FETCH_INFO_START = "Fetching {0} from POR...";
        public static string FETCH_INFO_ENDS = "Fetching {0} completed. {1} rows received.";
        public static string REBUILD_RELATIONSHIPS_START = "STEP {0}: Rebuild relationships between {1} and {2} started...";
        public static string REBUILD_RELATIONSHIPS_END = "STEP {0} ends.";
        public static string REBUILD_FAILS = "STEP: {0}: Rebuilding fails.";

        public static string CONFIGURATION_ERROR = "{0} key is not configured";
        public static string START_PROCESS = "Process started...";
        public static string END_PROCESS = "POR Upload Process has been finished";
        public static string UNEXPECTED_ERROR = "POR Import completed unsuccessfully. Please find details below.";
        public static string UPDATE_COST_BLOCKS_START = "STEP {0}: Updating cost block process started...";
        public static string UPDATE_COST_BLOCKS_END = "Cost blocks were updated.";
    }
}
