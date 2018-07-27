import * as React from "react";
import { Container, Button, CheckBoxField, ComboBoxField, Checkbox } from "@extjs/ext-react";
import { MultiSelect } from "./Components/MultiSelect";
import { ExtMsgHelper } from "../Common/Helpers/ExtMsgHelper";
import { NamedId } from "../Common/States/CommonStates";
import { ICapabilityMatrixService } from "./Services/ICapabilityMatrixService"
import { MatrixFactory } from "./Services/MatrixFactory";

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