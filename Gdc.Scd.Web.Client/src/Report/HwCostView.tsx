import { Column, Container, Grid, NumberColumn } from "@extjs/ext-react";
import * as React from "react";
import { CalcCostProps } from "./Components/CalcCostProps";
import { HwCalcFilter } from "./Components/HwCalcFilter";
import { HwCalcFilterModel } from "./Model/HwCalcFilterModel";
import { IReportService } from "./Services/IReportService";
import { ReportFactory } from "./Services/ReportFactory";

export class HwCostView extends React.Component<CalcCostProps, any> {

    private grid: Grid;

    private filter: HwCalcFilter;

    private srv: IReportService;

    public constructor(props: CalcCostProps) {
        super(props);
        this.init();
    }

    public render() {

        const editMode = !this.props.approved;

        return (
            <Container layout="fit">

                <HwCalcFilter ref="filter" docked="right" onSearch={this.onSearch} />

                <Grid
                    ref="grid"
                    store={this.state.costs}
                    width="100%"
                    platformConfig={this.pluginConf()}>

                    { /*dependencies*/}

                    <Column isHeaderGroup={true} text="Dependencies" dataIndex="" cls="calc-cost-result-green" defaults={{ flex: 1, cls: "x-text-el-wrap" }}>

                        <Column text="Country" dataIndex="country" />
                        <Column text="WG(Asset)" dataIndex="wg" />
                        <Column text="Availability" dataIndex="availability" />
                        <Column text="Duration" dataIndex="duration" />
                        <Column text="Reaction type" dataIndex="reactionType" />
                        <Column text="Reaction time" dataIndex="reactionTime" />
                        <Column text="Service location" dataIndex="serviceLocation" />

                    </Column>

                    { /*cost block results*/}

                    <Column isHeaderGroup={true} text="Cost block results" dataIndex="" cls="calc-cost-result-blue" defaults={{ flex: 1, cls: "x-text-el-wrap" }}>

                        <NumberColumn text="Field service cost" dataIndex="fieldServiceCost" />
                        <NumberColumn text="Service support cost" dataIndex="serviceSupport" />
                        <NumberColumn text="Logistic cost" dataIndex="logistic" />
                        <NumberColumn text="Availability fee" dataIndex="availabilityFee" />
                        <NumberColumn text="HDD retention" dataIndex="hddRetention" />
                        <NumberColumn text="Reinsurance" dataIndex="reinsurance" />
                        <NumberColumn text="Tax &amp; Duties iW period" dataIndex="taxAndDutiesW" />
                        <NumberColumn text="Tax &amp; Duties OOW period" dataIndex="taxAndDutiesOow" />
                        <NumberColumn text="Material cost iW period" dataIndex="materialW" />
                        <NumberColumn text="Material cost OOW period" dataIndex="materialOow" />
                        <NumberColumn text="Pro active" dataIndex="proActive" />

                    </Column>

                    { /*Resulting costs*/}

                    <Column isHeaderGroup={true} text="Resulting costs" dataIndex="" cls="calc-cost-result-yellow" defaults={{ flex: 1, cls: "x-text-el-wrap" }}>

                        <NumberColumn text="Service TC(calc)" dataIndex="serviceTC" />
                        <NumberColumn text="Service TC(manual)" dataIndex="serviceTCManual" editable={editMode} renderer={this.numberRenderer.bind(this)} />

                        <NumberColumn text="Service TP(calc)" dataIndex="serviceTP" />
                        <NumberColumn text="Service TP(manual)" dataIndex="serviceTPManual" editable={editMode} renderer={this.numberRenderer.bind(this)} />

                        <NumberColumn text="Other direct cost" dataIndex="otherDirect" />
                        <NumberColumn text="Local service standard warranty" dataIndex="localServiceStandardWarranty" />
                        <NumberColumn text="Credits" dataIndex="credits" />

                    </Column>

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
            costs: []
        };
    }

    private onSearch(filter: HwCalcFilterModel) {
        this.reload();
    }

    private reload() {
        let filter = this.filter.getModel();
        //
        this.srv.getHwCost(filter).then(x => this.setState({
            costs: {
                data: x.items,
                pageSize: 2
            }
        }));
    }

    private numberRenderer(value, { data }): string {
        return isNaN(value) ? ' ' : value;
    }

    private pluginConf(): any {

        let cfg: any = {
            desktop: {
                plugins: {
                    gridpagingtoolbar: true
                }
            },
            '!desktop': {
                plugins: {
                    gridpagingtoolbar: true
                }
            }
        };
        if (!this.props.approved) {
            cfg['desktop'].plugins['gridcellediting'] = true;
            cfg['!desktop'].plugins['grideditable'] = true;
        }

        return cfg;
    }
}