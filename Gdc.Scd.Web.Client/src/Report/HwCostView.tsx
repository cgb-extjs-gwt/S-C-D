import { Button, CheckColumn, Column, Container, Grid, NumberColumn, Panel, Toolbar } from "@extjs/ext-react";
import * as React from "react";
import { ExtDataviewHelper } from "../Common/Helpers/ExtDataviewHelper";
import { ExtMsgHelper } from "../Common/Helpers/ExtMsgHelper";
import { handleRequest } from "../Common/Helpers/RequestHelper";
import { buildMvcUrl, post } from "../Common/Services/Ajax";
import { Country } from "../Dict/Model/Country";
import { UserCountryService } from "../Dict/Services/UserCountryService";
import { CalcCostProps } from "./Components/CalcCostProps";
import { currencyRenderer, ddMMyyyyRenderer, emptyRenderer, EUR, IRenderer, percentRenderer, stringRenderer, yearRenderer } from "./Components/GridRenderer";
import { HwCostFilter } from "./Components/HwCostFilter";
import { HwReleasePanel } from "./Components/HwReleasePanel";
import { CurrencyType } from "./Model/CurrencyType";
import { HwCostFilterModel } from "./Model/HwCostFilterModel";
import { ExportService } from "./Services/ExportService";

const SELECTED_FIELD = 'selected';

Ext.require([
    'Ext.grid.plugin.Clipboard'
]);

export class HwCostView extends React.Component<CalcCostProps, any> {

    private grid: Grid & any;

    private filter: HwCostFilter;

    private currency: string;

    private exchangeRate: number;

    private localMoneyRenderer: IRenderer = (value: any, row: any) => {
        return currencyRenderer(value, this.currency);
    };

    private euroMoneyRenderer: IRenderer = (value: any, row: any) => {
        return currencyRenderer(value / this.exchangeRate, EUR);
    };

