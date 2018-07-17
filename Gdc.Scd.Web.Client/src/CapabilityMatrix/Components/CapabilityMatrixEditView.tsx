import * as React from "react";
import { Container, Button, CheckBoxField, SelectField, List, Label, ComboBoxField } from "@extjs/ext-react";
import { CapabilityMatrixMultiSelect } from "./CapabilityMatrixMultiSelect";

export class CapabilityMatrixEditView extends React.Component<{}> {

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

    onCountryChange() {
        console.log('onCountryChange()', new Date().getTime());
    }

    public render() {

        let isPortfolio = true;

        return (
            <Container layout="vbox" padding="10px">

                <ComboBoxField
                    width="250px"
                    label="Country:"
                    labelAlign="left"
                    labelWidth="80px"
                    options={this.countries}
                    displayField="name"
                    valueField="code"
                    queryMode="local"
                    typeAhead
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

                <Container layout={{ type: 'vbox', align: 'left' }} defaults={{ disabled: isPortfolio }}>
                    <CheckBoxField boxLabel="Fujitsu Global Portfolio" />
                    <CheckBoxField boxLabel="Master Portfolio" />
                    <CheckBoxField boxLabel="Core Portfolio" />
                </Container>

                <Container>
                    <Button text="Deny combinations" ui="decline" padding="0 10px 0 0" />
                    <Button text="Allow combinations" />
                </Container>

            </Container>
        );
    }
}