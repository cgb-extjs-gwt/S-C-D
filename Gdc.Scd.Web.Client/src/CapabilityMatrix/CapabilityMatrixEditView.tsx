import { Button, CheckBoxField, ComboBoxField, Container } from "@extjs/ext-react";
import * as React from "react";
import { ExtMsgHelper } from "../Common/Helpers/ExtMsgHelper";
import { MultiSelect } from "./Components/MultiSelect";
import { MultiSelectWg } from "./Components/MultiSelectWg";
import { CapabilityMatrixEditModel } from "./Model/CapabilityMatrixEditModel";
import { ICapabilityMatrixService } from "./Services/ICapabilityMatrixService";
import { MatrixFactory } from "./Services/MatrixFactory";

const SELECT_MAX_HEIGHT: string = '260px';
const ID_PROP = 'id';

export class CapabilityMatrixEditView extends React.Component<any, any> {

    private country: ComboBoxField & any;

    private wg: MultiSelect;

    private avail: MultiSelect;

    private dur: MultiSelect;

    private reacttype: MultiSelect;

    private reacttime: MultiSelect;

    private srvloc: MultiSelect;

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
            <Container layout="vbox" padding="10px" scrollable="true">

                <ComboBoxField
                    ref="country"
                    width="250px"
                    label="Country:"
                    labelAlign="left"
                    labelWidth="80px"
                    options={this.state.countries}
                    displayField="name"
                    valueField="id"
                    queryMode="local"
                    clearable="true"
                    onChange={this.onCountryChange}
                />

                <div className="matrix-edit-container">
                    <div>
                        <MultiSelectWg ref="wg" maxHeight={SELECT_MAX_HEIGHT} title="Asset(WG)" itemTpl="{name}" store={this.state.warrantyGroups} />
                    </div>
                    <div>
                        <MultiSelect ref="availability" maxHeight={SELECT_MAX_HEIGHT} title="Availability" itemTpl="{name}" store={this.state.availabilityTypes} />
                    </div>
                    <div>
                        <MultiSelect ref="duration" maxHeight={SELECT_MAX_HEIGHT} title="Duration" itemTpl="{name}" store={this.state.durationTypes} />
                    </div>
                    <div>
                        <MultiSelect ref="reactType" maxHeight={SELECT_MAX_HEIGHT} title="Reaction type" itemTpl="{name}" store={this.state.reactTypes} />
                    </div>
                    <div>
                        <MultiSelect ref="reactTime" maxHeight={SELECT_MAX_HEIGHT} title="Reaction time" itemTpl="{name}" store={this.state.reactionTimeTypes} />
                    </div>
                    <div>
                        <MultiSelect ref="srvLoc" maxHeight={SELECT_MAX_HEIGHT} title="Service location" itemTpl="{name}" store={this.state.serviceLocationTypes} />
                    </div>
                </div>

                <Container layout={{ type: 'vbox', align: 'left' }} defaults={{ disabled: !this.state.isPortfolio }} margin="15px 0">
                    <CheckBoxField ref="globPort" boxLabel="Fujitsu global portfolio" />
                    <CheckBoxField ref="masterPort" boxLabel="Master portfolio" />
                    <CheckBoxField ref="corePort" boxLabel="Core portfolio" />
                </Container>

                <Container>
                    <Button text="Deny combinations" ui="decline" padding="0 10px 0 0" handler={this.onDeny} />
                </Container>

            </Container>
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
        this.country = this.refs['country'] as ComboBoxField;
        this.wg = this.refs['wg'] as MultiSelect;
        this.avail = this.refs['availability'] as MultiSelect;
        this.dur = this.refs['duration'] as MultiSelect;
        this.reacttype = this.refs['reactType'] as MultiSelect;
        this.reacttime = this.refs['reactTime'] as MultiSelect;
        this.srvloc = this.refs['srvLoc'] as MultiSelect;
        this.globPort = this.refs['globPort'] as CheckBoxField;
        this.masterPort = this.refs['masterPort'] as CheckBoxField;
        this.corePort = this.refs['corePort'] as CheckBoxField;
    }

    private init() {
        this.srv = MatrixFactory.getMatrixService();
        //
        this.onCountryChange = this.onCountryChange.bind(this);
        this.onDeny = this.onDeny.bind(this);
        this.denyCombination = this.denyCombination.bind(this);
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

    private onDeny() {
        ExtMsgHelper.confirm('Deny combinations', 'Do you want to save the changes?', this.denyCombination);
    }

    private denyCombination() {
        this.srv.denyItem(this.getModel());
    }

    private setPortfolio(val: boolean) {
        this.setState({ isPortfolio: val });
    }

    private getModel(): CapabilityMatrixEditModel {
        return {
            isGlobalPortfolio: this.isGlobalPortfolio(),
            isMasterPortfolio: this.isMasterPortfolio(),
            isCorePortfolio: this.isCorePortfolio(),

            countryId: this.getCountry(),

            wgs: this.getWg(),
            availabilities: this.getAvailability(),
            durations: this.getDuration(),
            reactionTypes: this.getReactType(),
            reactionTimes: this.getReactTime(),
            serviceLocations: this.getServiceLocation()
        }
    }

    private getWg(): string[] {
        return this.wg.getSelectedKeys(ID_PROP);
    }

    private getAvailability(): string[] {
        return this.avail.getSelectedKeys(ID_PROP);
    }

    private getDuration(): string[] {
        return this.dur.getSelectedKeys(ID_PROP);
    }

    private getReactType(): string[] {
        return this.reacttype.getSelectedKeys(ID_PROP);
    }

    private getReactTime(): string[] {
        return this.reacttime.getSelectedKeys(ID_PROP);
    }

    private getServiceLocation(): string[] {
        return this.srvloc.getSelectedKeys(ID_PROP);
    }

    private getCountry(): string {
        let result: string = null;
        let selected = this.country.getSelection();
        if (selected) {
            result = selected.data.id;
        }
        return result;
    }

    private isGlobalPortfolio(): boolean {
        return this.isPortfolio(this.globPort);
    }

    private isMasterPortfolio(): boolean {
        return this.isPortfolio(this.masterPort);
    }

    private isCorePortfolio(): boolean {
        return this.isPortfolio(this.corePort);
    }

    private isPortfolio(cb: CheckBoxField): boolean {
        return this.state.isPortfolio ? (cb as any).getChecked() : false;
    }
}