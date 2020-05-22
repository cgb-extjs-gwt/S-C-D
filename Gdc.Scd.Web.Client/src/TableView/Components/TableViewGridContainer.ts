import { connectAdvanced } from "react-redux";
import { CommonState } from "../../Layout/States/AppStates";
import { ColumnInfo, ColumnType, FilterItem } from "../../Common/States/ColumnInfo";
import { getCostElementByAppMeta } from "../../Common/Helpers/MetaHelper";
import { NamedId } from "../../Common/States/CommonStates";
import { FieldType, CostElementMeta, CostMetaData, InputLevelMeta } from "../../Common/States/CostMetaStates";
import { buildGetRecordsUrl, getTableViewInfo, updateRecords, buildGetHistoryUrl, exportToExcel } from "../Services/TableViewService";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import { loadTableViewInfo, editRecord, resetChanges } from "../Actions/TableViewActions";
import { TableViewInfo, DataInfo } from "../States/TableViewState";
import { TableViewRecord } from "../States/TableViewRecord";
import { Dispatch } from "redux";
import { StoreOperation, Model, Store } from "../../Common/States/ExtStates";
import { isEqualCoordinates } from "../Helpers/TableViewHelper";
import { TableViewGridActions, TableViewGrid, TableViewGridProps } from "./TableViewGrid";
import { buildCostElementColumn } from "../../Common/Helpers/ColumnInfoHelper";
import { ApiUrls } from "../../Common/Components/AjaxDynamicGrid";
import { RouteComponentProps } from "react-router";
import { VALUE_DATA_INDEX, CHECKED_DATA_INDEX } from "../../Common/Components/LocalDynamicGrid";
import { toArray } from "../../Common/Helpers/ArrayHelper";
import { hasQueryParams, deleteQueryParams } from "../../Common/Helpers/RouterHelper";
import { saveTableViewToServer } from "../Actions/TableViewActionsAsync";

