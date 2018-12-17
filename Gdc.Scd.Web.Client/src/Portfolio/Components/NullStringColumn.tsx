import * as React from "react";
import { Column, ColumnProps } from "@extjs/ext-react";

export class NullStringColumn extends React.Component<ColumnProps, any> {

    private field: string;

    constructor(props: ColumnProps) {
        let dataIndex = props.dataIndex;
        super(props);
        this.field = dataIndex;
        this.renderer = this.renderer.bind(this);
    }

    public render() {
        return <Column {...this.props} dataIndex="none" renderer={this.renderer} />
    }

    private renderer(value: string, row: any) {
        return row.data[this.field] || '';
    }
}