import { Button, CheckBoxField, Container, Panel, PanelProps } from "@extjs/ext-react";
import * as React from "react";

import { DictField } from "../../Dict/Components/DictField";
import { CountryGroupField } from "../../Dict/Components/CountryGroupField";
import { CountryGroupLutField } from "../../Dict/Components/CountryGroupLutField";
import { CountryGroupDigitField } from "../../Dict/Components/CountryGroupDigitField";
import { CountryGroupIsoCodeField } from "../../Dict/Components/CountryGroupIsoCodeField";

import { CountryFilterModel } from "./CountryFilterModel";

export interface FilterPanelProps extends PanelProps {
    onSearch(filter: CountryFilterModel): void;
}

export class FilterPanel extends React.Component<FilterPanelProps, any> {

    private group: DictField;

    private lut: DictField;

    private digit: DictField;

    private iso: DictField;

    private isMaster: CheckBoxField;

    private storeListAndDealer: CheckBoxField;

    private overrideTCandTP: CheckBoxField;

    public constructor(props: any) {
        super(props);
        this.init();
    }

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
                    }}
                >

                    <CountryGroupField ref={x => this.group = x} label="Group:"/>
                    <CountryGroupLutField ref={x => this.lut = x} label="LUT:" />
                    <CountryGroupDigitField ref={x => this.digit = x} label="Digit:" />
                    <CountryGroupIsoCodeField ref={x => this.iso = x} label="ISO Code:" />
                    
                </Container>

                <Container layout={{ type: 'vbox', align: 'left' }} defaults={{ padding: '3px 0' }}>
                    <CheckBoxField ref={x => this.isMaster = x} boxLabel="Is Master" />
                    <CheckBoxField ref={x => this.storeListAndDealer = x} boxLabel="Store List and Dealer Prices" />
                    <CheckBoxField ref={x => this.overrideTCandTP = x} boxLabel="Override TC and TP" />
                </Container>

                <Button text="Search" ui="action" minWidth="85px" handler={this.onSearch} margin="20px auto" />

            </Panel>
        );
    }

    public getModel(): CountryFilterModel {
        return {
            group: this.group.getSelected(),
            lut: this.lut.getSelectedValue(),
            digit: this.digit.getSelectedValue(),
            iso: this.iso.getSelectedValue(),

            isMaster: this.getChecked(this.isMaster),
            storeListAndDealer: this.getChecked(this.storeListAndDealer),
            overrideTCandTP: this.getChecked(this.overrideTCandTP)
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
}