import { Column, Container, Grid, NumberColumn } from "@extjs/ext-react";
import * as React from "react";
import { buildMvcUrl } from "../Common/Services/Ajax";
import { CalcCostProps } from "./Components/CalcCostProps";
import { SwProactiveCostFilter } from "./Components/SwProactiveCostFilter";
import { SwCostFilterModel } from "./Model/SwCostFilterModel";

export class SwProactiveCostView extends React.Component<CalcCostProps, any> {

    private grid: Grid;

    private filter: SwProactiveCostFilter;

    private store: Ext.data.IStore = Ext.create('Ext.data.Store', {
        pageSize: 25,
        autoLoad: true,

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

                <SwProactiveCostFilter ref="filter" docked="right" onSearch={this.onSearch} />

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
                        <Column text="Digit" dataIndex="swDigit" />
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

                        <NumberColumn text="Pro active" dataIndex="proActive" />

                    </Column>

                </Grid>
            </Container>
        );
    }

    public componentDidMount() {
        this.grid = this.refs.grid as Grid;
        this.filter = this.refs.filter as SwProactiveCostFilter;
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
        let filter = this.filter.getModel();
        let params = Ext.apply({}, operation.getParams(), filter);
        operation.setParams(params);
    }
}