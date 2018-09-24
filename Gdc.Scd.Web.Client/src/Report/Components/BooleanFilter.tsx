import { CheckBoxField, CheckBoxFieldProps } from "@extjs/ext-react";
import * as React from "react";

export class BooleanFilter extends React.Component<CheckBoxFieldProps, any> {
    public render() {
        return (
            <CheckBoxField {...this.props} />
        );
    }
}