import * as React from "react";
import { Container, Label, List } from "@extjs/ext-react";
import { ExtDataviewHelper } from "../../Common/Helpers/ExtDataviewHelper";

export interface MultiSelectProps {

    width?: string;

    maxWidth?: string;

    height?: string;

    maxHeight?: string;

    title: string;

    itemTpl: string;

    store: any;

    selectable?: string;
}

export class MultiSelect extends React.Component<MultiSelectProps> {

    private lst: List;

    public render() {

        let { width, maxWidth, height, maxHeight, title, itemTpl, store, selectable } = this.props;

        title = '<h4>' + title + '</h4>';

        width = width || '100%';
        maxWidth = maxWidth || '200px';

        height = height || '100%';

        selectable = selectable || 'multi';

        return (
            <Container width={width} maxWidth={maxWidth}>
                <Label html={title} padding="7px" />
                <List ref="lst" itemTpl={itemTpl} store={store} height={height} maxHeight={maxHeight} selectable={selectable} scrollable="true" />
            </Container>
        );
    }

    public componentDidMount() {
        this.lst = this.refs['lst'] as List;
    }

    public getSelected<T>(): T[] {
        return ExtDataviewHelper.getListSelected(this.lst);
    }

    public getSelectedKeys<T>(field: string): T[] {
        return ExtDataviewHelper.getListSelected(this.lst, field);
    }
}