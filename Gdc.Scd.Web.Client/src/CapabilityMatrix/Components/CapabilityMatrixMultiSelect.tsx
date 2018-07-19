import * as React from "react";
import { Container, Label, List } from "@extjs/ext-react";

export interface CapabilityMatrixMultiSelectProps {

    width?: string;

    maxWidth?: string;

    title: string;

    itemTpl: string;

    store: any;
}

export class CapabilityMatrixMultiSelect extends React.Component<CapabilityMatrixMultiSelectProps> {

    public render() {

        let { width, maxWidth, title, itemTpl, store } = this.props;

        title = '<h4>' + title + '</h4>';

        width = width || '100%';
        maxWidth = maxWidth || '200px';

        return (
            <Container width={width} maxWidth={maxWidth}>
                <Label html={title} padding="7px" />
                <List itemTpl={itemTpl} store={store} selectable="multi" scrollable="true" />
            </Container>
        );
    }

}