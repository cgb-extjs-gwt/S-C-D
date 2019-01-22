import { Button, CheckBoxField, Container, Panel, PanelProps } from "@extjs/ext-react";
import * as React from "react";
import { AvailabilityField } from "../../Dict/Components/AvailabilityField";
import { CountryField } from "../../Dict/Components/CountryField";
import { DictField } from "../../Dict/Components/DictField";
import { DurationField } from "../../Dict/Components/DurationField";
import { ReactionTimeField } from "../../Dict/Components/ReactionTimeField";
import { ReactionTypeField } from "../../Dict/Components/ReactionTypeField";
import { ServiceLocationField } from "../../Dict/Components/ServiceLocationField";
import { WgField } from "../../Dict/Components/WgField";
import { PortfolioFilterModel } from "../Model/PortfolioFilterModel";
import { ProActiveField } from "../../Dict/Components/ProActiveField";
import { UserCountryField } from "../../Dict/Components/UserCountryField";

export interface FilterPanelProps extends PanelProps {
    isCountryUser: boolean;
    onSearch(filter: PortfolioFilterModel): void;
}

export class FilterPanel extends React.Component<FilterPanelProps, any> {

    private country: DictField;

    private wg: DictField;

    private av: DictField;

    private dur: DictField;

    private reacttype: DictField;

    private reacttime: DictField;

    private srvloc: DictField;

    private proactive: DictField;

    private globPort: CheckBoxField;

    private masterPort: CheckBoxField;

    private corePort: CheckBoxField;

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
                    <UserCountryField ref={x => this.country = x} label="Country:" cache={false} onChange={this.onCountryChange} />
                    <WgField ref={x => this.wg = x} label="Asset(WG):" />
                    <AvailabilityField ref={x => this.av = x} label="Availability:" />
                    <DurationField ref={x => this.dur = x} label="Duration:" />
                    <ReactionTypeField ref={x => this.reacttype = x} label="Reaction type:" />
                    <ReactionTimeField ref={x => this.reacttime = x} label="Reaction time:" />
                    <ServiceLocationField ref={x => this.srvloc = x} label="Service location:" />
                    <ProActiveField ref={x => this.proactive = x} label="ProActive:" />

                </Container>

                <Container layout={{ type: 'vbox', align: 'left' }} defaults={{ disabled: !this.state.isPortfolio, padding: '3px 0', hidden: this.props.isCountryUser }}>
                    <CheckBoxField ref={x => this.globPort = x} boxLabel="Fujitsu principal portfolio" />
                    <CheckBoxField ref={x => this.masterPort = x} boxLabel="Master portfolio" />
                    <CheckBoxField ref={x => this.corePort = x} boxLabel="Core portfolio" />
                </Container>

                <Button text="Search" ui="action" minWidth="85px" handler={this.onSearch} margin="20px auto" />

            </Panel>
        );
    }

    public getModel(): PortfolioFilterModel {
        return {
            country: this.country.getSelected(),
            wg: this.wg.getSelected(),
            availability: this.av.getSelected(),
            duration: this.dur.getSelected(),
            reactionType: this.reacttype.getSelected(),
            reactionTime: this.reacttime.getSelected(),
            serviceLocation: this.srvloc.getSelected(),
            proActive: this.proactive.getSelected(),

            isGlobalPortfolio: this.getChecked(this.globPort),
            isMasterPortfolio: this.getChecked(this.masterPort),
            isCorePortfolio: this.getChecked(this.corePort)
        };
    }

    private init() {
        this.onCountryChange = this.onCountryChange.bind(this);
        this.onSearch = this.onSearch.bind(this);
        //
        this.state = {
            isPortfolio: true
        };
    }

    private onCountryChange(combo, newVal, oldVal) {
        this.setPortfolio(!newVal);
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
        return this.state.isPortfolio ? (cb as any).getChecked() : false;
    }
}