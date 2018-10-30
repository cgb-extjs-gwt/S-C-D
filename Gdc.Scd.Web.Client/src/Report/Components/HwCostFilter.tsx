import { Button, Container, Panel, PanelProps } from "@extjs/ext-react";
import * as React from "react";
import { AvailabilityField } from "../../Dict/Components/AvailabilityField";
import { CountryField } from "../../Dict/Components/CountryField";
import { DictField } from "../../Dict/Components/DictField";
import { DurationField } from "../../Dict/Components/DurationField";
import { ReactionTimeField } from "../../Dict/Components/ReactionTimeField";
import { ReactionTypeField } from "../../Dict/Components/ReactionTypeField";
import { ServiceLocationField } from "../../Dict/Components/ServiceLocationField";
import { WgField } from "../../Dict/Components/WgField";
import { HwCostFilterModel } from "../Model/HwCostFilterModel";

export interface FilterPanelProps extends PanelProps {
    onSearch(filter: HwCostFilterModel): void;
}

export class HwCostFilter extends React.Component<FilterPanelProps, any> {

    private country: DictField;

    private wg: DictField;

    private avail: DictField;

    private dur: DictField;

    private reacttype: DictField;

    private reacttime: DictField;

    private srvloc: DictField;

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

                    <CountryField ref="country" label="Country:" />
                    <WgField ref="wg" label="Asset(WG):" />
                    <AvailabilityField ref="availability" label="Availability:" />
                    <DurationField ref="duration" label="Duration:" />
                    <ReactionTypeField ref="reactType" label="Reaction type:" />
                    <ReactionTimeField ref="reactTime" label="Reaction time:" />
                    <ServiceLocationField ref="srvLoc" label="Service location:" />

                </Container>

                <Button text="Search" ui="action" width="85px" handler={this.onSearch} margin="20px auto" />

            </Panel>
        );
    }

    public componentDidMount() {
        this.country = this.refs.country as DictField;
        this.wg = this.refs.wg as DictField;
        this.avail = this.refs.availability as DictField;
        this.dur = this.refs.duration as DictField;
        this.reacttype = this.refs.reactType as DictField;
        this.reacttime = this.refs.reactTime as DictField;
        this.srvloc = this.refs.srvLoc as DictField;
    }

    public getModel(): HwCostFilterModel {
        return {
            country: this.country.getSelected(),
            wg: this.wg.getSelected(),
            availability: this.avail.getSelected(),
            duration: this.dur.getSelected(),
            reactionType: this.reacttype.getSelected(),
            reactionTime: this.reacttime.getSelected(),
            serviceLocation: this.srvloc.getSelected(),
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