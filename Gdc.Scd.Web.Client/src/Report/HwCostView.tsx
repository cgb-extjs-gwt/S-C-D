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

                    <Column isHeaderGroup={true} text="Dependencies" dataIndex="" cls="calc-cost-result-green" defaults={{ minWidth: 100 }}>

                        <Column flex="1" text="Country" dataIndex="country" />
                        <Column flex="1" text="WG(Asset)" dataIndex="wg" />
                        <Column flex="1" text="Availability" dataIndex="availability" />
                        <Column flex="1" text="Duration" dataIndex="duration" />
                        <Column flex="1" text="Reaction<br>type" dataIndex="reactionType" />
                        <Column flex="1" text="Reaction<br>time" dataIndex="reactionTime" />
                        <Column flex="1" text="Service<br>location" dataIndex="serviceLocation" />

                    </Column>

                    { /*cost block results*/}

                    <Column isHeaderGroup={true} text="Cost block results" dataIndex="" cls="calc-cost-result-blue" defaults={{ minWidth: 100 }}>

                        <NumberColumn flex="1" text="Field<br>service<br>cost" dataIndex="fieldServiceCost" />
                        <NumberColumn flex="1" text="Service<br>support<br>cost" dataIndex="serviceSupport" />
                        <NumberColumn flex="1" text="Logistic<br>cost" dataIndex="logistic" />
                        <NumberColumn flex="1" text="Availability<br>fee" dataIndex="availabilityFee" />
                        <NumberColumn flex="1" text="HDD<br>retention" dataIndex="hddRetention" />
                        <NumberColumn flex="1" text="Reinsurance" dataIndex="reinsurance" />
                        <NumberColumn flex="1" text="Tax &amp; Duties<br>iW period" dataIndex="taxAndDutiesW" />
                        <NumberColumn flex="1" text="Tax &amp; Duties<br>OOW period" dataIndex="taxAndDutiesOow" />
                        <NumberColumn flex="1" text="Material<br>cost<br>iW period" dataIndex="materialW" />
                        <NumberColumn flex="1" text="Material<br>cost<br>OOW period" dataIndex="materialOow" />
                        <NumberColumn flex="1" text="Pro<br>active" dataIndex="proActive" />

                    </Column>

                    { /*Resulting costs*/}

                    <Column isHeaderGroup={true} text="Resulting costs" dataIndex="" cls="calc-cost-result-yellow" defaults={{ minWidth: 100 }}>

                        <Column isHeaderGroup={true} text="Service TC" dataIndex="" defaults={{ minWidth: 100 }}>
                            <NumberColumn flex="1" text="Calc" dataIndex="serviceTC" />
                            <NumberColumn flex="1" text="Manual" dataIndex="serviceTCManual" editable={editMode} renderer={this.numberRenderer.bind(this)} />
                        </Column>

                        <Column isHeaderGroup={true} text="Service TP" dataIndex="" defaults={{ minWidth: 100 }}>
                            <NumberColumn flex="1" text="Calc" dataIndex="serviceTP" />
                            <NumberColumn flex="1" text="Manual" dataIndex="serviceTPManual" editable={editMode} renderer={this.numberRenderer.bind(this)} />
                        </Column>

                        <NumberColumn flex="1" text="Other<br>direct<br>cost" dataIndex="otherDirect" />
                        <NumberColumn flex="1" text="Local<br>service<br>standard<br>warranty" dataIndex="localServiceStandardWarranty" />
                        <NumberColumn flex="1" text="Credits" dataIndex="credits" />

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