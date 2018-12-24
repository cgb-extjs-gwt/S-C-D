import { Column, Container, Grid, NumberColumn } from "@extjs/ext-react";
import * as React from "react";
import { buildMvcUrl } from "../Common/Services/Ajax";
import { CalcCostProps } from "./Components/CalcCostProps";
import { moneyRenderer } from "./Components/GridRenderer";
import { SwCostFilter } from "./Components/SwCostFilter";
import { SwCostFilterModel } from "./Model/SwCostFilterModel";

export class SwCostView extends React.Component<CalcCostProps, any> {

    private grid: Grid;

    private filter: SwCostFilter;

    private store: Ext.data.IStore = Ext.create('Ext.data.Store', {
        fields: [
            { name: 'serviceSupport', type: 'number', allowNull: true, convert: moneyRenderer },
            { name: 'reinsurance', type: 'number', allowNull: true, convert: moneyRenderer },
            { name: 'transferPrice', type: 'number', allowNull: true, convert: moneyRenderer },
            { name: 'maintenanceListPrice', type: 'number', allowNull: true, convert: moneyRenderer },
            { name: 'dealerPrice', type: 'number', allowNull: true, convert: moneyRenderer }
        ],

        pageSize: 25,
        autoLoad: true,

        proxy: {
            type: 'ajax',
            api: {
                read: buildMvcUrl('calc', 'getswcost')
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

                <SwCostFilter ref="filter" docked="right" onSearch={this.onSearch} />

                <Grid ref="grid" store={this.store} width="100%" plugins={['pagingtoolbar']}>

                    { /*dependencies*/}

                    <Column
                        flex="1"
                        isHeaderGroup={true}
                        text="Dependencies"
                        dataIndex=""
                        cls="calc-cost-result-green"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}>

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

                        <Column text="Service support cost" dataIndex="serviceSupport" />
                        <Column text="Reinsurance" dataIndex="reinsurance" />
                        <Column text="Transer price" dataIndex="transferPrice" />
                        <Column text="Maintenance list price" dataIndex="maintenanceListPrice" />
                        <Column text="Dealer reference price" dataIndex="dealerPrice" />

                    </Column>

                </Grid>
            </Container>
        );
    }

    public componentDidMount() {
        this.grid = this.refs.grid as Grid;
        this.filter = this.refs.filter as SwCostFilter;
    }

    private init() {
        this.onSearch = this.onSearch.bind(this);
        this.onBeforeLoad = this.onBeforeLoad.bind(this);

        this.store.on('beforeload', this.onBeforeLoad);
    }

    private onSearch(filter: SwCostFilterModel) {
        this.reload();
    }

    private reload() {
        this.store.load();
    }

    private onBeforeLoad(s, operation) {
        let filter = this.filter.getModel() as any;
        filter.approved = this.props.approved;
        let params = Ext.apply({}, operation.getParams(), filter);
        operation.setParams(params);
    }
}