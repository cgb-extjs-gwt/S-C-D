import { Button, Container, Panel, PanelProps, RadioField } from "@extjs/ext-react";
import * as React from "react";
import { NamedId, SortableNamedId } from "../../Common/States/CommonStates";
import { AvailabilityField } from "../../Dict/Components/AvailabilityField";
import { CountryField } from "../../Dict/Components/CountryField";
import { DictField } from "../../Dict/Components/DictField";
import { DurationField } from "../../Dict/Components/DurationField";
import { ProActiveField } from "../../Dict/Components/ProActiveField";
import { ReactionTimeField } from "../../Dict/Components/ReactionTimeField";
import { ReactionTypeField } from "../../Dict/Components/ReactionTypeField";
import { ServiceLocationField } from "../../Dict/Components/ServiceLocationField";
import { StandardWgField } from "../../Dict/Components/StandardWgField";
import { UserCountryField } from "../../Dict/Components/UserCountryField";
import { Country } from "../../Dict/Model/Country";
import { CurrencyType } from "../Model/CurrencyType";
import { HwCostFilterModel } from "../Model/HwCostFilterModel";
import { DictFactory } from "../../Dict/Services/DictFactory";
import { IDictService } from "../../Dict/Services/IDictService";
import { MultiSelect } from "../../Dict/Components/MultiSelect";
import { MultiSelectWg } from "../../Dict/Components/MultiSelectWg";
import { MultiSelectProActive } from "../../Dict/Components/MultiSelectProActive";

const SELECT_MAX_HEIGHT: string = '200px';

export interface FilterPanelProps extends PanelProps {
    onSearch(filter: HwCostFilterModel): void;
    onChange(filter: HwCostFilterModel): void;
    onDownload(filter: HwCostFilterModel): void;
}

export class HwCostFilter extends React.Component<FilterPanelProps, any> {

    private cnt: MultiSelect;

    private wg: MultiSelect;

    private av: MultiSelect;

    private dur: MultiSelect;

    private reacttype: MultiSelect;

    private reacttime: MultiSelect;

    private srvloc: MultiSelect;

    private proactive: MultiSelect;

    private localCur: RadioField & any;

    private euroCur: RadioField & any;

    private dictSrv: IDictService;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
        let valid = this.state && this.state.valid;

        let countryField;

        if (this.props.checkAccess) {
            countryField = <MultiSelect ref={x => this.cnt = x} width='200px' maxHeight={SELECT_MAX_HEIGHT} title="Country" store={this.dictSrv.getUserCountryNames} onselect={this.onCountryChange}/>
        }
        else {
            countryField = <MultiSelect ref={x => this.cnt = x} width='200px' maxHeight={SELECT_MAX_HEIGHT} title="Country" store={this.dictSrv.getMasterCountriesNames} onselect={this.onCountryChange}/>;
        }

        return (
            <Panel {...this.props} margin="0 0 5px 0" padding="4px 20px 7px 20px" layout={{ type: 'vbox', align: 'left' }}>

                <Container margin="10px 0"
                    defaults={{
                        maxWidth: '200px',
                        valueField: 'id',
                        displayField: 'name',
                        queryMode: 'local',
                        clearable: 'true'
                    }}
                >

                    {countryField}
                    <MultiSelectWg ref={x => this.wg = x} width='200px' maxHeight={SELECT_MAX_HEIGHT} title="Asset(WG)" store={this.dictSrv.getWG} />
                    <MultiSelect ref={x => this.av = x} width='200px' maxHeight={SELECT_MAX_HEIGHT} title="Availability" store={this.dictSrv.getAvailabilityTypes} />
                    <MultiSelect ref={x => this.dur = x} width='200px' maxHeight={SELECT_MAX_HEIGHT} title="Duration" store={this.dictSrv.getDurationTypes} />
                    <MultiSelect ref={x => this.reacttype = x} width='200px' maxHeight={SELECT_MAX_HEIGHT} title="Reaction type" store={this.dictSrv.getReactionTypes} />
                    <MultiSelect ref={x => this.reacttime = x} width='200px' maxHeight={SELECT_MAX_HEIGHT} title="Reaction time" store={this.dictSrv.getReactionTimeTypes} />
                    <MultiSelect ref={x => this.srvloc = x} width='200px' maxHeight={SELECT_MAX_HEIGHT} title="Service location" store={this.dictSrv.getServiceLocationTypes} />
                    <MultiSelectProActive ref={x => this.proactive = x} width='200px' maxHeight={SELECT_MAX_HEIGHT} title="ProActive" store={this.dictSrv.getProActive} />

                </Container>

                <Container layout={{ type: 'vbox', align: 'left' }} margin="5px 0 0 0" defaults={{ padding: '3px 0' }} >
                    <RadioField ref={x => this.localCur = x} name="currency" boxLabel="Show in local currency" checked onCheck={this.onChange} />
                    <RadioField ref={x => this.euroCur = x} name="currency" boxLabel="Show in EUR" onCheck={this.onChange} />
                </Container>

                <Button text="Search" ui="action" minWidth="85px" margin="20px auto" disabled={!valid} handler={this.onSearch} />

                <Button text="Download" ui="action" minWidth="85px" iconCls="x-fa fa-download" disabled={!valid} handler={this.onDownload} />

            </Panel>
        );
    }

    public getModel(): HwCostFilterModel {
        let m = new HwCostFilterModel();

        m.country = this.cnt.getSelectedKeys();
        m.wg = this.wg.getSelectedKeys();
        m.availability = this.av.getSelectedKeys();
        m.duration = this.dur.getSelectedKeys();
        m.reactionType = this.reacttype.getSelectedKeys();
        m.reactionTime = this.reacttime.getSelectedKeys();
        m.serviceLocation = this.srvloc.getSelectedKeys();
        m.proActive = this.proactive.getSelectedKeys();
        m.currency = this.getCurrency();

        return m;
    }

    public getCountry(): any {
        return this.cnt.getSelectedKeys();
    }

    private init() {
        this.dictSrv = DictFactory.getDictService();
        this.onCountryChange = this.onCountryChange.bind(this);
        this.onSearch = this.onSearch.bind(this);
        this.onChange = this.onChange.bind(this);
        this.onDownload = this.onDownload.bind(this);
    }

    private onCountryChange(field, records) {
        this.setState({ valid: !!this.cnt.getSelected() });
    }

    private onSearch() {
        let handler = this.props.onSearch;
        if (handler) {
            handler(this.getModel());
        }
    }

    private onChange() {
        let handler = this.props.onChange;
        if (handler) {
            handler(this.getModel());
        }
    }

    private onDownload() {
        let handler = this.props.onDownload;
        if (handler) {
            handler(this.getModel());
        }
    }

    private getCurrency(): CurrencyType {
        return this.euroCur.getChecked() ? CurrencyType.Euro : CurrencyType.Local
    }
}