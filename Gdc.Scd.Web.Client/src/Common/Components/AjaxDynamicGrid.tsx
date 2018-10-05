import * as React from "react";
import { DynamicGridProps, DynamicGridActions } from "./Props/DynamicGridProps";
import { DynamicGrid } from "./DynamicGrid";
import { ColumnInfo } from "../States/ColumnInfo";
import { Model, StoreOperation, Store, StoreUpdateEventFn } from "../States/ExtStates";
import { Container } from "@extjs/ext-react";
import { DynamicGridFilter, ColumnFilter, FilterItem, CHECKED_DATA_INDEX, VALUE_DATA_INDEX } from "./DynamicGridFilter";

export interface AjaxDynamicGridActions<T=any> extends DynamicGridActions {
    onUpdateRecord?: StoreUpdateEventFn<T>
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
    renderFn: (value, record: Model) => any
}

export class AjaxDynamicGrid extends React.Component<AjaxDynamicGridProps> {
    private store: Store
    private columnFilters: ColumnFilter[]
    private filterData: Map<string, FilterDataItem>

    public render() {
        const { children, columns, flex } = this.props;

        if (columns && columns.length > 0) {
            this.initStores();
            this.initFilterData();
            this.initColumnFilters();
        }

        return (
            <Container layout="vbox" flex={flex}>
                {
                    this.columnFilters &&  
                    this.columnFilters.length > 0 &&
                    <DynamicGridFilter columnFilters={this.columnFilters}/>
                }
                <DynamicGrid {...this.props} store={this.store} flex={1}>
                    {children}
                </DynamicGrid>
            </Container>
        );
    }

    private initColumnFilters() {
        if (!this.columnFilters) {
            const { columns, filterDataIndexes } = this.props;

            // this.columnFilters = this.props.columns.map(column => ({
            //     filteredColumn: column,
            //     flex: 1,
            //     store: this.filterData.get(column.dataIndex).store
            // } as ColumnFilter));

            this.columnFilters = [];
            
            columns.forEach(column => {
                if (filterDataIndexes.find(dataIndex => column.dataIndex == dataIndex)) {
                    this.columnFilters.push({
                        filteredColumn: column,
                        flex: 1,
                        store: this.filterData.get(column.dataIndex).store
                    } as ColumnFilter)
                }
            })
        }
    }

    private initStores() {
        const { columns, apiUrls, onUpdateRecord, onLoadData } = this.props;

        if (!this.store) {
            const listeners: any = {
                load: (store: Store, records: Model[]) => {
                    this.fillFilterStores(records);

                    onLoadData && onLoadData(store, records);
                }
            };

            if (onUpdateRecord) {
                listeners.update = onUpdateRecord;
            }

            const fields = columns.map(column => ({ 
                name: column.dataIndex, 
                mapping: column.mappingFn 
                    ? data => this.replaceNullValue(column.mappingFn(data)) 
                    : data => this.replaceNullValue(data[column.dataIndex])
            }));

            this.store = Ext.create('Ext.data.Store', {
                listeners,
                fields,
                autoLoad: true,
                //remoteFilter: true,
                //remoteSort: true,
                //pageSize : 300,
                pageSize: 0,
                proxy: {
                    type: 'ajax',
                    api: apiUrls,
                    reader: { 
                        type: 'json',
                        //rootProperty: 'items',
                        //totalProperty: 'total'
                    },
                    writer: {
                        type: 'json',
                        writeAllFields: true,
                        allowSingle: false
                    }
                }
            });
        }
    }

    private initFilterData() {
        if (!this.filterData) {
            const { columns } = this.props;

            this.filterData = new Map<string, FilterDataItem>();

            const defaultRender = (value, record: Model) => value;

            columns.forEach(column => {
                const store = Ext.create('Ext.data.Store', {
                    fields: [ CHECKED_DATA_INDEX, VALUE_DATA_INDEX ],
                    listeners: {
                        update: this.onUpdateFilterStore
                    }
                });

                const renderFn = column.rendererFn ? column.rendererFn : defaultRender;

                this.filterData.set(column.dataIndex, { store, renderFn })
            });
        }
    }

    private filtrateStore() {
        const { columns } = this.props;

        const records: Model[] = [];

        this.store.filterBy(record => {
            let result = true;

            columns.forEach(column => {
                const value = record.get(column.dataIndex);
                const { store: filterStore, renderFn } = this.filterData.get(column.dataIndex);

                filterStore.each(
                    ({ data: filterItem }) => 
                        result = renderFn(value, record) != filterItem.value || filterItem.checked
                );

                return result;
            })

            if (result) {
                records.push(record);
            }

            return result;
        });

        return records;
    }

    onUpdateFilterStore: StoreUpdateEventFn<FilterItem> = (store, record, operation, modifiedFieldNames) => {
        if (operation == StoreOperation.Edit && modifiedFieldNames[0] == CHECKED_DATA_INDEX) {
            const records = this.filtrateStore();

            this.fillFilterStores(records);
        }
    }

    private fillFilterStores(records: Model[]) {
        // type DataInfo = {
        //     set: Set<any>, 
        //     render(value, record: Model): any
        // };

        // const dataInfoMap = new Map<string, DataInfo>();
        // const defaultRender = (value, record: Model) => value;

        // columns.forEach(column =>  
        //     dataInfoMap.set(
        //         column.dataIndex, 
        //         {
        //             set: new Set<any>(),
        //             render: column.rendererFn ? column.rendererFn : defaultRender
        //         }
        //     )
        // );

        const { columns } = this.props;
        const dataSets = new Map<string, Set<any>>();

        columns.forEach(
            column => dataSets.set(column.dataIndex, new Set<any>())
        );

        records.forEach(record => {
            columns.forEach(({ dataIndex }) => {
                //const dataInfo = dataInfoMap.get(dataIndex);
                const dataSet = dataSets.get(dataIndex)
                const value = record.get(dataIndex);
                //const renderedValue = dataInfo.render(value, record);
                const renderedValue = this.filterData.get(dataIndex).renderFn(value, record);
                
                //dataInfo.set.add(renderedValue);
                dataSet.add(renderedValue);
            });
        });
     
        this.filterData.forEach((filterDataItem, dataIndex) => {
            const filterItems: FilterItem[] = [];

            dataSets.forEach(
                value => filterItems.push({
                    checked: true,
                    value
                })
            )

            this.filterData.get(dataIndex).store.loadData(filterItems);
        });
    }

    private replaceNullValue(value) {
        return value == null ? ' ' : value;
    }
}