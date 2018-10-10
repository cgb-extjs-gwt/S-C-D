import * as React from "react";
import { DynamicGridProps, DynamicGridActions } from "./Props/DynamicGridProps";
import { DynamicGrid } from "./DynamicGrid";
import { ColumnInfo, FilterItem, ColumnType } from "../States/ColumnInfo";
import { Model, StoreOperation, Store, StoreUpdateEventFn } from "../States/ExtStates";
import { Container, Column } from "@extjs/ext-react";
import { buildReferenceColumnRendered } from "../Helpers/GridHeper";

const CHECKED_DATA_INDEX = 'checked'
const VALUE_DATA_INDEX = 'value'

export interface AjaxDynamicGridActions<T=any> extends DynamicGridActions {
    onUpdateRecord?: StoreUpdateEventFn<T>
    onUpdateRecordSet? (records: Model<T>[], operation: StoreOperation, dataIndex: string)
    onLoadData?(store: Store<T>, records: Model<T>[])
}

export interface AjaxDynamicGridProps<T=any> extends DynamicGridProps, AjaxDynamicGridActions<T> {
    apiUrls: {
        read: string
        update?: string
        create?: string
        destroy?: string
    }
    filterDataIndexes: string[]
}

type FilterDataItem = {
    store: Store<FilterItem>
    dataSet: Set<any>
    filteredDataSet: Set<any>
    renderFn: (value, record: Model) => any
}

export class AjaxDynamicGrid extends React.Component<AjaxDynamicGridProps> {
    private store: Store
    private filterDatas: Map<string, FilterDataItem>
    private columns: ColumnInfo[]
    private executeFiltrateFilters = true;
    private executeFillFilterData = true;
    private updatedRecords: Model[] = [];

    public render() {
        const { children, columns, flex } = this.props;

        if (columns && columns.length > 0) {
            this.initFilterData();
            this.initStores();
            this.initColumns();
        }

        const props = {
            ...this.props,
            columns: this.columns || []
        } as AjaxDynamicGridProps;

        return (
            <Container layout="vbox" flex={flex}>
                <DynamicGrid 
                    {...props} 
                    store={this.store} 
                    flex={1}
                >
                    {children}
                </DynamicGrid>
            </Container>
        );
    }

    private initColumns() {
        if (!this.columns) {
            this.columns = this.getVisibleColumns().map(column => ({
                ...column,
                filter: column.filter || {
                    store: this.filterDatas.get(column.dataIndex).store,
                    checkedDataIndex: CHECKED_DATA_INDEX,
                    valueDataIndex: VALUE_DATA_INDEX
                }
            }))
        }
    }

    private initStores() {
        if (!this.store) {
            const { columns, apiUrls, onUpdateRecord, onUpdateRecordSet, onLoadData } = this.props;

            const fields = columns.map(column => ({ 
                name: column.dataIndex, 
                mapping: column.mappingFn 
                    ? data => this.replaceNullValue(column.mappingFn(data)) 
                    : data => this.replaceNullValue(data[column.dataIndex])
            }));

            this.store = Ext.create('Ext.data.Store', {
                fields,
                autoLoad: true,
                pageSize: 0,
                proxy: {
                    type: 'ajax',
                    api: apiUrls,
                    reader: { 
                        type: 'json',
                    },
                    writer: {
                        type: 'json',
                        writeAllFields: true,
                        allowSingle: false
                    }
                },
                listeners: {
                    load: (store: Store, records: Model[]) => {
                        this.fillFilterData();

                        onLoadData && onLoadData(store, records);
                    },
                    update: (store, record, operation, modifiedFieldNames, details) => {
                        this.fillFilterData();
    
                        onUpdateRecord &&  onUpdateRecord(store, record, operation, modifiedFieldNames, details);

                        if (onUpdateRecordSet) {
                            if (this.updatedRecords.length == 0) {
                                setTimeout(() => { 
                                    onUpdateRecordSet(this.updatedRecords, operation, modifiedFieldNames && modifiedFieldNames[0]);

                                    this.updatedRecords = [];
                                });
                            }

                            this.updatedRecords.push(record);
                        }
                    }
                },
            });
        }
    }

