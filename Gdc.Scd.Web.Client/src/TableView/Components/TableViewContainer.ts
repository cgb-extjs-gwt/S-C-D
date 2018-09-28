import { connect } from "react-redux";
import { AjaxDynamicGridProps, AjaxDynamicGridActions, AjaxDynamicGrid } from "../../Common/Components/AjaxDynamicGrid";
import { CommonState } from "../../Layout/States/AppStates";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { findMeta } from "../../Common/Helpers/MetaHelper";
import { NamedId } from "../../Common/States/CommonStates";
import { CostBlockMeta, FieldType, CostElementMeta } from "../../Common/States/CostMetaStates";
import { buildGetRecordsUrl, getTableViewInfo } from "../Services/TableViewService";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import { CommonAction } from "../../Common/Actions/CommonActions";
import { loadTableViewInfo } from "../Actions/TableViewActions";
import { TableViewInfo } from "../States/TableViewState";
import { TableViewRecord } from "../States/TableViewRecord";

const buildCostElementColumn = (costBlockMeta: CostBlockMeta, costElementMeta: CostElementMeta, state: TableViewInfo) => {
    let type: ColumnType;
    let referenceItems: Map<string, NamedId>;

    const dataIndex = `${costBlockMeta.id}.${costElementMeta.id}`;
    const fieldType = costElementMeta.typeOptions ? costElementMeta.typeOptions.Type : ColumnType.Numeric;

    switch (fieldType) {
        case FieldType.Double:
            type = ColumnType.Numeric;
            break;

        case FieldType.Flag:
            type = ColumnType.CheckBox;
            break;

        case FieldType.Reference:
            type = ColumnType.Reference;
            referenceItems = new Map<string, NamedId>();

            state.references[dataIndex].forEach(item => referenceItems.set(item.id, item));
            break;
    }

    return <ColumnInfo>{
        title: costElementMeta.name,
        dataIndex,
        isEditable: true,
        type,
        referenceItems
    };
}

export const TableViewContainer = connect<AjaxDynamicGridProps, AjaxDynamicGridActions, {}, CommonState>(
    state => {
        let dataLoadUrl: string;
        const coordinateColumns: ColumnInfo[]  = [];
        const costElementColumns: ColumnInfo[]  = [];
        const tableViewInfo = state.pages.tableView.info;
        const meta = state.app.appMetaData;

        if (tableViewInfo && meta) {
            dataLoadUrl = buildGetRecordsUrl();

            for (const costBlockInfo of tableViewInfo.costBlockInfos) {
                const costBlockMeta = findMeta(meta.costBlocks, costBlockInfo.metaId);

                for (const constElementId of costBlockInfo.costElementIds) {
                    const costElementMeta = findMeta(costBlockMeta.costElements, constElementId);
                    const items = costElementMeta.dependency 
                        ? [...costElementMeta.inputLevels, costElementMeta.dependency] 
                        : costElementMeta.inputLevels;

                    for (const item of items) {
                        coordinateColumns.push({
                            title: item.name,
                            dataIndex: item.id,
                            type: ColumnType.Text
                        });
                    }

                    costElementColumns.push(
                        buildCostElementColumn(costBlockMeta, costElementMeta, tableViewInfo)
                    );
                }
            }
        }

        const columns =  [...coordinateColumns, ...costElementColumns].map(column => ({
            ...column,
            mappingFn: (data: TableViewRecord) => data.data[column.dataIndex]
        }));

        return <AjaxDynamicGridProps>{
            columns,
            dataLoadUrl
        };
    },
    dispatch => (<AjaxDynamicGridActions>{
        init: () => handleRequest(
            getTableViewInfo().then(
                tableViewInfo => dispatch(loadTableViewInfo(tableViewInfo))
            )
        )
    })
)(AjaxDynamicGrid)