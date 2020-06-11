import { StoreDynamicGridProps, DynamicGrid } from "./DynamicGrid";
import { Model, StoreOperation, Store, StoreUpdateEventFn } from "../States/ExtStates";
import { FilterItem, ColumnInfo, ColumnType } from "../States/ColumnInfo";
import { buildReferenceColumnRendered } from "../Helpers/GridHeper";
import * as React from "react";
import { ToolbarDynamicGridProps } from "./Props/DynamicGridProps";

export const CHECKED_DATA_INDEX = 'checked'
export const VALUE_DATA_INDEX = 'value'

interface FilterDataItem {
    store: Store<FilterItem>
    dataSet: Set<any>
    filteredDataSet: Set<any>
    renderFn: (value, record: Model) => any
}

export interface LocalDynamicGridActions<T=any> {
    onUpdateRecord?: StoreUpdateEventFn<T>
    onUpdateRecordSet? (records: Model<T>[], operation: StoreOperation, dataIndex: string)
    onLoadData?(store: Store<T>, records: Model<T>[], filterStores: Map<string, Store<FilterItem>>)
}

export interface LocalDynamicGridProps<T=any> extends ToolbarDynamicGridProps, LocalDynamicGridActions<T>  {
}

export class LocalDynamicGrid<TData=any, TProps extends LocalDynamicGridProps<TData>=LocalDynamicGridProps<TData>> extends React.Component<TProps> {
    private readonly dataStoreEvents = {
        load: this.onDataStoreLoad,
        update: this.onDataStoreUpdate,
    };

    private store: Store<TData>
    private filterDatas: Map<string, FilterDataItem>
    private executeFiltrateFilters = true
    private executeFillFilterData = true
    private updatedRecords: Model[] = []
    private innerColumns: ColumnInfo[]
    private prevProps: TProps
    private lastLevelVisibleColumns: ColumnInfo[]
    private innerGrid: DynamicGrid

    public componentWillUnmount() {
        if (this.store) {
            this.forEachDataStoreEvents((eventName, handler) => this.store.un(eventName, handler));
        }        
    }

    public render(){
        this.init();

        return (
            <DynamicGrid 
                {...this.props}
                ref={this.innerGridRef}
                store={this.store} 
                columns={this.innerColumns} 
            />
        );
    }

    public commitChanges = () => {
        this.innerGrid.commitChanges();
    }

    public cancel = () => {
        this.innerGrid.cancel();
    }

    public saveWithCallback = (callback: () => void) => {
        this.innerGrid.saveWithCallback(callback);
    }

    public save = () => {
        this.innerGrid.save();
    }

    public getStore() {
        return this.store
    }

    protected init() {
        const { columns = [] } = this.props;        
        
        if (!this.prevProps || this.prevProps.columns != columns) {
            this.lastLevelVisibleColumns = this.getLastLevelVisibleColumns(columns);
            this.initFilterData(this.lastLevelVisibleColumns);
            this.innerColumns = this.getInnerColumns(columns);
        }

        this.initStore(this.props, this.lastLevelVisibleColumns);
        
        this.prevProps = this.props;
    }

    protected buildDataStoreFields(columns: ColumnInfo[]) {
       return columns.map(column => ({ 
            name: column.dataIndex, 
            mapping: column.mappingFn 
                ? data => this.replaceNullValue(column.mappingFn(data)) 
                : data => this.replaceNullValue(data[column.dataIndex])
        }));
    }

    protected buildDataStore(props: TProps, visibleColumns: ColumnInfo[]) {
        return Ext.create('Ext.data.Store', {
            fields: this.buildDataStoreFields(visibleColumns)
        });
    }

    protected isUpdatingDataStore(prevProps: TProps, currentProps: TProps) {
        return !prevProps || prevProps.columns != currentProps.columns;
    }

    private innerGridRef = (grid: DynamicGrid) => {
        this.innerGrid = grid;
    }

    private getInnerColumns(columns: ColumnInfo[]) {
        return columns.map(column => {
            let innerColumn: ColumnInfo;

            if (!column.isInvisible) {
                if (column.columns) {
                    innerColumn = {
                        ...column,
                        columns: this.getInnerColumns(column.columns)
                    }
                } else {
                    innerColumn = {
                        ...column,
                        filter: column.filter || {
                            store: this.filterDatas.get(column.dataIndex).store,
                            checkedDataIndex: CHECKED_DATA_INDEX,
                            valueDataIndex: VALUE_DATA_INDEX
                        }
                    };
                }
            }

            return innerColumn;
        });
    }

