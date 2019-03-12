import * as React from "react";
import { NamedId } from "../../Common/States/CommonStates";
import { DictField, DictFieldProps } from "./DictField";
import { MultiSelectProps } from "./MultiSelect";
import { MultiSelectField } from "./MultiSelectField";

export interface SelectFieldProps extends DictFieldProps, MultiSelectProps {
    itemTpl?: Function | string | string[] | any;
    store(): Promise<NamedId[]>;
    panelWidth?: string | number;
    filter?: NamedId;
}

class DictFieldWrap extends DictField<NamedId> {
    public getItems() {
        return this.props.store(this.canCache());
    }
}

//Wrap over dictfield and MultiSelectField
export class SelectField extends React.Component<SelectFieldProps, any> {

    private component: any;

    public render() {
        if (this.props.multiSelect) {
            return <MultiSelectField {...this.props} ref={x => this.component = x} />;
        }
        else {
            return <DictFieldWrap {...this.props} ref={x => this.component = x} />
        }
    }

    public getValue(): string | string[] {
        return this.component.getValue();
    }

    public componentDidUpdate() {
        let filter = this.props.filter;
        if (filter && this.component.filter) {
            this.component.filter(filter.name, filter.id);
        }
    }
}