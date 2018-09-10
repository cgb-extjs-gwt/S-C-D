import { Column, Container, Grid, NumberColumn } from "@extjs/ext-react";
import * as React from "react";
import { CalcCostProps } from "./Components/CalcCostProps";
import { SwCalcFilter } from "./Components/SwCalcFilter";
import { SwCalcFilterModel } from "./Model/SwCalcFilterModel";

export class SwCostView extends React.Component<CalcCostProps, any> {

    private grid: Grid;

    private filter: SwCalcFilter;

    private store: Ext.data.IStore = Ext.create('Ext.data.Store', {
        pageSize: 25,
        autoLoad: true,

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

        let serviceSupport: string = 'serviceSupport';
        let reinsurance: string = 'reinsurance';
        let transferPrice: string = 'transferPrice';
        let maintenanceListPrice: string = 'maintenanceListPrice';
        let dealerPrice: string = 'dealerPrice';

        if (this.props.approved) {
            serviceSupport = 'serviceSupport_Approved';
            reinsurance = 'reinsurance_Approved';
            transferPrice = 'transferPrice_Approved';
            maintenanceListPrice = 'maintenanceListPrice_Approved';
            dealerPrice = 'dealerPrice_Approved';
        }

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

                        <NumberColumn text="Service support cost" dataIndex={serviceSupport} />
                        <NumberColumn text="Reinsurance" dataIndex={reinsurance} />
                        <NumberColumn text="Transer price" dataIndex={transferPrice} />
                        <NumberColumn text="Maintenance list price" dataIndex={maintenanceListPrice} />
                        <NumberColumn text="Dealer reference price" dataIndex={dealerPrice} />

                    </Column>

                </Grid>
            </Container>
        );
    }

    public componentDidMount() {
        this.grid = this.refs.grid as Grid;
        this.filter = this.refs.filter as SwCalcFilter;
    }

    private init() {
        this.onSearch = this.onSearch.bind(this);
        this.onBeforeLoad = this.onBeforeLoad.bind(this);

        this.store.on('beforeload', this.onBeforeLoad);
    }

    private onSearch(filter: SwCalcFilterModel) {
        this.reload();
    }

    private reload() {
        this.store.load();
    }

    private onBeforeLoad(s, operation) {
        let filter = this.filter.getModel();
        let params = Ext.apply({}, operation.getParams(), filter);
        operation.setParams(params);
    }
}