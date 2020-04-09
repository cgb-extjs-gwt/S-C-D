import { Button, Panel, PanelProps } from "@extjs/ext-react";
import * as React from "react";

export interface ReleasePanelProps extends PanelProps {
    onRelease?(): void,
    onReleaseAll?(): void,
    onUploadToSap?(): void,
    onUploadToSapAll?() : void,
    disabled: boolean;
}

export class HwReleasePanel extends React.Component<ReleasePanelProps, any> {

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {

        return (
            <Panel {...this.props} margin="0 0 5px 0" padding="4px 20px 7px 25px" layout={{ type: 'vbox', align: 'left' }}>
                <Button text="Release selected" ui="action" minWidth="85px" margin="5px 0px" handler={this.onRelease} disabled={this.props.disabled} />
                <Button text="Release all" ui="action" minWidth="85px" margin="5px 0px" handler={this.onReleaseAll} disabled={this.props.disabled} />
                <Button text="Upload selected to SAP" ui="action" minWidth="85px" margin="5px 0px" handler={this.onUploadToSap} disabled={this.props.disabled} />
                <Button text="Upload all to SAP" ui="action" minWidth="85px" margin="5px 0px" handler={this.onUploadToSapAll} disabled={this.props.disabled} />
            </Panel>
        );
    }

    private init() {
        this.onRelease = this.onRelease.bind(this);
        this.onReleaseAll = this.onReleaseAll.bind(this);
    }

    private onRelease() {
        let handler = this.props.onRelease;
        if (handler) {
            handler();
        }
    }

    private onReleaseAll() {
        let handler = this.props.onReleaseAll;
        if (handler) {
            handler();
        }
    }

    private onUploadToSap = () => {
        const { onUploadToSap } = this.props;

        onUploadToSap && onUploadToSap();
    }

    private onUploadToSapAll = () => {
        const { onUploadToSapAll } = this.props;

        onUploadToSapAll && onUploadToSapAll();
    }
}