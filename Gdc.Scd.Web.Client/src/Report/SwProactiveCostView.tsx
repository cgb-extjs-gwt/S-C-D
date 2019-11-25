import { Column, Container, Grid } from "@extjs/ext-react";
import * as React from "react";
import { ExtDataviewHelper } from "../Common/Helpers/ExtDataviewHelper";
import { buildMvcUrl } from "../Common/Services/Ajax";
import { CalcCostProps } from "./Components/CalcCostProps";
import { moneyRenderer, stringRenderer } from "./Components/GridRenderer";
import { SwProactiveCostFilter } from "./Components/SwProactiveCostFilter";
import { SwCostFilterModel } from "./Model/SwCostFilterModel";
import { ExportService } from "./Services/ExportService";

export class SwProactiveCostView extends React.Component<CalcCostProps, any> {

    private grid: Grid;

    private filter: SwProactiveCostFilter;

    private store: Ext.data.IStore = Ext.create('Ext.data.Store', {

        pageSize: 25,
        autoLoad: false,

        proxy: {
            type: 'ajax',
            api: {
                read: buildMvcUrl('calc', 'getswproactivecost')
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

                <SwProactiveCostFilter
                    ref={this.filterRef}
                    docked="right"
                    onSearch={this.onSearch}
                    onDownload={this.onDownload}
                    checkAccess={!this.props.approved}
                    scrollable={true} />

                <Grid ref={this.gridRef}
                    store={this.store}
                    width="100%"
                    platformConfig={this.pluginCfg}
                    cls="grid-paging-no-count grid-small-head"
                >

                    { /*dependencies*/}

                    <Column
                        flex="1"
                        isHeaderGroup={true}
                        text="Dependencies"
                        dataIndex=""
                        cls="calc-cost-result-green"
                        defaults={{ align: 'center', minWidth: 60, flex: 1, cls: "x-text-el-wrap" }}>

                        <Column text="FSP" dataIndex="Fsp" minWidth="180" renderer={stringRenderer} />
                        <Column text="Country" dataIndex="Country" />
                        <Column text="SW digit" dataIndex="SwDigit" />
                        <Column text="SOG(Asset)" dataIndex="Sog" />
                        <Column text="Availability" dataIndex="Availability" renderer={stringRenderer} />
                        <Column text="Year" dataIndex="Year" renderer={stringRenderer} />

                    </Column>

                    { /*Resulting costs*/}

                    <Column
                        flex="1"
                        isHeaderGroup={true}
                        text="Resulting costs"
                        dataIndex=""
                        cls="calc-cost-result-blue"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}>

                        <Column text="ProActive" dataIndex="ProActive" renderer={moneyRenderer} />

                    </Column>

                </Grid>
            </Container>
        );
    }

    private init() {
        this.onSearch = this.onSearch.bind(this);
        this.onDownload = this.onDownload.bind(this);
        this.store.on('beforeload', this.onBeforeLoad, this);
        this.pluginCfg = this.getPluginCfg();
    }

    private filterRef = (x) => {
        this.filter = x;
    }

    private gridRef = (x) => {
        this.grid = x;
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
                    extensible: 'both',
                    rows: true,
                    cells: true,
                    columns: true,
                    drag: true,
                    checkbox: false
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

    private onSearch(filter: SwCostFilterModel) {
        this.reload();
    }

    private onDownload(filter: SwCostFilterModel) {
        ExportService.Download('SW-PROACTIVE-CALC-RESULT', this.props.approved, filter);
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