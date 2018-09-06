import * as React from "react";
import { Grid, Column, CheckColumn } from "@extjs/ext-react";
import { ColumnInfo, ColumnType } from "../States/ColumnInfo";

export interface DynamicGridActions {
    onSelectionChange?(grid, records: any[])
}

export interface DynamicGridProps extends DynamicGridActions {
    store
    columns: ColumnInfo[]
    id?: string
    minHeight?: number
    minWidth?: number
}

export class DynamicGrid extends React.Component<DynamicGridProps> {
    render() {
        const { store, columns, id, minHeight, minWidth, children, onSelectionChange } = this.props;

        return (
            <Grid 
                store={store} 
                columnLines={true} 
                minHeight={minHeight}
                minWidth={minWidth}
                onSelect={onSelectionChange}>
                {
                    columns.map(column => {
                        const columnOption = {
                            key: `${id}_${column.dataIndex}`,
                            text: column.title, 
                            dataIndex: column.dataIndex,
                            flex: 1,
                            editable: false
                        };

                        switch(column.type) {
                            case ColumnType.Checkbox:
                                return (<CheckColumn {...columnOption} disabled={true}/>)

                            default:
                                return (<Column {...columnOption}/>)
                        }
                    })
                }

                {children}
            </Grid>
        );
    }
}