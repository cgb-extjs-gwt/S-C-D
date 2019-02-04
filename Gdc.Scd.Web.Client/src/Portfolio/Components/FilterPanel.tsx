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
import { NamedId, SortableNamedId } from "../../Common/States/CommonStates";
import { DictFactory } from "../../Dict/Services/DictFactory";
import { IDictService } from "../../Dict/Services/IDictService";
import { MultiSelect } from "./MultiSelect";
import { MultiSelectWg } from "./MultiSelectWg";
import { MultiSelectProActive } from "./MultiSelectProActive";

const SELECT_MAX_HEIGHT: string = '200px';


export interface FilterPanelProps extends PanelProps {
    isCountryUser: boolean;
    onSearch(filter: PortfolioFilterModel): void;
}

export class FilterPanel extends React.Component<FilterPanelProps, any> {

    private country: MultiSelect;

    private wg: MultiSelect;

    private av: MultiSelect;

    private dur: MultiSelect;

    private reacttype: MultiSelect;

    private reacttime: MultiSelect;

    private srvloc: MultiSelect;

    private proactive: MultiSelect;

    private globPort: CheckBoxField;

    private masterPort: CheckBoxField;

    private corePort: CheckBoxField;

    private dictSrv: IDictService;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {

        return (
            <Container margin="10px 0"
                defaults={{
                    maxWidth: '200px',
                    width: '200px',
                    valueField: 'id',
                    displayField: 'name',
                    queryMode: 'local',
                    clearable: 'true'
                }}
            >           
                <Panel {...this.props} margin="0 0 5px 0" padding="4px 20px 7px 20px">

                <Container margin="10px 0"
                    defaults={{
                        maxWidth: '200px',
                        width: '200px',
                        valueField: 'id',
                        displayField: 'name',
                        queryMode: 'local',
                        clearable: 'true'              
                    }}
                >            
                    <MultiSelect ref={x => this.country = x} width='200px' maxHeight={SELECT_MAX_HEIGHT} title="Country" store={this.dictSrv.getUserCountryNames} />
                    <MultiSelectWg ref={x => this.wg = x} width='200px' maxHeight="204px" title="Asset(WG)" store={this.dictSrv.getWG} />
                    <MultiSelect ref={x => this.av = x} width='200px' maxHeight={SELECT_MAX_HEIGHT} title="Availability" store={this.dictSrv.getAvailabilityTypes} />
                    <MultiSelect ref={x => this.dur = x} width='200px' maxHeight={SELECT_MAX_HEIGHT} title="Duration" store={this.dictSrv.getDurationTypes} />
                    <MultiSelect ref={x => this.reacttype = x} width='200px' maxHeight={SELECT_MAX_HEIGHT} title="Reaction type" store={this.dictSrv.getReactionTypes} />
                    <MultiSelect ref={x => this.reacttime = x} width='200px' maxHeight={SELECT_MAX_HEIGHT} title="Reaction time" store={this.dictSrv.getReactionTimeTypes} />
                    <MultiSelect ref={x => this.srvloc = x} width='200px' maxHeight={SELECT_MAX_HEIGHT} title="Service location" store={this.dictSrv.getServiceLocationTypes} />
                    <MultiSelectProActive ref={x => this.proactive = x} width='200px' maxHeight={SELECT_MAX_HEIGHT} title="ProActive" store={this.dictSrv.getProActive} />

                </Container>

                <Container layout={{ type: 'vbox', align: 'left' }} defaults={{ disabled: !this.state.isPortfolio, padding: '3px 0', hidden: this.props.isCountryUser }}>
                    <CheckBoxField ref={x => this.globPort = x} boxLabel="Fujitsu principal portfolio" />
                    <CheckBoxField ref={x => this.masterPort = x} boxLabel="Master portfolio" />
                    <CheckBoxField ref={x => this.corePort = x} boxLabel="Core portfolio" />
                </Container>

               

            </Panel>    
            <Panel {...this.props} margin="0 0 5px 0" padding="4px 20px 7px 20px">
                <Button text="Search" ui="action" minWidth="85px" handler={this.onSearch} margin="20px auto" />
            </Panel>
             </Container >
        );
    }

    public getModel() {
        let m = new PortfolioFilterModel();

        m.country = this.country.getSelectedKeys();
        m.wg = this.wg.getSelectedKeys();
        m.availability = this.av.getSelectedKeys();
        m.duration = this.dur.getSelectedKeys();
        m.reactionType = this.reacttype.getSelectedKeys();
        m.reactionTime = this.reacttime.getSelectedKeys();
        m.serviceLocation = this.srvloc.getSelectedKeys();
        m.proActive = this.proactive.getSelectedKeys();

        m.isGlobalPortfolio= this.getChecked(this.globPort);
        m.isMasterPortfolio= this.getChecked(this.masterPort);
        m.isCorePortfolio = this.getChecked(this.corePort);

        return m;
    }

    private init() {
        this.dictSrv = DictFactory.getDictService();
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