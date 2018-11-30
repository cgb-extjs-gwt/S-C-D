import { Container, Dialog, DialogProps, TabPanel } from "@extjs/ext-react";
import * as React from "react";
import { QualityGateToolbar } from "../../QualityGate/Components/QualityGateToolbar";
import { QualtityGateTab } from "../../TableView/Components/QualtityGateSetView";
import { TableViewErrorGrid } from "./TableViewErrorGrid";

export interface TableViewErrorDialogProps extends DialogProps {
    onForceSave(message: string): void;
    onCancel(): void;
}

export class TableViewErrorDialog extends React.Component<TableViewErrorDialogProps, any> {

    private dlg: Dialog & any;

    private toolbar: QualityGateToolbar;

    public state = {
        tabs: []
    };

    public constructor(props: TableViewErrorDialogProps) {
        super(props);
        this.init();
    }

    public componentWillReceiveProps() {
        this.toolbar.reset(); //clear form
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
            <Container layout="vbox">
                <TabPanel tabBar={{ layout: { pack: 'left' } }} flex={10}>
                    {this.state.tabs.map(this.createTab)}
                </TabPanel>
                <QualityGateToolbar ref={x => this.toolbar = x} onSave={this.onSave} onCancel={this.onCancel} flex={1} />
            </Container>
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

    private createTab({ title, costElement, errors }, index) {
        return <Container key={index} title={title}>
            <TableViewErrorGrid
                store={errors}
                minHeight="450"
                costElement={costElement}
                flex={1}
            />
        </Container>
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