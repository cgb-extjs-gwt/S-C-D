import * as React from "react";
import { Dialog, DialogProps } from "@extjs/ext-react";

export interface TableViewErrorDialogProps extends DialogProps { }

export class TableViewErrorDialog extends React.Component<TableViewErrorDialogProps, any> {

    public state = {
        showDialog: false
    }

    public constructor(props: TableViewErrorDialogProps) {
        super(props);
        this.init();
    }

    public render() {
        return <Dialog
            {...this.props}
            displayed={this.state.showDialog}
            closable
            closeAction="hide"
            draggable={false}
            maximizable
            resizable={{
                dynamic: true,
                edges: 'all'
            }}
            minHeight="50%"
            minWidth="60%"
            layout="fit"
        >
            <div>
                <h1>TableViewErrorDialog here </h1>
            </div>
        </Dialog>;
    }

    public show() {
        this.setState({ showDialog: true });
    }

    public hide() {
        this.setState({ showDialog: false });
    }

    private init() {
    }

}