    private initStore(props: TProps, visibleColumns: ColumnInfo[]) {
        if (this.isUpdatingDataStore(this.prevProps, props)) {
            this.store = this.buildDataStore(props, visibleColumns);

            if (this.store) {
                this.forEachDataStoreEvents((eventName, handler) => this.store.on(eventName, handler, this));
            }
        }
    }

    private forEachDataStoreEvents(fn: (eventName: string, handler: Function) => void) {
        Object.keys(this.dataStoreEvents).forEach(eventName => fn(eventName, this.dataStoreEvents[eventName]));
    }

    private onDataStoreLoad(store: Store, records: Model[]) {
        const { onLoadData } = this.props;

        this.fillFilterData(() => {
            if (onLoadData) {
                const filterStores = new Map<string, Store<FilterItem>>();
    
                this.filterDatas.forEach((filterDataItem, dataIndex) => {
                    filterStores.set(dataIndex.toLowerCase(), filterDataItem.store)
                });
    
                onLoadData(store, records, filterStores);
            }
        });
    }

    private onDataStoreUpdate(store, record, operation, modifiedFieldNames, details) {
        const { onUpdateRecord, onUpdateRecordSet } = this.props;

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

    private initFilterData(visibleColumns: ColumnInfo[]) {
        this.filterDatas = new Map<string, FilterDataItem>();

        const defaultRender = (value, record: Model) => value;

        visibleColumns.forEach(column => {
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

    private filtrateStore(visibleColumns: ColumnInfo[]) {
        const records: Model[] = [];

        this.store.filterBy(record => {
            let isVisible = true;

            for (const column of visibleColumns) {
                const value = record.get(column.dataIndex);
                const { store: filterStore, renderFn } = this.filterDatas.get(column.dataIndex);

                filterStore.each(
                    ({ data: filterItem }) => {
                        isVisible = renderFn(value, record) != filterItem.value || filterItem.checked

                        return isVisible;
                    },
                    this,
                    true
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
            this.filtreteFilters(dataIndex);
        }
    }

    private filtreteFilters(dataIndex: string) {
        if (this.executeFiltrateFilters) {
            this.executeFiltrateFilters = false;

            setTimeout(() => {
                const visibleColumns = this.getLastLevelVisibleColumns(this.props.columns);
                const records = this.filtrateStore(visibleColumns);
                const dataSets = this.buildDataSets(records);

                const headerId = dataIndex.replace(/\./g, '');
                const headerIndex = dataIndex;

                let header = Ext.ComponentQuery.query(`[itemId="${headerId}"]`)[0];
                if(header) header.addCls('filtered-column')

                this.filterDatas.forEach((filterData, dataIndex) => {
                    let allChecked = true;

                    filterData.store.each(record => allChecked = record.data.checked);

                    if (allChecked && headerIndex == dataIndex) {
                        if (header) header.removeCls('filtered-column')
                     }

                    if (allChecked && headerIndex != dataIndex) {
                        filterData.filteredDataSet = dataSets.get(dataIndex);

                        const filters = filterData.store.getFilters();
                        filters.each(filter => filters.remove(filter));

                        filterData.store.filterBy(
                            record => filterData.filteredDataSet.has(record.data.value)
                        );
                    }
                });

                this.executeFiltrateFilters = true;
            });
        }
    }

    private fillFilterData(calback?: () => void) {
        if (this.executeFillFilterData) {
            this.executeFillFilterData = false;

            setTimeout(() => {
                const records: Model[] = [];
    
                this.store.each(record => records.push(record), this, true);
                
                const dataSets = this.buildDataSets(records);
    
                this.updateFilterData(dataSets);

                this.executeFillFilterData = true;

                calback && calback();
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
        return value == null ? undefined : value;
    }

    private getLastLevelVisibleColumns(columns: ColumnInfo[]) {
        const result: ColumnInfo[] = [];

        for (let column of columns) {
            if (!column.isInvisible) {
                if (column.columns) {
                    result.push(...this.getLastLevelVisibleColumns(column.columns));
                } else {
                    result.push(column);
                }
            }
        }

        return result;
    }
}