const buildProps = (() => {
    let oldMeta: CostMetaData;
    let oldTableViewInfo: TableViewInfo;
    let oldEditedRecords: TableViewRecord[];
    let oldProps = buildGridProps(oldTableViewInfo, oldEditedRecords, oldMeta);
    let oldColumns = oldProps.columns;
    let oldApiUrls = oldProps.apiUrls;

    return (state: CommonState) => {
        let newResult: TableViewGridProps;

        const newMeta = state.app.appMetaData;
        const newTableViewInfo = state.pages.tableView.info;
        const newEditedRecords = state.pages.tableView.editedRecords;

        if (oldMeta == newMeta && oldTableViewInfo == newTableViewInfo && oldEditedRecords == newEditedRecords) {
            newResult = oldProps;
        } else {
            newResult = oldProps = buildGridProps(newTableViewInfo, newEditedRecords, newMeta);
            oldMeta = newMeta;
            oldTableViewInfo = newTableViewInfo;
            oldEditedRecords = newEditedRecords;
            oldColumns = newResult.columns;
            oldApiUrls = newResult.apiUrls;
        }

        return newResult;
    }

    function buildGridProps (tableViewInfo: TableViewInfo, editedRecords: TableViewRecord[], meta: CostMetaData) {
        let readUrl: string;
        let columns: ColumnInfo<TableViewRecord>[];
        let apiUrls: ApiUrls;

        const hasChanges = editedRecords && editedRecords.length > 0;

        if (tableViewInfo == oldTableViewInfo) {
            columns = oldColumns;
            apiUrls = oldApiUrls;
        } else {
            const roleCodeReferences = new Map<number, NamedId<number>>();

            tableViewInfo.roleCodeReferences.forEach(roleCode => roleCodeReferences.set(roleCode.id, roleCode));

            columns = [
                ...buildCoordinateColumns(tableViewInfo.recordInfo.coordinates),
                ...buildAdditionalColumns(),
                {
                    title: 'Role code',
                    dataIndex: 'wgRoleCodeId',
                    isEditable: true,
                    type: ColumnType.Reference,
                    width: 100,
                    referenceItems: roleCodeReferences,
                },
                {
                    title: 'Responsible person',
                    dataIndex: 'wgResponsiblePerson',
                    isEditable: true,
                    type: ColumnType.Text,
                    width: 100,
                },
                {
                    title: 'PSM Release',
                    dataIndex: 'wgPsmRelease',
                    isEditable: true,
                    type: ColumnType.CheckBox,
                    width: 80,
                },
                ...buildCostElementColumns()
            ];

            apiUrls = {
                read: buildGetRecordsUrl()
            };
        }

        return <TableViewGridProps>{
            columns,
            apiUrls: apiUrls,
            hasChanges: editedRecords && editedRecords.length > 0
        };

        function buildCoordinateColumns (coordinateIds: string[]) { 
            const coordinateIdSet = new Set<string>(coordinateIds);
            const inputLevelMap = new Map<string, InputLevelMeta>();
        
            for (const { inputLevels } of meta.costBlocks[0].costElements) {
                for (const inputLevel of inputLevels){
                    if (coordinateIdSet.has(inputLevel.id)){
                        inputLevelMap.set(inputLevel.id, inputLevel);
                    }
        
                    if (inputLevelMap.size == coordinateIds.length) {
                        return coordinateIds.map(coordinateId => (<ColumnInfo<TableViewRecord>>{
                            title: inputLevelMap.get(coordinateId).name,
                            dataIndex: coordinateId,
                            type: ColumnType.Text,
                            width: 100,
                            mappingFn: (record: TableViewRecord) => record.coordinates[coordinateId].name,
                        }))
                    }
                }
            }
        }

        function buildCostElementColumns () {
            const columns: ColumnInfo<TableViewRecord>[] = [];
            const topColumns = new Map<string, Map<string, ColumnInfo<TableViewRecord>>>();
        
            for (const dataInfo of tableViewInfo.recordInfo.data) {
                const { dependencyItemId, costBlockId, costElementId } = dataInfo;
                const costElement = getCostElementByAppMeta(meta, costBlockId, costElementId);
        
                if (dependencyItemId == null) {
                    columns.push(buildColumn(costElement.name, dataInfo, costElement))
                } else {
                    let costBlockColumns = topColumns.get(costBlockId);
                    if (!costBlockColumns) {
                        costBlockColumns = new Map<string, ColumnInfo<TableViewRecord>>();
                        
                        topColumns.set(costBlockId, costBlockColumns);
                    }
        
                    let column = costBlockColumns.get(costElementId);
                    if (!column) {
                        column = <ColumnInfo<TableViewRecord>>{
                            title: costElement.name,
                            dataIndex: costElement.id,
                            columns: []
                        }
        
                        columns.push(column);
        
                        costBlockColumns.set(costElementId, column);
                    } 
        
                    const title = tableViewInfo.dependencyItems[costElement.dependency.id][dependencyItemId].name;
                    const subColumn = buildColumn(title, dataInfo, costElement);
        
                    column.columns.push(subColumn);
                }
            }
        
            return columns;

            function buildColumn(title: string, { costBlockId, dataIndex }: DataInfo, costElement: CostElementMeta) {
                const fieldType = costElement.typeOptions ? costElement.typeOptions.Type : FieldType.Double;
                const references = fieldType == FieldType.Reference 
                    ? tableViewInfo.costBlockReferences[costBlockId].references[costElement.id] 
                    : null
        
                return buildCostElementColumn<TableViewRecord>({
                    title,
                    dataIndex,
                    references,
                    type: fieldType,
                    inputType: costElement.inputType,
                    width: 100,
                    mappingFn: record => {
                        const { value }  = record.data[dataIndex]

                        return value == null ? ' ' : value;
                    },
                    editMappingFn: (record, dataIndex) => record.data.data[dataIndex].value = record.get(dataIndex),
                    getCountFn: record => record.data.data[dataIndex].count,
                    getIsApprovedFn: record => record.data.data[dataIndex].isApproved,
                })
            }
        }

        function buildAdditionalColumns() {
            return tableViewInfo.recordInfo.additionalData.map(({ title, dataIndex }) => (<ColumnInfo<TableViewRecord>>{
                title: title,
                dataIndex: dataIndex,
                type: ColumnType.Text,
                width: 150,
                mappingFn: record => record.additionalData[dataIndex],
            }));
        }
    }
})()

