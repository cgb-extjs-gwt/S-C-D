import * as React from "react";
import { Grid, SelectField, Column, Container, CheckBoxField, CheckColumn, NumberField, ComboBoxField } from "@extjs/ext-react";
import { EditItem } from "../States/CostBlockStates";
import { NamedId } from "../../Common/States/CommonStates";
import { large, small } from "../../responsiveFormulas";
import { FieldType } from "../../Common/States/CostMetaStates";
import { ColumnInfo } from "../../Common/States/ColumnInfo";
import { buildCostElementColumn } from "../../Common/Helpers/ColumnInfoHelper";
import { SaveToolbar } from "../../Common/Components/SaveToolbar";
import { SaveApprovalToollbar } from "../../Approval/Components/SaveApprovalToollbar";
import { StoreUpdateEventFn } from "../../Common/States/ExtStates";
import { DynamicGrid } from "../../Common/Components/DynamicGrid";
import { AjaxDynamicGrid, ApiUrls } from "../../Common/Components/AjaxDynamicGrid";
import { objectPropsEqual } from "../../Common/Helpers/CommonHelpers";

export interface ValueColumnProps {
    title: string
    type: FieldType,
    selectedItems: NamedId<number>[]
}

export interface EditGridActions {
    onItemEdited?(item: EditItem)
    onSelected?(items: EditItem[])
    onApprove?()
    onCancel?()
    onSave?()
}

export interface EditGridProps extends EditGridActions {
    nameColumnTitle: string
    valueColumn: ValueColumnProps
    //items: EditItem[]
    url: string
}

export class EditGrid extends React.Component<EditGridProps> {
    // private url: string;
    // private grid: AjaxDynamicGrid;
    // private gridMap = new Map<string, AjaxDynamicGrid>();

    // public componentWillReceiveProps({ url }: EditGridProps) {
    //     if (this.url != url && this.grid) {
    //         this.url = url;

    //         const store = this.grid.getStore();
    //         if (store) {
    //             const proxy = store.getProxy();
            
    //             proxy.setApi({ read: url });
    //             store.reload();
    //         }
    //     }
    // }

    // private url: string
    // private apiUrls: ApiUrls

    // public componentWillReceiveProps({ url }: EditGridProps) {
    //     if (this.url != url) {
    //         this.apiUrls = { 
    //             read: url 
    //         };
    //     }
    // 

    public shouldComponentUpdate(nextProps: EditGridProps) {
        return (
            !objectPropsEqual(this.props, nextProps, 'url', 'nameColumnTitle') ||
            !objectPropsEqual(this.props.valueColumn, nextProps.valueColumn)
        );
    }

    public render() {
        const { url, valueColumn, nameColumnTitle } = this.props;
        const columns = this.buildColumnInfos(nameColumnTitle, valueColumn);

        return (
            url &&
            <AjaxDynamicGrid 
                flex={1}
                columns={columns} 
                apiUrls={{ read: url }}
                //ref={grid => this.grid = grid}
                getSaveToolbar={this.getSaveToolbar}
                onSelectionChange={this.onSelected}
                onUpdateRecord={this.onUpdateRecord}
                // onCancel={onCancel}
                // onSave={onSave}
                onCancel={this.onCancel}
                onSave={this.onSave}
            />
        );
    }

    // public componentWillReceiveProps({ url }: EditGridProps) {
    //     if (url && this.url != url) {
    //         this.url = url;

    //         const grid = this.gridMap.get(url);

    //         if (grid) {
    //             const store = grid.getStore();

    //             if (store) {
    //                 const proxy = store.getProxy();
                
    //                 proxy.setApi({ read: url });
    //                 store.reload();
    //             }
    //         }
    //     }
    // }

    // public render() {
    //     return  this.props.url && this.getGrid();
    // }

