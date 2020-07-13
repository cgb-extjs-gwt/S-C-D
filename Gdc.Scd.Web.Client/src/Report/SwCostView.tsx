import { Column, Container, Grid } from "@extjs/ext-react";
import * as React from "react";
import { ExtDataviewHelper } from "../Common/Helpers/ExtDataviewHelper";
import { buildMvcUrl } from "../Common/Services/Ajax";
import { CalcCostProps } from "./Components/CalcCostProps";
import { moneyRenderer, stringRenderer } from "./Components/GridRenderer";
import { SwCostFilter } from "./Components/SwCostFilter";
import { SwCostFilterModel } from "./Model/SwCostFilterModel";
import { ExportService } from "./Services/ExportService";
import { LinkColumn } from "./Components/LinkColumn";
import { PlausibilityCheckSwDialog } from "./Components/PlausibilityCheckSwDialog";

Ext.require([
    'Ext.grid.plugin.Clipboard'
]);

export class SwCostView extends React.Component<CalcCostProps, any> {

    private grid: Grid;

    private filter: SwCostFilter;

    private plausiWnd: PlausibilityCheckSwDialog;

    private store: Ext.data.IStore = Ext.create('Ext.data.Store', {

        pageSize: 25,
        autoLoad: false,

        proxy: {
            type: 'ajax',
            api: {
                read: buildMvcUrl('calc', 'getswcost')
            },
            actionMethods: {
                read: 'POST'
            },
            reader: {
                type: 'json',
                rootProperty: 'items',
                totalProperty: 'total'
            },
            paramsAsJson: true
        }
    });

    private pluginCfg: any;

    public constructor(props: CalcCostProps) {
        super(props);
        this.init();
    }

    public render() {
        return (
            <Container layout="fit">

                <SwCostFilter ref="filter" docked="right" onSearch={this.onSearch} onDownload={this.onDownload} scrollable={true} />

                <Grid ref="grid"
                    store={this.store}
                    width="100%"
                    platformConfig={this.pluginCfg}
                    cls="grid-paging-no-count grid-small-head data-calc">

                    { /*dependencies*/}

                    <Column
                        flex="1"
                        isHeaderGroup={true}
                        text="Dependencies"
                        dataIndex=""
                        cls="calc-cost-result-green"
                        defaults={{ align: 'center', minWidth: 60, flex: 1, cls: "x-text-el-wrap" }}>

                        <Column text="FSP" dataIndex="Fsp" minWidth="180" renderer={stringRenderer} />
                        <Column text="SW digit" dataIndex="SwDigit" />
                        <Column text="SOG(Asset)" dataIndex="Sog" />
                        <Column text="Availability" dataIndex="Availability" />
                        <Column text="Duration" dataIndex="Duration" />

                    </Column>

                    { /*Resulting costs*/}

                    <Column
                        flex="1"
                        isHeaderGroup={true}
                        text="Resulting costs"
                        dataIndex=""
                        cls="calc-cost-result-blue"
                        defaults={{ align: 'center', minWidth: 60, flex: 1, cls: "x-text-el-wrap", renderer: moneyRenderer }}>

                        <LinkColumn flex="1" renderer={moneyRenderer} text="Service support cost" dataIndex="ServiceSupport" dataAction="service-support" />
                        <LinkColumn flex="1" renderer={moneyRenderer} text="Reinsurance" dataIndex="Reinsurance" dataAction="reinsurance" />
                        <LinkColumn flex="1" renderer={moneyRenderer} text="Transfer price" dataIndex="TransferPrice" dataAction="transfer" />
                        <LinkColumn flex="1" renderer={moneyRenderer} text="Maintenance list price" dataIndex="MaintenanceListPrice" dataAction="maintenance" />
                        <LinkColumn flex="1" renderer={moneyRenderer} text="Dealer reference price" dataIndex="DealerPrice" dataAction="dealer" />

                    </Column>

                </Grid>

                <PlausibilityCheckSwDialog ref="plausiWndRef" />

            </Container>
        );
    }

    public componentDidMount() {
        this.grid = this.refs.grid as Grid;
        this.filter = this.refs.filter as SwCostFilter;
        this.plausiWnd = this.refs.plausiWndRef as PlausibilityCheckSwDialog;
        document.querySelector('.data-calc').addEventListener('click', this.onMoreDetails);
    }

    private init() {
        this.onSearch = this.onSearch.bind(this);
        this.onDownload = this.onDownload.bind(this);
        this.onBeforeLoad = this.onBeforeLoad.bind(this);

        this.store.on('beforeload', this.onBeforeLoad);
        this.pluginCfg = this.getPluginCfg();
    }

    private approved(): boolean {
        return this.props.approved;
    }

    private getPluginCfg() {
        let clipboardCfg = {
            formats: {
                text: { put: 'noPut' }
            },
            noPut: function () { }
        };
        return {
            'desktop': {
                plugins: {
                    gridpagingtoolbar: true,
                    clipboard: clipboardCfg
                },
                selectable: {
                    rows: true,
                    cells: true,
                    columns: true,
                    drag: true,
                    checkbox: false,
                    extensible: 'both'
                }
            },
            '!desktop': {
                plugins: {
                    gridpagingtoolbar: true,
                    clipboard: clipboardCfg
                }
            }
        };
    }

    private onMoreDetails = (e) => {
        let target = e.target;
        let action = target.getAttribute('data-action');
        if (!action) {
            return;
        }
        let rowID = target.getAttribute('data-rowid');
        if (rowID) {
            this.plausiWnd.show(rowID, this.approved(), action);
        }
    }

    private onSearch(filter: SwCostFilterModel) {
        this.reload();
    }

    private onDownload(filter: SwCostFilterModel) {
        ExportService.Download('SW-CALC-RESULT', this.props.approved, filter);
    }

    private reload() {
        ExtDataviewHelper.refreshToolbar(this.grid);
        this.store.load();
    }

    private onBeforeLoad(s, operation) {
        let filter = this.filter.getModel() as any;
        filter.approved = this.props.approved;
        operation.setParams(filter);
    }
}