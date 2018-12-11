import { Button, CheckBoxField, Container, Panel, PanelProps, RadioField, ContainerField } from "@extjs/ext-react";
import * as React from "react";

import { DictField } from "../../Dict/Components/DictField";
import { CountryField } from "../../Dict/Components/CountryField";
import { CountryGroupField } from "../../Dict/Components/CountryGroupField";
import { CountryGroupLutField } from "../../Dict/Components/CountryGroupLutField";
import { CountryGroupDigitField } from "../../Dict/Components/CountryGroupDigitField";
import { CountryGroupIsoCodeField } from "../../Dict/Components/CountryGroupIsoCodeField";

import { CountryFilterModel } from "./CountryFilterModel";

export interface FilterPanelProps extends PanelProps {
    onSearch(filter: CountryFilterModel): void;
}

export class FilterPanel extends React.Component<FilterPanelProps, any> {

    private country: DictField;

    private group: DictField;

    private lut: DictField;

    private digit: DictField;

    private iso: DictField;

    private isMaster: RadioField;

    private storeListAndDealer: CheckBoxField;

    private overrideTCandTP: CheckBoxField;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    private containerFieldProps = {
        layout: { type: 'hbox', align: 'left' },
        defaults: { padding: '0 4px' }
    };

    public render() {
        return (
            <Panel title="Filter By" {...this.props} margin="0 0 5px 0" padding="4px 20px 7px 20px">

                <Container margin="10px 0"
                    defaults={{
                        maxWidth: '200px',
                        valueField: 'id',
                        displayField: 'name',
                        queryMode: 'local',
                        clearable: 'true'
                    }}>
                    <CountryField ref={x => this.country = x} label="Country:" />
                    <CountryGroupField ref={x => this.group = x} label="Group:" />
                    <CountryGroupLutField ref={x => this.lut = x} label="LUT:" />
                    <CountryGroupDigitField ref={x => this.digit = x} label="Digit:" />
                    <CountryGroupIsoCodeField ref={x => this.iso = x} label="ISO Code:" />                
                </Container>

                <Container layout={{ type: 'vbox', align: 'left' }} defaults={{ padding: '3px 0' }}>
                    <ContainerField ref={x => this.isMaster = x} label="Is Master" {...this.containerFieldProps}>
                        <RadioField name="isMaster" boxLabel="All" value="All" checked/>
                        <RadioField name="isMaster" boxLabel="Yes" value="True"/>
                        <RadioField name="isMaster" boxLabel="No" value="False"/>
                    </ContainerField>
                    <ContainerField ref={x => this.storeListAndDealer = x} label="Store List and Dealer Prices" {...this.containerFieldProps}>
                        <RadioField name="storeListAndDealer" boxLabel="All" value="All" checked />
                        <RadioField name="storeListAndDealer" boxLabel="Yes" value="True" />
                        <RadioField name="storeListAndDealer" boxLabel="No" value="False" />
                    </ContainerField>
                    <ContainerField ref={x => this.overrideTCandTP = x} label="Override TC and TP" {...this.containerFieldProps}>
                        <RadioField name="overrideTCandTP" boxLabel="All" value="All" checked />
                        <RadioField name="overrideTCandTP" boxLabel="Yes" value="True" />
                        <RadioField name="overrideTCandTP" boxLabel="No" value="False" />
                    </ContainerField>
                </Container>

                <Button text="Search" ui="action" minWidth="85px" handler={this.onSearch} margin="20px auto" />

            </Panel>
        );
    }

    public getModel(): CountryFilterModel {
        let isMasterValue = (this.isMaster as any).getValues()['isMaster']

        return {
            country: this.country.getSelected(),
            group: this.group.getSelected(),
            lut: this.lut.getSelectedValue(),
            digit: this.digit.getSelectedValue(),
            iso: this.iso.getSelectedValue(),

            isMaster: this.getCheckedRadio(this.isMaster, 'isMaster'),
            storeListAndDealer: this.getCheckedRadio(this.storeListAndDealer, 'storeListAndDealer'),
            overrideTCandTP: this.getCheckedRadio(this.overrideTCandTP, 'overrideTCandTP')
        };
    }

    private init() {
        this.onSearch = this.onSearch.bind(this);
    }

    private onSearch() {
        let handler = this.props.onSearch;
        if (handler) {
            handler(this.getModel());
        }
    }

    private setPortfolio(val: boolean) {
        this.setState({ isPortfolio: val });
    }

    private getChecked(cb: CheckBoxField): boolean {
        return (cb as any).getChecked();
    }

    private getCheckedRadio(cf: ContainerField, fieldName: string): boolean {
        let value = (cf as any).getValues()[fieldName];
        if (value == "True") {
            return true;
        }
        else if(value == "False") {
            return false;
        }
        return null;
    }
}