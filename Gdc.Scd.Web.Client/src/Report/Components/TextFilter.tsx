import { TextField, TextFieldProps } from "@extjs/ext-react";
import * as React from "react";

export class TextFilter extends React.Component<TextFieldProps, any> {
    public render() {
        return (
            <TextField {...this.props} />
        );
    }
}