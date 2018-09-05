import { Column, Container, Grid } from "@extjs/ext-react";
import * as React from "react";
import { HwCalcFilter } from "./Components/HwCalcFilter";
import { HwCalcFilterModel } from "./Model/HwCalcFilterModel";
import { IReportService } from "./Services/IReportService";
import { ReportFactory } from "./Services/ReportFactory";

export class HwCostView extends React.Component<any, any> {

    private grid: Grid;

    private filter: HwCalcFilter;

    private srv: IReportService;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {

        return (
            <Container layout="fit">

                <HwCalcFilter ref="filter" docked="right" onSearch={this.onSearch} />

                <Grid ref="grid" store={this.state.denied} width="100%" plugins={['pagingtoolbar']}>
                    <Column flex="1" text="Country" dataIndex="country" />
                    <Column flex="1" text="WG(Asset)" dataIndex="wg" />
                    <Column flex="1" text="Availability" dataIndex="availability" />
                    <Column flex="1" text="Duration" dataIndex="duration" />
                    <Column flex="1" text="Reaction type" dataIndex="reactionType" />
                    <Column flex="1" text="Reaction time" dataIndex="reactionTime" />
                    <Column flex="1" text="Service location" dataIndex="serviceLocation" />
                </Grid>

            </Container>
        );
    }

    public componentDidMount() {
        this.grid = this.refs.grid as Grid;
        this.filter = this.refs.filter as HwCalcFilter;
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

    private onSearch(filter: HwCalcFilterModel) {
        this.reload();
    }

    private reload() {
        let filter = this.filter.getModel();
    }

}