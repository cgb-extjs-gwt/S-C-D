import { Button, CheckBoxField, Container, Toolbar } from "@extjs/ext-react";
import * as React from "react";
import { ExtMsgHelper } from "../Common/Helpers/ExtMsgHelper";
import { handleRequest } from "../Common/Helpers/RequestHelper";
import { buildComponentUrl } from "../Common/Services/Ajax";
import { MultiSelect } from "../Dict/Components/MultiSelect";
import { MultiSelectProActive } from "../Dict/Components/MultiSelectProActive";
import { MultiSelectWg } from "../Dict/Components/MultiSelectWg";
import { DictFactory } from "../Dict/Services/DictFactory";
import { IDictService } from "../Dict/Services/IDictService";
import { UserCountryService } from "../Dict/Services/UserCountryService";
import { PortfolioEditModel } from "./Model/PortfolioEditModel";
import { IPortfolioService } from "./Services/IPortfolioService";
import { PortfolioServiceFactory } from "./Services/PortfolioServiceFactory";

const SELECT_MAX_HEIGHT: string = '260px';

export class PortfolioEditView extends React.Component<any, any> {

    private country: MultiSelect;

    private wg: MultiSelect;

    private av: MultiSelect;

    private dur: MultiSelect;

    private reacttype: MultiSelect;

    private reacttime: MultiSelect;

    private srvloc: MultiSelect;

    private proactive: MultiSelect;

    private globPort: CheckBoxField & any;

    private masterPort: CheckBoxField & any;

    private corePort: CheckBoxField & any;

    private srv: IPortfolioService;

    private dictSrv: IDictService;

    public state = {
        isPortfolio: true,
        isCountryUser: true
    };

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        return (
            <Container layout="vbox" padding="10px" scrollable="true">

                <Toolbar docked="top">
                    <Button iconCls="x-fa fa-arrow-left" text="back to Portfolio" handler={this.onBack} />
                    <Button text="Deny combinations" ui="decline" padding="0 10px 0 0" handler={this.onDeny} />
                    <Button text="Allow combinations" padding="0 10px 0 0" handler={this.onAllow} />
                    <Button iconCls="x-fa fa-history" text="History" ui="forward" handler={this.onViewHistory} />
                </Toolbar>

                <div className="portfolio-edit-container">
                    <div>
                        <MultiSelect ref={x => this.country = x} maxHeight={SELECT_MAX_HEIGHT} title="Country" store={this.countryStore} onSelectionChange={this.onCountryChange} />
                    </div>
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
                    <div>
                        <MultiSelectProActive ref={x => this.proactive = x} maxHeight={SELECT_MAX_HEIGHT} title="ProActive" store={this.dictSrv.getProActive} />
                    </div>
                </div>

                <Container layout={{ type: 'vbox', align: 'left' }} defaults={{ disabled: !this.state.isPortfolio, hidden: this.state.isCountryUser }} margin="15px 0">
                    <CheckBoxField ref={x => this.globPort = x} boxLabel="Fujitsu principal portfolio" />
                    <CheckBoxField ref={x => this.masterPort = x} boxLabel="Master portfolio" />
                    <CheckBoxField ref={x => this.corePort = x} boxLabel="Core portfolio" />
                </Container>

            </Container>
        );
    }

    private init() {
        this.srv = PortfolioServiceFactory.getPortfolioService();
        this.dictSrv = DictFactory.getDictService();
        //
        this.countryStore = this.countryStore.bind(this);
        this.onCountryChange = this.onCountryChange.bind(this);
        this.onAllow = this.onAllow.bind(this);
        this.onDeny = this.onDeny.bind(this);
        this.onBack = this.onBack.bind(this);
        this.save = this.save.bind(this);
        this.onViewHistory = this.onViewHistory.bind(this);

        const srv = new UserCountryService();
        srv.isCountryUser().then(x => this.setState({ isCountryUser: x }));
    }

    private countryStore() {
        return this.dictSrv.getUserCountries(true);
    }

    private onCountryChange(view: any, rows: any[]) {
        this.setState({ isPortfolio: rows.length === 0 });
    }

    private onAllow() {
        this.showChangeDialog(false);
    }

    private onDeny() {
        this.showChangeDialog(true);
    }

    private onBack() {
        this.openLink('/portfolio');
    }

    private onViewHistory() {
        this.openLink('/portfolio/history');
    }

    private openLink(url: string) {
        this.props.history.push(buildComponentUrl(url));
    }

    private showChangeDialog(deny: boolean) {
        if (this.getModel().isValid()) {
            let msg = deny ? 'Deny combinations' : 'Allow combinations';
            ExtMsgHelper.confirm(msg, 'Do you want to save the changes?', () => this.save(deny));
        }
        else {
            Ext.Msg.alert('Invalid input!', 'Please choose master or local portfolio and SLA!');
        }
    }

    private save(deny: boolean) {
        let m = this.getModel();
        if (m.isValid()) {
            let p = deny ? this.srv.deny(m) : this.srv.allow(m);
            handleRequest(p).then(() => this.reset());
        }
    }

    private getModel(): PortfolioEditModel {
        let m = new PortfolioEditModel();

        m.isGlobalPortfolio = this.isGlobalPortfolio();
        m.isMasterPortfolio = this.isMasterPortfolio();
        m.isCorePortfolio = this.isCorePortfolio();

        m.countries = this.country.getSelectedKeys();

        m.wgs = this.wg.getSelectedKeys();
        m.availabilities = this.av.getSelectedKeys();
        m.durations = this.dur.getSelectedKeys();
        m.reactionTypes = this.reacttype.getSelectedKeys();
        m.reactionTimes = this.reacttime.getSelectedKeys();
        m.serviceLocations = this.srvloc.getSelectedKeys();
        m.proActives = this.proactive.getSelectedKeys()

        return m;
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
        this.proactive.reset();
        //
        this.globPort.reset();
        this.masterPort.reset();
        this.corePort.reset();
    }
}