import { ComboBoxField, ComboBoxFieldProps } from "@extjs/ext-react";
import * as React from "react";
import { NamedId } from "../../Common/States/CommonStates";
import { IDictService } from "../../Dict/Services/IDictService";
import { DictFactory } from "../Services/DictFactory";

export abstract class DictField extends React.Component<ComboBoxFieldProps, any> {

    private combo: ComboBoxField & any;

    protected srv: IDictService;

    protected valueField: string = 'id';

    protected nameField: string = 'name';

    public constructor(props: ComboBoxFieldProps) {
        super(props);
        this.init();
    }

    public render() {
        return <ComboBoxField
            {...this.props}
            ref={x => this.combo = x}
            options={this.state.items}
            valueField={this.valueField}
            displayField={this.nameField}
            queryMode="local"
            clearable="true"
            forceSelection={true}
        />;
    }

    public componentDidMount() {
        let store = this.combo.getStore() as any;
        let sorters = store.getSorters();
        sorters.remove(this.nameField);
        sorters.add(this.nameField);

        this.getItems().then(x => store.setData(x));
    }

    public getValue(): string {
        return this.getSelected();
    }

    public getSelected(): string {
        let result: string = null;
        let selected = this.combo.getSelection();
        if (selected) {
            result = selected.data.id;
        }
        return result;
    }

    public getSelectedValue(): string {
        let result: string = null;
        let selected = this.combo.getSelection();
        if (selected) {
            result = selected.data.name;
        }
        return result;
    }

    public reset() {
        this.combo.reset();
    }

    protected abstract getItems(): Promise<NamedId[]>;

    private init() {
        this.srv = DictFactory.getDictService();
        this.state = {
            items: []
        };
    }
}