using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdc.Scd.BusinessLogicLayer.Import
{
    public static class PorImportLoggingMessage
    {
        public const string UNKNOWN_PLA = "{0} was not uploaded as PLA {1} does not exist in the database";
        public const string UNKNOW_SOG = "{0} was not uploaded as SOG {1} does not exist in the database";
        public const string ADD_STEP_BEGIN = "Adding or updating new {0}s";
        public const string ADD_STEP_END = "Finish adding or updating process. {0} rows affected.";
        public const string DEACTIVATE_STEP_BEGIN = "Deactivating {0}s";
        public const string DEACTIVATE_STEP_END = "Deactivation finished. {0} rows affected.";
        public const string UNEXPECTED_ERROR = "Unexpected error occured. Please see details below.";
        public const string ADDED_OR_UPDATED_ENTITY = "{0}:{1} was updated or added successfully.";
        public const string DEACTIVATED_ENTITY = "{0}:{1} was deactivated.";
    }
}
