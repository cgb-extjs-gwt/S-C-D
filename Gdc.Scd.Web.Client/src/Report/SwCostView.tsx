import { Column, Container, Grid, NumberColumn } from "@extjs/ext-react";
import * as React from "react";
import { CalcCostProps } from "./Components/CalcCostProps";
import { SwCalcFilter } from "./Components/SwCalcFilter";
import { SwCalcFilterModel } from "./Model/SwCalcFilterModel";
import { IReportService } from "./Services/IReportService";
import { ReportFactory } from "./Services/ReportFactory";

function numOrEmpty(v: number): any {
    return typeof v === 'number' ? v : ' ';
}

export class SwCostView extends React.Component<CalcCostProps, any> {

    private grid: Grid;

    private filter: SwCalcFilter;

    private srv: IReportService;

    private store = Ext.create('Ext.data.Store', {
        pageSize: 25,
        autoLoad: true,

        fields: [{
            name: 'serviceSupportCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.serviceSupport);
            }
        }, {
            name: 'reinsuranceCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.reinsurance);
            }
        }, {
            name: 'transferPriceCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.transferPrice);
            }
        }, {
            name: 'maintenanceListPriceCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.maintenanceListPrice);
            }
        }, {
            name: 'dealerPriceCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.dealerPrice);
            }
        }],

        proxy: {
            type: 'ajax',
            api: {
                read: '/api/calc/getswcost'
            },
            reader: {
                type: 'json',
                rootProperty: 'items',
                totalProperty: 'total'
            }
        }
    });

    public constructor(props: CalcCostProps) {
        super(props);
        this.init();
    }

    public render() {

        return (
            <Container layout="fit">

                <SwCalcFilter ref="filter" docked="right" onSearch={this.onSearch} />

                <Grid ref="grid" store={this.store} width="100%" plugins={['pagingtoolbar']}>

                    { /*dependencies*/}

                    <Column
                        flex="1"
                        isHeaderGroup={true}
                        text="Dependencies"
                        dataIndex=""
                        cls="calc-cost-result-green"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}>

                        <Column text="Country" dataIndex="country" />
                        <Column text="SOG(Asset)" dataIndex="sog" />
                        <Column text="Availability" dataIndex="availability" />
                        <Column text="Year" dataIndex="year" />

                    </Column>

                    { /*Resulting costs*/}

                    <Column
                        flex="1"
                        isHeaderGroup={true}
                        text="Resulting costs"
                        dataIndex=""
                        cls="calc-cost-result-blue"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}>

                        <Column text="Service support cost" dataIndex="serviceSupportCalc" />
                        <Column text="Reinsurance" dataIndex="reinsuranceCalc" />
                        <Column text="Transer price" dataIndex="transferPriceCalc" />
                        <Column text="Maintenance list price" dataIndex="maintenanceListPriceCalc" />
                        <Column text="Dealer reference price" dataIndex="dealerPriceCalc" />

                    </Column>

                </Grid>
            </Container>
        );
    }

    public componentDidMount() {
        this.grid = this.refs.grid as Grid;
        this.filter = this.refs.filter as SwCalcFilter;
        //
        this.reload();
    }

    private init() {
        this.srv = ReportFactory.getReportService();
        this.onSearch = this.onSearch.bind(this);
        //
        this.state = {
            allowed: [],
            denied: []
        };
    }

    private onSearch(filter: SwCalcFilterModel) {
        this.reload();
    }

    private reload() {
        let filter = this.filter.getModel();
    }
}