import { Column, Container, Grid, NumberColumn } from "@extjs/ext-react";
import * as React from "react";
import { CalcCostProps } from "./Components/CalcCostProps";
import { SwCalcFilter } from "./Components/SwCalcFilter";
import { SwCalcFilterModel } from "./Model/SwCalcFilterModel";
import { IReportService } from "./Services/IReportService";
import { ReportFactory } from "./Services/ReportFactory";

export class SwCostView extends React.Component<CalcCostProps, any> {

    private grid: Grid;

    private filter: SwCalcFilter;

    private srv: IReportService;

    public constructor(props: CalcCostProps) {
        super(props);
        this.init();
    }

    public render() {

        return (
            <Container layout="fit">

                <SwCalcFilter ref="filter" docked="right" onSearch={this.onSearch} />

                <Grid ref="grid" store={this.state.denied} width="100%" plugins={['pagingtoolbar']}>

                    { /*dependencies*/}

                    <Column flex="1" isHeaderGroup={true} text="Dependencies" dataIndex="none" cls="calc-cost-result-green" defaults={{ minWidth: 100 }}>

                        <Column flex="1" text="Country" dataIndex="country" />
                        <Column flex="1" text="SOG(Asset)" dataIndex="wg" />
                        <Column flex="1" text="Availability" dataIndex="availability" />
                        <Column flex="1" text="Year" dataIndex="duration" />

                    </Column>

                    { /*Resulting costs*/}

                    <Column flex="2" isHeaderGroup={true} text="Resulting costs" dataIndex="none" cls="calc-cost-result-blue" defaults={{ minWidth: 100 }}>

                        <NumberColumn flex="1" text="Service<br>support cost" dataIndex="serviceSupport" />
                        <NumberColumn flex="1" text="Reinsurance" dataIndex="reinsurance" />
                        <NumberColumn flex="1" text="Transer<br>price" dataIndex="transferPrice" />

                        <Column isHeaderGroup={true} text="Maintenance<br>list price" dataIndex="" defaults={{ minWidth: 100 }}>
                            <NumberColumn flex="1" text="Calc" dataIndex="maintenanceListPrice" />
                            <NumberColumn flex="1" text="Manual" dataIndex="maintenanceListPriceManual" />
                        </Column>

                        <Column isHeaderGroup={true} text="Dealer<br>reference price" dataIndex="" defaults={{ minWidth: 100 }}>
                            <NumberColumn flex="1" text="Calc" dataIndex="dealerPrice" />
                            <NumberColumn flex="1" text="Manual" dataIndex="dealerPriceManual" />
                        </Column>


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