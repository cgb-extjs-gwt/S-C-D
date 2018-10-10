import * as React from "react";
import { Column, ColumnProps } from "@extjs/ext-react";

export class PercentColumn extends React.Component<ColumnProps, any> {

    constructor(props: ColumnProps) {
        super(props);
        this.renderer = this.renderer.bind(this);
    }

    public render() {
        return <Column {...this.props} renderer={this.renderer} />
    }

    private renderer(value: any, row: any): string {
        return typeof value === 'number' ? Ext.util.Format.number(value, '0.000') + '%' : '';
    }
}