    private buildColumnInfos(nameColumnTitle: string, valueColumn: ValueColumnProps) {
        return [
            { 
                title: nameColumnTitle, 
                dataIndex: "name", 
                extensible: false 
            },
            buildCostElementColumn<EditItem>({
                title: valueColumn.title,
                dataIndex: "value",
                type: valueColumn.type,
                references: valueColumn.selectedItems,
                getCountFn: ({ data }) => data.valueCount
            })
        ] as ColumnInfo[]
    }

    private getSaveToolbar = (
        hasChanges: boolean, 
        ref: (toolbar: SaveToolbar) => void, 
        { cancel, save, saveWithCallback }: DynamicGrid
    ) => {
        return (
            <SaveApprovalToollbar 
                ref={ref}
                isEnableClear={hasChanges} 
                isEnableSave={hasChanges}
                onCancel={cancel}
                onSave={save}
                onApproval={() => saveWithCallback(this.props.onApprove)}
            />
        );
    }

    private onSelected = (grid, records: { data: EditItem }[]) => {
        const { onSelected } = this.props;

        if (onSelected) {
            const editItems = records ? records.map(record => record.data) : [];

            onSelected(editItems);
        }
    }

    private onUpdateRecord: StoreUpdateEventFn<EditItem> = (store, record, operation, modifiedFieldNames, details) => {
        const { onItemEdited } = this.props;

        if (modifiedFieldNames) {
            if (modifiedFieldNames[0] === 'name') {
                record.reject();
            } else {
                const item = record.data as EditItem;
    
                record.set('valueCount', 1);
    
                onItemEdited(record.data);
            }
        }
    }

    // private createGrid() {
    //     const { url } = this.props;
    //     const columns = this.buildColumnInfos()

    //     return (
    //         <AjaxDynamicGrid 
    //             flex={1}
    //             key={`gird_${this.gridMap.size}`}
    //             ref={grid => {
    //                 this.gridMap.set(url, grid)
    //             }}
    //             columns={columns} 
    //             apiUrls={{ read: url }}
    //             getSaveToolbar={this.getSaveToolbar}
    //             onSelectionChange={this.onSelected}
    //             onUpdateRecord={this.onUpdateRecord}
    //             onCancel={this.onCancel}
    //             onSave={this.onSave}
    //         />
    //     );
    // }

    private onSave = () => {
        const { onSave } = this.props;

        onSave && onSave();
    }

    private onCancel = () => {
        const { onCancel } = this.props;

        onCancel && onCancel();
    }

    // private getGrid() {
    //     const grid = this.gridMap.get(this.props.url);

    //     return grid ? grid.render() : this.createGrid();
    // }
}

// export class EditGridOld extends React.Component<EditGridProps> {
//     private itemsMap = new Map<string, EditItem>();

//     constructor(props: EditGridProps) {
//         super(props);

//         if (this.props.items) {
//             this.updateItemsMap(this.props.items) ;
//         }
//     }

//     public shouldComponentUpdate(nextProps: EditGridProps) {
//         let result = false;

//         const { items } = nextProps;

//         if (!items || items.length != this.itemsMap.size) {
//             result = true;
//         } else {
//             result = !items.every(item => { 
//                 const mapItem = this.itemsMap.get(item.id);

//                 return (
//                     mapItem &&
//                     mapItem.name == item.name && 
//                     mapItem.value == item.value && 
//                     mapItem.valueCount == item.valueCount
//                 );
//             });
//         }

//         if (result && nextProps.items) {
//             this.updateItemsMap(nextProps.items);
//         }

//         return result;
//     }

//     public render() {
//         const { nameColumnTitle, valueColumn } = this.props;
//         const store = this.buildStore();

//         return (
//             <Grid 
//                 flex={1}
//                 store={store} 
//                 shadow 
//                 columnLines={true}
//                 plugins={['cellediting', 'selectionreplicator']}
//                 selectable={{
//                     rows: true,
//                     cells: true,
//                     columns: true,
//                     drag: true,
//                     extensible: 'y',
//                 }}
//                 onSelectionchange={this.onSelected}
//             >
//                 <Column text={nameColumnTitle} dataIndex="name" flex={1} extensible={false} />
//                 {this.getValueColumn(valueColumn)}
//             </Grid>
//         );
//     }

