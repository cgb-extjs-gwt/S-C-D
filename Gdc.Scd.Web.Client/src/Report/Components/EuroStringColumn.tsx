import * as React from "react";
import { Column, ColumnProps } from "@extjs/ext-react";

export class EuroStringColumn extends React.Component<ColumnProps, any> {

    constructor(props: ColumnProps) {
        super(props);
        this.renderer = this.renderer.bind(this);
    }

    public render() {
        return <Column {...this.props} renderer={this.renderer} />
    }

    private renderer(value: any, row: any) {
        return typeof value === 'number' ? Ext.util.Format.currency(value, '€') : '';
    }
}