import { Button, CheckBoxField, Container } from "@extjs/ext-react";
import * as React from "react";
import { ExtMsgHelper } from "../Common/Helpers/ExtMsgHelper";
import { handleRequest } from "../Common/Helpers/RequestHelper";
import { buildComponentUrl } from "../Common/Services/Ajax";
import { CountryField } from "../Dict/Components/CountryField";
import { DictField } from "../Dict/Components/DictField";
import { DictFactory } from "../Dict/Services/DictFactory";
import { IDictService } from "../Dict/Services/IDictService";
import { MultiSelect } from "./Components/MultiSelect";
import { MultiSelectWg } from "./Components/MultiSelectWg";
import { PortfolioEditModel } from "./Model/PortfolioEditModel";
import { IPortfolioService } from "./Services/IPortfolioService";
import { MatrixFactory } from "./Services/PortfolioServiceFactory";

const SELECT_MAX_HEIGHT: string = '260px';

export class PortfolioEditView extends React.Component<any, any> {

    private country: DictField;

    private wg: MultiSelect;

    private av: MultiSelect;

    private dur: MultiSelect;

    private reacttype: MultiSelect;

    private reacttime: MultiSelect;

    private srvloc: MultiSelect;

    private globPort: CheckBoxField & any;

    private masterPort: CheckBoxField & any;

    private corePort: CheckBoxField & any;

    private srv: IPortfolioService;

    private dictSrv: IDictService;

    public state = {
        isPortfolio: true
    };

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        return (
            <Container layout="vbox" padding="10px" scrollable="true">

                <CountryField
                    ref={x => this.country = x}
                    width="250px"
                    label="Country:"
                    labelAlign="left"
                    labelWidth="80px"
                    clearable="true"
                    onChange={this.onCountryChange}
                />

                <div className="matrix-edit-container">
                    <div>
                        <MultiSelectWg ref={x => this.wg = x} maxHeight="204px" title="Asset(WG)" store={this.dictSrv.getWG} />
                    </div>
                    <div>
                        <MultiSelect ref={x => this.av = x} maxHeight={SELECT_MAX_HEIGHT} title="Availability" store={this.dictSrv.getAvailabilityTypes} />
                    </div>
                    <div>
                        <MultiSelect ref={x => this.dur = x} maxHeight={SELECT_MAX_HEIGHT} title="Duration" store={this.dictSrv.getDurationTypes} />
                    </div>
                    <div>
                        <MultiSelect ref={x => this.reacttype = x} maxHeight={SELECT_MAX_HEIGHT} title="Reaction type" store={this.dictSrv.getReactionTypes} />
                    </div>
                    <div>
                        <MultiSelect ref={x => this.reacttime = x} maxHeight={SELECT_MAX_HEIGHT} title="Reaction time" store={this.dictSrv.getReactionTimeTypes} />
                    </div>
                    <div>
                        <MultiSelect ref={x => this.srvloc = x} maxHeight={SELECT_MAX_HEIGHT} title="Service location" store={this.dictSrv.getServiceLocationTypes} />
                    </div>
                </div>

                <Container layout={{ type: 'vbox', align: 'left' }} defaults={{ disabled: !this.state.isPortfolio }} margin="15px 0">
                    <CheckBoxField ref={x => this.globPort = x} boxLabel="Fujitsu global portfolio" />
                    <CheckBoxField ref={x => this.masterPort = x} boxLabel="Master portfolio" />
                    <CheckBoxField ref={x => this.corePort = x} boxLabel="Core portfolio" />
                </Container>

                <Container>
                    <Button iconCls="x-fa fa-arrow-left" text="back to Portfolio" handler={this.onBack} />
                    <Button text="Deny combinations" ui="decline" padding="0 10px 0 0" handler={this.onDeny} />
                </Container>

            </Container>
        );
    }

    private init() {
        this.srv = MatrixFactory.getMatrixService();
        this.dictSrv = DictFactory.getDictService();
        //
        this.onCountryChange = this.onCountryChange.bind(this);
        this.onDeny = this.onDeny.bind(this);
        this.onBack = this.onBack.bind(this);
        this.denyCombination = this.denyCombination.bind(this);
    }

    private onCountryChange(combo, newVal, oldVal) {
        this.setPortfolio(!newVal);
    }

    private onDeny() {
        let m = this.getModel();
        let isValid = m.countryId || m.isGlobalPortfolio || m.isMasterPortfolio || m.isCorePortfolio;

        if (isValid) {
            ExtMsgHelper.confirm('Deny combinations', 'Do you want to save the changes?', this.denyCombination);
        }
        else {
            Ext.Msg.alert('Invalid input!', 'Please choose master or local portfolio!');
        }
    }

    private onBack() {
        this.props.history.push(buildComponentUrl('/capability-matrix'));
    }

    private denyCombination() {
        let p = this.srv.denyItem(this.getModel());
        handleRequest(p).then(() => this.reset());
    }

    private setPortfolio(val: boolean) {
        this.setState({ isPortfolio: val });
    }

    private getModel(): PortfolioEditModel {
        return {
            isGlobalPortfolio: this.isGlobalPortfolio(),
            isMasterPortfolio: this.isMasterPortfolio(),
            isCorePortfolio: this.isCorePortfolio(),

            countryId: this.country.getSelected(),

            wgs: this.wg.getSelectedKeys(),
            availabilities: this.av.getSelectedKeys(),
            durations: this.dur.getSelectedKeys(),
            reactionTypes: this.reacttype.getSelectedKeys(),
            reactionTimes: this.reacttime.getSelectedKeys(),
            serviceLocations: this.srvloc.getSelectedKeys()
        }
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

    private reset() {
        this.country.reset();
        this.wg.reset();
        this.av.reset();
        this.dur.reset();
        this.reacttype.reset();
        this.reacttime.reset();
        this.srvloc.reset();
        //
        this.globPort.reset();
        this.masterPort.reset();
        this.corePort.reset();
    }
}