import { LocalDynamicGrid, LocalDynamicGridProps } from "./LocalDynamicGrid";
import { ColumnInfo } from "../States/ColumnInfo";

export interface ApiUrls {
    read: string
    update?: string
    create?: string
    destroy?: string
}

export interface AjaxDynamicGridProps<T=any> extends LocalDynamicGridProps<T> {
    apiUrls: ApiUrls
}

export class AjaxDynamicGrid<T=any> extends LocalDynamicGrid<T, AjaxDynamicGridProps<T>> {
    protected buildDataStore(props: AjaxDynamicGridProps<T>, visibleColumns: ColumnInfo[]) {
        const { columns, apiUrls } = props;

        return apiUrls && apiUrls.read 
            ?  Ext.create('Ext.data.Store', {
                fields: this.buildDataStoreFields(visibleColumns),
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
                }
            })
            : null;
    }

    protected isUpdatingDataStore(prevProps: AjaxDynamicGridProps<T>, currentProps: AjaxDynamicGridProps<T>) {
        return (
            super.isUpdatingDataStore(prevProps, currentProps) || 
            prevProps.apiUrls != currentProps.apiUrls
        )
             
    }
}