import { Button, Column, Container, Grid, NumberColumn, Toolbar } from "@extjs/ext-react";
import * as React from "react";
import { buildMvcUrl } from "../Common/Services/Ajax";
import { Country } from "../Dict/Model/Country";
import { CalcCostProps } from "./Components/CalcCostProps";
import { HwCostFilter } from "./Components/HwCostFilter";
import { HwCostFilterModel } from "./Model/HwCostFilterModel";

export class HwCostView extends React.Component<CalcCostProps, any> {

    private filter: HwCostFilter;

    private store: Ext.data.IStore = Ext.create('Ext.data.Store', {

        fields: [
            'ListPrice', 'DealerDiscount',
            {
                name: 'DealerPriceCalc',
                calculate: function (d) {
                    let result = null;
                    if (d && d.ListPrice) {
                        result = d.ListPrice;
                        if (d.DealerDiscount) {
                            result = result - (result * d.DealerDiscount / 100);
                        }
                    }
                    return result;
                }
            }
        ],

        pageSize: 25,
        autoLoad: false,

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
                idProperty: "Id"
            },
            reader: {
                type: 'json',
                rootProperty: 'items',
                idProperty: "Id",
                totalProperty: 'total'
            }
        },
        listeners: {
            update: () => {
                const changed = this.store.getUpdatedRecords().length;
                this.toggleToolbar(changed == 0);
            }
        }
    });

    public state = {
        disableSaveButton: true,
        disableCancelButton: true,
        selectedCountry: null
    };

    public constructor(props: CalcCostProps) {
        super(props);
        this.init();
    }

    public render() {
        const canEditTC: boolean = this.canEditTC();
        const canEditListPrice: boolean = this.canEditListPrice();

        return (
            <Container layout="fit">

                <HwCostFilter ref={x => this.filter = x} docked="right" onSearch={this.onSearch} />

                <Grid
                    store={this.store}
                    width="100%"
                    platformConfig={this.pluginConf()}
                >

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
                        <Column text="ProActive sla" dataIndex="ProActiveSla" />

                    </Column>

                    {/*cost block results*/}

                    <Column
                        isHeaderGroup={true}
                        text="Cost block results"
                        dataIndex=""
                        cls="calc-cost-result-blue"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}>

                        <NumberColumn text="Field service cost" dataIndex="FieldServiceCost" />
                        <NumberColumn text="Service support cost" dataIndex="ServiceSupportCost" />
                        <NumberColumn text="Logistic cost" dataIndex="Logistic" />
                        <NumberColumn text="Availability fee" dataIndex="AvailabilityFee" />
                        <NumberColumn text="HDD retention" dataIndex="HddRet" />
                        <NumberColumn text="Reinsurance" dataIndex="Reinsurance" />
                        <NumberColumn text="Tax &amp; Duties iW period" dataIndex="TaxAndDutiesW" />
                        <NumberColumn text="Tax &amp; Duties OOW period" dataIndex="TaxAndDutiesOow" />
                        <NumberColumn text="Material cost iW period" dataIndex="MaterialW" />
                        <NumberColumn text="Material cost OOW period" dataIndex="MaterialOow" />
                        <NumberColumn text="ProActive" dataIndex="ProActive" />

                    </Column>

                    { /*Resulting costs*/}

                    <Column
                        isHeaderGroup={true}
                        text="Resulting costs"
                        dataIndex=""
                        cls="calc-cost-result-yellow"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}>

                        <NumberColumn text="Service TC(calc)" dataIndex="ServiceTC" />
                        <NumberColumn text="Service TC(manual)" dataIndex="ServiceTCManual" editable={canEditTC} />
                        <NumberColumn text="Service TP(calc)" dataIndex="ServiceTP" />
                        <NumberColumn text="Service TP(manual)" dataIndex="ServiceTPManual" editable={canEditTC} />

                        <NumberColumn text="List price" dataIndex="ListPrice" editable={canEditListPrice} />
                        <NumberColumn text="Dealer discount" dataIndex="DealerDiscount" editable={canEditListPrice} />
                        <NumberColumn text="Dealer price" dataIndex="DealerPriceCalc" />

                        <NumberColumn text="Other direct cost" dataIndex="OtherDirect" />
                        <NumberColumn text="Local service standard warranty" dataIndex="LocalServiceStandardWarranty" />
                        <NumberColumn text="Credits" dataIndex="Credits" />

                    </Column>

                </Grid>

                {this.toolbar()}

            </Container>
        );
    }

    private init() {
        this.onSearch = this.onSearch.bind(this);
        this.cancelChanges = this.cancelChanges.bind(this);
        this.saveRecords = this.saveRecords.bind(this);

        this.store.on('beforeload', this.onBeforeLoad, this);
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
                this.reset();
                this.reload();
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
        this.reset();
        //
        let filter = this.filter.getModel() as any;
        filter.approved = this.props.approved;
        let params = Ext.apply({}, operation.getParams(), filter);
        operation.setParams(params);
    }

    private pluginConf(): any {
        let cfg: any = {
            'desktop': {
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
            cfg['desktop'].plugins.gridcellediting = true;
            cfg['!desktop'].plugins.grideditable = true;
        }

        return cfg;
    }

    private canEdit(): boolean {
        return !this.props.approved;
    }

    private canEditListPrice(): boolean {
        let result: boolean = this.canEdit();
        if (result) {
            const cnt: Country = this.state.selectedCountry;
            result = cnt && cnt.canStoreListAndDealerPrices;
        }
        return result;
    }

    private canEditTC(): boolean {
        let result: boolean = this.canEdit();
        if (result) {
            const cnt: Country = this.state.selectedCountry;
            result = cnt && cnt.canOverrideTransferCostAndPrice;
        }
        return result;
    }

    private toolbar() {
        if (this.canEdit()) {
            return <Toolbar docked="top">
                <Button
                    text="Cancel"
                    iconCls="x-fa fa-trash"
                    handler={this.cancelChanges}
                    disabled={this.state.disableCancelButton}
                />
                <Button
                    text="Save"
                    iconCls="x-fa fa-save"
                    handler={this.saveRecords}
                    disabled={this.state.disableSaveButton}
                />
            </Toolbar>;
        }
    }

    private reset() {
        this.setState({
            disableCancelButton: true,
            disableSaveButton: true,
            selectedCountry: this.filter.getCountry()
        });
    }
}