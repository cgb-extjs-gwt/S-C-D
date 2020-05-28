import { asyncAction } from "../../Common/Actions/AsyncAction";
import { CommonState } from "../../Layout/States/AppStates";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import * as CostBlockService from "../../Common/Services/CostBlockService";
import { loadImportStatus, loadQualityGateErrors, loadDependencyItems, loadRegions } from "../Actions/CostImportActions";
import { getDependencyItems, getRegions } from "../../Common/Services/CostBlockService";
import { ApprovalOption } from "../../QualityGate/States/ApprovalOption";
import { ImportData } from "../../Common/States/ImportData";

export const loadRegionsFromServer = (applicationId: string, costBlockId: string, costElementId: string) => asyncAction<CommonState>(
    (dispatch, getState) => {
        handleRequest(
            getRegions({ applicationId, costBlockId, costElementId }).then(
                regions => dispatch(loadRegions(regions))
            )
        )
    }
)

export const loadDependencyItemsFromServer = (applicationId: string, costBlockId: string, costElementId: string, regionId?: number) => asyncAction<CommonState>(
    (dispatch, getState) => {
        handleRequest(
            getDependencyItems({ applicationId, costBlockId, costElementId }, regionId).then(
                dependencyItems => dispatch(loadDependencyItems(dependencyItems))
            )
        )
    }
)

export const importExcel = (approvalOption: ApprovalOption) => asyncAction<CommonState>(
    (dispatch, getState) => {
        const { pages: { costImport } } = getState();
        const importData: ImportData = {
            context: {
                applicationId: costImport.applicationId, 
                costBlockId: costImport.costBlockId,
                costElementId: costImport.costElementId,
                inputLevelId: costImport.inputLevelId,
                dependencyItemId: costImport.dependencyItems.selectedItemId,
                regionId: costImport.regions.selectedItemId,
            },
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