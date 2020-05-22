import { ApprovalOption } from "../../QualityGate/States/ApprovalOption";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { CommonState } from "../../Layout/States/AppStates";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import { updateRecords, importFromExcel } from "../Services/TableViewService";
import { loadQualityCheckResult, resetQualityCheckResult, resetChanges, loadImportResults } from "./TableViewActions";
import { ImportData } from "../../Common/States/ImportData";
import { getBase64Data } from "../../Common/Helpers/FileHelper";

export const saveTableViewToServer = (approvalOption: ApprovalOption) => asyncAction<CommonState>(
    (dispatch, getState) => {
        const state = getState();

        handleRequest(
            updateRecords(state.pages.tableView.editedRecords, approvalOption).then(
                qualityGateResultSet => {
                    if (qualityGateResultSet.hasErrors) {
                        dispatch(loadQualityCheckResult(qualityGateResultSet));
                    }
                    else {
                        dispatch(resetQualityCheckResult());
                        dispatch(resetChanges());
                    }
                }
            )
        )
    }
)

export const importExcel = (file, isApproving: boolean) => asyncAction<CommonState>(
    dispatch => {
        getBase64Data(file).then(fileData => {
            const importData: ImportData = {
                excelFile: fileData,
                approvalOption: {
                    isApproving
                }
            }

            handleRequest(
                importFromExcel(importData).then(
                    ({ errors, qualityGateResult }) => {
                        if (qualityGateResult.hasErrors){
                            dispatch(loadQualityCheckResult(qualityGateResult));
                        } 

                        const status = errors && errors.length > 0 
                            ? [ ...errors, 'Import completed' ]
                            : ['Import successfully completed']

                        dispatch(loadImportResults(status));
                    },
                    () => dispatch(loadImportResults(['Error during import']))
                )
            )
        });
    }
)