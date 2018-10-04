import * as React from "react";
import { DynamicGridProps, DynamicGridActions } from "./Props/DynamicGridProps";
import { DynamicGrid } from "./DynamicGrid";
import { ColumnInfo } from "../States/ColumnInfo";
import { Model, StoreOperation } from "../States/ExtStates";
import { Container } from "@extjs/ext-react";
import { DynamicGridFilter, ColumnFilter } from "./DynamicGridFilter";

export interface AjaxDynamicGridActions<T=any> extends DynamicGridActions {
    updateRecord?(store, record: Model<T>, operation: StoreOperation, modifiedFieldNames: string[], details)
    loadData?(store, records: Model<T>[])
}

export interface AjaxDynamicGridProps<T=any> extends DynamicGridProps, AjaxDynamicGridActions {
    apiUrls: {
        read: string
        update?: string
        create?: string
        destroy?: string
    }
}

export class AjaxDynamicGrid extends React.Component<AjaxDynamicGridProps> {
    private store;

    public render() {
        const { children } = this.props;
        
        this.initStore();

        return (
            <Container layout="vbox">
                <DynamicGridFilter columnFilters={this.buildColumnFilters()}/>

                <DynamicGrid {...this.props} store={this.store}>
                    {children}
                </DynamicGrid>
            </Container>
        );
    }

    private buildColumnFilters() {
        return this.props.columns.map(column => {
            return {
                filteredColumn: column,
                flex: 1,
                store: Ext.create('Ext.data.Store', {
                    fields,
                    
                });
            } as ColumnFilter
        });
    }

    private initStore() {
        const { columns, apiUrls, updateRecord, loadData } = this.props;

        if (!this.store && columns && columns.length > 0) {
            const listeners: any = {};

            if (updateRecord) {
                listeners.update = updateRecord;
            }

            if (loadData) {
                listeners.load = loadData;
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

    private replaceNullValue(value) {
        return value == null ? ' ' : value;
    }
}