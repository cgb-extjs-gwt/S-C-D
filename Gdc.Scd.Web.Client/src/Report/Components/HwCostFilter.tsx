import { Button, Container, Panel, PanelProps } from "@extjs/ext-react";
import * as React from "react";
import { AvailabilityField } from "../../Dict/Components/AvailabilityField";
import { CountryField } from "../../Dict/Components/CountryField";
import { DictField } from "../../Dict/Components/DictField";
import { DurationField } from "../../Dict/Components/DurationField";
import { ProActiveField } from "../../Dict/Components/ProActiveField";
import { ReactionTimeField } from "../../Dict/Components/ReactionTimeField";
import { ReactionTypeField } from "../../Dict/Components/ReactionTypeField";
import { ServiceLocationField } from "../../Dict/Components/ServiceLocationField";
import { WgField } from "../../Dict/Components/WgField";
import { Country } from "../../Dict/Model/Country";
import { HwCostFilterModel } from "../Model/HwCostFilterModel";

export interface FilterPanelProps extends PanelProps {
    onSearch(filter: HwCostFilterModel): void;
}

export class HwCostFilter extends React.Component<FilterPanelProps, any> {

    private country: CountryField;

    private wg: DictField;

    private av: DictField;

    private dur: DictField;

    private reacttype: DictField;

    private reacttime: DictField;

    private srvloc: DictField;

    private proactive: DictField;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        let valid = this.state && this.state.valid;
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

                    <CountryField ref={x => this.country = x} label="Country:" cache={false} onChange={this.onCountryChange} />
                    <WgField ref={x => this.wg = x} label="Asset(WG):" />
                    <AvailabilityField ref={x => this.av = x} label="Availability:" />
                    <DurationField ref={x => this.dur = x} label="Duration:" />
                    <ReactionTypeField ref={x => this.reacttype = x} label="Reaction type:" />
                    <ReactionTimeField ref={x => this.reacttime = x} label="Reaction time:" />
                    <ServiceLocationField ref={x => this.srvloc = x} label="Service location:" />
                    <ProActiveField ref={x => this.proactive = x} label="ProActive:" />

                </Container>

                <Button text="Search" ui="action" minWidth="85px" margin="20px auto" disabled={!valid} handler={this.onSearch} />

            </Panel>
        );
    }

    public getModel(): HwCostFilterModel {
        return {
            country: this.country.getSelected(),
            wg: this.wg.getSelected(),
            availability: this.av.getSelected(),
            duration: this.dur.getSelected(),
            reactionType: this.reacttype.getSelected(),
            reactionTime: this.reacttime.getSelected(),
            serviceLocation: this.srvloc.getSelected(),
            proActive: this.proactive.getSelected()
        };
    }

    public getCountry(): Country {
        return this.country.getSelectedModel();
    }

    private init() {
        this.onCountryChange = this.onCountryChange.bind(this);
        this.onSearch = this.onSearch.bind(this);
    }

    private onCountryChange() {
        this.setState({ valid: !!this.country.getSelected() });
    }

    private onSearch() {
        let handler = this.props.onSearch;
        if (handler) {
            handler(this.getModel());
        }
    }
}