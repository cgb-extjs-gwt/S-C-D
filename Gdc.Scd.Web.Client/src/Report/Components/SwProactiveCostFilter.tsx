import { Button, Container, Panel, PanelProps } from "@extjs/ext-react";
import * as React from "react";
import { AvailabilityField } from "../../Dict/Components/AvailabilityField";
import { CountryField } from "../../Dict/Components/CountryField";
import { DictField } from "../../Dict/Components/DictField";
import { SwDigitField } from "../../Dict/Components/SwDigitField";
import { UserCountryField } from "../../Dict/Components/UserCountryField";
import { YearField } from "../../Dict/Components/YearField";
import { SwCostFilterModel } from "../Model/SwCostFilterModel";
import { NamedId } from "../../Common/States/CommonStates";

export interface FilterPanelProps extends PanelProps {
    checkAccess: boolean;
    onSearch(filter: SwCostFilterModel): void;
    onDownload(filter: SwCostFilterModel): void;
}

export class SwProactiveCostFilter extends React.Component<FilterPanelProps, any> {

    private cnt: DictField<NamedId>;

    private digit: DictField<NamedId>;

    private av: DictField<NamedId>;

    private year: DictField<NamedId>;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {

        let valid = this.state && this.state.valid;

        let countryField;

        if (this.props.checkAccess) {
            countryField = <UserCountryField ref={x => this.cnt = x} label="Country:" onChange={this.onCountryChange} />;
        }
        else {
            countryField = <CountryField ref={x => this.cnt = x} label="Country:" onChange={this.onCountryChange} />
        }

        return (
            <Panel {...this.props} margin="0 0 5px 0" padding="4px 20px 7px 20px" layout={{ type: 'vbox', align: 'left' }}>

                <Container margin="10px 0"
                    defaults={{
                        maxWidth: '200px',
                        valueField: 'id',
                        displayField: 'name',
                        queryMode: 'local',
                        clearable: 'true'
                    }}
                >

                    {countryField}
                    <SwDigitField ref={x => this.digit = x} label="SW digit:" />
                    <AvailabilityField ref={x => this.av = x} label="Availability:" />
                    <YearField ref={x => this.year = x} label="Year:" />

                </Container>

                <Button text="Search" ui="action" minWidth="85px" margin="20px auto" disabled={!valid} handler={this.onSearch} />

                <Button text="Download" ui="action" minWidth="85px" iconCls="x-fa fa-download" disabled={!valid} handler={this.onDownload} />

            </Panel>
        );
    }

    public getModel(): SwCostFilterModel {
        return {
            country: this.cnt.getSelected(),
            digit: this.digit.getSelected(),
            availability: this.av.getSelected(),
            year: this.year.getSelected()
        };
    }

    private init() {
        this.onCountryChange = this.onCountryChange.bind(this);
        this.onSearch = this.onSearch.bind(this);
        this.onDownload = this.onDownload.bind(this);
    }

    private onCountryChange() {
        this.setState({ valid: !!this.cnt.getSelected() });
    }

    private onSearch() {
        let handler = this.props.onSearch;
        if (handler) {
            handler(this.getModel());
        }
    }

    private onDownload() {
        let handler = this.props.onDownload;
        if (handler) {
            handler(this.getModel());
        }
    }
}