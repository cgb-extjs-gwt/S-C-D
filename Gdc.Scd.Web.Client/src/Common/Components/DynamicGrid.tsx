import * as React from "react";
import { Grid, Column, CheckColumn, NumberField, TextField, SelectField, Toolbar, Button } from "@extjs/ext-react";
import { ColumnInfo, ColumnType } from "../States/ColumnInfo";
import { SaveToolbar } from "./SaveToolbar";
import { DynamicGridProps } from "./Props/DynamicGridProps";
import { Model, StoreOperation, Store } from "../States/ExtStates";

export interface StoreDynamicGridProps extends DynamicGridProps {
    store: Store
    useStoreSync?: boolean
}

export class DynamicGrid extends React.Component<StoreDynamicGridProps> {
    private saveToolbar: SaveToolbar
    private columnsMap = new Map<string, ColumnInfo>()

    constructor(props: StoreDynamicGridProps) {
        super(props);

        this.state = {
            hasChanges: false
        };
    }

    public componentDidMount() {
        const { init } = this.props;

        init && init();
    }

    public componentWillUnmount() {
        this.removeStoreListeners();
    }

    public componentWillReceiveProps(nextProps: StoreDynamicGridProps) {
        if (this.props.store != nextProps.store) {
            this.removeStoreListeners();
            this.addStoreListeners(nextProps.store);
        }

        if (this.props.columns != nextProps.columns) {
            this.removeStoreListeners();
            this.addStoreListeners(nextProps.store);
            this.columnsMap.clear();

            nextProps.columns.forEach(column => this.columnsMap.set(column.dataIndex, column));
        }
    }

    public render() {
        const { store, columns, id, minHeight, minWidth, children, onSelectionChange, flex } = this.props;
        const isEditable = !!columns.find(column => column.isEditable);
        const hasChanges = this.hasChanges();

        let plugins;
        let selectable;

        if (isEditable) {
            plugins = ['cellediting', 'selectionreplicator'];
            selectable = {
                rows: true,
                cells: true,
                columns: false,
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
                flex={flex}
            >
                {
                    columns.filter(column => !column.isInvisible)
                           .map(column => this.buildColumn(id, column))
                }
                {children}
                {
                    isEditable &&
                    <SaveToolbar 
                        ref={toolbar => this.saveToolbar = toolbar}
                        isEnableClear={hasChanges} 
                        isEnableSave={hasChanges}
                        onCancel={this.onCancel}
                        onSave={this.onSave}
                    />
                }
            </Grid>
        );
    }

    private hasChanges() {
        const { store } = this.props;

        return store ? store.getModifiedRecords().length > 0 : false;
    }

    private onCancel = () => {
        const { store, onCancel } = this.props;

        store.rejectChanges();

        onCancel && onCancel();
    }

    private onSave = () => {
        const { store, onSave, useStoreSync } = this.props;
        const saveFn = () => {
            onSave && onSave();

            this.saveToolbar.enable(this.hasChanges());
        };

        if (useStoreSync) {
            store.sync({
                callback: saveFn
            });
        }
        else {
            store.commitChanges();
            saveFn();
        }
    }

    private buildColumn(gridId: string, column: ColumnInfo) {
        const columnOption: any = {
            key: `${gridId}_${column.dataIndex}`,
            text: column.title, 
            dataIndex: column.dataIndex,
            flex: 1,
            editable: column.isEditable
        };

        if (column.rendererFn) {
            columnOption.renderer = column.rendererFn;
        }

        switch(column.type) {
            case ColumnType.CheckBox:
                return (<CheckColumn {...columnOption} disabled={!column.isEditable}/>);

            default:
                let editor = null;

                if (column.isEditable) {
                    switch (column.type) {
                        case ColumnType.Numeric:
                            editor = (<NumberField required validators={{ type:"number", message:"Invalid value" }}/>);
                            break;
                        
                        case ColumnType.Text:
                            editor = (<TextField />);
                            break;
        
                        case ColumnType.Reference:
                            editor = this.getReferenceEditor(column);
                            const getReferenceName = value => value == ' ' ? value : column.referenceItems.get(value).name;

                            columnOption.renderer = column.rendererFn 
                                ? (value, record) => column.rendererFn(getReferenceName(value), record) 
                                : (value, record) => getReferenceName(value)
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

    private onUpdateStore = (store: Store, record: Model, operation: StoreOperation, modifiedFieldNames: string[], details) => {
        switch (operation) {
            case StoreOperation.Edit: 
                if (this.saveToolbar) {
                    this.saveToolbar.enable(true);
                }

                modifiedFieldNames.forEach(dataIndex => {
                    const column = this.columnsMap.get(dataIndex);

                    column.editMappingFn && column.editMappingFn(record, dataIndex);
                });
                break;
            
            case StoreOperation.Reject: 
            case StoreOperation.Commit: 
                if (this.saveToolbar) {
                    this.saveToolbar.enable(false);
                }
                break;
        }
    }

    private addStoreListeners = (store: Store) => {
        if (store) {
            store.on('update', this.onUpdateStore, this);
        }
    }

    private removeStoreListeners = () => {
        const { store } = this.props;

        store && store.un('update', this.onUpdateStore);
    }
}