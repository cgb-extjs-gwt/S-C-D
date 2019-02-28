import { CheckBoxField, Container, List, ListProps } from "@extjs/ext-react";
import * as React from "react";
import { ExtDataviewHelper } from "../../Common/Helpers/ExtDataviewHelper";
import { NamedId } from "../../Common/States/CommonStates";

const ID_PROP = 'id';

export interface MultiSelectProps extends ListProps {

    title?: string;

    store(): Promise<NamedId[]>;

    hideCheckbox?: boolean;
}

export class MultiSelect extends React.Component<MultiSelectProps, any> {

    protected nameField: string = 'name';

    protected nameFieldTpl: string = '{' + this.nameField + '}';

    protected cb: CheckBoxField & any;

    protected lst: List & any;

    protected flag: boolean; //stub for correct checkbox work

    public state = {
        items: []
    }

    public constructor(props: MultiSelectProps) {
        super(props);
        this.init();
    }

    public componentDidMount() {
        let store = this.lst.getStore() as any;
        let sorters = store.getSorters();
        sorters.remove(this.nameField);
        sorters.add(this.nameField);
        //
        this.props.store().then(x => store.setData(x));
        store.on('endupdate', () => {
            if (this.props.value) {
                this.lst.select(+this.props.value);
            }
        })
    }

    public render() {

        let { width, height, maxHeight, title, selectable, onSelect, hideCheckbox } = this.props;

        title = '<h4>' + title + '</h4>';

        width = width || '100%';

        height = height || '100%';

        selectable = selectable || 'multi';

        return (
            <Container width={width}>
                <CheckBoxField
                    ref={x => this.cb = x}
                    boxLabel={title}
                    padding="7px"
                    bodyAlign="left"
                    onChange={this.onTopSelectionChange}
                    hidden={hideCheckbox ? hideCheckbox : false}
                />
                <div onClick={this.onListClick}>
                    <Container>
                        <List
                            ref={x => this.lst = x}
                            itemTpl={this.nameFieldTpl}
                            store={this.state.items}
                            height={height}
                            maxHeight={maxHeight}
                            selectable={selectable}
                            scrollable="true"
                            onSelect={onSelect}
                        />
                    </Container>
                </div>
            </Container>
        );
    }

    public getSelected<T>(): T[] {
        return ExtDataviewHelper.getListSelected(this.lst);
    }

    public getSelectedKeys<T>(): T[] {
        return ExtDataviewHelper.getListSelected(this.lst, ID_PROP);
    }

    public getSelectedKeysOrNull<T>(): T[] {
        return ExtDataviewHelper.getListSelected(this.lst, ID_PROP).length > 0 ? ExtDataviewHelper.getListSelected(this.lst, ID_PROP) : null;
    }

    public reset() {
        this.onTopSelectionChange(this.cb, false, false);
    }

    protected init() {
        this.flag = true;
        //
        this.onListClick = this.onListClick.bind(this);
        this.onTopSelectionChange = this.onTopSelectionChange.bind(this);
    }

    protected onListClick() {
        this.flag = false;

        let checked = this.lst.getSelections().length > 0;

        this.cb.setChecked(checked);
    }

    protected onTopSelectionChange(cb: any, newVal: boolean, oldVal: boolean) {
        if (newVal) {
            if (this.flag) {
                this.lst.selectAll();
            }
        }
        else {
            this.cb.reset();
            this.lst.deselectAll();
        }
        this.flag = true;
    }
}