import { Button, CheckBoxField, Container, Panel, PanelProps } from "@extjs/ext-react";
import * as React from "react";
import { PortfolioFilterModel } from "../Model/PortfolioFilterModel";
import { NamedId, SortableNamedId } from "../../Common/States/CommonStates";
import { DictFactory } from "../../Dict/Services/DictFactory";
import { IDictService } from "../../Dict/Services/IDictService";
import { MultiSelect } from "../../Dict/Components/MultiSelect";
import { MultiSelectField } from "../../Dict/Components/MultiSelectField";
import { MultiSelectWg } from "../../Dict/Components/MultiSelectWg";
import { MultiSelectProActive } from "../../Dict/Components/MultiSelectProActive";

Ext.require('Ext.panel.Collapser');

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
        let multiProps = {
            width: '200px',
            maxHeight: SELECT_MAX_HEIGHT,
            headerCheckboxHidden: false,
            title: ' ',
            hideCheckbox: true
        };
        let panelProps = {
            collapsible: {
                direction: 'top',
                dynamic: true,
                collapsed: true
            },
            userCls: 'multiselect-filter',
            margin: "0 0 2px 0"
        };
        return (
            <Container layout="vbox" height="100%" docked="right">     
                <Panel {...this.props}
                    layout={{ type: 'vbox', align: 'left' }}
                    height="90%"
                    margin="0 0 5px 0"
                    padding="4px 20px 7px 20px"
                    scrollable={true}>
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

                        <MultiSelectField ref={x => this.country = x} {...multiProps} hideCheckbox={false} store={this.dictSrv.getUserCountryNames} label='Country'/>
                        <Panel title='Asset(WG)'
                            {...panelProps}>
                            <MultiSelectWg ref={x => this.wg = x} {...multiProps} store={this.dictSrv.getWG} />
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
                            <MultiSelectProActive ref={x => this.proactive = x} {...multiProps} store={this.dictSrv.getProActive} />
                        </Panel>
                    </Container>
                    <Container layout={{ type: 'vbox', align: 'left' }} defaults={{ disabled: !this.state.isPortfolio, padding: '3px 0', hidden: this.props.isCountryUser }}>
                        <CheckBoxField ref={x => this.globPort = x} boxLabel="Fujitsu principal portfolio" />
                        <CheckBoxField ref={x => this.masterPort = x} boxLabel="Master portfolio" />
                        <CheckBoxField ref={x => this.corePort = x} boxLabel="Core portfolio" />
                    </Container>              
                    <Button text="Search" ui="action" minWidth="85px" handler={this.onSearch} margin="20px auto" />
                </Panel>       
            </Container>
        );
    }

    public getModel(): PortfolioFilterModel {
        return {
            country: this.country.getSelectedKeysOrNull(),
            wg: this.wg.getSelectedKeysOrNull(),
            availability: this.av.getSelectedKeysOrNull(),
            duration: this.dur.getSelectedKeysOrNull(),
            reactionType: this.reacttype.getSelectedKeysOrNull(),
            reactionTime: this.reacttime.getSelectedKeysOrNull(),
            serviceLocation: this.srvloc.getSelectedKeysOrNull(),
            proActive: this.proactive.getSelectedKeysOrNull(),

            isGlobalPortfolio: this.getChecked(this.globPort),
            isMasterPortfolio: this.getChecked(this.masterPort),
            isCorePortfolio: this.getChecked(this.corePort)
        }
    }

    private getSelectedKeysOrNull() {

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