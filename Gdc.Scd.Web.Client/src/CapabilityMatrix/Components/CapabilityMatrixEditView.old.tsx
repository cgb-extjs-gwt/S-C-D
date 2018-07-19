import * as React from "react";
import { Container, Button, CheckBoxField, SelectField, List, Label, ComboBoxField } from "@extjs/ext-react";
import { CapabilityMatrixMultiSelect } from "./CapabilityMatrixMultiSelect";
import { ExtMsgHelper } from '../../Common/Helpers/ExtMsgHelper'

export class CapabilityMatrixEditView extends React.Component<any> {

    store = Ext.create('Ext.data.Store', {
        data: [
            { title: 'Item 1' },
            { title: 'Item 2' },
            { title: 'Item 3' },
            { title: 'Item 4' }
        ]
    });

    countries = [
        { "name": "Alabama", "abbrev": "AL" },
        { "name": "Alaska", "abbrev": "AK" },
        { "name": "Arizona", "abbrev": "AZ" }
    ];

    constructor(props: any) {
        super(props);

        this.state = { isPortfolio: true };

        this.onCountryChange = this.onCountryChange.bind(this);
        this.onAllow = this.onAllow.bind(this);
        this.onDeny = this.onDeny.bind(this);
    }

    public render() {

        let isPortfolio = this.state.isPortfolio;

        return (
            <Container layout="vbox" padding="10px">

                <ComboBoxField
                    width="250px"
                    label="Country:"
                    labelAlign="left"
                    labelWidth="80px"
                    options={this.countries}
                    displayField="name"
                    valueField="abbrev"
                    queryMode="local"
                    clearable="true"

                    onChange={this.onCountryChange}
                />

                <Container layout="hbox">
                    <CapabilityMatrixMultiSelect title="Asset(WG)" itemTpl="{title}" store={this.store} />
                    <CapabilityMatrixMultiSelect title="Availability" itemTpl="{title}" store={this.store} />
                    <CapabilityMatrixMultiSelect title="Duration" itemTpl="{title}" store={this.store} />
                    <CapabilityMatrixMultiSelect title="React type" itemTpl="{title}" store={this.store} />
                    <CapabilityMatrixMultiSelect title="Reaction time" itemTpl="{title}" store={this.store} />
                    <CapabilityMatrixMultiSelect title="Service location" itemTpl="{title}" store={this.store} />
                </Container>

                <Container layout={{ type: 'vbox', align: 'left' }} defaults={{ disabled: !isPortfolio }}>
                    <CheckBoxField boxLabel="Fujitsu Global Portfolio" />
                    <CheckBoxField boxLabel="Master Portfolio" />
                    <CheckBoxField boxLabel="Core Portfolio" />
                </Container>

                <Container>
                    <Button text="Deny combinations" ui="decline" padding="0 10px 0 0" handler={this.onDeny} />
                    <Button text="Allow combinations" handler={this.onAllow} />
                </Container>

            </Container>
        );
    }

    private onCountryChange(combo, newVal, oldVal) {
        this.setState({ isPortfolio: !newVal });
    }

    private onAllow() {
        this.showSaveDialog(true);
    }

    private onDeny() {
        this.showSaveDialog(false);
    }

    private showSaveDialog(allow: boolean) {
        ExtMsgHelper.confirm(
            allow ? 'Allow combinations' : 'Deny combinations',
            'Do you want to save the changes?',
            () => this.save(allow)
        );
    }

    private save(allow: boolean) {
        console.log('save()', allow);
    }
}