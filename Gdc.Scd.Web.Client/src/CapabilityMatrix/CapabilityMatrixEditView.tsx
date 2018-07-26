import * as React from "react";
import { Container, Button, CheckBoxField, ComboBoxField, Checkbox } from "@extjs/ext-react";
import { CapabilityMatrixMultiSelect } from "./Components/CapabilityMatrixMultiSelect";
import { ExtMsgHelper } from "../Common/Helpers/ExtMsgHelper";
import { NamedId } from "../Common/States/CommonStates";
import * as srv from "./fakes/FakeCapabilityMatrixServices";

const selectMaxH: string = '260px';

export class CapabilityMatrixEditView extends React.Component<any, any> {

    private country: ComboBoxField;

    private wg: CapabilityMatrixMultiSelect;

    private avail: CapabilityMatrixMultiSelect;

    private dur: CapabilityMatrixMultiSelect;

    private reacttype: CapabilityMatrixMultiSelect;

    private reacttime: CapabilityMatrixMultiSelect;

    private srvloc: CapabilityMatrixMultiSelect;

    private globPort: CheckBoxField;

    private masterPort: CheckBoxField;

    private corePort: CheckBoxField;

    constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {

        return (
            <Container layout="vbox" padding="10px">

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

                <Container layout="hbox">
                    <CapabilityMatrixMultiSelect ref="wg" maxHeight={selectMaxH} title="Asset(WG)" itemTpl="{name}" store={this.state.warrantyGroups} />
                    <CapabilityMatrixMultiSelect ref="availability" maxHeight={selectMaxH} title="Availability" itemTpl="{name}" store={this.state.availabilityTypes} />
                    <CapabilityMatrixMultiSelect ref="duration" maxHeight={selectMaxH} title="Duration" itemTpl="{name}" store={this.state.durationTypes} />
                    <CapabilityMatrixMultiSelect ref="reactType" maxHeight={selectMaxH} title="React type" itemTpl="{name}" store={this.state.reactTypes} />
                    <CapabilityMatrixMultiSelect ref="reactTime" maxHeight={selectMaxH} title="Reaction time" itemTpl="{name}" store={this.state.reactionTimeTypes} />
                    <CapabilityMatrixMultiSelect ref="srvLoc" maxHeight={selectMaxH} title="Service location" itemTpl="{name}" store={this.state.serviceLocationTypes} />
                </Container>

                <Container layout={{ type: 'vbox', align: 'left' }} defaults={{ disabled: !this.state.isPortfolio }}>
                    <CheckBoxField ref="globPort" boxLabel="Fujitsu Global Portfolio" />
                    <CheckBoxField ref="masterPort" boxLabel="Master Portfolio" />
                    <CheckBoxField ref="corePort" boxLabel="Core Portfolio" />
                </Container>

                <Container>
                    <Button text="Deny combinations" ui="decline" padding="0 10px 0 0" handler={this.onDeny} />
                    <Button text="Allow combinations" handler={this.onAllow} />
                </Container>

            </Container>
        );
    }

    public componentDidMount() {
        Promise.all([
            srv.getCountries(),
            srv.getWG(),
            srv.getAvailabilityTypes(),
            srv.getDurationTypes(),
            srv.getReactTypes(),
            srv.getReactionTimeTypes(),
            srv.getServiceLocationTypes()
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
        this.wg = this.refs['wg'] as CapabilityMatrixMultiSelect;
        this.avail = this.refs['availability'] as CapabilityMatrixMultiSelect;
        this.dur = this.refs['duration'] as CapabilityMatrixMultiSelect;
        this.reacttype = this.refs['reactType'] as CapabilityMatrixMultiSelect;
        this.reacttime = this.refs['reactTime'] as CapabilityMatrixMultiSelect;
        this.srvloc = this.refs['srvLoc'] as CapabilityMatrixMultiSelect;
        this.globPort = this.refs['globPort'] as CheckBoxField;
        this.masterPort = this.refs['masterPort'] as CheckBoxField;
        this.corePort = this.refs['corePort'] as CheckBoxField;
    }

    private init() {
        this.onCountryChange = this.onCountryChange.bind(this);
        this.onAllow = this.onAllow.bind(this);
        this.onDeny = this.onDeny.bind(this);
        this.allowCombination = this.allowCombination.bind(this);
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

    private onAllow() {
        this.showSaveDialog('Allow combinations', this.allowCombination);
    }

    private onDeny() {
        this.showSaveDialog('Deny combinations', this.denyCombination);
    }

    private showSaveDialog(title: string, ok: Function) {
        ExtMsgHelper.confirm(title, 'Do you want to save the changes?', ok);
    }

    private allowCombination() {
        console.log('allowCombination()', this.getModel());
    }

    private denyCombination() {
        console.log('denyCombination()', this.getModel());
    }

    private setPortfolio(val: boolean) {
        this.setState({ isPortfolio: val });
    }

    private getModel() {

        return {
            country: this.getCountry(),

            isGlobalPortfolio: this.isGlobalPortfolio(),
            isMasterPortfolio: this.isMasterPortfolio(),
            isCorePortfolio: this.isCorePortfolio(),

            wg: this.getWg(),
            avail: this.getAvailability(),
            dur: this.getDuration(),
            reactType: this.getReactType(),
            reactTime: this.getReactTime(),
            srvLoc: this.getServiceLocation()
        }
    }

    private getWg(): NamedId[] {
        return this.wg.getSelected();
    }

    private getAvailability(): NamedId[] {
        return this.avail.getSelected();
    }

    private getDuration(): NamedId[] {
        return this.dur.getSelected();
    }

    private getReactType(): NamedId[] {
        return this.reacttype.getSelected();
    }

    private getReactTime(): NamedId[] {
        return this.reacttime.getSelected();
    }

    private getServiceLocation(): NamedId[] {
        return this.srvloc.getSelected();
    }

    private getCountry(): NamedId {
        let result: NamedId = null;
        let selected = this.country.getSelection();
        if (selected) {
            result = selected.data;
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
        return this.state.isPortfolio ? cb.getChecked() : false;
    }
}