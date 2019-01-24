import { asyncAction } from "../../Common/Actions/AsyncAction";
import { CommonState } from "../../Layout/States/AppStates";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import * as CostBlockService from "../../Common/Services/CostBlockService";
import { loadImportStatus, loadQualityGateErrors } from "../Actions/CostImportActions";
import { ImportData } from "../../Common/Services/CostBlockService";
import { ApprovalOption } from "../../QualityGate/States/ApprovalOption";

export const importExcel = (approvalOption: ApprovalOption) => asyncAction<CommonState>(
    (dispatch, getState) => {
        const { pages: { costImport } } = getState();
        const importData: ImportData = {
            costElementId: {
                applicationId: costImport.applicationId, 
                costBlockId: costImport.costBlockId,
                costElementId: costImport.costElementId
            },
            dependencyItemId: costImport.dependencyItems.selectedItemId,
            regionId: costImport.regions.selectedItemId,
            excelFile: costImport.file.base64Data,
            approvalOption
        }

        handleRequest(
            CostBlockService.importExcel(importData).then(
                ({ errors, qualityGateResult }) => {
                    if (qualityGateResult.hasErrors){
                        dispatch(loadQualityGateErrors(qualityGateResult.errors));
                    } else {
                        dispatch(loadQualityGateErrors(null));
                        const status = errors && errors.length > 0 
                            ? [ ...errors, 'Import completed' ]
                            : ['Import successfully completed']

                        dispatch(loadImportStatus(status));
                    }
                },
                () => dispatch(loadImportStatus(['Error during import']))
            )
        )
    }
)