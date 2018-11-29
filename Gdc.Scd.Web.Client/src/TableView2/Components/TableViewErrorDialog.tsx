import * as React from "react";
import { Dialog, DialogProps } from "@extjs/ext-react";
import { QualityGateResultSet } from "../../TableView/States/TableViewState";

export interface TableViewErrorDialogProps extends DialogProps {
    //onOk(message: string): void;
    //onCancel(): void;
}

export class TableViewErrorDialog extends React.Component<TableViewErrorDialogProps, any> {

    private dlg: Dialog & any;

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
            ref={x => this.dlg = x}
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
        this.dlg.show();
    }

    public hide() {
        this.setState({ showDialog: false });
    }

    public setModel(m: QualityGateResultSet) {
        console.log(m);
    }

    private init() {
    }

}