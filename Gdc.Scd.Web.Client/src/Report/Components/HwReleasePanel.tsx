import { Button, Panel, PanelProps } from "@extjs/ext-react";
import * as React from "react";

export interface ReleasePanelProps extends PanelProps {
    onApprove(): void;
}

export class HwReleasePanel extends React.Component<ReleasePanelProps, any> {

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {

        return (
            <Panel {...this.props} margin="0 0 5px 0" padding="4px 20px 7px 20px">

                <Button text="Approve for Release" ui="action" minWidth="85px" margin="20px auto" handler={this.onApprove} />

            </Panel>
        );
    }

    private init() {
        this.onApprove = this.onApprove.bind(this);
    }

    private onApprove() {
        let handler = this.props.onApprove;
        if (handler) {
            handler();
        }
    }
}