    private initFilterData() {
        if (!this.filterDatas) {
            this.filterDatas = new Map<string, FilterDataItem>();

            const defaultRender = (value, record: Model) => value;

            this.getVisibleColumns().forEach(column => {
                const store = Ext.create('Ext.data.Store', {
                    fields: [ CHECKED_DATA_INDEX, VALUE_DATA_INDEX ],
                    sorters: [{
                        property: VALUE_DATA_INDEX,
                        direction: 'ASC'
                    }],
                    listeners: {
                        update: (store, record, operation, modifiedFieldNames, details) => {
                            this.onUpdateFilterStore(store, record, operation, modifiedFieldNames, column.dataIndex);
                        }
                    }
                });

                let renderFn;

                if (column.type === ColumnType.Reference) {
                    renderFn = buildReferenceColumnRendered(column);
                } 
                else {
                    renderFn = column.rendererFn ? column.rendererFn : defaultRender;
                }

                this.filterDatas.set(column.dataIndex, { 
                    store, 
                    renderFn,
                    dataSet: new Set<any>(),
                    filteredDataSet: new Set<any>()
                });
            });
        }
    }

    private filtrateStore() {
        const { columns } = this.props;

        const records: Model[] = [];

        this.store.filterBy(record => {
            let isVisible = true;

            for (const column of this.getVisibleColumns()) {
                const value = record.get(column.dataIndex);
                const { store: filterStore, renderFn } = this.filterDatas.get(column.dataIndex);

                filterStore.each(
                    ({ data: filterItem }) => {
                        isVisible = renderFn(value, record) != filterItem.value || filterItem.checked

                        return isVisible;
                    }
                );

                if (!isVisible) {
                    break;
                }
            }

            if (isVisible) {
                records.push(record);
            }

            return isVisible;
        });

        return records;
    }

    onUpdateFilterStore = (store, record, operation, modifiedFieldNames, dataIndex) => {
        if (operation == StoreOperation.Edit && modifiedFieldNames[0] == CHECKED_DATA_INDEX) {
            this.filtreteFilters();
        }
    }

    private filtreteFilters() {
        if (this.executeFiltrateFilters) {
            this.executeFiltrateFilters = false;

            setTimeout(() => {
                const records = this.filtrateStore();
                const dataSets = this.buildDataSets(records);

                this.filterDatas.forEach((filterData, dataIndex) => {
                    let allChecked = true;

                    filterData.store.each(record => allChecked = record.data.checked);

                    if (allChecked) {
                        filterData.filteredDataSet = dataSets.get(dataIndex);

                        filterData.store.filterBy(
                            record => filterData.filteredDataSet.has(record.data.value)
                        );
                    }
                });

                this.executeFiltrateFilters = true;
            });
        }
    }

    private fillFilterData() {
        if (this.executeFillFilterData) {
            this.executeFillFilterData = true;

            setTimeout(() => {
                const records: Model[] = [];
    
                this.store.each(record => records.push(record), this, true);
                
                const dataSets = this.buildDataSets(records);
    
                this.updateFilterData(dataSets);

                this.executeFillFilterData = false;
            });
        }
    }

    private buildDataSets(records: Model[]) {
        const newDataSets = new Map<string, Set<any>>();

        this.filterDatas.forEach((data, dataIndex) => newDataSets.set(dataIndex, new Set<any>()));

        records.forEach(record => {
            this.filterDatas.forEach(({ renderFn }, dataIndex) => {
                const value = record.get(dataIndex);
                const renderedValue = renderFn(value, record);

                newDataSets.get(dataIndex).add(renderedValue);
            });
        });

        return newDataSets;
    }

    private updateFilterData(newDataSets: Map<string, Set<any>>) {
        newDataSets.forEach((newDataSet, dataIndex) => {
            const { dataSet, store, filteredDataSet } = this.filterDatas.get(dataIndex);

            dataSet.forEach(value => {
                if (!newDataSet.has(value)) {
                    dataSet.delete(value);
                    filteredDataSet.delete(value);

                    store.remove(
                        store.findBy(record => record.data.value === value)
                    )
                }
            });

            newDataSet.forEach(value => {
                if (!dataSet.has(value)) {
                    dataSet.add(value);
                    filteredDataSet.add(value);
                    store.add({ checked: true, value } as FilterItem);
                }
            });
        });
    }

    private replaceNullValue(value) {
        return value == null ? ' ' : value;
    }

    private getVisibleColumns() {
        return this.props.columns.filter(column => !column.isInvisible);
    }
}