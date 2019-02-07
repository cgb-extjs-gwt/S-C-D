import { Button, Column, Container, Grid, NumberColumn, Panel, Toolbar } from "@extjs/ext-react";
import * as React from "react";
import { ExtDataviewHelper } from "../Common/Helpers/ExtDataviewHelper";
import { ExtMsgHelper } from "../Common/Helpers/ExtMsgHelper";
import { handleRequest } from "../Common/Helpers/RequestHelper";
import { buildMvcUrl, post } from "../Common/Services/Ajax";
import { Country } from "../Dict/Model/Country";
import { UserCountryService } from "../Dict/Services/UserCountryService";
import { CalcCostProps } from "./Components/CalcCostProps";
import { emptyRenderer, IRenderer, localMoneyRendererFactory, localToEuroMoneyRendererFactory, percentRenderer, yearRenderer } from "./Components/GridRenderer";
import { HwCostFilter } from "./Components/HwCostFilter";
import { HwReleasePanel } from "./Components/HwReleasePanel";
import { CurrencyType } from "./Model/CurrencyType";
import { HwCostFilterModel } from "./Model/HwCostFilterModel";
import { ExportService } from "./Services/ExportService";

const localMoneyRenderer = localMoneyRendererFactory('Currency');
const euroMoneyRenderer = localToEuroMoneyRendererFactory('ExchangeRate');

export class HddCostView extends React.Component<CalcCostProps, any> {

    private grid: Grid & any;

    private filter: HwCostFilter;

    private store = Ext.create('Ext.data.Store', {

        fields: [
            'WgId', 'ListPrice', 'DealerDiscount', 'ChangeUserName', 'ChangeUserEmail',
            {
                name: 'DealerPriceCalc',
                calculate: function (d) {
                    let result: any;
                    if (d && d.ListPrice) {
                        result = d.ListPrice;
                        if (d.DealerDiscount) {
                            result = result - (result * d.DealerDiscount / 100);
                        }
                    }
                    return result;
                }
            },
            {
                name: 'ChangeUserCalc',
                calculate: function (d) {
                    let result: string = '';
                    if (d) {
                        if (d.ChangeUserName) {
                            result += d.ChangeUserName;
                        }
                        if (d.ChangeUserEmail) {
                            result += '[' + d.ChangeUserEmail + ']';
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
                read: buildMvcUrl('hdd', 'getcost')
            },
            reader: {
                type: 'json',
                rootProperty: 'items',
                idProperty: "WgId",
                totalProperty: 'total'
            }
        },
        //listeners: {
        //    update: () => {
        //        const changed = this.store.getUpdatedRecords().length;
        //        this.toggleToolbar(changed == 0);
        //    }
        //}
    });

    public state = {
        disableSaveButton: true,
        disableCancelButton: true,
        selectedCountry: null,
        showInLocalCurrency: true,
        hideReleaseButton: true
    };

    public constructor(props: CalcCostProps) {
        super(props);
        this.init();
    }

    public render() {

        let canEdit: boolean = false;
        let moneyRndr: IRenderer = euroMoneyRenderer;

        //if (this.state.showInLocalCurrency) {

        //    //allow manual edit in LOCAL CURRENCY mode only for well view!!!

        //    canEditTC = this.canEditTC();
        //    canEditListPrice = this.canEditListPrice();
        //    //
        //    moneyRndr = localMoneyRenderer;
        //}
        //else {
        //    moneyRndr = euroMoneyRenderer;
        //}

        return (
            <Container layout="fit">

                <Grid
                    ref={x => this.grid = x}
                    store={this.store}
                    width="100%"
                    platformConfig={this.pluginConf()}

                    defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap", renderer: moneyRndr }}

                >

                    { /*dependencies*/}

                    <Column
                        flex="1"
                        isHeaderGroup={true}
                        text="Dependencies"
                        dataIndex=""
                        cls="calc-cost-result-green"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}>

                        <Column text="WG(Asset)" dataIndex="Wg" />

                    </Column>

                    { /*Resulting costs*/}

                    <Column
                        flex="6"
                        isHeaderGroup={true}
                        text="Resulting costs"
                        dataIndex=""
                        cls="calc-cost-result-blue"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}>

                        <NumberColumn text="HDD retention" dataIndex="hddRetention" />

                        <NumberColumn text="Transfer price" dataIndex="transferPrice" editable={canEdit} />
                        <NumberColumn text="List price" dataIndex="listPrice" editable={canEdit} />
                        <NumberColumn text="Dealer discount in %" dataIndex="dealerDiscount" editable={canEdit} renderer={percentRenderer} />
                        <NumberColumn text="Dealer price" dataIndex="dealerPriceCalc" />

                        <Column flex="2" minWidth="250" text="Change user" dataIndex="ChangeUserCalc" renderer={emptyRenderer} />

                    </Column>

                </Grid>

            </Container>
        );
    }

    private init() {
        //this.onSearch = this.onSearch.bind(this);
        //this.onFilterChange = this.onFilterChange.bind(this);
        //this.onDownload = this.onDownload.bind(this);
        //this.cancelChanges = this.cancelChanges.bind(this);
        //this.saveRecords = this.saveRecords.bind(this);
        //this.releaseCosts = this.releaseCosts.bind(this);

        this.store.on('beforeload', this.onBeforeLoad, this);
        this.store.on('datachanged', this.ondDataChanged, this);
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

        if (this.approved()) {
            cfg['!desktop'].plugins.grideditable = true;
            const desktop = cfg['desktop'];
            desktop.plugins.gridcellediting = true;
            desktop.plugins.selectionreplicator = true;
            desktop.selectable = {
                cells: true,
                rows: true,
                columns: false,
                drag: true,
                extensible: 'y',
                checkbox: true
            };
        }

        return cfg;
    }

    private approved() {
        return this.props.approved;
    }

    private reload() {
        this.store.load();
    }

    private onBeforeLoad(s, operation) {
        this.reset();
        //
        //let filter = this.filter.getModel() as any;
        //filter.approved = this.props.approved;
        //let params = Ext.apply({}, operation.getParams(), filter);
        //operation.setParams(params);
    }

    private ondDataChanged(s, operation) {
        //const srv = new UserCountryService();
        //let cntId = 0;
        //if (this.state && this.state.selectedCountry) {
        //    cntId = this.state.selectedCountry.id;
        //    srv.isCountryUser(cntId).then(x => {
        //        this.setState({ hideReleaseButton: !this.props.approved || !x || !this.state.disableSaveButton })
        //    });
        //}
    }

    private reset() {
        this.setState({
            disableCancelButton: true,
            disableSaveButton: true,
            //selectedCountry: this.filter.getCountry()
        });
    }
}