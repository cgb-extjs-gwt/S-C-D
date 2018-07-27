import * as React from "react";
import { CheckColumn, CheckColumnProps } from "@extjs/ext-react";

export class ReadonlyCheckColumn extends React.Component<CheckColumnProps, any> {

    public render() {
        return (
            <CheckColumn {...this.props} disabled={true} headerCheckbox={false} />
        );
    }

}