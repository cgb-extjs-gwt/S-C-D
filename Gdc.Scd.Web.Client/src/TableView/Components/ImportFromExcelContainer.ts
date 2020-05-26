import { connect } from "react-redux";
import { ImportFromExcelViewProps, ImportFromExcelViewActions, ImportFromExcelView } from "./ImportFromExcelView";
import { CommonState } from "../../Layout/States/AppStates";
import { importExcel } from "../Actions/TableViewActionsAsync";
import { resetImportResults } from "../Actions/TableViewActions";
import { Paths } from "../../Layout/Components/LayoutContainer";
import { buildComponentUrl } from "../../Common/Services/Ajax";

export const ImportFromExcelContainer = connect<ImportFromExcelViewProps, ImportFromExcelViewActions, any, CommonState>(
    state => ({
        importResults: state.pages.tableView.import.results
    }),
    (dispatch, { history }) => ({
        onSave: file => dispatch(importExcel(file, false)),
        onApproveSave: file => dispatch(importExcel(file, true)),
        onCancel: () => {
            dispatch(resetImportResults());
            history.push(buildComponentUrl(Paths.tableView));
        }
    })
)(ImportFromExcelView);