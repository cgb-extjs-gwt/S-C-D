import * as React from "react";
import { Container, Button, CheckBoxField, ComboBoxField } from "@extjs/ext-react";
import { CapabilityMatrixMultiSelect } from "./Components/CapabilityMatrixMultiSelect";
import { ExtMsgHelper } from "../Common/Helpers/ExtMsgHelper";
import { NamedId } from "../Common/States/CommonStates";

import

export class CapabilityMatrixEditView extends React.Component<any, any> {

    constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        return (
            <Container layout="vbox" padding="10px">

                <ComboBoxField
                    width="250px"
                    label="Country:"
                    labelAlign="left"
                    labelWidth="80px"
                    options={this.state.countries}
                    displayField="name"
                    valueField="id"
                    queryMode="local"
                    clearable="true"
                    onChange={this.onCountryChange}
                />

                <Container layout="hbox">
                    <CapabilityMatrixMultiSelect title="Asset(WG)" itemTpl="{name}" store={this.state.warrantyGroups} />
                    <CapabilityMatrixMultiSelect title="Availability" itemTpl="{name}" store={this.state.availabilityTypes} />
                    <CapabilityMatrixMultiSelect title="Duration" itemTpl="{name}" store={this.state.durationTypes} />
                    <CapabilityMatrixMultiSelect title="React type" itemTpl="{name}" store={this.state.reactTypes} />
                    <CapabilityMatrixMultiSelect title="Reaction time" itemTpl="{name}" store={this.state.reactionTimeTypes} />
                    <CapabilityMatrixMultiSelect title="Service location" itemTpl="{name}" store={this.state.serviceLocationTypes} />
                </Container>

                <Container layout={{ type: 'vbox', align: 'left' }} defaults={{ disabled: !this.state.isPortfolio }}>
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

    private init() {
        this.onCountryChange = this.onCountryChange.bind(this);
        this.onAllow = this.onAllow.bind(this);
        this.onDeny = this.onDeny.bind(this);
        this.allowCombination = this.allowCombination.bind(this);
        this.denyCombination = this.denyCombination.bind(this);
        //
        this.state = {
            isPortfolio: true,
            countries: [],
            warrantyGroups: [],
            availabilityTypes: [],
            durationTypes: [],
            reactTypes: [],
            reactionTimeTypes: [],
            serviceLocationTypes: []
        };
    }

    private onCountryChange(combo, newVal, oldVal) {
        this.setPortfolio(!newVal);
    }

    private onAllow() {
        this.showSaveDialog('Allow combinations', this.allowCombination);
    }

    private onDeny() {
        this.showSaveDialog('Deny combinations', this.denyCombination);
    }

    private showSaveDialog(title: string, ok: Function) {
        ExtMsgHelper.confirm(title, 'Do you want to save the changes?', ok);
    }

    private allowCombination() {
        console.log('allowCombination()');
    }

    private denyCombination() {
        console.log('denyCombination()');
    }

    private setPortfolio(val: boolean) {
        this.setState({ isPortfolio: val });
    }
}