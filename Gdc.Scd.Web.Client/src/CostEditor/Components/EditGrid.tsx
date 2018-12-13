import * as React from "react";
import { Grid, SelectField, Column, Container, CheckBoxField, CheckColumn, NumberField, ComboBoxField } from "@extjs/ext-react";
import { EditItem } from "../States/CostBlockStates";
import { NamedId } from "../../Common/States/CommonStates";
import { large, small } from "../../responsiveFormulas";
import { FieldType } from "../../Common/States/CostMetaStates";

export interface ValueColumnProps {
    title: string
    type: FieldType,
    selectedItems: NamedId<number>[]
}

export interface EditGridActions {
    onItemEdited?(item: EditItem)
    onSelected?(items: EditItem[])
}

export interface EditGridProps extends EditGridActions {
    nameColumnTitle: string
    valueColumn: ValueColumnProps
    items: EditItem[]
}

export class EditGrid extends React.Component<EditGridProps> {
    private itemsMap = new Map<string, EditItem>();
    private sorters;

    constructor(props: EditGridProps) {
        super(props);

        if (this.props.items) {
            this.updateItemsMap(this.props.items) ;
        }
    }

    public shouldComponentUpdate(nextProps: EditGridProps) {
        let result = false;

        const { items } = nextProps;

        if (!items || items.length != this.itemsMap.size) {
            result = true;
        } else {
            result = !items.every(item => { 
                const mapItem = this.itemsMap.get(item.id);

                return (
                    mapItem &&
                    mapItem.name == item.name && 
                    mapItem.value == item.value && 
                    mapItem.valueCount == item.valueCount
                );
            });
        }

        if (result && nextProps.items) {
            this.updateItemsMap(nextProps.items);
        }

        return result;
    }

    public render() {
        const { nameColumnTitle, valueColumn } = this.props;
        const store = this.buildStore();

        return (
            <Grid 
                flex={1}
                store={store} 
                shadow 
                columnLines={true}
                plugins={['cellediting', 'selectionreplicator']}
                selectable={{
                    rows: true,
                    cells: true,
                    columns: true,
                    drag: true,
                    extensible: 'y',
                }}
                onSelectionchange={this.onSelected}
            >
                <Column text={nameColumnTitle} dataIndex="name" flex={1} extensible={false} />
                {this.getValueColumn(valueColumn)}
            </Grid>
        );
    }

    private buildStore() {
        const { items, onItemEdited } = this.props;
        const me = this;

        return Ext.create('Ext.data.Store', {
            data: Array.from(this.itemsMap.values()),
            fields: ['id', 'name', 'valueCount',
                {
                    name: 'value',
                    mapping: data => data.value == null ? ' ' : data.value
                }],
            sorters: this.sorters,
            listeners: onItemEdited && {
                update: (store, record, operation, modifiedFieldNames, details) => {
                    if (modifiedFieldNames[0] === 'name') {
                        record.reject();
                    } else {
                        const item = record.data as EditItem;

                        me.itemsMap.set(item.id, item);
                        
                        record.set('valueCount', 1);

                        onItemEdited(record.data);
                    }
                },

                sort: (store) => {
                    this.sorters = store.getSorters().items;
                }
            }
        }); 
    }

    private getValueColumn(columProps: ValueColumnProps) {
        let column;

        const columnOptions = {
            text: columProps.title,
            dataIndex: "value",
            flex: 1,
            editable: true,
            renderer: (value, { data }: { data: EditItem }) => {
                return data.valueCount == 1 ? value : this.getValueCountMessage(data);
            }
        };
    
        switch (columProps.type) {
            case FieldType.Reference:
                columnOptions.renderer = (value, { data }) => {
                    let result: string;

                    if (data.valueCount == 1) {
                        const selectedItem = columProps.selectedItems.find(item => item.id == data.value);

                        result = selectedItem.name;
                    } else {
                        result = this.getValueCountMessage(data);
                    }

                    return result;
                }

                column = (
                    <Column {...columnOptions}>
                        <SelectField 
                            options={
                                columProps.selectedItems.map(item => ({text: item.name, value: item.id}))
                        }/>
                    </Column>
                );
                break;

            case FieldType.Double:
                column = (
                    <Column {...columnOptions}>
                        <NumberField required validators={{type:"number", message:"Invalid value"}}/>
                    </Column>
                );
                break;

            case FieldType.Flag:
                columnOptions.renderer = (value, { data }) => {
                    let result: string;

                    if (data.valueCount == 1) {
                        result = value ? 'true' : 'false';
                    } else {
                        result = this.getValueCountMessage(data);
                    }

                    return result;
                };

                column = (
                    <Column {...columnOptions}>
                        <SelectField
                            options={[
                                { text: 'true', value: 1 },
                                { text: 'false', value: 0 }
                            ]}
                        />
                    </Column>
                );
                
                break;
        }
    
        return column;
    }

    private getValueCountMessage(editItem: EditItem) {
        return `(${editItem.valueCount} values)`
    }

    private onSelected = (grid, records: { data: EditItem }[]) => {
        const { onSelected } = this.props;

        if (onSelected) {
            const editItems = records ? records.map(record => record.data) : [];

            onSelected(editItems);
        }
    }

    private updateItemsMap(items: EditItem[]) {
        this.itemsMap.clear();
            
        for (const item of items) {
            this.itemsMap.set(item.id, { ...item });
        }
    }
} 