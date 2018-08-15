import * as React from "react";
import { Grid, SelectField, Column, Container } from "@extjs/ext-react";
import { FieldType } from "../States/CostEditorStates";
import { EditItem } from "../States/CostBlockStates";
import { NamedId } from "../../Common/States/CommonStates";
import { large, small } from "../../responsiveFormulas";

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
        
    public shouldComponentUpdate(nextProps: EditGridProps) {
        let result = false;

        const { items } = nextProps;

        if (!items || items.length != this.itemsMap.size) {
            result = true;
        } else {
            result = !items.every(item => { 
                const mapItem = this.itemsMap.get(item.id);

                return (
                    mapItem.name == item.name && 
                    mapItem.value == item.value && 
                    mapItem.valueCount == item.valueCount
                );
            });
        }

        if (result && nextProps.items) {
            this.itemsMap.clear();
            
            for (const item of nextProps.items) {
                this.itemsMap.set(item.id, { ...item });
            }
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

        return Ext.create('Ext.data.Store', {
            data: Array.from(this.itemsMap.values()),
            listeners: onItemEdited && {
                update: (store, record, operation, modifiedFieldNames, details) => {
                    if (modifiedFieldNames[0] === 'name') {
                        record.reject();
                    } else {
                        onItemEdited(record.data);
                    }
                }
            }
        }); 
    }

    private getValueColumn(columProps: ValueColumnProps) {
        let columnOptions;
        let renderer: (value, data: { data: EditItem }) => string;
    
        switch (columProps.type) {
            case FieldType.Reference:
                var options = columProps.selectedItems.map(item => ({text: item.name, value: item.id}));
    
                columnOptions = (
                    <SelectField options={options}/>
                );

                renderer = (value, { data }) => {
                    let result: string;

                    if (data.valueCount == 1) {
                        const selectedItem = columProps.selectedItems.find(item => item.id == data.value);

                        result = selectedItem.name;
                    } else {
                        result = this.getValueCountMessage(data);
                    }

                    return result;
                }
                break;

            case FieldType.Double:
                renderer = (value, { data }) => data.valueCount == 1 ? value : this.getValueCountMessage(data);
                break;
        }
    
        return (
            <Column 
                text={columProps.title} 
                dataIndex="value" 
                flex={1} 
                editable={true}
                renderer={renderer}
            >
                {columnOptions}
            </Column>
        )
    }

    private getValueCountMessage(editItem: EditItem) {
        return `(${editItem.valueCount} values)`
    }

    private onSelected = (grid, records: { data: EditItem }[]) => {
        const { onSelected } = this.props;

        if (onSelected) {
            const editItems = records.map(record => record.data);

            onSelected(editItems);
        }
    }
} 