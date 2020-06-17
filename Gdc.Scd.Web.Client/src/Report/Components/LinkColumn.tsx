﻿import * as React from "react";
import { Column, ColumnProps, GridCell } from "@extjs/ext-react";
import { IRenderer, emptyRenderer } from "./GridRenderer";

export interface LinkColumnProps extends ColumnProps {
    renderer?: IRenderer;
    dataAction?: string;
    linkTooltip?: string;
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
        let rnd = this.props.renderer || emptyRenderer;
        let title = this.props.linkTooltip || '';
        return '<a class="lnk underline" data-rowid="'+ row.get('Id') +'" data-action="' + this.props.dataAction + '">' + rnd(value, row) + '</a>';
    }
}