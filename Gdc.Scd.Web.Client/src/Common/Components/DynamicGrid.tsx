import * as React from "react";
import { Grid, Column, CheckColumn, NumberField, TextField, SelectField, Toolbar, Button } from "@extjs/ext-react";
import { ColumnInfo, ColumnType } from "../States/ColumnInfo";
import { SaveToolbar } from "./SaveToolbar";
import { DynamicGridProps } from "./Props/DynamicGridProps";

export interface StoreDynamicGridProps extends DynamicGridProps {
    store
}

export class DynamicGrid extends React.Component<StoreDynamicGridProps> {
    public componentDidMount() {
        const { init } = this.props;

        init && init();
    }

    public render() {
        const { store, columns, id, minHeight, minWidth, children, onSelectionChange } = this.props;
        const isEditable = !!columns.find(column => column.isEditable);
        const hasChanges = store ? this.storeHasChanges(store) : false;

        let plugins;
        let selectable;

        if (isEditable) {
            plugins = ['cellediting', 'selectionreplicator'];
            selectable = {
                rows: true,
                cells: true,
                columns: true,
                drag: true,
                extensible: 'y',
            }
        } 

        plugins = isEditable ? ['cellediting', 'selectionreplicator'] : [];

        return (
            columns && columns.length > 0 &&
            <Grid 
                store={store} 
                columnLines={true} 
                minHeight={minHeight}
                minWidth={minWidth}
                onSelect={onSelectionChange}
                plugins={plugins}
                selectable={selectable}
            >
                {columns.map(column => this.buildColumn(id, column))}
                {children}
                {
                    isEditable &&
                    <SaveToolbar 
                        isEnableClear={hasChanges} 
                        isEnableSave={hasChanges}
                        onCancel={this.onCancel}
                        onSave={this.onSave}
                    />
                }
            </Grid>
        );
    }

    private onCancel = () => {
        const { store, onCancel } = this.props;

        store.rejectChanges();

        onCancel && onCancel();
    }

    private onSave = () => {
        const { store, onSave } = this.props;

        store.sync({
            callback: onSave
        });
    }

    private storeHasChanges(store) {
        const records: any[] = store.getRange();

        return !!records.find(record => record.dirty);
    }

    private buildColumn(gridId: string, column: ColumnInfo) {
        const columnOption: any = {
            key: `${gridId}_${column.dataIndex}`,
            text: column.title, 
            dataIndex: column.dataIndex,
            flex: 1,
            editable: column.isEditable
        };

        switch(column.type) {
            case ColumnType.CheckBox:
                return (<CheckColumn {...columnOption} disabled={!column.isEditable}/>);

            default:
                let editor = null;

                if (!column.isEditable) {
                    switch (column.type) {
                        case ColumnType.Numeric:
                            editor = (<NumberField required validators={{ type:"number", message:"Invalid value" }}/>);
                            break;
                        
                        case ColumnType.Text:
                            editor = (<TextField />);
                            break;
        
                        case ColumnType.Reference:
                            editor = this.getReferenceEditor(column);

                            columnOption.renderer = value => column.referenceItems.get(value).name;
                            break;
                    }
                }

                return (
                    <Column {...columnOption}>
                        {editor}
                    </Column>
                );
        }
    }

    private getReferenceEditor(column: ColumnInfo) {
        const options = 
            Array.from(column.referenceItems.values())
                 .map(item => ({ text: item.name, value: item.id }));

        return (
            <SelectField options={options} />
        )
    }
}