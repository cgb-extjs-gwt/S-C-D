import { CheckBoxField, Container, List } from "@extjs/ext-react";
import * as React from "react";
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

    public constructor(props: MultiSelectProps) {
        super(props);
        this.init();
    }

    public render() {

        let { width, maxWidth, height, maxHeight, title, itemTpl, store, selectable } = this.props;

        title = '<h4>' + title + '</h4>';

        width = width || '100%';
        maxWidth = maxWidth || '200px';

        height = height || '100%';

        selectable = selectable || 'multi';

        return (
            <Container width={width} maxWidth={maxWidth}>
                <CheckBoxField boxLabel={title} padding="7px" bodyAlign="left" onChange={this.onTopSelectionChange} />
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

    private init() {
        this.onTopSelectionChange = this.onTopSelectionChange.bind(this);
    }

    private onTopSelectionChange(cb: CheckBoxField, newVal: boolean, oldVal: boolean) {
        let view = this.lst as any;
        if (newVal) {
            view.selectAll();
        }
        else {
            view.deselectAll();
        }
    }
}