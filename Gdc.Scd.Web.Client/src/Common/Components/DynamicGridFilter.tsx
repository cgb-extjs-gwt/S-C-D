import * as React from "react";
import { Panel, Grid, CheckColumn, Column, Toolbar, Button } from "@extjs/ext-react";
import { ColumnInfo } from "../States/ColumnInfo";
import { Store, StoreUpdateEventFn, StoreOperation } from "../States/ExtStates";

export const CHECKED_DATA_INDEX = 'checked';
export const VALUE_DATA_INDEX = 'value'

export interface FilterItem {
    checked: boolean
    value
}

export interface ColumnFilter {
    filteredColumn: ColumnInfo
    flex: number
    store: Store<FilterItem>
}

export interface DynamicGridFilterActions {
    onReset?()
    onApply?(stores: Store<FilterItem>[])
}

export interface DynamicGridFilterProps extends DynamicGridFilterActions {
    columnFilters: ColumnFilter[]
}

export interface DynamicGridFilterActions {

}

export class DynamicGridFilter extends React.Component<DynamicGridFilterProps, DynamicGridFilterActions> {
    private toolbar

    public componentWillReceiveProps(nextProps: DynamicGridFilterProps) {
        if (this.props.columnFilters != nextProps.columnFilters && nextProps.columnFilters) {

        }
    }

    public render() {
        const { columnFilters } = this.props;

        return (
            <Panel 
                title="Filter"
                layout="hbox"
                collapsible={{
                    direction: 'top',
                    dynamic: true
                }}
            >
                {columnFilters.map(this.buildColumnFilter)}

                <Toolbar 
                    docked="bottom"
                    ref={toolbar => this.toolbar = toolbar}
                >
                    <Button text="Reset" flex={1} handler={this.onReset} />
                    {/* <Button text="Apply" flex={1} handler={this.onApply} /> */}
                </Toolbar>
            </Panel>
        );
    }

    private buildColumnFilter(filter: ColumnFilter) {
        return (
            <Grid 
                key={filter.filteredColumn.dataIndex}
                //title={filter.filteredColumn.title} 
                store={filter.store} 
                flex={filter.flex} 
                shadow
                columnLines={true}
                width="200"
                height="150"
            >
                <CheckColumn width="70" dataIndex={CHECKED_DATA_INDEX}/>
                <Column text="" dataIndex={VALUE_DATA_INDEX} flex={1}/>
            </Grid>
        );
    }

    // private handleFilterStores(filterColumns: ColumnFilter[]) {
    //     filterColumns.forEach(filterColumn => {
    //         filterColumn.store.on('update', this.onUpdateFilterStore, this);
    //     });
    // }

    // private onUpdateFilterStore: StoreUpdateEventFn<FilterItem> = (store, record, operation, modifiedFieldNames) => {
    //     if (operation == StoreOperation.Edit && modifiedFieldNames[0] == CHECKED_DATA_INDEX) {
    //         //store.getModifiedRecords()
    //     }
    // }

    private setToolbarDisable(isDisabled: boolean) {
        this.toolbar.setDisabled(isDisabled);
    }

    private onReset = () => {
        this.props.columnFilters.forEach(
            columnFilter => columnFilter.store.rejectChanges()
        );

        this.setToolbarDisable(true);
    }

    // private onApply = () => {
    //     this.props.columnFilters.forEach(
    //         columnFilter => columnFilter.store.commitChanges()
    //     );

    //     this.setToolbarDisable(true);
    // }
}