import { Button, Container, NumberField, Panel, PanelProps, TextField } from "@extjs/ext-react";
import * as React from "react";
import { RemoteAction } from "../Actions/NotifyActions";
import { RemoteNotify } from "../../Webworker/RemoteNotify";

export class AlertPanel extends React.Component<PanelProps, any> {

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        return (
            <Panel {...this.props} margin="0 0 5px 0" padding="4px 20px 7px 20px">
                <h3>alert panel here</h3>
            </Panel>
        );
    }

    private init() {
        this.onAlert = this.onAlert.bind(this);
        //
        RemoteNotify(this.onAlert);
    }

    private onAlert(data: RemoteAction) {
        console.log(data);
    }
}