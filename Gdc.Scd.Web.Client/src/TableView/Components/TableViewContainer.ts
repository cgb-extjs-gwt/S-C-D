import { connect } from "react-redux";
import { TableViewProps, TableView } from "./TableView";
import { CommonState } from "../../Layout/States/AppStates";
import { CostElementIdentifier } from "../../Common/States/CostElementIdentifier";
import { TableViewInfo } from "../States/TableViewState";
import { Model } from "../../Common/States/ExtStates";
import { TableViewRecord } from "../States/TableViewRecord";
import { buildGetHistoryUrl } from "../Services/TableViewService";

const buildHistotyDataLoadUrl = (tableViewInfo: TableViewInfo, [selection]: Model<TableViewRecord>[], selectedDataIndex: string) => {
    const costElementField =
        tableViewInfo.recordInfo.data.find(fieldInfo => fieldInfo.dataIndex == selectedDataIndex);

    const costElementId: CostElementIdentifier = {
        applicationId: costElementField.schemaId,
        costBlockId: costElementField.metaId,
        costElementId: costElementField.fieldName
    };

    const coordinates = {};

    for (const key of Object.keys(selection.data.coordinates)) {
        coordinates[key] = selection.data.coordinates[key].id;
    }

    return buildGetHistoryUrl(costElementId, coordinates);
}

export const TableViewContainer = connect<TableViewProps, {}, {}, CommonState>(
    ({ pages: { tableView } }) => ({
        buildHistotyDataLoadUrl: tableView.info
            ? (selection, selectedDataIndex) => buildHistotyDataLoadUrl(tableView.info, selection, selectedDataIndex)
            : () => ''
    })
)(TableView)