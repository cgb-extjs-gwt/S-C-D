import { Button, Container, Panel, PanelProps } from "@extjs/ext-react";
import * as React from "react";
import { AvailabilityField } from "../../Dict/Components/AvailabilityField";
import { DictField } from "../../Dict/Components/DictField";
import { SogField } from "../../Dict/Components/SogField";
import { YearField } from "../../Dict/Components/YearField";
import { SwCostFilterModel } from "../Model/SwCostFilterModel";

export interface FilterPanelProps extends PanelProps {
    onSearch(filter: SwCostFilterModel): void;
}

export class SwCostFilter extends React.Component<FilterPanelProps, any> {

    private sog: DictField;

    private avail: DictField;

    private year: DictField;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
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

                    <SogField ref="sog" label="Asset(SOG):" />
                    <AvailabilityField ref="availability" label="Availability:" />
                    <YearField ref="year" label="Year:" />

                </Container>

                <Button text="Search" ui="action" minWidth="85px" handler={this.onSearch} margin="20px auto" />

            </Panel>
        );
    }

    public componentDidMount() {
        this.sog = this.refs.sog as DictField;
        this.avail = this.refs.availability as DictField;
        this.year = this.refs.year as DictField;
    }

    public getModel(): SwCostFilterModel {
        return {
            sog: this.sog.getSelected(),
            availability: this.avail.getSelected(),
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