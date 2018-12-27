import { Button, Container, Panel, PanelProps } from "@extjs/ext-react";
import * as React from "react";
import { CountryField } from "../../Dict/Components/CountryField";
import { DictField } from "../../Dict/Components/DictField";
import { SogField } from "../../Dict/Components/SogField";
import { YearField } from "../../Dict/Components/YearField";
import { SwCostFilterModel } from "../Model/SwCostFilterModel";
import { UserCountryField } from "../../Dict/Components/UserCountryField";

export interface FilterPanelProps extends PanelProps {
    checkAccess: boolean;
    onSearch(filter: SwCostFilterModel): void;
}

export class SwProactiveCostFilter extends React.Component<FilterPanelProps, any> {

    private cnt: DictField;

    private sog: DictField;

    private year: DictField;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {

        let countryField;

        if (this.props.checkAccess) {
            countryField = <UserCountryField ref={x => this.cnt = x} label="Country:" />;
        }
        else {
            countryField = <CountryField ref={x => this.cnt = x} label="Country:" />
        }

        return (
            <Panel {...this.props} margin="0 0 5px 0" padding="4px 20px 7px 20px">

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
                    <SogField ref={x => this.sog = x} label="Asset(SOG):" />
                    <YearField ref={x => this.year = x} label="Year:" />

                </Container>

                <Button text="Search" ui="action" minWidth="85px" handler={this.onSearch} margin="20px auto" />

            </Panel>
        );
    }

    public getModel(): SwCostFilterModel {
        return {
            country: this.cnt.getSelected(),
            sog: this.sog.getSelected(),
            year: this.year.getSelected()
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
}