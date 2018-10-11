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

    protected cb: any;

    protected lst: List;

    protected flag: boolean; //stub for correct checkbox work

    public constructor(props: MultiSelectProps) {
        super(props);
        this.init();
    }

    public render() {

        let { width, height, maxHeight, title, itemTpl, store, selectable } = this.props;

        title = '<h4>' + title + '</h4>';

        width = width || '100%';

        height = height || '100%';

        selectable = selectable || 'multi';

        return (
            <Container width={width}>
                <CheckBoxField
                    ref="cb"
                    boxLabel={title}
                    padding="7px"
                    bodyAlign="left"
                    onChange={this.onTopSelectionChange}
                />
                <div onClick={this.onListClick}>
                    <Container>
                        <List
                            ref="lst"
                            itemTpl={itemTpl}
                            store={store}
                            height={height}
                            maxHeight={maxHeight}
                            selectable={selectable}
                            scrollable="true"
                        />
                    </Container>
                </div>
            </Container>
        );
    }

    public componentDidMount() {
        this.cb = this.refs['cb'];
        this.lst = this.refs['lst'] as List;
    }

    public getSelected<T>(): T[] {
        return ExtDataviewHelper.getListSelected(this.lst);
    }

    public getSelectedKeys<T>(field: string): T[] {
        return ExtDataviewHelper.getListSelected(this.lst, field);
    }

    protected init() {
        this.flag = true;
        //
        this.onListClick = this.onListClick.bind(this);
        this.onTopSelectionChange = this.onTopSelectionChange.bind(this);
    }

    protected onListClick() {
        this.flag = false;

        let lst = this.lst as any;
        let checked = lst.getSelections().length > 0;

        this.cb.setChecked(checked);
    }

    protected onTopSelectionChange(cb: any, newVal: boolean, oldVal: boolean) {
        let lst = this.lst as any;
        if (newVal) {
            if (this.flag) {
                lst.selectAll();
            }
        }
        else {
            lst.deselectAll();
        }
        this.flag = true;
    }
}