    private store = Ext.create('Ext.data.Store', {

        fields: [
            'Id', SELECTED_FIELD, 'ListPrice', 'DealerDiscount', 'ChangeUserName', 'ChangeUserEmail', 'ReleaseDate',
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
                read: buildMvcUrl('calc', 'gethwcost')
            },
            actionMethods: {
                read: 'POST'
            },
            reader: {
                type: 'json',
                rootProperty: 'items',
                idProperty: "Id",
                totalProperty: 'total'
            },
            paramsAsJson: true
        },
        listeners: {
            update: (store, record, operation, modifiedFieldNames: string[], details) => {

                if (modifiedFieldNames && modifiedFieldNames.indexOf('LocalServiceStandardWarrantyManual') !== -1) {
                    this.updateStdw(store, record);
                }

                const changed = this.store.getUpdatedRecords().length;
                if (modifiedFieldNames && modifiedFieldNames.length > 0 && modifiedFieldNames[0] == SELECTED_FIELD) {
                    this.onCheckChange();
                }
                else {
                    this.toggleToolbar(changed == 0);
                }

                store.suspendEvents(false);
                store.fixNullValue(record, 'ServiceTCManual');
                store.fixNullValue(record, 'ServiceTPManual');
                store.fixNullValue(record, 'ListPrice');
                store.fixNullValue(record, 'DealerDiscount');
                store.fixNullValue(record, 'LocalServiceStandardWarrantyManual');
                store.resumeEvents();
            }
        },
        fixNullValue: function (record, field) {
            var d = record.data;
            //
            //stub, for correct null imput
            var v = typeof d[field] === 'number' ? d[field] : '';
            record.set(field, v); 
        }
    });

    public state = {
        disableSaveButton: true,
        disableCancelButton: true,
        disableSearchButton: true,
        selectedCountry: null,
        showInLocalCurrency: true,
        hideReleaseButton: true,
        userCanEdit: false,
        selectable: {
            rows: true,
            cells: true,
            columns: true,
            drag: true,
            checkbox: false
        },
        extensible: 'both',
        message: 'No Selection'
    };

    private pluginCfg: any;

    public constructor(props: CalcCostProps) {
        super(props);
        this.init();
    }

    public render() {
        let canEditTC: boolean = false;
        let canEditListPrice: boolean = false;
        let moneyRndr: IRenderer;
        const { selectable, extensible } = this.state;
        if (this.state.showInLocalCurrency) {

            //allow manual edit in LOCAL CURRENCY mode only for well view!!!

            canEditTC = this.canEditTC() && this.state.userCanEdit;
            canEditListPrice = this.canEditListPrice() && this.state.userCanEdit;
            //
            moneyRndr = this.localMoneyRenderer;
        }
        else {
            moneyRndr = this.euroMoneyRenderer;
        }

        return (
            <Container layout="fit">

                <Panel {...this.props} docked="right" scrollable={true} >
                    <HwCostFilter
                        ref={this.filterRef}
                        onChange={this.onFilterChange}
                        checkAccess={!this.props.approved} />

                    <HwReleasePanel
                        onRelease={this.releaseSelected}
                        onReleaseAll={this.releaseAll}
                        checkAccess={!this.props.approved}
                        hidden={this.state.hideReleaseButton}
                        disabled={!this.state.disableSaveButton} />
                </Panel>

                <Grid
                    ref={this.gridRef}
                    store={this.store}
                    width="100%"
                    platformConfig={this.pluginCfg}
                    onSelectionChange={this.onSelectionChange}
                    selectable={{
                        extensible,
                        ...selectable
                    }}
                    shadow
                >

                    { /*dependencies*/}

                    <Column
                        isHeaderGroup={true}
                        text="Dependencies"
                        dataIndex=""
                        cls="calc-cost-result-green"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}>

                        <CheckColumn dataIndex={SELECTED_FIELD} sortable={false} flex="0.5" minWidth="50" hidden={!this.approved()} />
                        <Column text="Country" dataIndex="Country" />
                        <Column text="SOG(Asset)" dataIndex="Sog" renderer={emptyRenderer} />
                        <Column text="WG(Asset)" dataIndex="Wg" />
                        <Column text="Availability" dataIndex="Availability" />
                        <Column text="Duration" dataIndex="Duration" />
                        <Column text="Reaction type" dataIndex="ReactionType" />
                        <Column text="Reaction time" dataIndex="ReactionTime" />
                        <Column text="Service location" dataIndex="ServiceLocation" />
                        <Column text="ProActive SLA" dataIndex="ProActiveSla" />
                        <Column text="Standard warranty duration" dataIndex="StdWarranty" renderer={yearRenderer} flex="0.5" minWidth="50" />
                        <Column text="Standard Warranty Service Location" dataIndex="StdWarrantyLocation" renderer={stringRenderer} />

                    </Column>

                    { /*Resulting costs*/}

                    <Column
                        isHeaderGroup={true}
                        text="Resulting costs"
                        dataIndex=""
                        cls="calc-cost-result-yellow"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap", renderer: moneyRndr }}>

                        <NumberColumn text="Service TC(calc)" dataIndex="ServiceTC" />
                        <NumberColumn text="Service TC(manual)" dataIndex="ServiceTCManual" editable={canEditTC} />

                        <NumberColumn text="Service TP(calc)" dataIndex="ServiceTP" />
                        <NumberColumn text="Service TP(manual)" dataIndex="ServiceTPManual" editable={canEditTC} />
                        <NumberColumn text="Service TP(released)" dataIndex="ServiceTP_Released" />

                        <NumberColumn text="List price" dataIndex="ListPrice" editable={canEditListPrice} />
                        <NumberColumn text="Dealer discount in %" dataIndex="DealerDiscount" editable={canEditListPrice} renderer={percentRenderer} />
                        <NumberColumn text="Dealer price" dataIndex="DealerPriceCalc" />

                        <Column flex="2" minWidth="250" text="Change user" dataIndex="ChangeUserCalc" renderer={emptyRenderer} />
                        <Column text="Release date" dataIndex="ReleaseDate" renderer={ddMMyyyyRenderer} />

                        <NumberColumn text="Other direct cost" dataIndex="OtherDirect" />
                        <NumberColumn text="Local service standard warranty(calc)" dataIndex="LocalServiceStandardWarranty" />
                        <NumberColumn text="Local service standard warranty(manual)" dataIndex="LocalServiceStandardWarrantyManual" editable={canEditTC} />
                        <NumberColumn text="Credits" dataIndex="Credits" />

                    </Column>

                    {/*cost block results*/}

                    <Column
                        isHeaderGroup={true}
                        text="Cost block results"
                        dataIndex=""
                        cls="calc-cost-result-blue"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap", renderer: moneyRndr }}
                        editable
                    >

                        <NumberColumn text="Field service cost" dataIndex="FieldServiceCost" />
                        <NumberColumn text="Service support cost" dataIndex="ServiceSupportCost" />
                        <NumberColumn text="Logistic cost" dataIndex="Logistic" />
                        <NumberColumn text="Availability fee" dataIndex="AvailabilityFee" />
                        <NumberColumn text="Reinsurance" dataIndex="Reinsurance" />
                        <NumberColumn text="Tax &amp; Duties iW period" dataIndex="TaxAndDutiesW" />
                        <NumberColumn text="Tax &amp; Duties OOW period" dataIndex="TaxAndDutiesOow" />
                        <NumberColumn text="Material cost iW period" dataIndex="MaterialW" />
                        <NumberColumn text="Material cost OOW period" dataIndex="MaterialOow" />
                        <NumberColumn text="ProActive" dataIndex="ProActive" />

                    </Column>

                </Grid>

                {this.toolbar()}

            </Container>
        );
    }

    private filterRef = (x) => {
        this.filter = x;
    }

    private gridRef = (x) => {
        this.grid = x;
    }

    toggleSelectable = field => {
        this.setState({
            selectable: { ...this.state.selectable, [field]: !this.state.selectable[field] }
        });
    };

    setExtensible = extensible => {
        this.setState({ extensible });
    };

    private onSelectionChange = (grid, records, selecting, selection) => {
        let message = '??',
            firstRowIndex,
            firstColumnIndex,
            lastRowIndex,
            lastColumnIndex;

        if (!selection) {
            message = 'No selection';
        }

        else if (selection.isCells) {
            firstRowIndex = selection.getFirstRowIndex();
            firstColumnIndex = selection.getFirstColumnIndex();
            lastRowIndex = selection.getLastRowIndex();
            lastColumnIndex = selection.getLastColumnIndex();

            message = 'Selected cells: ' + (lastColumnIndex - firstColumnIndex + 1) + 'x' + (lastRowIndex - firstRowIndex + 1) +
                ' at (' + firstColumnIndex + ',' + firstRowIndex + ')';
        }
        else if (selection.isRows) {
            message = 'Selected rows: ' + selection.getCount();
        }
        else if (selection.isColumns) {
            message = 'Selected columns: ' + selection.getCount();
        }

        this.setState({ message });
    };

    private init() {
        this.onSearch = this.onSearch.bind(this);
        this.onFilterChange = this.onFilterChange.bind(this);
        this.onDownload = this.onDownload.bind(this);
        this.cancelChanges = this.cancelChanges.bind(this);
        this.saveRecords = this.saveRecords.bind(this);
        this.releaseSelected = this.releaseSelected.bind(this);
        this.releaseAll = this.releaseAll.bind(this);

        this.store.on('beforeload', this.onBeforeLoad, this);
        this.store.on('load', this.onLoad, this);
        //
        if (this.approved()) { //using CanEdit() does not work cause await http request
            this.pluginCfg = this.editPluginConf();
        } else {
            this.pluginCfg = this.readPluginConf();
        }
    }

    private toggleToolbar(disable: boolean) {
        this.setState({ disableSaveButton: disable, disableCancelButton: disable });
    }

    private cancelChanges() {
        this.store.rejectChanges();
        this.toggleToolbar(true);
    }

    private saveRecords() {
        let recs = this.store.getModifiedRecords().map(x => x.getData());
        let cnt = this.state.selectedCountry;

        if (recs && cnt) {
            let me = this;
            let p = post('calc', 'savehwcost', { items: recs, countryId: cnt.id }).then(() => {
                me.reset();
                me.reload();
            });
            handleRequest(p);
        }
    }

    private releaseSelected() {
        let recs = this.getSelectedRows();
        let cnt = this.state.selectedCountry;

        if (cnt) {
            if (recs && recs.length > 0) {
                recs = this.store.getData().items.filter(x => recs.includes(x.data.Id)).map(x => x.data);
            }
        }

        ExtMsgHelper.confirm('Release', `Do you want to approve for release ${recs.length} record(s)?`, () => {
            let me = this;
            let p = post('calc', 'releasehwcost', { items: recs, countryId: cnt.id, filter: this.filter.getModel() }).then(() => {
                me.reset();
                me.reload();
            });
            handleRequest(p);
        });
    }

    private releaseAll() {
        ExtMsgHelper.confirm('Release', `Do you want to approve for release all filtered records?`, () => {
            let me = this;
            let p = post('calc', 'releasehwcostall', { ...this.filter.getModel() }).then(() => {
                me.reset();
                me.reload();
            });
            handleRequest(p);
        });
    }

    private onSearch(filter: HwCostFilterModel) {
        ExtDataviewHelper.refreshToolbar(this.grid);
        this.reload();
    }

    private onDownload() {
        let filter: any = this.filter.getModel() || {};
        filter.local = filter.currency;
        ExportService.Download('HW-CALC-RESULT', this.props.approved, filter);
    }

    private onFilterChange(filter: HwCostFilterModel) {
        this.setState({
            showInLocalCurrency: filter.currency === CurrencyType.Local,
            disableSearchButton: !(filter.country && filter.country.length > 0)
        });
        this.grid.refresh();
    }

    private reload() {
        this.store.load();
    }

    private onBeforeLoad(s, operation) {
        this.reset();

        let filter = this.filter.getModel() as any;
        filter.approved = this.props.approved;
        operation.setParams(filter);

        const srv = new UserCountryService();
        let cntId = 0;
        if (this.state && this.state.selectedCountry) {
            cntId = this.state.selectedCountry.id;
            srv.isCountryUser(cntId).then(x => {
                this.setState({
                    hideReleaseButton: !this.props.approved || !x || !this.state.disableSaveButton,
                    userCanEdit: x
                });

            });
        };
    }

    private onLoad(s, recs) {

        let first = recs[0];
        if (first) {
            let d = first.data;

            //stub for correct clipboard rendering

            this.currency = d.Currency;
            this.exchangeRate = d.ExchangeRate;

        }
        this.grid.refresh();
    }

    private readPluginConf() {
        let clipboardCfg = {
            formats: {
                text: { put: 'noPut' }
            },
            noPut: function () { }
        };
        return {
            'desktop': {
                plugins: {
                    gridpagingtoolbar: true,
                    clipboard: clipboardCfg
                }
            },
            '!desktop': {
                plugins: {
                    gridpagingtoolbar: true,
                    clipboard: clipboardCfg
                }
            }
        };
    }

    private editPluginConf() {
        let cb = {
            formats: {
                text: {
                    get: 'getTextData',
                    put: 'putTextData'
                }
            }
        };
        return {
            'desktop': {
                plugins: {
                    gridpagingtoolbar: true,
                    gridcellediting: true,
                    selectionreplicator: true,
                    clipboard: cb
                },
                selectable: {
                    cells: true,
                    rows: true,
                    columns: false,
                    drag: true,
                    extensible: 'y'
                }
            },
            '!desktop': {
                plugins: {
                    gridpagingtoolbar: true,
                    clipboard: cb,
                    grideditable: true
                }
            }
        };
    }

    private approved() {
        return this.props.approved;
    }

    private canEdit(): boolean {
        return this.state.userCanEdit && (this.canEditListPrice() || this.canEditTC());
    }

    private canEditListPrice(): boolean {
        let result: boolean = this.approved();
        if (result) {
            const cnt: Country = this.state.selectedCountry;
            result = cnt && cnt.canStoreListAndDealerPrices;
        }
        return result;
    }

    private canEditTC(): boolean {
        let result: boolean = this.approved();
        if (result) {
            const cnt: Country = this.state.selectedCountry;
            result = cnt && cnt.canOverrideTransferCostAndPrice;
        }
        return result;
    }

    private toolbar() {

        let invalid = this.state.disableSearchButton;
        let canedit = this.canEdit();

        return <Toolbar docked="top">
            <Button
                text="Search"
                disabled={invalid}
                handler={this.onSearch} />
            <Button
                text="Download"
                iconCls="x-fa fa-download"
                disabled={invalid}
                handler={this.onDownload} />
            {
                canedit &&
                <Button
                    text="Cancel"
                    iconCls="x-fa fa-trash"
                    handler={this.cancelChanges}
                    disabled={this.state.disableCancelButton}
                />
            }
            {
                canedit &&
                <Button
                    text="Save"
                    iconCls="x-fa fa-save"
                    handler={this.saveRecords}
                    disabled={this.state.disableSaveButton}
                />
            }
        </Toolbar>;
    }

    private reset() {
        this.setState({
            disableCancelButton: true,
            disableSaveButton: true,
            selectedCountry: this.filter.getCountry()
        });
    }

    private getSelectedRows(): string[] {
        return this.store.getData().items.filter(record => record.data[SELECTED_FIELD] === true).map(record => record.data.Id);
    }

    private onCheckChange = () => {
        this.grid.select(this.store.getData().items.filter(record => record.data[SELECTED_FIELD] === true));
    };

    private updateStdw = (store: any, record: any) => {
        let items = store.getData();
        let cnt = record.get('Country');
        let wg = record.get('Wg');
        for (let i = 0, len = items.count(); i < len; i++) {
            let row = items.getAt(i);
            if (row.get('Country') === cnt && row.get('Wg') === wg) {
                row.set('LocalServiceStandardWarrantyManual', record.get('LocalServiceStandardWarrantyManual'));
            }
        }
    }
}
