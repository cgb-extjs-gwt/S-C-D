import { Container, Panel, PanelProps, RadioField, SearchField } from "@extjs/ext-react";
import * as React from "react";
import { CountryField } from "../../Dict/Components/CountryField";
import { MultiSelect } from "../../Dict/Components/MultiSelect";
import { MultiSelectProActive } from "../../Dict/Components/MultiSelectProActive";
import { MultiSelectWg } from "../../Dict/Components/MultiSelectWg";
import { UserCountryField } from "../../Dict/Components/UserCountryField";
import { Country } from "../../Dict/Model/Country";
import { DictFactory } from "../../Dict/Services/DictFactory";
import { IDictService } from "../../Dict/Services/IDictService";
import { CurrencyType } from "../Model/CurrencyType";
import { HwCostFilterModel } from "../Model/HwCostFilterModel";

Ext.require('Ext.panel.Collapser');

const SELECT_MAX_HEIGHT: string = '200px';

export interface FilterPanelProps extends PanelProps {
    onChange(filter: HwCostFilterModel): void;
}

export class HwCostFilter extends React.Component<FilterPanelProps, any> {

    private cnt: CountryField;

    private fsp: SearchField & any;

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
        let countryField;

        let multiProps = {
            width: '200px',
            maxHeight: SELECT_MAX_HEIGHT,
            title: "",
            hideCheckbox: true
        };
        let panelProps = {
            width: '300px',
            collapsible: {
                direction: 'top',
                dynamic: true,
                collapsed: true
            },
            userCls: 'multiselect-filter',
            margin: "0 0 2px 0"
        };

        if (this.props.checkAccess) {
            countryField = <UserCountryField ref={x => this.cnt = x} margin="5px 5px 15px 15px" label="Country:" cache={false} onChange={this.onChange} />;
        }
        else {
            countryField = <CountryField ref={x => this.cnt = x} margin="5px 5px 15px 15px" label="Country:" cache={false} onChange={this.onChange} />;
        }

        return (
            <Panel {...this.props} margin="0 0 5px 0" padding="5px 5px 5px 5px" layout={{ type: 'vbox', align: 'left' }}>

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

                    <SearchField ref={x => this.fsp = x} label="FSP" placeholder="Search by FSP..." />

                    <Panel title='Asset(WG)'
                        {...panelProps}>
                        <MultiSelectWg ref={x => this.wg = x} {...multiProps} store={this.dictSrv.getStandardWg} />
                    </Panel>
                    <Panel title='Availability'
                        {...panelProps}>
                        <MultiSelect ref={x => this.av = x} {...multiProps} store={this.dictSrv.getAvailabilityTypes} />
                    </Panel>
                    <Panel title='Duration'
                        {...panelProps}>
                        <MultiSelect ref={x => this.dur = x} {...multiProps} store={this.dictSrv.getDurationTypes} />
                    </Panel>
                    <Panel title='Reaction type'
                        {...panelProps}>
                        <MultiSelect ref={x => this.reacttype = x} {...multiProps} store={this.dictSrv.getReactionTypes} />
                    </Panel>
                    <Panel title='Reaction time'
                        {...panelProps}>
                        <MultiSelect ref={x => this.reacttime = x} {...multiProps} store={this.dictSrv.getReactionTimeTypes} />
                    </Panel>
                    <Panel title='Service location'
                        {...panelProps}>
                        <MultiSelect ref={x => this.srvloc = x} {...multiProps} store={this.dictSrv.getServiceLocationTypes} />
                    </Panel>
                    <Panel title='ProActive'
                        {...panelProps}>
                        <MultiSelectProActive ref={x => this.proactive = x} {...multiProps} store={this.dictSrv.getProActive} value="0" />
                    </Panel>
                </Container>

                <Container layout={{ type: 'vbox', align: 'left' }} margin="5px 0 0 0" defaults={{ padding: '3px 0' }} >
                    <RadioField ref={x => this.localCur = x} name="currency" boxLabel="Show in local currency" checked onCheck={this.onChange} />
                    <RadioField ref={x => this.euroCur = x} name="currency" boxLabel="Show in EUR" onCheck={this.onChange} />
                </Container>

            </Panel>
        );
    }

    public getModel(): HwCostFilterModel {
        let cnt = this.cnt.getSelected();
        return {
            country: cnt ? [cnt] : null,
            wg: this.wg.getSelectedKeysOrNull(),
            availability: this.av.getSelectedKeysOrNull(),
            duration: this.dur.getSelectedKeysOrNull(),
            reactionType: this.reacttype.getSelectedKeysOrNull(),
            reactionTime: this.reacttime.getSelectedKeysOrNull(),
            serviceLocation: this.srvloc.getSelectedKeysOrNull(),
            proActive: this.proactive.getSelectedKeysOrNull(),
            currency: this.getCurrency(),
            fsp: this.fsp.getValue()
        }
    }

    public getCountry(): Country {
        return this.cnt.getSelectedModel();
    }

    private init() {
        this.dictSrv = DictFactory.getDictService();
        this.onChange = this.onChange.bind(this);
    }

    private onChange() {
        let handler = this.props.onChange;
        if (handler) {
            handler(this.getModel());
        }
    }

    private getCurrency(): CurrencyType {
        return this.euroCur.getChecked() ? CurrencyType.Euro : CurrencyType.Local
    }
}