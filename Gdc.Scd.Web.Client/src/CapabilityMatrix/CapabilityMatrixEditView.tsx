import * as React from "react";
import { Container, Button, CheckBoxField, ComboBoxField } from "@extjs/ext-react";
import { MultiSelect } from "./Components/MultiSelect";
import { CapabilityMatrixEditModel } from "./Model/CapabilityMatrixEditModel";
import { ICapabilityMatrixService } from "./Services/ICapabilityMatrixService"
import { MatrixFactory } from "./Services/MatrixFactory";
import { ExtMsgHelper } from "../Common/Helpers/ExtMsgHelper";
import { ArrayHelper } from "../Common/Helpers/ArrayHelper";

const selectMaxH: string = '260px';

export class CapabilityMatrixEditView extends React.Component<any, any> {

    private country: ComboBoxField;

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
                    <MultiSelect ref="wg" maxHeight={selectMaxH} title="Asset(WG)" itemTpl="{name}" store={this.state.warrantyGroups} />
                    <MultiSelect ref="availability" maxHeight={selectMaxH} title="Availability" itemTpl="{name}" store={this.state.availabilityTypes} />
                    <MultiSelect ref="duration" maxHeight={selectMaxH} title="Duration" itemTpl="{name}" store={this.state.durationTypes} />
                    <MultiSelect ref="reactType" maxHeight={selectMaxH} title="React type" itemTpl="{name}" store={this.state.reactTypes} />
                    <MultiSelect ref="reactTime" maxHeight={selectMaxH} title="Reaction time" itemTpl="{name}" store={this.state.reactionTimeTypes} />
                    <MultiSelect ref="srvLoc" maxHeight={selectMaxH} title="Service location" itemTpl="{name}" store={this.state.serviceLocationTypes} />
                </Container>

                <Container layout={{ type: 'vbox', align: 'left' }} defaults={{ disabled: !this.state.isPortfolio }}>
                    <CheckBoxField ref="globPort" boxLabel="Fujitsu global portfolio" />
                    <CheckBoxField ref="masterPort" boxLabel="Master portfolio" />
                    <CheckBoxField ref="corePort" boxLabel="Core portfolio" />
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

    private getModel(): CapabilityMatrixEditModel {
        return {
            isGlobalPortfolio: this.isGlobalPortfolio(),
            isMasterPortfolio: this.isMasterPortfolio(),
            isCorePortfolio: this.isCorePortfolio(),

            countryId: this.getCountry(),

            wgId: this.getWg(),
            availabilityId: this.getAvailability(),
            durationId: this.getDuration(),
            reactTypeId: this.getReactType(),
            reactionTimeId: this.getReactTime(),
            serviceLocationId: this.getServiceLocation()
        }
    }

    private getWg(): string {
        return ArrayHelper.firstOrDefault(this.wg.getSelectedKeys('id'));
    }

    private getAvailability(): string {
        return ArrayHelper.firstOrDefault(this.avail.getSelectedKeys('id'));
    }

    private getDuration(): string {
        return ArrayHelper.firstOrDefault(this.dur.getSelectedKeys('id'));
    }

    private getReactType(): string {
        return ArrayHelper.firstOrDefault(this.reacttype.getSelectedKeys('id'));
    }

    private getReactTime(): string {
        return ArrayHelper.firstOrDefault(this.reacttime.getSelectedKeys('id'));
    }

    private getServiceLocation(): string {
        return ArrayHelper.firstOrDefault(this.srvloc.getSelectedKeys('id'));
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
        return this.state.isPortfolio ? cb.getChecked() : false;
    }
}