import { ComboBoxField, ComboBoxFieldProps } from "@extjs/ext-react";
import * as React from "react";
import { NamedId } from "../../Common/States/CommonStates";
import { IDictService } from "../../Dict/Services/IDictService";
import { DictFactory } from "../Services/DictFactory";

export abstract class DictField extends React.Component<ComboBoxFieldProps, any> {

    private combo: ComboBoxField;

    protected srv: IDictService;

    public constructor(props: ComboBoxFieldProps) {
        super(props);
        this.init();
    }

    public render() {
        return (
            <ComboBoxField
                {...this.props}
                ref="combo"
                options={this.state.items}
                valueField="id"
                displayField="name"
                queryMode="local"
                clearable="true"
            />
        );
    }

    public componentDidMount() {
        this.getItems().then(x => this.setState({ items: x }));
        this.combo = this.refs.combo as ComboBoxField;
    }

    public getValue(): string {
        return this.getSelected();
    }

    public getSelected(): string {
        let result: string = null;
        let selected = (this.combo as any).getSelection();
        if (selected) {
            result = selected.data.id;
        }
        return result;
    }

    protected abstract getItems(): Promise<NamedId[]>;

    private init() {
        this.srv = DictFactory.getDictService();
        this.state = {
            items: []
        };
    }
}