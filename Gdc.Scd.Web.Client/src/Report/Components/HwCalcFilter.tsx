import { Button, ComboBoxField, Container, Panel, PanelProps } from "@extjs/ext-react";
import * as React from "react";
import { IDictService } from "../../Dict/Services/IDictService";
import { HwCalcFilterModel } from "../Model/HwCalcFilterModel";
import { ReportFactory } from "../Services/ReportFactory";

export interface FilterPanelProps extends PanelProps {
    onSearch(filter: HwCalcFilterModel): void;
}

export class HwCalcFilter extends React.Component<FilterPanelProps, any> {

    private country: ComboBoxField;

    private wg: ComboBoxField;

    private avail: ComboBoxField;

    private dur: ComboBoxField;

    private reacttype: ComboBoxField;

    private reacttime: ComboBoxField;

    private srvloc: ComboBoxField;

    private srv: IDictService;

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
                    <ComboBoxField ref="country" label="Country:" options={this.state.countries} />
                    <ComboBoxField ref="wg" label="Asset(WG):" options={this.state.warrantyGroups} />
                    <ComboBoxField ref="availability" label="Availability:" options={this.state.availabilityTypes} />
                    <ComboBoxField ref="duration" label="Duration:" options={this.state.durationTypes} />
                    <ComboBoxField ref="reactType" label="Reaction type:" options={this.state.reactTypes} />
                    <ComboBoxField ref="reactTime" label="Reaction time:" options={this.state.reactionTimeTypes} />
                    <ComboBoxField ref="srvLoc" label="Service location:" options={this.state.serviceLocationTypes} />
                </Container>

                <Button text="Search" ui="action" width="85px" handler={this.onSearch} margin="20px auto" />

            </Panel>
        );
    }

    public componentDidMount() {
        Promise.all([
            this.srv.getCountries(),
            this.srv.getWG(),
            this.srv.getAvailabilityTypes(),
            this.srv.getDurationTypes(),
            this.srv.getReactionTypes(),
            this.srv.getReactionTimeTypes(),
            this.srv.getServiceLocationTypes()
        ]).then(x => {
            this.setState({
                countries: x[0],
                warrantyGroups: x[1],
                availabilityTypes: x[2],
                durationTypes: x[3],
                reactTypes: x[4],
                reactionTimeTypes: x[5],
                serviceLocationTypes: x[6]
            });
        });
        //
        this.country = this.refs.country as ComboBoxField;
        this.wg = this.refs.wg as ComboBoxField;
        this.avail = this.refs.availability as ComboBoxField;
        this.dur = this.refs.duration as ComboBoxField;
        this.reacttype = this.refs.reactType as ComboBoxField;
        this.reacttime = this.refs.reactTime as ComboBoxField;
        this.srvloc = this.refs.srvLoc as ComboBoxField;
    }

    public getModel(): HwCalcFilterModel {
        return {
            country: this.getSelected(this.country),
            wg: this.getSelected(this.wg),
            availability: this.getSelected(this.avail),
            duration: this.getSelected(this.dur),
            reactionType: this.getSelected(this.reacttype),
            reactionTime: this.getSelected(this.reacttime),
            serviceLocation: this.getSelected(this.srvloc),
        };
    }

    private init() {
        this.srv = ReportFactory.getDictService();
        //
        this.onSearch = this.onSearch.bind(this);
        //
        this.state = {
            countries: [],
            warrantyGroups: [],
            availabilityTypes: [],
            durationTypes: [],
            reactTypes: [],
            reactionTimeTypes: [],
            serviceLocationTypes: []
        };
    }

    private onSearch() {
        let handler = this.props.onSearch;
        if (handler) {
            handler(this.getModel());
        }
    }

    private getSelected(cb: ComboBoxField): string {
        let result: string = null;
        let selected = (cb as any).getSelection();
        if (selected) {
            result = selected.data.id;
        }
        return result;
    }
}