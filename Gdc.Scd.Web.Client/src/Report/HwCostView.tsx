import { Button, Column, Container, Grid, NumberColumn, Toolbar } from "@extjs/ext-react";
import * as React from "react";
import { buildMvcUrl } from "../Common/Services/Ajax";
import { CalcCostProps } from "./Components/CalcCostProps";
import { HwCostFilter } from "./Components/HwCostFilter";
import { HwCostFilterModel } from "./Model/HwCostFilterModel";

export class HwCostView extends React.Component<CalcCostProps, any> {

    private grid: Grid;

    private filter: HwCostFilter;

    private store: Ext.data.IStore = Ext.create('Ext.data.Store', {
        pageSize: 25,
        autoLoad: true,

        proxy: {
            type: 'ajax',
            api: {
                read: buildMvcUrl('calc', 'gethwcost'),
                update: buildMvcUrl('calc', 'savehwcost')
            },
            writer: {
                type: 'json',
                writeAllFields: true,
                allowSingle: false,
                idProperty: "id"
            },
            reader: {
                type: 'json',
                rootProperty: 'items',
                totalProperty: 'total'
            }
        },
        listeners: {
            update: (store, record, operation, modifiedFieldNames, details, eOpts) => {
                const changed = this.store.getUpdatedRecords().length;
                this.toggleToolbar(changed == 0);
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

        let fieldServiceCost: string = 'FieldServiceCost';
        let serviceSupport: string = 'ServiceSupportCost';
        let logistic: string = 'Logistic';
        let availabilityFee: string = 'AvailabilityFee';
        let hddRetention: string = 'HddRet';
        let reinsurance: string = 'Reinsurance';
        let taxAndDutiesW: string = 'TaxAndDutiesW';
        let taxAndDutiesOow: string = 'TaxAndDutiesOow';
        let materialW: string = 'MaterialW';
        let materialOow: string = 'MaterialOow';
        let proActive: string = 'ProActive';
        let serviceTC: string = 'ServiceTC';
        let serviceTCManual: string = 'ServiceTCManual';
        let serviceTP: string = 'ServiceTP';
        let serviceTPManual: string = 'ServiceTPManual';
        let listPrice: string = 'ListPrice';
        let dealerDiscount: string = 'DealerDiscount';
        let dealerPrice: string = 'DealerPrice';
        let otherDirect: string = 'OtherDirect';
        let localServiceStandardWarranty: string = 'LocalServiceStandardWarranty';
        let credits: string = 'Credits';

        return (
            <Container layout="fit">

                <HwCostFilter ref="filter" docked="right" onSearch={this.onSearch} />

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

                        <Column text="Country" dataIndex="Country" />
                        <Column text="WG(Asset)" dataIndex="Wg" />
                        <Column text="Availability" dataIndex="Availability" />
                        <Column text="Duration" dataIndex="Duration" />
                        <Column text="Reaction type" dataIndex="ReactionType" />
                        <Column text="Reaction time" dataIndex="ReactionTime" />
                        <Column text="Service location" dataIndex="ServiceLocation" />

                    </Column>

                    { /*cost block results*/}

                    <Column
                        isHeaderGroup={true}
                        text="Cost block results"
                        dataIndex=""
                        cls="calc-cost-result-blue"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}>

                        <NumberColumn text="Field service cost" dataIndex={fieldServiceCost} />
                        <NumberColumn text="Service support cost" dataIndex={serviceSupport} />
                        <NumberColumn text="Logistic cost" dataIndex={logistic} />
                        <NumberColumn text="Availability fee" dataIndex={availabilityFee} />
                        <NumberColumn text="HDD retention" dataIndex={hddRetention} />
                        <NumberColumn text="Reinsurance" dataIndex={reinsurance} />
                        <NumberColumn text="Tax &amp; Duties iW period" dataIndex={taxAndDutiesW} />
                        <NumberColumn text="Tax &amp; Duties OOW period" dataIndex={taxAndDutiesOow} />
                        <NumberColumn text="Material cost iW period" dataIndex={materialW} />
                        <NumberColumn text="Material cost OOW period" dataIndex={materialOow} />
                        <NumberColumn text="Pro active" dataIndex={proActive} />

                    </Column>

                    { /*Resulting costs*/}

                    <Column
                        isHeaderGroup={true}
                        text="Resulting costs"
                        dataIndex=""
                        cls="calc-cost-result-yellow"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}>

                        <NumberColumn text="Service TC(calc)" dataIndex={serviceTC} />
                        <NumberColumn text="Service TC(manual)" dataIndex={serviceTCManual} editable={canEdit} />

                        <NumberColumn text="Service TP(calc)" dataIndex={serviceTP} />
                        <NumberColumn text="Service TP(manual)" dataIndex={serviceTPManual} editable={canEdit} />

                        <NumberColumn text="List price" dataIndex={listPrice} editable={canEdit} />
                        <NumberColumn text="Dealer discount" dataIndex={dealerDiscount} editable={canEdit} />
                        <NumberColumn text="Dealer price" dataIndex={dealerPrice} />

                        <NumberColumn text="Other direct cost" dataIndex={otherDirect} />
                        <NumberColumn text="Local service standard warranty" dataIndex={localServiceStandardWarranty} />
                        <NumberColumn text="Credits" dataIndex={credits} />

                    </Column>

                </Grid>

                {this.toolbar()}

            </Container>
        );
    }

    public componentDidMount() {
        this.grid = this.refs.grid as Grid;
        this.filter = this.refs.filter as HwCostFilter;
    }

    private init() {
        this.onSearch = this.onSearch.bind(this);
        this.onBeforeLoad = this.onBeforeLoad.bind(this);
        this.cancelChanges = this.cancelChanges.bind(this);
        this.saveRecords = this.saveRecords.bind(this);

        this.store.on('beforeload', this.onBeforeLoad);
    }

    private toggleToolbar(disable: boolean) {
        this.setState({ disableSaveButton: disable, disableCancelButton: disable });
    }

    private cancelChanges() {
        this.store.rejectChanges();
        this.toggleToolbar(true);
    }

    private saveRecords() {
        this.store.sync({
            scope: this,

            success: function (batch, options) {
                //TODO: show successfull message box
                this.setState({
                    disableSaveButton: true,
                    disableCancelButton: true
                });
                this.store.load();
            },

            failure: (batch, options) => {
                //TODO: show error
                this.store.rejectChanges();
            }
        });

    }

    private onSearch(filter: HwCostFilterModel) {
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