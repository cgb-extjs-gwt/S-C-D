import { connect } from "react-redux";
import { AjaxDynamicGridProps, AjaxDynamicGridActions, AjaxDynamicGrid } from "../../Common/Components/AjaxDynamicGrid";
import { CommonState } from "../../Layout/States/AppStates";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { findMeta } from "../../Common/Helpers/MetaHelper";
import { NamedId } from "../../Common/States/CommonStates";
import { CostBlockMeta, FieldType, CostElementMeta, CostMetaData } from "../../Common/States/CostMetaStates";
import { buildGetRecordsUrl, getTableViewInfo } from "../Services/TableViewService";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import { CommonAction } from "../../Common/Actions/CommonActions";
import { loadTableViewInfo } from "../Actions/TableViewActions";
import { TableViewInfo } from "../States/TableViewState";
import { TableViewRecord } from "../States/TableViewRecord";
import { FieldInfo } from "../../Common/States/FieldInfo";

// const mapToColumnInfo = (
//     fieldIfnos: FieldInfo[],
//     meta: CostMetaData,
//     mapFn: (costBlock: CostBlockMeta, costElement: CostElementMeta, fieldInfo: FieldInfo) => ColumnInfo
// ) => fieldIfnos.map(fieldInfo => {
//     const costBlockMeta = findMeta(meta.costBlocks, fieldInfo.metaId);
//     const costElementMeta = findMeta(costBlockMeta.costElements, fieldInfo.fieldName);

//     return mapFn(costBlockMeta, costElementMeta, fieldInfo);
// })

const mapToColumnInfo = (
    fieldIfnos: FieldInfo[],
    meta: CostMetaData,
    costBlockCache: Map<string, CostBlockMeta>,
    mapFn: (costBlock: CostBlockMeta, fieldInfo: FieldInfo) => ColumnInfo
) => fieldIfnos.map(fieldInfo => {
    let costBlockMeta = costBlockCache.get(fieldInfo.metaId);
    if (!costBlockMeta) {
        costBlockMeta = findMeta(meta.costBlocks, fieldInfo.metaId);

        costBlockCache.set(fieldInfo.metaId, costBlockMeta);
    }
    
    return mapFn(costBlockMeta, fieldInfo);
}) 

const buildColumn = (item: NamedId, fieldInfo: FieldInfo) => ({
    title: item.name,
    dataIndex: fieldInfo.dataIndex,
})

const buildCoordinateColumn = (costBlock: CostBlockMeta, fieldInfo: FieldInfo) => { 
    let item: NamedId;

    for (const { dependency, inputLevels } of costBlock.costElements) {
        const items = dependency ? [dependency, ...inputLevels] : inputLevels;

        item = items.find(x => x.id == fieldInfo.fieldName);

        if (item) {
            break;    
        }
    }

    return <ColumnInfo>{
        ...buildColumn(item, fieldInfo),
        type: ColumnType.Text,
        mappingFn: (record: TableViewRecord) => record.coordinates[fieldInfo.dataIndex].name
    };
}

const buildCostElementColumn = (costBlock: CostBlockMeta, fieldInfo: FieldInfo, state: TableViewInfo) => {
    let type: ColumnType;
    let referenceItems: Map<string, NamedId>;

    const costElement = findMeta(costBlock.costElements, fieldInfo.fieldName);
    const fieldType = costElement.typeOptions ? costElement.typeOptions.Type : ColumnType.Numeric;

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

            state.references[fieldInfo.dataIndex].forEach(item => referenceItems.set(item.id, item));
            break;
    }

    return <ColumnInfo>{
        ...buildColumn(costElement, fieldInfo),
        isEditable: true,
        type,
        referenceItems,
        mappingFn: (record: TableViewRecord) => {
            const valueCount = record.data[fieldInfo.dataIndex];

            return valueCount.count == 1 
                ? valueCount.value 
                : `(${valueCount.count} values)`;
        }
    };
}

export const TableViewContainer = connect<AjaxDynamicGridProps, AjaxDynamicGridActions, {}, CommonState>(
    state => {
        let dataLoadUrl: string;
        
        const columns = [];
        const tableViewInfo = state.pages.tableView.info;
        const meta = state.app.appMetaData;

        if (tableViewInfo && meta) {
            dataLoadUrl = buildGetRecordsUrl();

            // const coordinateColumns = mapToColumnInfo(tableViewInfo.recordInfo.coordinates, meta, buildCoordinateColumn);
            // const costElementColumns = mapToColumnInfo(
            //     tableViewInfo.recordInfo.data, 
            //     meta, 
            //     (costBlock, costElement, fieldInfo) => buildCostElementColumn(costBlock, costElement, fieldInfo, tableViewInfo));

            const costBlockCache = new Map<string, CostBlockMeta>();
            const coordinateColumns = mapToColumnInfo(tableViewInfo.recordInfo.coordinates, meta, costBlockCache, buildCoordinateColumn);
            const costElementColumns = mapToColumnInfo(
                tableViewInfo.recordInfo.data, 
                meta, 
                costBlockCache, 
                (costBlock, fieldInfo) => buildCostElementColumn(costBlock, fieldInfo, tableViewInfo));

            columns.push(...coordinateColumns, ...costElementColumns);
        }

        return <AjaxDynamicGridProps>{
            dataLoadUrl,
            columns
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