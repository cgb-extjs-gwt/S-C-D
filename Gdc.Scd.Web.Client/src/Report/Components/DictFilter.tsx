import { ComboBoxField, ComboBoxFieldProps } from "@extjs/ext-react";
import * as React from "react";
import { NamedId } from "../../Common/States/CommonStates";
import { IDictService } from "../../Dict/Services/IDictService";
import { ReportFactory } from "../Services/ReportFactory";

export interface DictFilterProps extends ComboBoxFieldProps {
    getItems(srv: IDictService): Promise<NamedId[]>;
}

export class DictFilter extends React.Component<DictFilterProps, any> {

    private combo: ComboBoxField;

    private srv: IDictService;

    public constructor(props: DictFilterProps) {
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
        this.props.getItems(this.srv).then(x => this.setState({ items: x }));
        this.combo = this.refs.country as ComboBoxField;
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

    private init() {
        this.srv = ReportFactory.getDictService();
        this.state = {
            items: []
        };
    }
}