import { Button, CheckBoxField, Container, ContainerField, Panel, PanelProps, RadioField } from "@extjs/ext-react";
import * as React from "react";
import { CountryGroupDigitField } from "../../Dict/Components/CountryGroupDigitField";
import { CountryGroupField } from "../../Dict/Components/CountryGroupField";
import { CountryGroupIsoCodeField } from "../../Dict/Components/CountryGroupIsoCodeField";
import { CountryGroupLutField } from "../../Dict/Components/CountryGroupLutField";
import { CountryNameField } from "../../Dict/Components/CountryNameField";
import { CountryQualityGroupField } from "../../Dict/Components/CountryQualityGroupField";
import { DictField } from "../../Dict/Components/DictField";
import { CountryFilterModel } from "./CountryFilterModel";
import { NamedId } from "../../Common/States/CommonStates";
import { RegionNameField } from "../../Dict/Components/RegionNameField";

export interface FilterPanelProps extends PanelProps {
    onSearch(filter: CountryFilterModel): void;
}

export class FilterPanel extends React.Component<FilterPanelProps, any> {

    private country: DictField<NamedId>;

    private group: DictField<NamedId>;

    private region: DictField<NamedId>;

    private lut: DictField<NamedId>;

    private digit: DictField<NamedId>;

    private iso: DictField<NamedId>;

    private qualityGroup: DictField<NamedId>;

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
                    <RegionNameField ref={x => this.region = x} label="Region:"/>
                    <CountryNameField ref={x => this.country = x} label="Country:" />
                    <CountryGroupField ref={x => this.group = x} label="Country Group:" />
                    <CountryGroupLutField ref={x => this.lut = x} label="LUT:" />
                    <CountryGroupDigitField ref={x => this.digit = x} label="Digit:" />
                    <CountryGroupIsoCodeField ref={x => this.iso = x} label="ISO Code:" />        
                    <CountryQualityGroupField ref={x => this.qualityGroup = x} label="Quality Group:" />        
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
            region: this.region.getSelected(),
            country: this.country.getSelectedValue(),
            group: this.group.getSelected(),
            lut: this.lut.getSelectedValue(),
            digit: this.digit.getSelectedValue(),
            iso: this.iso.getSelectedValue(),
            qualityGroup: this.qualityGroup.getSelectedValue(),

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