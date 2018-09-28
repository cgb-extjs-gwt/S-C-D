import * as React from "react";
import { DynamicGridProps, DynamicGridActions } from "./Props/DynamicGridProps";
import { DynamicGrid } from "./DynamicGrid";
import { ColumnInfo } from "../States/ColumnInfo";

export interface AjaxDynamicGridActions extends DynamicGridActions {
    updateRecord?(store, record, operation, modifiedFieldNames, details)
}

export interface AjaxDynamicGridProps extends DynamicGridProps, AjaxDynamicGridActions {
    dataLoadUrl: string
}

export class AjaxDynamicGrid extends React.Component<AjaxDynamicGridProps> {
    private store;

    // constructor(props: AjaxDynamicGridProps) {
    //     super(props)

    //     this.store = this.buildStore();
    // }

    public render() {
        const { children } = this.props;
        
        this.initStore();

        return (
            <DynamicGrid {...this.props} store={this.store}>
                {children}
            </DynamicGrid>
        );
    }

    private initStore() {
        const { columns, dataLoadUrl, updateRecord } = this.props;

        if (!this.store && columns && columns.length > 0) {
            const listeners: any = {};

            if (updateRecord){
                listeners.update = updateRecord;
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
                remoteFilter: true,
                remoteSort: true,
                pageSize : 300,
                proxy: {
                    type: 'ajax',
                    url: dataLoadUrl,
                    reader: { 
                        type: 'json',
                        rootProperty: 'items',
                        totalProperty: 'total'
                    }
                }
            });
        }
    }

    private replaceNullValue(value) {
        return value == null ? ' ' : value;
    }
}