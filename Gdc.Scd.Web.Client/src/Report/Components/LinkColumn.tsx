import * as React from "react";
import { Column, ColumnProps, GridCell } from "@extjs/ext-react";
import { IRenderer, emptyRenderer } from "./GridRenderer";

export interface LinkColumnProps extends ColumnProps {
    renderer?: IRenderer;
    dataAction?: string;
    linkTooltip?: string;
    rowID?: string;
}

export class LinkColumn extends React.Component<LinkColumnProps, any> {

    constructor(props: LinkColumnProps) {
        super(props);
        this.renderer = this.renderer.bind(this);
    }

    public render() {
        return <Column {...this.props} renderer={this.renderer} align="center">
            <GridCell encodeHtml={false} />
        </Column>;
    }

    private renderer(value: any, row: any): string {

        let a = [];
        a.push('<a class="lnk underline"');

        let rowid = row.get(this.props.rowID || 'Id');
        if (rowid) {
            a.push('data-rowid="' + rowid + '"');
        }

        let action = this.props.dataAction;
        if (action) {
            a.push('data-action="' + action + '"');
        }

        a.push('>');

        let rnd = this.props.renderer;
        a.push(rnd ? rnd(value, row) : value);

        a.push('</a>');

        return a.join(' ');
    }
}