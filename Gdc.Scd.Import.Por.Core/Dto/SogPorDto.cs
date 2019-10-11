using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdc.Scd.Import.Por.Core.DataAccessLayer;
using Gdc.Scd.Import.Por.Core.Impl;

namespace Gdc.Scd.Import.Por.Core.Dto
{
    public struct SogPorDto
    {
        public string Alignment { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Pla { get; set; }
        public string FabGrp { get; set; }
        public string SCD_ServiceType { get; set; }
        public bool IsSoftware { get; set; }
        public bool IsSolution { get; set; }
        public string ServiceTypes { get; set; }
        public bool ActivePorFlag { get; set; }

        public SogPorDto(SCD2_ServiceOfferingGroups porSog, 
            IEnumerable<string> softwareTypes,
            string solutionIdentifier,
            string[] exceptionalHardwareWgs)
        {
            Alignment = porSog.Alignment;
            Description = porSog.Service_Offering_Group_Name;
            Name = porSog.Service_Offering_Group;
            Pla = porSog.SOG_PLA;
            FabGrp = porSog.FabGrp;
            SCD_ServiceType = porSog.SCD_ServiceType;
            ServiceTypes = porSog.Service_Types;
            ActivePorFlag = porSog.Active_Flag == "1";
            IsSoftware = !exceptionalHardwareWgs.Contains(porSog.Service_Offering_Group, StringComparer.OrdinalIgnoreCase) 
                         && ImportHelper.IsSoftware(porSog.SCD_ServiceType, softwareTypes,
                             porSog.Alignment);
            IsSolution = ImportHelper.IsSolution(porSog.Service_Types, solutionIdentifier);
        }
    }
}
