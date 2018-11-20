import { Button, Dialog, NumberField, Toolbar, DialogProps } from '@extjs/ext-react';
import * as React from 'react';

interface HwManualCostDialogProps extends DialogProps {
    onOk(): void;
}

export class HwManualCostDialog extends React.Component<HwManualCostDialogProps, any> {

    private serviceTC: NumberField & any;

    private serviceTP: NumberField & any;

    private listPrice: NumberField & any;

    private dealerDiscount: NumberField & any;

    public state = {
        isValid: false,
        showDialog: false
    };

    public constructor(props: HwManualCostDialogProps) {
        super(props);
        this.init();
    }

    public render() {
        //const { isVisibleForm } = this.props;

        return (
            <Dialog
                {...this.props}
                displayed={this.state.showDialog}
                closable
                closeAction="hide"
                platformConfig={{
                    "!phone": {
                        maxHeight: 500,
                        width: 350
                    }
                }}
                onHide={this.onCancel}
            >

                <NumberField
                    ref={x => this.serviceTC = x}
                    label="Service TC"
                    onChange={this.onValueChange}
                />
                <NumberField
                    ref={x => this.serviceTP = x}
                    label="Service TP"
                    onChange={this.onValueChange}
                />
                <NumberField
                    ref={x => this.listPrice = x}
                    label="List price"
                    onChange={this.onValueChange}
                />
                <NumberField
                    ref={x => this.dealerDiscount = x}
                    label="Dealer discount"
                    onChange={this.onValueChange}
                />

                <Toolbar docked="bottom">
                    <Button text="Ok" handler={this.onOk} flex={1} disabled={!this.state.isValid} />
                    <Button text="Cancel" handler={this.onCancel} flex={1} />
                </Toolbar>

            </Dialog>
        )
    }

    public reset() {
        this.serviceTC.reset();
        this.serviceTP.reset();
        this.listPrice.reset();
        this.dealerDiscount.reset();
    }

    public show() {
        this.reset();
        this.setState({ showDialog: true });
    }

    public hide() {
        if (this.state.showDialog) {
            this.setState({ showDialog: false });
        }
    }

    private init() {
        this.onCancel = this.onCancel.bind(this);
        this.onOk = this.onOk.bind(this);
        this.onValueChange = this.onValueChange.bind(this);
    }

    private onValueChange() {
        let isValid = this.serviceTC.getValue() || this.serviceTP.getValue() || this.listPrice.getValue() || this.dealerDiscount.getValue();
        this.setState({ isValid: isValid });
    }

    private onCancel() {
        this.hide();
    }

    private onOk() {
        this.props.onOk();
        this.hide();
    }
}