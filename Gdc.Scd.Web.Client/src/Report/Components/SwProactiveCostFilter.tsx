import { Button, Container, Panel, PanelProps } from "@extjs/ext-react";
import * as React from "react";
import { SwCostFilterModel } from "../Model/SwCostFilterModel";
import { NamedId } from "../../Common/States/CommonStates";
import { DictFactory } from "../../Dict/Services/DictFactory";
import { IDictService } from "../../Dict/Services/IDictService";
import { MultiSelect } from "../../Dict/Components/MultiSelect";

Ext.require('Ext.panel.Collapser');

const SELECT_MAX_HEIGHT: string = '200px';

export interface FilterPanelProps extends PanelProps {
    checkAccess: boolean;
    onSearch(filter: SwCostFilterModel): void;
    onDownload(filter: SwCostFilterModel): void;
}

export class SwProactiveCostFilter extends React.Component<FilterPanelProps, any> {

    private cnt: MultiSelect;

    private digit: MultiSelect;

    private av: MultiSelect;

    private year: MultiSelect;

    private dictSrv: IDictService;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {

        let valid = this.state && this.state.valid;

        let countryField;

        let multiProps = {
            width: '200px',
            maxHeight: SELECT_MAX_HEIGHT,
            title: ""
        };
        let panelProps = {
            width: '300px',
            collapsible: {
                direction: 'top',
                dynamic: true,
                collapsed: true
            }
        };

        if (this.props.checkAccess) {
            countryField = <MultiSelect ref={x => this.cnt = x} {...multiProps} store={this.dictSrv.getUserCountryNames} onselect={this.onCountryChange} />
        }
        else {
            countryField = <MultiSelect ref={x => this.cnt = x} {...multiProps} store={this.dictSrv.getMasterCountriesNames} onselect={this.onCountryChange} />;
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

                    <Panel title='Country'
                        {...panelProps}>
                        {countryField}
                    </Panel>
                    <Panel title='SW digit'
                        {...panelProps}>
                        <MultiSelect ref={x => this.digit = x} {...multiProps} store={this.dictSrv.getSwDigit} />
                    </Panel>
                    <Panel title='Availability'
                        {...panelProps}>
                        <MultiSelect ref={x => this.av = x} {...multiProps} store={this.dictSrv.getAvailabilityTypes} />
                    </Panel>
                    <Panel title='Year'
                        {...panelProps}>
                        <MultiSelect ref={x => this.year = x} {...multiProps} store={this.dictSrv.getYears} />
                    </Panel>

                </Container>

                <Button text="Search" ui="action" minWidth="85px" margin="5px 20px" disabled={!valid} handler={this.onSearch} />

                <Button text="Download" ui="action" minWidth="85px" margin="5px 20px" iconCls="x-fa fa-download" disabled={!valid} handler={this.onDownload} />

            </Panel>
        );
    }

    public getModel(): SwCostFilterModel {
        return {
            country: this.cnt.getSelectedKeysOrNull(),
            digit: this.digit.getSelectedKeysOrNull(),
            availability: this.av.getSelectedKeysOrNull(),
            year: this.year.getSelectedKeysOrNull()
        };
    }

    private init() {
        this.dictSrv = DictFactory.getDictService();
        this.onCountryChange = this.onCountryChange.bind(this);
        this.onSearch = this.onSearch.bind(this);
        this.onDownload = this.onDownload.bind(this);
    }

    private onCountryChange() {
        this.setState({ valid: !!this.cnt.getSelected() });
    }

    private onSearch() {
        let handler = this.props.onSearch;
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
}