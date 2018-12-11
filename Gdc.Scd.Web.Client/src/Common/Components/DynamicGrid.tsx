import * as React from "react";
import { Grid, Column, CheckColumn, NumberField, TextField, SelectField, Toolbar, Button } from "@extjs/ext-react";
import { ColumnInfo, ColumnType, FilterItem, ColumnFilter } from "../States/ColumnInfo";
import { SaveToolbar } from "./SaveToolbar";
import { Model, StoreOperation, Store } from "../States/ExtStates";
import { ReactNode } from "react-redux";
import { DynamicGridProps } from "./Props/DynamicGridProps";

export interface StoreDynamicGridProps extends DynamicGridProps {
    store: Store
    useStoreSync?: boolean
}

export class DynamicGrid<TProps extends StoreDynamicGridProps = StoreDynamicGridProps, TState={}> extends React.Component<TProps, TState> {
    private saveToolbar: SaveToolbar
    private columnsMap = new Map<string, ColumnInfo>()
    private store: Store = this.getStore()
    private columns: ColumnInfo[] = this.getColumns()

    constructor(props) {
        super(props);

        this.onSelectionChange = this.onSelectionChange.bind(this);
    }

    public componentDidMount() {
        const { init } = this.props;

        init && init();
    }

    public componentWillUnmount() {
        this.removeStoreListeners();
    }

    public componentWillReceiveProps(nextProps: TProps) {
        const store = this.getStore();

        if (this.store != store) {
            this.removeStoreListeners();
            this.addStoreListeners(store);

            this.store = store;
        } 

        const columns = this.getColumns();

        if (this.columns != columns) {
            this.columnsMap.clear();

            if (columns) {
                columns.forEach(column => this.columnsMap.set(column.dataIndex, column));

                this.columns = columns;
            }
            else {
                this.columns = [];
            }
        }
    }

    public render() {
        const { id, minHeight, minWidth, children, onSelectionChange, flex } = this.props;
        const isEditable = this.columns && !!this.columns.find(column => column.isEditable);
        const hasChanges = this.hasChanges();

        const gridProps = isEditable 
            ? {
                plugins: ['cellediting', 'selectionreplicator'],
                selectable: {
                    rows: true,
                    cells: true,
                    columns: false,
                    drag: true,
                    extensible: 'y',
                }
            }
            : {};

        return (
            <Grid 
                {...gridProps}
                store={this.store} 
                columnLines={true} 
                minHeight={minHeight}
                minWidth={minWidth}
                onSelectionchange={this.onSelectionChange}
                flex={flex}
            >
                {
                    this.columns &&
                    this.columns.filter(column => !column.isInvisible)
                                .map(column => this.buildColumn(id, column))
                }
                {children}
                {
                    isEditable && this.getSaveToolbar(hasChanges, this.toolbarRef)
                }
            </Grid>
        );
    }

    protected onSelectionChange(grid, records: Model[], selecting: boolean, selectionInfo){
        const { onSelectionChange } = this.props;

        onSelectionChange && onSelectionChange(grid, records, selecting, selectionInfo);
    }

    protected getStore() {
        return this.props.store;
    }

    protected getColumns() {
        return this.props.columns;
    }

    protected getSaveToolbar(hasChanges: boolean, ref: (toolbar: SaveToolbar) => void) {
        return (
            <SaveToolbar 
                ref={ref}
                isEnableClear={hasChanges} 
                isEnableSave={hasChanges}
                onCancel={this.onCancel}
                onSave={this.onSave}
            />
        );
    }

    protected onCancel = () => {
        const { onCancel } = this.props;

        this.store.rejectChanges();

        onCancel && onCancel();
    }

    protected saveWithCallback = (callback: () => void) => {
        const { useStoreSync } = this.props;
        const save = () => {
            callback && callback();

            this.saveToolbar.enable(this.hasChanges());
        };

        if (useStoreSync) {
            this.store.sync({
                callback: save
            });
        }
        else {
            this.store.commitChanges();
            save();
        }
    }

    protected onSave = () => {
        this.saveWithCallback(this.props.onSave);
    }

    private toolbarRef = (toolbar: SaveToolbar) => {
        this.saveToolbar = toolbar;
    }

    private hasChanges() {
        return this.store ? this.store.getModifiedRecords().length > 0 : false;
    }

    private buildColumn(gridId: string, column: ColumnInfo) {
        const columnOption: any = {
            key: `${gridId}_${column.dataIndex}`,          
            dataIndex: column.dataIndex,
            flex: 1,
            editable: column.isEditable,
            text: column.title,
            id: column.dataIndex.replace('.', '')
        };

        if (column.rendererFn) {
            columnOption.renderer = column.rendererFn;
        }

        if (column.filter) {
            columnOption.menu = this.buildFilterMenu(column.filter);
        }

        switch(column.type) {
            case ColumnType.CheckBox:
                return (<CheckColumn {...columnOption} disabled={!column.isEditable} headerCheckbox={column.isEditable} />);

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

    private buildFilterMenu(filter: ColumnFilter) {
        let searchFn = null;
        const searchField = {
            xtype: 'textfield',
            label: 'Search',
            margin: '-15 10 10 10',
            listeners: {
                change: (field, newValue: string, oldValue: string) => {
                    const filters = filter.store.getFilters();

                    //if (searchFn) {
                    //    filters.each(filter => filters.remove(filter));
                    //}

                    if (searchFn) {
                        filters.remove(searchFn);
                    }

                    searchFn = (item: Model<FilterItem>) => { 
                        let valueStr: string = item.data.value.toString();
                        valueStr = valueStr.toLowerCase()

                        return valueStr.includes(newValue.toLowerCase())
                    }

                    filters.add(searchFn);
                }
            }
        };

        const grid = {
            xtype: 'grid',
            columnLines: true,
            rowLines: true,
            store: filter.store,
            height: 450,
            margin: '-5 10 10 10',
            style: 'border: 1px solid rgb(226, 226, 226);',
            columns: [
                { xtype: 'checkcolumn', dataIndex: filter.checkedDataIndex, width: 70, sortable: false },
                { text: 'Value', dataIndex: filter.valueDataIndex, width: 200  }
            ]
        };

        return [ '-', searchField, grid];
    }

    private onUpdateStore = (store: Store, record: Model, operation: StoreOperation, modifiedFieldNames: string[], details) => {
        switch (operation) {
            case StoreOperation.Edit: 
                if (this.saveToolbar) {
                    this.saveToolbar.enable(true);
                }

                modifiedFieldNames.forEach(dataIndex => {
                    const column = this.columnsMap.get(dataIndex);

                    column && column.editMappingFn && column.editMappingFn(record, dataIndex);
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
        this.store && this.store.un('update', this.onUpdateStore);
    }
}