const buildActions = (() => {
    let oldActions: TableViewGridActions;
    let oldTableViewInfo: TableViewInfo | {} = {};
    let editRecords: TableViewRecord[];
    
    return (state: CommonState, dispatch: Dispatch, ownProps: TableViewGridContainerProps) => {
        let newActions: TableViewGridActions;
        const newTableViewInfo = state.pages.tableView.info;

        editRecords = state.pages.tableView.editedRecords;

        if (oldTableViewInfo == newTableViewInfo) {
            newActions = oldActions;
        } else {
            newActions = oldActions = buildTableViewGridActions(newTableViewInfo, dispatch, ownProps);
            oldTableViewInfo = newTableViewInfo;
        }
        
        return newActions;
    }

    function buildTableViewGridActions (tableViewInfo: TableViewInfo, dispatch: Dispatch, ownProps: TableViewGridContainerProps) { 
        const buildSaveFn = (isApproving: boolean) => () => dispatch(saveTableViewToServer({ isApproving: isApproving }));

        return <TableViewGridActions>{
            init: () => !tableViewInfo && handleRequest(
                getTableViewInfo().then(
                    tableViewInfo => dispatch(loadTableViewInfo(tableViewInfo))
                )
            ),
            onUpdateRecord: (store, record, operation, modifiedFieldNames) => {
                switch (operation) {
                    case StoreOperation.Edit:
                        const [dataIndex] = modifiedFieldNames;
                        const tableViewRecord = record.data;

                        if (dataIndex in record.data.additionalData || dataIndex in record.data.coordinates) {
                            record.reject();
                        }
                        else {
                            const value = record.get(dataIndex);
                            const valueCount = record.data.data[dataIndex];
                            
                            if (valueCount){
                                if (value == null) {
                                    record.set(dataIndex, undefined);
    
                                    valueCount.count = 0;
                                } else {
                                    valueCount.count = 1;
                                }
    
                                valueCount.isApproved = false;
                            }
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
            onSave: buildSaveFn(false),
            onApprove: buildSaveFn(true),
            onCancel: () => dispatch(resetChanges()),
            onLoadData: (store, records, filterStores) => {
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

                setFiltersByUrl(filterStores, ownProps);
            }
        }

        function setFiltersByUrl(filterStores: Map<string, Store<FilterItem>>, router: RouteComponentProps) {
            if (hasQueryParams(router)) {
                const queryData = Ext.Object.fromQueryString(router.location.search);

                Object.keys(queryData).map(key => key.toLowerCase()).forEach(dataIndex => {
                    const store = filterStores.get(dataIndex);

                    if (store) {
                        const selectedRecordsSet = new Set<Model<FilterItem>>();

                        toArray(queryData[dataIndex]).forEach(value => {
                            const record = store.findRecord(VALUE_DATA_INDEX, value, 0, false, false, true);

                            if (record) {
                                selectedRecordsSet.add(record);
                            }
                        });

                        store.each(record => {
                            if (!selectedRecordsSet.has(record)) {
                                record.set(CHECKED_DATA_INDEX, false);
                            }
                        });

                        deleteQueryParams(router);
                    }
                });
            }
        }
    }
})()

const buildPropsActions = (() => { 
    let oldProps: TableViewGridProps;
    let oldActions: TableViewGridActions;
    let oldPropsActions: TableViewGridProps;
    let oldOnSelectionChange: Function;

    return (dispatch: Dispatch, state: CommonState, ownProps: TableViewGridContainerProps) => {
        let propsActions: TableViewGridProps;
        const props = buildProps(state);
        const actions = buildActions(state, dispatch, ownProps);
        const { onSelectionChange } = ownProps;

        if (oldProps == props && oldActions == actions && oldOnSelectionChange == onSelectionChange) {
            propsActions = oldPropsActions;
        } else {
            oldProps = props;
            oldActions = actions;
            oldOnSelectionChange = onSelectionChange;

            oldPropsActions = propsActions = <TableViewGridProps>{
                ...props,
                ...actions,
                onSelectionChange
            }
        }

        return propsActions;
    }
})()

export interface TableViewGridContainerProps extends RouteComponentProps {
    onSelectionChange?(grid, records: Model[], selecting: boolean, selectionInfo)
}

export const TableViewGridContainer = connectAdvanced<CommonState, TableViewGridProps, TableViewGridContainerProps>(
    dispatch => (state, ownProps) => buildPropsActions(dispatch, state, ownProps)
)(TableViewGrid)
