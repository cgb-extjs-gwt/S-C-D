import { CheckBoxField, Container, List, SearchField } from "@extjs/ext-react";
import * as React from "react";
import { ExtDataviewHelper } from "../../Common/Helpers/ExtDataviewHelper";
import { PlaField } from "../../Dict/Components/PlaField";

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

export class MultiSelectWg extends React.Component<MultiSelectProps> {

    private cb: any;

    private lst: List;

    private flag: boolean; //stub for correct checkbox work

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
                    padding="2px"
                    bodyAlign="left"
                    onChange={this.onTopSelectionChange}
                />
                <PlaField placeholder="PLA" onChange={this.onPlaChange} />
                <SearchField placeholder="Search by wg/sog..." onChange={this.onSearch} />
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

    private init() {
        this.flag = true;
        //
        this.onPlaChange = this.onPlaChange.bind(this);
        this.onSearch = this.onSearch.bind(this);
        this.onListClick = this.onListClick.bind(this);
        this.onTopSelectionChange = this.onTopSelectionChange.bind(this);
    }

    private onListClick() {
        this.flag = false;

        let lst = this.lst as any;
        let checked = lst.getSelections().length > 0;

        this.cb.setChecked(checked);
    }

    private onTopSelectionChange(cb: any, newVal: boolean, oldVal: boolean) {
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

    private onPlaChange(view: any, newValue: string, oldValue: string) {
        let lst = this.lst as any
        lst.getStore().filter('plaId', newValue);
    }

    private onSearch(view: any, newValue: string, oldValue: string) {
        let lst = this.lst as any
        lst.getStore().filter('name', newValue);
    }
}