//     private buildStore() {
//         const { items, onItemEdited } = this.props;
//         const me = this;

//         return Ext.create('Ext.data.Store', {
//             data: Array.from(this.itemsMap.values()),
//             fields: ['id', 'name', 'valueCount',
//                 {
//                     name: 'value',
//                     mapping: data => data.value == null ? ' ' : data.value
//                 }],
//             listeners: onItemEdited && {
//                 update: (store, record, operation, modifiedFieldNames, details) => {
//                     if (modifiedFieldNames[0] === 'name') {
//                         record.reject();
//                     } else {
//                         const item = record.data as EditItem;

//                         me.itemsMap.set(item.id, item);
                        
//                         record.set('valueCount', 1);

//                         onItemEdited(record.data);
//                     }
//                 }
//             }
//         }); 
//     }

//     private getValueColumn(columProps: ValueColumnProps) {
//         let column;

//         const columnOptions = {
//             text: columProps.title,
//             dataIndex: "value",
//             flex: 1,
//             editable: true,
//             renderer: (value, { data }: { data: EditItem }) => {
//                 return data.valueCount == 1 ? value : this.getValueCountMessage(data);
//             }
//         };
    
//         switch (columProps.type) {
//             case FieldType.Reference:
//                 columnOptions.renderer = (value, { data }) => {
//                     let result: string;

//                     if (data.valueCount == 1) {
//                         const selectedItem = columProps.selectedItems.find(item => item.id == data.value);

//                         result = selectedItem.name;
//                     } else {
//                         result = this.getValueCountMessage(data);
//                     }

//                     return result;
//                 }

//                 column = (
//                     <Column {...columnOptions}>
//                         <SelectField 
//                             options={
//                                 columProps.selectedItems.map(item => ({text: item.name, value: item.id}))
//                         }/>
//                     </Column>
//                 );
//                 break;

//             case FieldType.Double:
//                 column = (
//                     <Column {...columnOptions}>
//                         <NumberField required validators={{type:"number", message:"Invalid value"}}/>
//                     </Column>
//                 );
//                 break;

//             case FieldType.Flag:
//                 columnOptions.renderer = (value, { data }) => {
//                     let result: string;

//                     if (data.valueCount == 1) {
//                         result = value ? 'true' : 'false';
//                     } else {
//                         result = this.getValueCountMessage(data);
//                     }

//                     return result;
//                 };

//                 column = (
//                     <Column {...columnOptions}>
//                         <SelectField
//                             options={[
//                                 { text: 'true', value: 1 },
//                                 { text: 'false', value: 0 }
//                             ]}
//                         />
//                     </Column>
//                 );
                
//                 break;

//             case FieldType.Percent:
//                 column = (
//                     <Column
//                         {...columnOptions}
//                         renderer={value => 
//                             Ext.util.Format.number(value, '0.##%')
//                         }
//                     >
//                         <NumberField required validators={{ type: "number", message: "Invalid value" }} />
//                     </Column>
//                 );
//                 break;
//         }
    
//         return column;
//     }

//     private getValueCountMessage(editItem: EditItem) {
//         return `(${editItem.valueCount} values)`
//     }

//     private onSelected = (grid, records: { data: EditItem }[]) => {
//         const { onSelected } = this.props;

//         if (onSelected) {
//             const editItems = records ? records.map(record => record.data) : [];

//             onSelected(editItems);
//         }
//     }

//     private updateItemsMap(items: EditItem[]) {
//         this.itemsMap.clear();
            
//         for (const item of items) {
//             this.itemsMap.set(item.id, { ...item });
//         }
//     }
// } 