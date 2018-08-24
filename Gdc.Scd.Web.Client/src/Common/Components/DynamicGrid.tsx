import * as React from "react";
import { Grid, Column } from "@extjs/ext-react";
import { ColumnInfo } from "../States/ColumnInfo";

export interface DynamicGridProps {
    store
    columns: ColumnInfo[]
    id?: string
    height?: number
}

export class DynamicGrid extends React.Component<DynamicGridProps> {
    render() {
        const { store, columns, id, height, children } = this.props;

        return (
            <Grid store={store} columnLines={true} height={height}>
                {
                    columns.map(column => (
                        <Column 
                            key={`${id}_${column.dataIndex}`} 
                            text={column.title} 
                            dataIndex={column.dataIndex} 
                            flex={1}
                        />
                    ))
                }

                {children}
            </Grid>
        );
    }
}