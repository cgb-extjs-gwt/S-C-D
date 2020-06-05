import { connect } from "react-redux";
import { TableViewProps, TableView, TableViewActions } from "./TableView";
import { CommonState } from "../../Layout/States/AppStates";
import { TableViewInfo } from "../States/TableViewState";
import { Model } from "../../Common/States/ExtStates";
import { TableViewRecord } from "../States/TableViewRecord";
import { buildGetHistoryUrl, exportToExcel } from "../Services/TableViewService";
import { CostMetaData } from "../../Common/States/CostMetaStates";
import { getCostElementByAppMeta } from "../../Common/Helpers/MetaHelper";
import { Paths } from "../../Layout/Components/LayoutContainer";
import { buildComponentUrl } from "../../Common/Services/Ajax";

const buildHistotyDataLoadUrl = (meta: CostMetaData, tableViewInfo: TableViewInfo, [selection]: Model<TableViewRecord>[], selectedDataIndex: string) => {
    const costElementField =
        tableViewInfo.recordInfo.data.find(fieldInfo => fieldInfo.dataIndex == selectedDataIndex);

    const coordinates = {};

    for (const key of Object.keys(selection.data.coordinates)) {
        coordinates[key] = selection.data.coordinates[key].id;
    }

    if (costElementField.dependencyItemId != null) {
        const costElement = getCostElementByAppMeta(meta, costElementField.costBlockId, costElementField.costElementId);

        coordinates[costElement.dependency.id] = costElementField.dependencyItemId;
    }

    return buildGetHistoryUrl(costElementField, coordinates);
}

export const TableViewContainer = connect<TableViewProps, TableViewActions, any, CommonState>(
    ({ app: { appMetaData },  pages: { tableView } }) => (<TableViewProps>{
        buildHistotyDataLoadUrl: tableView.info
            ? (selection, selectedDataIndex) => buildHistotyDataLoadUrl(appMetaData, tableView.info, selection, selectedDataIndex)
            : () => ''
    }),
    (dispatch, { history }) => ({
        onExportToExcelClick: exportToExcel,
        onImportFromExcelClick: () => history.push(buildComponentUrl(Paths.tableViewImport))
    })
)(TableView)