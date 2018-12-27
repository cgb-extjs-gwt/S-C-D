import { Column, Container, Grid } from "@extjs/ext-react";
import * as React from "react";
import { buildMvcUrl } from "../Common/Services/Ajax";
import { CalcCostProps } from "./Components/CalcCostProps";
import { moneyRenderer, stringRenderer } from "./Components/GridRenderer";
import { SwProactiveCostFilter } from "./Components/SwProactiveCostFilter";
import { SwCostFilterModel } from "./Model/SwCostFilterModel";

export class SwProactiveCostView extends React.Component<CalcCostProps, any> {

    private grid: Grid;

    private filter: SwProactiveCostFilter;

    private store: Ext.data.IStore = Ext.create('Ext.data.Store', {

        fields: [
            { name: 'year', convert: stringRenderer },
            { name: 'proActive', type: 'number', allowNull: true, convert: moneyRenderer }
        ],

        pageSize: 25,
        autoLoad: false,

        proxy: {
            type: 'ajax',
            api: {
                read: buildMvcUrl('calc', 'getswproactivecost')
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

                <SwProactiveCostFilter ref={x => this.filter = x} docked="right" onSearch={this.onSearch} checkAccess={!this.props.approved} />

                <Grid ref={x => this.grid = x} store={this.store} width="100%" plugins={['pagingtoolbar']}>

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

                        <Column text="ProActive" dataIndex="proActive" />

                    </Column>

                </Grid>
            </Container>
        );
    }

    private init() {
        this.onSearch = this.onSearch.bind(this);
        this.store.on('beforeload', this.onBeforeLoad, this);
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