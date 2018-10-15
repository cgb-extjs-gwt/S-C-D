using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Core
{
    public class ImportConstants
    {
        public static string PARSE_START = "Parsing is started...";
        public static string PARSE_END = "Parse has been ended. {0} rows parsed.";
        public static string PARSE_EMPTY_FILE = "File cannot be parsed as it is empty";
        public static string PARSE_INVALID_FILE = "File cannot be parsed as its content is invalid";
        public static string PARSE_CANNOT_PARSE = "Value {0} can not be parsed to type {1}";
        public static string CHECK_LAST_MODIFIED_DATE = "Checking Last Modified Date...";
        public static string CHECK_LAST_MODIFIED_DATE_END = "Last Modified Date is {0}";
        public static string CHECK_CONFIGURATION = "Checking if file {0} with last processed date {1} and occurence {2} should be uploaded.";
        public static string SKIP_UPLOADING = "Uploading is skipped.";
        public static string DOWNLOAD_FILE_START = "Starting download file {0}";
        public static string DOWNLOAD_FILE_END = "File has been downloaded";
        public static string UPLOAD_START = "Uploading info in database is started...";
        public static string UPLOAD_END = "Info has been uploaded to database. {0} rows affected.";
        public static string MOVE_FILE_START = "Moving file to processed folder {0}...";
        public static string MOVE_FILE_END = "File was moved.";
        public static string UPDATE_PROCESSING_DATE = "Processing date was updated.";
        public static string UNKNOWN_COUNTRY = "Could not find Master Country {0} with Code {1}";
        public static string EMPTY_COUNTRY = "Skip uploading for one record as country is empty";
        public static string DEACTIVATE_START = "Deactivation for {0} is started...";
        public static string DEACTIVATE_END = "Deactivation finished. {0} rows affected.";
        public static string DEACTIVATING_ENTITY = "{0} with CountryId {1} is deactivated.";
        public static string UNKNOWN_PLA = "Skip upload {0}: {1} as PLA {2} does not exist";
        public static string NEW_WG = "Adding new WG {0}...";
        public static string UPDATE_WG = "Updating WG {0}...";
        public static string DEACTIVATING_WG = "Deactivating WG {0}...";
        public static string UPLOAD_WG_START = "Starting Upload WGs...";
        public static string UPLOAD_WG_END = "WGs were uploaded. {0} rows affected.";
        public static string UPLOAD_AVAILABILITY_FEE_START = "Starting to Upload Availability Fee for WG {0}";
        public static string UPLOAD_AVAILABILITY_FEE_END = "Availability fees was uploaded. {0} rows affected.";
    }
}
