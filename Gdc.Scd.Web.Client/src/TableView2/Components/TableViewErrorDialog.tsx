import { Dialog, DialogProps } from "@extjs/ext-react";
import * as React from "react";
import { QualtityGateSetView, QualtityGateTab } from "../../TableView/Components/QualtityGateSetView";

export interface TableViewErrorDialogProps extends DialogProps {
    onForceSave(message: string): void;
    onCancel(): void;
}

export class TableViewErrorDialog extends React.Component<TableViewErrorDialogProps, any> {

    private dlg: Dialog & any;

    public state = {
        tabs: []
    };

    public constructor(props: TableViewErrorDialogProps) {
        super(props);
        this.init();
    }

    public render() {
        return <Dialog
            {...this.props}
            ref={x => this.dlg = x}
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
            <QualtityGateSetView tabs={this.state.tabs} onSave={this.onSave} onCancel={this.onCancel} />
        </Dialog>;
    }

    public display(errs: QualtityGateTab[]) {
        this.setModel(errs);
        this.show();
    }

    public show() {
        this.dlg.show();
    }

    public hide() {
        this.dlg.hide();
    }

    public setModel(m: QualtityGateTab[]) {
        this.setState({ tabs: m || [] });
    }

    private init() {
        this.onCancel = this.onCancel.bind(this);
        this.onSave = this.onSave.bind(this);
    }

    private onCancel() {
        this.props.onCancel();
        this.hide();
    }

    private onSave(msg: string) {
        this.props.onForceSave(msg);
        this.hide();
    }

}