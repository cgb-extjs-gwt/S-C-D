using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.Import.Por.Core.Impl
{
    public static class PorImportLoggingMessage
    {
        public const string UNKNOWN_PLA = "{0} was not uploaded as PLA {1} does not exist in the database";
        public const string UNKNOWN_SOG = "{0} was not uploaded as SOG {1} does not exist in the database";
        public const string UNKNOWN_SLA_TRANSLATION = "{0} was not uploaded as SLA Translation coulde not be mapped";
        public const string UNKNOW_DIGIT = "{0} was not uploaded as SW Digit {1} does not exist in the database";
        public const string UNKNOWN_LICENSE = "{0} was not uploaded as SW License {1} does not exist in the database";
        public const string UNKNOWN_COUNTRY_DIGIT = "{0} was not uploaded as Country code {1} does not exist in the database";
        public const string UNKNOW_WG = "{0} was not uploaded as WG {1} does not exist in the database";
        public const string ADD_STEP_BEGIN = "Adding or updating new {0}s";
        public const string ADD_STEP_END = "Finish adding or updating process. {0} rows affected.";
        public const string DEACTIVATE_STEP_BEGIN = "Deactivating {0}s";
        public const string DEACTIVATE_STEP_END = "Deactivation finished. {0} rows affected.";
        public const string UNEXPECTED_ERROR = "Unexpected error occured. Please see details below.";
        public const string ADDED_OR_UPDATED_ENTITY = "{0}:{1} was updated or added successfully.";
        public const string DEACTIVATED_ENTITY = "{0}:{1} was deactivated.";
        public const string HANDLE_PROACTIVECODES = "Handling Proactive codes";
        public const string HANDLE_STANDARDWARRANTY = "Handling Standard Warranty";
        public const string ADDING_SWDIGIT_SWLICENSE = "Adding combination: SW License: {0}, SW Digit: {1}";
        public const string EMPTY_SOG_WG = "WG and SOG are empty for FSP Code {0} ";
        public const string UPLOAD_HW_CODES_START = "Uploading {0} is started...";
        public const string UPLOAD_HW_CODES_ENDS = "Uploading is ended. Code: {0}";
        public const string INCORRECT_SOFTWARE_FSPCODE_DIGIT_MAPPING = "Mapping between FSP Code {0} and Digit. ({1}) digits were found.";
        public const string SOG_NOT_EXISTS = "SOG {0} does not exist in the SCD.";
        public const string DELETE_BEGIN = "Deletion {0} is started...";
        public const string DELETE_END = "Deletion ended.";
    }
}
