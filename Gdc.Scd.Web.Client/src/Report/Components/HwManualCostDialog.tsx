import { Button, Dialog, DialogProps, NumberField, Toolbar } from '@extjs/ext-react';
import * as React from 'react';
import { HwCostListModel } from '../Model/HwCostListModel';

interface HwManualCostDialogProps extends DialogProps {
    onOk(model: HwCostListModel): void;
}

export class HwManualCostDialog extends React.Component<HwManualCostDialogProps, any> {

    private serviceTC: NumberField & any;

    private serviceTP: NumberField & any;

    private listPrice: NumberField & any;

    private dealerDiscount: NumberField & any;

    private model: HwCostListModel;

    public state = {
        showDialog: false
    };

    public constructor(props: HwManualCostDialogProps) {
        super(props);
        this.init();
    }

    public render() {

        let disableTC: boolean = true;
        let disablePrice: boolean = true;

        if (this.model) {
            disableTC = !this.model.CanOverrideTransferCostAndPrice;
            disablePrice = !this.model.CanStoreListAndDealerPrices;
        }

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

                <NumberField ref={x => this.serviceTC = x} label="Service TC" disabled={disableTC} />
                <NumberField ref={x => this.serviceTP = x} label="Service TP" disabled={disableTC} />
                <NumberField ref={x => this.listPrice = x} label="List price" disabled={disablePrice} />
                <NumberField ref={x => this.dealerDiscount = x} label="Dealer discount" disabled={disablePrice} />

                <Toolbar docked="bottom">
                    <Button text="Ok" handler={this.onOk} flex={1} />
                    <Button text="Cancel" handler={this.onCancel} flex={1} />
                </Toolbar>

            </Dialog>
        )
    }

    public reset() {
        this.model = null;

        this.serviceTC.reset();
        this.serviceTP.reset();
        this.listPrice.reset();
        this.dealerDiscount.reset();
    }

    public getModel(): HwCostListModel {
        return {
            ...this.model,
            ServiceTCManual: this.serviceTC.getValue(),
            ServiceTPManual: this.serviceTP.getValue(),
            ListPrice: this.listPrice.getValue(),
            DealerDiscount: this.dealerDiscount.getValue()
        };
    }

    public setModel(m: HwCostListModel) {
        if (m) {
            this.model = { ...m };

            this.serviceTC.setValue(m.ServiceTCManual || m.ServiceTC);
            this.serviceTP.setValue(m.ServiceTPManual || m.ServiceTP);
            this.listPrice.setValue(m.ListPrice);
            this.dealerDiscount.setValue(m.DealerDiscount);
        }
        else {
            this.reset();
        }
    }

    public show() {
        this.setState({ showDialog: true });
    }

    public hide() {
        this.setState({ showDialog: false });
    }

    private init() {
        this.onCancel = this.onCancel.bind(this);
        this.onOk = this.onOk.bind(this);
    }

    private onCancel() {
        this.hide();
    }

    private onOk() {
        this.props.onOk(this.getModel());
        this.hide();
    }
}