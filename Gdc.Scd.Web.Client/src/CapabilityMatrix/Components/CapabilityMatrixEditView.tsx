import * as React from "react";
import { Container, Button, CheckBoxField, ComboBoxField } from "@extjs/ext-react";
import { CapabilityMatrixMultiSelect } from "./CapabilityMatrixMultiSelect";
import { NamedId } from "../../Common/States/CommonStates";

export interface CapabilityMatrixEditViewProps {

    isPortfolio: boolean;

    countries: NamedId[];

    warrantyGroups: NamedId[];

    availabilityTypes: NamedId[];

    durationTypes: NamedId[];

    reactTypes: NamedId[];

    reactionTimeTypes: NamedId[];

    serviceLocationTypes: NamedId[];

    onCountryChange(newVal: string, oldVal: string);

    onAllow();

    onDeny();
}

export class CapabilityMatrixEditView extends React.Component<CapabilityMatrixEditViewProps> {

    constructor(props: CapabilityMatrixEditViewProps) {
        super(props);

        this.onCountryChange = this.onCountryChange.bind(this);
        this.onAllow = this.onAllow.bind(this);
        this.onDeny = this.onDeny.bind(this);
    }

    public render() {
        return (
            <Container layout="vbox" padding="10px">

                <ComboBoxField
                    width="250px"
                    label="Country:"
                    labelAlign="left"
                    labelWidth="80px"
                    options={this.props.countries}
                    displayField="name"
                    valueField="id"
                    queryMode="local"
                    clearable="true"
                    onChange={this.onCountryChange}
                />

                <Container layout="hbox">
                    <CapabilityMatrixMultiSelect title="Asset(WG)" itemTpl="{name}" store={this.props.warrantyGroups} />
                    <CapabilityMatrixMultiSelect title="Availability" itemTpl="{name}" store={this.props.availabilityTypes} />
                    <CapabilityMatrixMultiSelect title="Duration" itemTpl="{name}" store={this.props.durationTypes} />
                    <CapabilityMatrixMultiSelect title="React type" itemTpl="{name}" store={this.props.reactTypes} />
                    <CapabilityMatrixMultiSelect title="Reaction time" itemTpl="{name}" store={this.props.reactionTimeTypes} />
                    <CapabilityMatrixMultiSelect title="Service location" itemTpl="{name}" store={this.props.serviceLocationTypes} />
                </Container>

                <Container layout={{ type: 'vbox', align: 'left' }} defaults={{ disabled: !this.props.isPortfolio }}>
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
        this.props.onCountryChange(newVal, oldVal);
    }

    private onAllow() {
        this.props.onAllow();
    }

    private onDeny() {
        this.props.onDeny();
    }
}