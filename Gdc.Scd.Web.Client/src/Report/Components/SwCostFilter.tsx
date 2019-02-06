﻿import { Button, Container, Panel, PanelProps } from "@extjs/ext-react";
import * as React from "react";
import { AvailabilityField } from "../../Dict/Components/AvailabilityField";
import { DictField } from "../../Dict/Components/DictField";
import { SwDigitField } from "../../Dict/Components/SwDigitField";
import { YearField } from "../../Dict/Components/YearField";
import { SwCostFilterModel } from "../Model/SwCostFilterModel";
import { NamedId } from "../../Common/States/CommonStates";
import { DictFactory } from "../../Dict/Services/DictFactory";
import { IDictService } from "../../Dict/Services/IDictService";
import { MultiSelect } from "../../Dict/Components/MultiSelect";

Ext.require('Ext.panel.Collapser');

const SELECT_MAX_HEIGHT: string = '200px';

export interface FilterPanelProps extends PanelProps {
    onSearch(filter: SwCostFilterModel): void;
    onDownload(filter: SwCostFilterModel): void;
}

export class SwCostFilter extends React.Component<FilterPanelProps, any> {

    private digit: MultiSelect;

    private avail: MultiSelect;

    private year: MultiSelect;

    private dictSrv: IDictService;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {
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

                    <Panel title='SW digit'
                        {...panelProps}>
                        <MultiSelect ref={x => this.digit = x} {...multiProps} store={this.dictSrv.getSwDigit} />
                    </Panel>
                    <Panel title='Availability'
                        {...panelProps}>
                        <MultiSelect ref={x => this.avail = x} {...multiProps} store={this.dictSrv.getAvailabilityTypes} />
                    </Panel>
                    <Panel title='Year'
                        {...panelProps}>
                        <MultiSelect ref={x => this.year = x} {...multiProps} store={this.dictSrv.getYears} />
                    </Panel>

                </Container>

                <Button text="Search" ui="action" minWidth="85px" handler={this.onSearch} margin="20px auto" />

                <Button text="Download" ui="action" minWidth="85px" iconCls="x-fa fa-download" handler={this.onDownload} />

            </Panel>
        );
    }

    public getModel(): SwCostFilterModel {
        return {
            digit: this.digit.getSelectedKeys(),
            availability: this.avail.getSelectedKeys(),
            year: this.year.getSelectedKeys()
        }
    }

    private init() {
        this.dictSrv = DictFactory.getDictService();
        this.onSearch = this.onSearch.bind(this);
        this.onDownload = this.onDownload.bind(this);
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