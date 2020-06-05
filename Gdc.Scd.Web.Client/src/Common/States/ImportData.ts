import { ApprovalOption } from "../../QualityGate/States/ApprovalOption";

export interface ImportData {
    context?: {
        applicationId: string
        costBlockId:string
        costElementId: string
        inputLevelId: string
        dependencyItemId?: number
        regionId?: number
    }
    approvalOption: ApprovalOption
    excelFile: string
}