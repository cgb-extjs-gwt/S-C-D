import * as React from "react";
import { Container, Label, List } from "@extjs/ext-react";
import { ExtDataviewHelper } from "../../Common/Helpers/ExtDataviewHelper";

export interface CapabilityMatrixMultiSelectProps {

    width?: string;

    maxWidth?: string;

    height?: string;

    maxHeight?: string;

    title: string;

    itemTpl: string;

    store: any;
}

export class CapabilityMatrixMultiSelect extends React.Component<CapabilityMatrixMultiSelectProps> {

    private lst: List;

    public render() {

        let { width, maxWidth, height, maxHeight, title, itemTpl, store } = this.props;

        title = '<h4>' + title + '</h4>';

        width = width || '100%';
        maxWidth = maxWidth || '200px';

        height = height || '100%';

        return (
            <Container width={width} maxWidth={maxWidth}>
                <Label html={title} padding="7px" />
                <List ref="lst" itemTpl={itemTpl} store={store} height={height} maxHeight={maxHeight} selectable="multi" scrollable="true" />
            </Container>
        );
    }

    public componentDidMount() {
        this.lst = this.refs['lst'] as List;
    }

    public getSelected<T>(): T[] {
        return ExtDataviewHelper.getListSelected(this);
    }

}