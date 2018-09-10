import { Button, Column, Container, Grid, NumberColumn, Toolbar } from "@extjs/ext-react";
import * as React from "react";
import { CalcCostProps } from "./Components/CalcCostProps";
import { HwCalcFilter } from "./Components/HwCalcFilter";
import { HwCalcFilterModel } from "./Model/HwCalcFilterModel";
import { IReportService } from "./Services/IReportService";
import { ReportFactory } from "./Services/ReportFactory";
import { numOrEmpty } from "./Helpers/numOrEmpty";

export class HwCostView extends React.Component<CalcCostProps, any> {

    private grid: Grid;

    private filter: HwCalcFilter;

    private store: Ext.data.IStore = Ext.create('Ext.data.Store', {
        pageSize: 25,
        autoLoad: true,

        fields: [{
            name: 'serviceSupportCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.serviceSupport);
            }
        }, {
            name: 'serviceSupportCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.serviceSupport_Approved);
            }
        }, {
            name: 'fieldServiceCostCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.fieldServiceCost);
            }
        }, {
            name: 'fieldServiceCostCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.fieldServiceCost_Approved);
            }
        }, {
            name: 'reinsuranceCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.reinsurance);
            }
        }, {
            name: 'reinsuranceCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.reinsurance_Approved);
            }
        }, {
            name: 'logisticCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.logistic);
            }
        }, {
            name: 'logisticCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.logistic_Approved);
            }
        }, {
            name: 'availabilityFeeCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.availabilityFee);
            }
        }, {
            name: 'availabilityFeeCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.availabilityFee_Approved);
            }
        }, {
            name: 'hddRetentionCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.hddRetention);
            }
        }, {
            name: 'hddRetentionCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.hddRetention_Approved);
            }
        }, {
            name: 'taxAndDutiesWCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.taxAndDutiesW);
            }
        }, {
            name: 'taxAndDutiesWCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.taxAndDutiesW_Approved);
            }
        }, {
            name: 'taxAndDutiesOowCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.taxAndDutiesOow);
            }
        }, {
            name: 'taxAndDutiesOowCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.taxAndDutiesOow_Approved);
            }
        }, {
            name: 'materialWCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.materialW);
            }
        }, {
            name: 'materialWCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.materialW_Approved);
            }
        }, {
            name: 'materialOowCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.materialOow);
            }
        }, {
            name: 'materialOowCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.materialOow_Approved);
            }
        }, {
            name: 'proActiveCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.proActive);
            }
        }, {
            name: 'proActiveCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.proActive_Approved);
            }
        }, {
            name: 'serviceTCCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.serviceTC);
            }
        }, {
            name: 'serviceTCCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.serviceTC_Approved);
            }
        }, {
            name: 'serviceTCManualCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.serviceTCManual);
            }
        }, {
            name: 'serviceTCManualCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.serviceTCManual_Approved);
            }
        }, {
            name: 'serviceTPCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.serviceTP);
            }
        }, {
            name: 'serviceTPCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.serviceTP_Approved);
            }
        }, {
            name: 'serviceTPManualCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.serviceTPManual);
            }
        }, {
            name: 'serviceTPManualCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.serviceTPManual_Approved);
            }
        }, {
            name: 'otherDirectCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.otherDirect);
            }
        }, {
            name: 'otherDirectCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.otherDirect_Approved);
            }
        }, {
            name: 'localServiceStandardWarrantyCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.localServiceStandardWarranty);
            }
        }, {
            name: 'localServiceStandardWarrantyCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.localServiceStandardWarranty_Approved);
            }
        }, {
            name: 'creditsCalc',
            convert: function (val, row) {
                return numOrEmpty(row.data.credits);
            }
        }, {
            name: 'creditsCalc_Approved',
            convert: function (val, row) {
                return numOrEmpty(row.data.credits_Approved);
            }
        }],

        proxy: {
            type: 'ajax',
            api: {
                read: '/api/calc/gethwcost'
            },
            reader: {
                type: 'json',
                rootProperty: 'items',
                totalProperty: 'total'
            }
        }
    });

    public state = {
        disableSaveButton: true,
        disableCancelButton: true
    };

    public constructor(props: CalcCostProps) {
        super(props);
        this.init();
    }

    public render() {

        const canEdit = this.canEdit();

        let fieldServiceCost: string = 'fieldServiceCostCalc';
        let serviceSupport: string = 'serviceSupportCalc';
        let logistic: string = 'logisticCalc';
        let availabilityFee: string = 'availabilityFeeCalc';
        let hddRetention: string = 'hddRetentionCalc';
        let reinsurance: string = 'reinsuranceCalc';
        let taxAndDutiesW: string = 'taxAndDutiesWCalc';
        let taxAndDutiesOow: string = 'taxAndDutiesOowCalc';
        let materialW: string = 'materialWCalc';
        let materialOow: string = 'materialOowCalc';
        let proActive: string = 'proActiveCalc';
        let serviceTC: string = 'serviceTCCalc';
        let serviceTCManual: string = 'serviceTCManualCalc';
        let serviceTP: string = 'serviceTPCalc';
        let serviceTPManual: string = 'serviceTPManualCalc';
        let otherDirect: string = 'otherDirectCalc';
        let localServiceStandardWarranty: string = 'localServiceStandardWarrantyCalc';
        let credits: string = 'creditsCalc';

        if (this.props.approved) {
            fieldServiceCost = 'fieldServiceCostCalc_Approved';
            serviceSupport = 'serviceSupportCalc_Approved';
            logistic = 'logisticCalc_Approved';
            availabilityFee = 'availabilityFeeCalc_Approved';
            hddRetention = 'hddRetentionCalc_Approved';
            reinsurance = 'reinsuranceCalc_Approved';
            taxAndDutiesW = 'taxAndDutiesWCalc_Approved';
            taxAndDutiesOow = 'taxAndDutiesOowCalc_Approved';
            materialW = 'materialWCalc_Approved';
            materialOow = 'materialOowCalc_Approved';
            proActive = 'proActiveCalc_Approved';
            serviceTC = 'serviceTCCalc_Approved';
            serviceTCManual = 'serviceTCManualCalc_Approved';
            serviceTP = 'serviceTPCalc_Approved';
            serviceTPManual = 'serviceTPManualCalc_Approved';
            otherDirect = 'otherDirectCalc_Approved';
            localServiceStandardWarranty = 'localServiceStandardWarrantyCalc_Approved';
            credits = 'creditsCalc_Approved';
        }

        return (
            <Container layout="fit">

                <HwCalcFilter ref="filter" docked="right" onSearch={this.onSearch} />

                <Grid
                    ref="grid"
                    store={this.store}
                    width="100%"
                    platformConfig={this.pluginConf()}>

                    { /*dependencies*/}

                    <Column
                        isHeaderGroup={true}
                        text="Dependencies"
                        dataIndex=""
                        cls="calc-cost-result-green"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}>

                        <Column text="Country" dataIndex="country" />
                        <Column text="WG(Asset)" dataIndex="wg" />
                        <Column text="Availability" dataIndex="availability" />
                        <Column text="Duration" dataIndex="duration" />
                        <Column text="Reaction type" dataIndex="reactionType" />
                        <Column text="Reaction time" dataIndex="reactionTime" />
                        <Column text="Service location" dataIndex="serviceLocation" />

                    </Column>

                    { /*cost block results*/}

                    <Column
                        isHeaderGroup={true}
                        text="Cost block results"
                        dataIndex=""
                        cls="calc-cost-result-blue"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}>

                        <Column text="Field service cost" dataIndex={fieldServiceCost} />
                        <Column text="Service support cost" dataIndex={serviceSupport} />
                        <Column text="Logistic cost" dataIndex={logistic} />
                        <Column text="Availability fee" dataIndex={availabilityFee} />
                        <Column text="HDD retention" dataIndex={hddRetention} />
                        <Column text="Reinsurance" dataIndex={reinsurance} />
                        <Column text="Tax &amp; Duties iW period" dataIndex={taxAndDutiesW} />
                        <Column text="Tax &amp; Duties OOW period" dataIndex={taxAndDutiesOow} />
                        <Column text="Material cost iW period" dataIndex={materialW} />
                        <Column text="Material cost OOW period" dataIndex={materialOow} />
                        <Column text="Pro active" dataIndex={proActive} />

                    </Column>

                    { /*Resulting costs*/}

                    <Column
                        isHeaderGroup={true}
                        text="Resulting costs"
                        dataIndex=""
                        cls="calc-cost-result-yellow"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}>

                        <Column text="Service TC(calc)" dataIndex={serviceTC} />
                        <Column text="Service TC(manual)" dataIndex={serviceTCManual} editable={canEdit} renderer={this.numberRenderer.bind(this)} />

                        <Column text="Service TP(calc)" dataIndex={serviceTP} />
                        <Column text="Service TP(manual)" dataIndex={serviceTPManual} editable={canEdit} renderer={this.numberRenderer.bind(this)} />

                        <Column text="Other direct cost" dataIndex={otherDirect} />
                        <Column text="Local service standard warranty" dataIndex={localServiceStandardWarranty} />
                        <Column text="Credits" dataIndex={credits} />

                    </Column>

                </Grid>

                {this.toolbar()}

            </Container>
        );
    }

    public componentDidMount() {
        this.grid = this.refs.grid as Grid;
        this.filter = this.refs.filter as HwCalcFilter;
    }

    private init() {
        this.onSearch = this.onSearch.bind(this);
        this.onBeforeLoad = this.onBeforeLoad.bind(this);

        this.store.on('beforeload', this.onBeforeLoad);
    }

    private cancelChanges() {
        console.log('cancelChanges()');
    }

    private saveRecords() {
        console.log('saveRecords()');
    }

    private onSearch(filter: HwCalcFilterModel) {
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

        if (this.canEdit()) {
            cfg['desktop'].plugins['gridcellediting'] = true;
            cfg['!desktop'].plugins['grideditable'] = true;
        }

        return cfg;
    }

    private canEdit() {
        return !this.props.approved;
    }

    private toolbar() {
        if (this.canEdit()) {
            return (
                <Toolbar docked="bottom">
                    <Button
                        text="Cancel"
                        flex={1}
                        iconCls="x-fa fa-trash"
                        handler={this.cancelChanges}
                        disabled={this.state.disableCancelButton}
                    />
                    <Button
                        text="Save"
                        flex={1}
                        iconCls="x-fa fa-save"
                        handler={this.saveRecords}
                        disabled={this.state.disableSaveButton}
                    />
                </Toolbar>
            );
        }
    }
}