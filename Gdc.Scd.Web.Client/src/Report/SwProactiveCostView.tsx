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

    private grid: Grid & any;

    private filter: SwProactiveCostFilter;

    private selectable: any = {
        extensible: 'both',
        rows: true,
        cells: true,
        columns: true,
        drag: true,
        checkbox: false
    };

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

    public constructor(props: CalcCostProps) {
        super(props);
        this.init();
    }

    public render() {
        return (
            <Container layout="fit">

                <SwProactiveCostFilter
                    ref={x => this.filter = x}
                    docked="right"
                    onSearch={this.onSearch}
                    onDownload={this.onDownload}
                    checkAccess={!this.props.approved}
                    scrollable={true} />

                <Grid ref={x => this.grid = x}
                    store={this.store}
                    width="100%"
                    plugins={['pagingtoolbar', 'clipboard']}
                    selectable={this.selectable}
                >

                    { /*dependencies*/}

                    <Column
                        flex="1"
                        isHeaderGroup={true}
                        text="Dependencies"
                        dataIndex=""
                        cls="calc-cost-result-green"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}>

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