import { StoreDynamicGridProps, DynamicGrid } from "./DynamicGrid";
import { Model, StoreOperation, Store, StoreUpdateEventFn } from "../States/ExtStates";
import { FilterItem, ColumnInfo, ColumnType } from "../States/ColumnInfo";
import { buildReferenceColumnRendered } from "../Helpers/GridHeper";
import * as React from "react";
import { ToolbarDynamicGridProps } from "./Props/DynamicGridProps";

const CHECKED_DATA_INDEX = 'checked'
const VALUE_DATA_INDEX = 'value'

interface FilterDataItem {
    store: Store<FilterItem>
    dataSet: Set<any>
    filteredDataSet: Set<any>
    renderFn: (value, record: Model) => any
}

export interface LocalDynamicGridActions<T=any> {
    onUpdateRecord?: StoreUpdateEventFn<T>
    onUpdateRecordSet? (records: Model<T>[], operation: StoreOperation, dataIndex: string)
    onLoadData?(store: Store<T>, records: Model<T>[])
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

    public componentWillUnmount() {
        if (this.store) {
            this.forEachDataStoreEvents((eventName, handler) => this.store.un(eventName, handler));
        }        
    }

    public render(){
        this.init(this.props);

        return (
            <DynamicGrid 
                {...this.props}
                store={this.store} 
                columns={this.innerColumns} 
            />
        );
    }

    public getStore() {
        return this.store;
    }

    protected init(props: TProps) {
        const { columns = [] } = props;        
        const visibleColumns = this.getVisibleColumns(columns);

        this.initFilterData(visibleColumns);
        this.initStore(props);
        this.initColumns(visibleColumns);
    }

    protected buildDataStoreFields(columns: ColumnInfo[]) {
       return columns.map(column => ({ 
            name: column.dataIndex, 
            mapping: column.mappingFn 
                ? data => this.replaceNullValue(column.mappingFn(data)) 
                : data => this.replaceNullValue(data[column.dataIndex])
        }));
    }

    protected buildDataStore(props: TProps) {
        const { columns = [] } = props;

        return Ext.create('Ext.data.Store', {
            fields: this.buildDataStoreFields(columns)
        });
    }

    private initColumns(visibleColumns: ColumnInfo[]) {
        this.innerColumns = visibleColumns.map(column => ({
            ...column,
            filter: column.filter || {
                store: this.filterDatas.get(column.dataIndex).store,
                checkedDataIndex: CHECKED_DATA_INDEX,
                valueDataIndex: VALUE_DATA_INDEX
            }
        }))
    }

    private initStore(props: TProps) {
        this.store = this.buildDataStore(props);

        this.forEachDataStoreEvents((eventName, handler) => this.store.on(eventName, handler, this));
    }

    private forEachDataStoreEvents(fn: (eventName: string, handler: Function) => void) {
        Object.keys(this.dataStoreEvents).forEach(eventName => fn(eventName, this.dataStoreEvents[eventName]));
    }

    private onDataStoreLoad(store: Store, records: Model[]) {
        const { onLoadData } = this.props;

        this.fillFilterData();

        onLoadData && onLoadData(store, records);
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
                const visibleColumns = this.getVisibleColumns(this.props.columns);
                const records = this.filtrateStore(visibleColumns);
                const dataSets = this.buildDataSets(records);

                const headerId = dataIndex.replace('.', '');
                const headerIndex = dataIndex;
                const headerText = visibleColumns.find(x => x.dataIndex == dataIndex).title;
                const filterSymbol = "&#8704;"
                this.setColumnHeaderText(headerId, headerText + "  " + filterSymbol);

                this.filterDatas.forEach((filterData, dataIndex) => {
                    let allChecked = true;

                    filterData.store.each(record => allChecked = record.data.checked);

                    if (allChecked && headerIndex == dataIndex) {
                        this.setColumnHeaderText(headerId, headerText)
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

    private setColumnHeaderText(headerId: string, text: string) {
        Ext.getCmp(headerId).setText(text);
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

    private getVisibleColumns(columns: ColumnInfo[]) {
        return columns.filter(column => !column.isInvisible);
    }
}