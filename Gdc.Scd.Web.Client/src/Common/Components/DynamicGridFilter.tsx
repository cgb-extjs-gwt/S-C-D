import * as React from "react";
import { Panel, Grid, CheckColumn, Column } from "@extjs/ext-react";
import { ColumnInfo } from "../States/ColumnInfo";

export interface ColumnFilter {
    filteredColumn: ColumnInfo
    flex: number
    store
}

export interface DynamicGridFilterProps {
    columnFilters: ColumnFilter[]
}

export class DynamicGridFilter extends React.Component<DynamicGridFilterProps> {
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
            </Panel>
        );
    }

    private buildColumnFilter(filter: ColumnFilter) {
        return (
            <Grid 
                key={filter.filteredColumn.dataIndex}
                title={filter.filteredColumn.title} 
                store={filter.store} 
                flex={filter.flex} 
                shadow
                columnLines={true}
            >
                <CheckColumn width="70" dataIndex="checked"/>
                <Column text="" dataIndex="value" flex={1}/>
            </Grid>
        );
    }
}