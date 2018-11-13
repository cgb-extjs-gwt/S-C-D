import * as React from "react";
import { ComboBoxField, CheckBoxField, Container, Button, Panel, PanelProps } from "@extjs/ext-react";
import { ICapabilityMatrixService } from "../Services/ICapabilityMatrixService";
import { MatrixFactory } from "../Services/MatrixFactory";
import { CapabilityMatrixFilterModel } from "../Model/CapabilityMatrixFilterModel";

export interface FilterPanelProps extends PanelProps {
    onSearch(filter: CapabilityMatrixFilterModel): void;
}

export class FilterPanel extends React.Component<FilterPanelProps, any> {

    private country: ComboBoxField;

    private wg: ComboBoxField;

    private avail: ComboBoxField;

    private dur: ComboBoxField;

    private reacttype: ComboBoxField;

    private reacttime: ComboBoxField;

    private srvloc: ComboBoxField;

    private globPort: CheckBoxField;

    private masterPort: CheckBoxField;

    private corePort: CheckBoxField;

    private srv: ICapabilityMatrixService;

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
                    <ComboBoxField ref="country" label="Country:" options={this.state.countries} onChange={this.onCountryChange} />
                    <ComboBoxField ref="wg" label="Asset(WG):" options={this.state.warrantyGroups} />
                    <ComboBoxField ref="availability" label="Availability:" options={this.state.availabilityTypes} />
                    <ComboBoxField ref="duration" label="Duration:" options={this.state.durationTypes} />
                    <ComboBoxField ref="reactType" label="Reaction type:" options={this.state.reactTypes} />
                    <ComboBoxField ref="reactTime" label="Reaction time:" options={this.state.reactionTimeTypes} />
                    <ComboBoxField ref="srvLoc" label="Service location:" options={this.state.serviceLocationTypes} />
                </Container>

                <Container layout={{ type: 'vbox', align: 'left' }} defaults={{ disabled: !this.state.isPortfolio, padding: '3px 0' }}>
                    <CheckBoxField ref="globPort" boxLabel="Fujitsu global portfolio" />
                    <CheckBoxField ref="masterPort" boxLabel="Master portfolio" />
                    <CheckBoxField ref="corePort" boxLabel="Core portfolio" />
                </Container>

                <Button text="Search" ui="action" minWidth="85px" handler={this.onSearch} margin="20px auto" />

            </Panel>
        );
    }

    public componentDidMount() {
        Promise.all([
            this.srv.getCountries(),
            this.srv.getWG(),
            this.srv.getAvailabilityTypes(),
            this.srv.getDurationTypes(),
            this.srv.getReactTypes(),
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
        this.globPort = this.refs.globPort as CheckBoxField;
        this.masterPort = this.refs.masterPort as CheckBoxField;
        this.corePort = this.refs.corePort as CheckBoxField;
    }

    public getModel(): CapabilityMatrixFilterModel {
        return {
            country: this.getSelected(this.country),
            wg: this.getSelected(this.wg),
            availability: this.getSelected(this.avail),
            duration: this.getSelected(this.dur),
            reactionType: this.getSelected(this.reacttype),
            reactionTime: this.getSelected(this.reacttime),
            serviceLocation: this.getSelected(this.srvloc),

            isGlobalPortfolio: this.getChecked(this.globPort),
            isMasterPortfolio: this.getChecked(this.masterPort),
            isCorePortfolio: this.getChecked(this.corePort)
        };
    }

    private init() {
        this.srv = MatrixFactory.getMatrixService();
        //
        this.onCountryChange = this.onCountryChange.bind(this);
        this.onSearch = this.onSearch.bind(this);
        //
        this.state = {
            isPortfolio: true,
            countries: [],
            warrantyGroups: [],
            availabilityTypes: [],
            durationTypes: [],
            reactTypes: [],
            reactionTimeTypes: [],
            serviceLocationTypes: []
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

    private getSelected(cb: ComboBoxField): string {
        let result: string = null;
        let selected = (cb as any).getSelection();
        if (selected) {
            result = selected.data.id;
        }
        return result;
    }

    private getChecked(cb: CheckBoxField): boolean {
        return this.state.isPortfolio ? (cb as any).getChecked() : false;
    }
}