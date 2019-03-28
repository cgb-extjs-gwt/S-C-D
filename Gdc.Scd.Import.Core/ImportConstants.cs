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
        public static string CHECK_CONFIGURATION = "Checking if file {0} with last modified date {1} should be uploaded.";
        public static string SKIP_UPLOADING = "Uploading is skipped.";
        public static string DOWNLOAD_FILE_START = "Starting download file {0}";
        public static string DOWNLOAD_FILE_END = "File has been downloaded";
        public static string UPLOAD_START = "Uploading info in database is started...";
        public static string UPLOAD_END = "Info has been uploaded to database. {0} rows affected.";
        public static string MOVE_FILE_START = "Moving file to processed folder {0}...";
        public static string MOVE_FILE_END = "File was moved.";
        public static string UPDATE_PROCESSING_DATE = "Processing date was updated.";
        public static string UNKNOWN_COUNTRY = "Could not find Master Country {0} with Code {1}";
        public static string UNKNOWN_COUNTRY_CODE = "Country Group with Country Code {0} is either unknown or skipped from auto upload";
        public static string UNKNOWN_WARRANTY = "Could not find Warranty Group {0}";
        public static string UNKNOWN_YEAR = "Could not find Year Dependency {0}";
        public static string UNKNOWN_CURRENCY = "Could not find Currency Code {0}";
        public static string EMPTY_COUNTRY = "Skip uploading for one record as country is empty";
        public static string DEACTIVATE_START = "Deactivation for {0} is started...";
        public static string DEACTIVATE_END = "Deactivation finished. {0} rows affected.";
        public static string UNKNOWN_PLA = "Skip upload {0}: {1} as PLA {2} does not exist";
        public static string NEW_WG = "Adding new WG {0}...";
        public static string UPDATE_WG = "Updating WG {0}...";
        public static string DEACTIVATING_WG = "Deactivating WG {0}...";
        public static string UPLOAD_WG_START = "Starting Upload WGs...";
        public static string UPLOAD_WG_END = "WGs were uploaded. {0} rows affected.";
        public static string UPLOAD_AVAILABILITY_FEE_START = "Starting to Upload Availability Fee for WG {0}";
        public static string UPLOAD_AVAILABILITY_FEE_END = "Availability fees was uploaded. {0} rows affected.";
        public static string UPDATE_AVAILABILITY_FEE_NEW_WG_START = "Starting update Availability Fee for new WGs";
        public static string UPDATE_AVAILABILITY_FEE_NEW_WG_FINISH = "Availability Fee for new WGs was updated. {0} rows added";
        public static string UPDATE_AVAILABILITY_FEE_ERROR = "Error while updating Availability Fee.";
        public static string UPDATE_INSTALL_BASE_CENTRAL_EUROPE = "Updating Install Base for Central Europe.";
        public static string DEACTIVATING_AVAILABILITY_FEE = "Deactivating Availability Fee for WG {0}.";
        public static string ADD_NEW_SFAB = "Adding new Sfab {0}...";
        public static string UPDATE_SFAB = "Updating Sfab {0}...";
        public static string UPLOAD_SFAB_END = "Uploading Sfabs finished. {0} rows affected.";
        public static string ADD_NEW_CCG = "Adding new Central Contract Group {0}...";
        public static string UPDATE_CCG = "Updating Central Contract Group {0}...";
        public static string UPLOAD_CCG_END = "Uploading Central Contract Groups finished. {0} rows affected.";
        public static string DEACTIVATING_SFAB_START = "Deactivating SFabs...";
        public static string DEACTIVATING_SFAB = "Deactivating SFab: {0}";
        public static string DEACTIVATING_SFAB_END = "Deactivating SFabs finished. {0} rows affected.";
        public static string UPDATING_WGS_AND_SOGS_START = "Starting updating WGs and Sogs...";
        public static string UPDATING_ENTITY = "Updating {0}: {1}...";
        public static string UPDATING_WGS_AND_SOGS_END = "WGs and SOGs were updated.";
        public static string ADD_EXCHANGE_RATE = "Exchanhe Rate for currency {0} was added.";
        public static string UPDATE_EXCHANGE_RATE = "Exchanhe Rate for currency {0} was updated.";
        public static string UPDATING_WGS = "Starting updating WGs...";
        public static string UPDATING_WGS_END = "WGs were updated.";
        public static string SET_ZEROS_INSTALL_BASE = "Set zero values for not received install bases";
        public static string ZEROS_SET = "Zero were set. {0} values were affected.";
        public static string COULD_NOT_FIND_FILE = "File {0} couldn't be found.";
        public static string FILE_WASNT_DELIVERED = "Uploiding is skipped as there are no file was delivered.";
        public static string IMPORT_DATA_STARTED = "Starting importing {0}s...";
        public static string IMPORT_DATA_END = "Importing was finished. {0} rows received.";
    }
}
