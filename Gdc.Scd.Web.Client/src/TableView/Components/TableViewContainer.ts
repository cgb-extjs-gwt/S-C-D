import { connect, connectAdvanced } from "react-redux";
import { AjaxDynamicGridProps, AjaxDynamicGridActions, AjaxDynamicGrid } from "../../Common/Components/AjaxDynamicGrid";
import { CommonState } from "../../Layout/States/AppStates";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { findMeta } from "../../Common/Helpers/MetaHelper";
import { NamedId } from "../../Common/States/CommonStates";
import { CostBlockMeta, FieldType, CostElementMeta, CostMetaData } from "../../Common/States/CostMetaStates";
import { buildGetRecordsUrl, getTableViewInfo, updateRecords } from "../Services/TableViewService";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import { CommonAction } from "../../Common/Actions/CommonActions";
import { loadTableViewInfo, editRecord, resetChanges } from "../Actions/TableViewActions";
import { TableViewInfo } from "../States/TableViewState";
import { TableViewRecord } from "../States/TableViewRecord";
import { FieldInfo } from "../../Common/States/FieldInfo";
import { Dispatch } from "redux";
import { StoreOperation, Model } from "../../Common/States/ExtStates";
import { isEqualCoordinates } from "../Helpers/TableViewHelper";

const mapToColumnInfo = (
    fieldIfnos: FieldInfo[],
    meta: CostMetaData,
    costBlockCache: Map<string, CostBlockMeta>,
    mapFn: (costBlock: CostBlockMeta, fieldInfo: FieldInfo) => ColumnInfo<TableViewRecord>
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

    return <ColumnInfo<TableViewRecord>>{
        ...buildColumn(item, fieldInfo),
        type: ColumnType.Text,
        mappingFn: (record: TableViewRecord) => record.coordinates[fieldInfo.dataIndex].name
    };
}

const buildCostElementColumn = (costBlock: CostBlockMeta, fieldInfo: FieldInfo, state: TableViewInfo) => {
    let type: ColumnType;
    let referenceItems: Map<string, NamedId>;

    const costElement = findMeta(costBlock.costElements, fieldInfo.fieldName);
    const fieldType = costElement.typeOptions ? costElement.typeOptions.Type : FieldType.Double;

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

    return <ColumnInfo<TableViewRecord>>{
        ...buildColumn(costElement, fieldInfo),
        isEditable: true,
        type,
        referenceItems,
        mappingFn: record => record.data[fieldInfo.dataIndex].value,
        editMappingFn: (record, dataIndex) => record.data.data[dataIndex].value = record.get(dataIndex),
        rendererFn: (value, record) => {
            const dataIndex = buildCountDataIndex(fieldInfo.dataIndex);
            const count = record.get(dataIndex);

            return count == 1 ? value : `(${count} values)`;
        }
    };
}

const buildCountColumns = (costBlock: CostBlockMeta, fieldInfo: FieldInfo) => (<ColumnInfo<TableViewRecord>>{
    isInvisible: true,
    dataIndex: buildCountDataIndex(fieldInfo.dataIndex),
    mappingFn: record => record.data[fieldInfo.dataIndex].count
})

const buildCountDataIndex = (dataIndex: string) => `${dataIndex}_Count`

const buildProps = (state: CommonState) => {
    let readUrl: string;
    let updateUrl: string;
    
    const columns = [];
    const filterDataIndexes = [];
    const tableViewInfo = state.pages.tableView.info;
    const meta = state.app.appMetaData;

    if (tableViewInfo && meta) {
        readUrl = buildGetRecordsUrl();

        const costBlockCache = new Map<string, CostBlockMeta>();
        const coordinateColumns = mapToColumnInfo(tableViewInfo.recordInfo.coordinates, meta, costBlockCache, buildCoordinateColumn);
        const costElementColumns = mapToColumnInfo(
            tableViewInfo.recordInfo.data, 
            meta, 
            costBlockCache, 
            (costBlock, fieldInfo) => buildCostElementColumn(costBlock, fieldInfo, tableViewInfo));
        
        const countColumns = mapToColumnInfo(tableViewInfo.recordInfo.data, meta, costBlockCache, buildCountColumns);

        columns.push(...countColumns, ...coordinateColumns, ...costElementColumns);

        coordinateColumns.forEach(column => filterDataIndexes.push(column.dataIndex));
    }

    return <AjaxDynamicGridProps<TableViewRecord>>{
        columns,
        filterDataIndexes,
        apiUrls: {
            read: readUrl
        }
    };
}

const buildActions = (state: CommonState, dispatch: Dispatch) => (<AjaxDynamicGridActions<TableViewRecord>>{
    init: () => !state.pages.tableView.info && handleRequest(
        getTableViewInfo().then(
            tableViewInfo => dispatch(loadTableViewInfo(tableViewInfo))
        )
    ),
    onUpdateRecord: (store, record, operation, modifiedFieldNames) => {
        switch (operation) {
            case StoreOperation.Edit:
                const [dataIndex] = modifiedFieldNames;
                const tableViewRecord = record.data;
                const countDataIndex = buildCountDataIndex(dataIndex);

                if (record.get(countDataIndex) == 0) {
                    record.data[countDataIndex] = 1;
                }
                break;
        }
    },
    onUpdateRecordSet: (records, operation, dataIndex) => {
        if (operation == StoreOperation.Edit) {
            const tableViewRecords = records.map(rec => rec.data);

            dispatch(editRecord(tableViewRecords, dataIndex));
        }
    },
    onSave: () =>  handleRequest(
        updateRecords(state.pages.tableView.editedRecords).then(
            () => dispatch(resetChanges())
        )
    ),
    onCancel: () => dispatch(resetChanges()),
    onLoadData: (store, records) => {
        const editRecords = state.pages.tableView.editedRecords;
        if (editRecords && editRecords.length > 0) {
            for (const editRecord of editRecords) {
                const record = records.find(item => isEqualCoordinates(item.data, editRecord));

                if (record) {
                    Object.keys(editRecord.data).forEach(key => {
                        record.set(key, editRecord.data[key].value);
                    });
                }
            }
        }
    }
})

export const TableViewContainer = connectAdvanced<CommonState, AjaxDynamicGridProps, any>(
    dispatch => state => ({
        ...buildProps(state),
        ...buildActions(state, dispatch)
    })
)(AjaxDynamicGrid)
