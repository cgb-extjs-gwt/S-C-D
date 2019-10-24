namespace Gdc.Scd.Import.Por
{
    public static class ImportConstantMessages
    {
        public const string UPLOAD_START = "STEP {0}: Uploading {1}s...";
        public const string UPLOAD_ENDS = "STEP {0} ends.";
        public const string FETCH_INFO_START = "Fetching {0} from POR...";
        public const string FETCH_INFO_ENDS = "Fetching {0} completed. {1} rows received.";
        public const string REBUILD_RELATIONSHIPS_START = "STEP {0}: Rebuild relationships between {1} and {2} started...";
        public const string REBUILD_RELATIONSHIPS_END = "STEP {0} ends.";
        public const string REBUILD_FAILS = "STEP: {0}: Rebuilding fails.";

        public const string CONFIGURATION_ERROR = "{0} key is not configured";
        public const string START_PROCESS = "Process started...";
        public const string END_PROCESS = "POR Upload Process has been finished";
        public const string UNEXPECTED_ERROR = "POR Import completed unsuccessfully. Please find details below.";
        public const string UPDATE_COST_BLOCKS_START = "STEP {0}: Updating cost block process started...";
        public const string UPDATE_COST_BLOCKS_END = "Cost blocks were updated.";
        public const string UPDATE_COSTS_START = "STEP {0}: Updating 2nd Level Support Costs of new digits process started...";
        public const string UPDATE_COSTS_END = "2nd Level Support Costs were updated.";

        public const string UPDATE_COSTS_BY_PLA_START = "STEP {0}: Updating cost block by pla started...";
        public const string UPDATE_COSTS_BY_PLA_END = "Cost block by pla updated.";

        public const string UPDATE_SW_COSTS_BY_SOG_START = "STEP {0}: Updating software cost block by sog started...";
        public const string UPDATE_SW_COSTS_BY_SOG_END = "Software cost block by sog updated.";
    }
}
