import { Button, CheckColumn, Column, Container, Grid, NumberColumn, Panel, Toolbar } from "@extjs/ext-react";
import * as React from "react";
import { ExtDataviewHelper } from "../Common/Helpers/ExtDataviewHelper";
import { ExtMsgHelper } from "../Common/Helpers/ExtMsgHelper";
import { handleRequest } from "../Common/Helpers/RequestHelper";
import { post } from "../Common/Services/Ajax";
import { Country } from "../Dict/Model/Country";
import { UserCountryService } from "../Dict/Services/UserCountryService";
import { CalcCostProps } from "./Components/CalcCostProps";
import { readonly, setFloatOrEmpty } from "./Components/GridExts";
import { EUR, IRenderer, currencyRenderer, ddMMyyyyRenderer, emptyRenderer, locationRenderer, percentRenderer, stringRenderer, yearRenderer } from "./Components/GridRenderer";
import { HwCostFilter } from "./Components/HwCostFilter";
import { HwReleasePanel } from "./Components/HwReleasePanel";
import { LinkColumn } from "./Components/LinkColumn";
import { PlausibilityCheckDialog } from "./Components/PlausibilityCheckDialog";
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

    private plausiWnd: PlausibilityCheckDialog;

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
            'Id',
            SELECTED_FIELD,
            'ListPrice',
            'DealerDiscount',

            'ProActive',
            'ReActiveTPManual',

            'ReleaseUserName',
            'ReleaseDate',

            'ChangeDate',
            'ChangeUserName',
            'ChangeUserEmail',

            { name: 'roFsp', calculate: readonly('Fsp') },
            { name: 'roCountry', calculate: readonly('Country') },
            { name: 'roSog', calculate: readonly('Sog') },
            { name: 'roWg', calculate: readonly('Wg') },
            { name: 'roAvailability', calculate: readonly('Availability') },
            { name: 'roDuration', calculate: readonly('Duration') },
            { name: 'roReactionType', calculate: readonly('ReactionType') },
            { name: 'roReactionTime', calculate: readonly('ReactionTime') },
            { name: 'roServiceLocation', calculate: readonly('ServiceLocation') },
            { name: 'roProActiveSla', calculate: readonly('ProActiveSla') },
            { name: 'roStdWarranty', calculate: readonly('StdWarranty') },
            { name: 'roStdWarrantyLocation', calculate: readonly('StdWarrantyLocation') },

            { name: 'roServiceTC', calculate: readonly('ServiceTC') },
            { name: 'roServiceTCManual', calculate: readonly('ServiceTCManual') },

            { name: 'roServiceTP', calculate: readonly('ServiceTP') },

            {
                name: 'roServiceTPManual',
                calculate: function (d) {
                    let result: any;
                    if (d) {
                        result = d.ReActiveTPManual;
                        if (d.ProActive) {
                            result = result + d.ProActive
                        }
                    }
                    return result;
                }
            },

            { name: 'roServiceTP_Released', calculate: readonly('ServiceTP_Released') },

            { name: 'roListPrice', calculate: readonly('ListPrice') },
            { name: 'roDealerDiscount', calculate: readonly('DealerDiscount') },
            { name: 'roDealerPriceCalc', calculate: readonly('DealerPriceCalc') },

            { name: 'roReleaseUserCalc', calculate: readonly('ReleaseUserCalc') },
            { name: 'roReleaseDate', calculate: readonly('ReleaseDate') },

            { name: 'roChangeUserCalc', calculate: readonly('ChangeUserCalc') },
            { name: 'roChangeDate', calculate: readonly('ChangeDate') },

            { name: 'roOtherDirect', calculate: readonly('OtherDirect') },

            { name: 'roLocalServiceStandardWarranty', calculate: readonly('LocalServiceStandardWarranty') },
            { name: 'roLocalServiceStandardWarrantyManual', calculate: readonly('LocalServiceStandardWarrantyManual') },
            { name: 'roLocalServiceStandardWarrantyWithRisk', calculate: readonly('LocalServiceStandardWarrantyWithRisk') },

            { name: 'roCredits', calculate: readonly('Credits') },
            { name: 'roFieldServiceCost', calculate: readonly('FieldServiceCost') },
            { name: 'roServiceSupportCost', calculate: readonly('ServiceSupportCost') },
            { name: 'roLogistic', calculate: readonly('Logistic') },
            { name: 'roAvailabilityFee', calculate: readonly('AvailabilityFee') },
            { name: 'roReinsurance', calculate: readonly('Reinsurance') },
            { name: 'roTaxAndDutiesW', calculate: readonly('TaxAndDutiesW') },
            { name: 'roTaxAndDutiesOow', calculate: readonly('TaxAndDutiesOow') },
            { name: 'roMaterialW', calculate: readonly('MaterialW') },
            { name: 'roMaterialOow', calculate: readonly('MaterialOow') },

            { name: 'roReActiveTC', calculate: readonly('ReActiveTC') },
            { name: 'roReActiveTP', calculate: readonly('ReActiveTP') },
            { name: 'roProActive', calculate: readonly('ProActive') },

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
                name: 'ReleaseUserCalc',
                calculate: function (d) {
                    return d && d.ReleaseUserName ? d.ReleaseUserName : '';
                }
            },
            {
                name: 'ChangeUserCalc',
                calculate: function (d) {
                    return d && d.ChangeUserName ? d.ChangeUserName : '';
                }
            }
        ],

        pageSize: 100,
        autoLoad: false,

        proxy: {
            type: 'ajax',
            api: {
                // read: buildMvcUrl('calc', 'gethwcost')
                read: 'http://localhost:11167/scd/Content/fake/data.json'
            },
            //actionMethods: {
            //    read: 'POST'
            //},
            reader: {
                type: 'json',
                rootProperty: 'items',
                idProperty: "Id",
                totalProperty: 'total'
            },
            paramsAsJson: true
        },

        fixOrUndo: (canEdit: boolean, record, modifiedFieldNames: string[], field) => {

            if (!modifiedFieldNames || modifiedFieldNames.indexOf(field) === -1) {
                return; //no changes
            }

            if (canEdit) {
                setFloatOrEmpty(record, field);
            }
            else {
                record.set(field, record.previousValues[field], { dirty: false });
            }

        },
        listeners: {
            update: (store, record, operation, modifiedFieldNames: string[], details) => {

                store.suspendEvents(false);

                let canEditTC = this.canEditTC();
                let canEditListPrice = this.canEditListPrice();

                store.fixOrUndo(canEditTC, record, modifiedFieldNames, 'ServiceTCManual');
                store.fixOrUndo(canEditTC, record, modifiedFieldNames, 'ReActiveTPManual');
                store.fixOrUndo(canEditTC, record, modifiedFieldNames, 'LocalServiceStandardWarrantyManual');

                store.fixOrUndo(canEditListPrice, record, modifiedFieldNames, 'ListPrice');
                store.fixOrUndo(canEditListPrice, record, modifiedFieldNames, 'DealerDiscount');

                if (modifiedFieldNames && modifiedFieldNames.indexOf('LocalServiceStandardWarrantyManual') !== -1) {
                    this.updateStdw(store, record);
                }

                store.resumeEvents();

                if (modifiedFieldNames && modifiedFieldNames.length > 0 && modifiedFieldNames[0] == SELECTED_FIELD) {
                    this.onCheckChange();
                }
                else {
                    const changed = this.store.getUpdatedRecords().length;
                    this.toggleToolbar(changed == 0);
                }

                this.grid.refresh();
            }
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

    public componentDidMount() {
        document.querySelector('.data-calc').addEventListener('click', this.onMoreDetails);
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

        return <Container layout="fit">

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
                cls="grid-paging-no-count grid-small-head data-calc"
            >

                { /*dependencies*/}

                <Column
                    isHeaderGroup={true}
                    text="Dependencies"
                    dataIndex=""
                    cls="calc-cost-result-green"
                    defaults={{ align: 'center', minWidth: 40, cls: "x-text-el-wrap" }}>

                    <CheckColumn dataIndex={SELECTED_FIELD} sortable={false} width="50" hidden={!this.approved()} />
                    <Column text="FSP code" dataIndex="roFsp" renderer={stringRenderer} minWidth="180" />
                    <Column text="SOG" width="50" dataIndex="roSog" renderer={emptyRenderer} />
                    <Column text="WG" width="50" dataIndex="roWg" />
                    <Column text="Avail." width="50" dataIndex="roAvailability" />
                    <Column text="Duration" dataIndex="roDuration" />
                    <Column text="Reaction type" maxWidth="85" dataIndex="roReactionType" />
                    <Column text="Reaction time" maxWidth="85" dataIndex="roReactionTime" />
                    <Column text="Service location" dataIndex="ServiceLocation" renderer={locationRenderer} />
                    <Column text="ProActive SLA" dataIndex="roProActiveSla" />
                    <Column text="STDW duration" dataIndex="roStdWarranty" renderer={yearRenderer} />
                    <Column text="STDW Service Location" dataIndex="roStdWarrantyLocation" renderer={locationRenderer} />

                </Column>

                { /*Resulting costs*/}

                <Column
                    isHeaderGroup={true}
                    text="Final results"
                    dataIndex=""
                    cls="calc-cost-result-yellow"
                    defaults={{ align: 'center', minWidth: 40, cls: "x-text-el-wrap", renderer: moneyRndr }}>

                    <LinkColumn text="Service TC (calc)" dataIndex="roServiceTC" renderer={moneyRndr} dataAction="view-tc" />
                    <NumberColumn text="Service TC (manual)" dataIndex="ServiceTCManual" editable={canEditTC} />

                    <LinkColumn text="Service TP (calc)" dataIndex="roServiceTP" renderer={moneyRndr} dataAction="view-tp" />
                    <NumberColumn text="Service TP (manual)" dataIndex="roServiceTPManual" />
                    <NumberColumn text="Service TP (released)" dataIndex="roServiceTP_Released" />

                    <NumberColumn text="Local STDW (TP) external" dataIndex="roLocalServiceStandardWarrantyWithRisk" />

                    <NumberColumn text="List price" dataIndex="ListPrice" editable={canEditListPrice} />
                    <NumberColumn text="Dealer discount %" dataIndex="DealerDiscount" editable={canEditListPrice} renderer={percentRenderer} />
                    <NumberColumn text="Dealer price" dataIndex="DealerPriceCalc" />

                    <Column text="Change user" minWidth="60" maxWidth="90" dataIndex="ChangeUserCalc" renderer={emptyRenderer} />
                    <Column text="Change date" dataIndex="roChangeDate" renderer={ddMMyyyyRenderer} />

                    <Column text="Release user" minWidth="60" maxWidth="90" dataIndex="ReleaseUserCalc" renderer={emptyRenderer} />
                    <Column text="Release date" dataIndex="roReleaseDate" renderer={ddMMyyyyRenderer} />

                </Column>

                <Column
                    isHeaderGroup={true}
                    text="Intermediate calculation results"
                    dataIndex=""
                    cls="calc-cost-result-brown"
                    defaults={{ align: 'center', minWidth: 40, cls: "x-text-el-wrap" }}>

                    <LinkColumn text="Local STDW (calc)" dataIndex="roLocalServiceStandardWarranty" renderer={moneyRndr} />
                    <NumberColumn text="Local STDW (manual)" dataIndex="LocalServiceStandardWarrantyManual" editable={canEditTC} renderer={moneyRndr} />
                    <LinkColumn text="Credits" dataIndex="roCredits" renderer={moneyRndr} />
                    <LinkColumn text="ReActive TC (calc)" dataIndex="roReActiveTC" renderer={moneyRndr} />
                    <LinkColumn text="ReActive TP (calc)" dataIndex="roReActiveTP" renderer={moneyRndr} />
                    <NumberColumn text="ReActive TP (manual)" dataIndex="ReActiveTPManual" editable={canEditTC} renderer={moneyRndr} />
                    <LinkColumn text="ProActive (calc)" dataIndex="roProActive" renderer={moneyRndr} />

                </Column>

                {/*cost block results*/}

                <Column
                    isHeaderGroup={true}
                    text="Cost block results"
                    dataIndex=""
                    cls="calc-cost-result-blue"
                    defaults={{ align: 'center', minWidth: 40, cls: "x-text-el-wrap" }}>

                    <LinkColumn text="Field service cost" dataIndex="roFieldServiceCost" renderer={moneyRndr} />
                    <LinkColumn text="Service support cost" dataIndex="roServiceSupportCost" renderer={moneyRndr} />
                    <LinkColumn text="Logistic cost" dataIndex="roLogistic" renderer={moneyRndr} />
                    <LinkColumn text="Avail. fee" dataIndex="roAvailabilityFee" renderer={moneyRndr} />
                    <LinkColumn text="Reinsurance" dataIndex="roReinsurance" renderer={moneyRndr} />
                    <LinkColumn text="Other direct cost" dataIndex="roOtherDirect" renderer={moneyRndr} />
                    <LinkColumn text="Tax &amp; Duties iW period" dataIndex="roTaxAndDutiesW" renderer={moneyRndr} />
                    <LinkColumn text="Tax &amp; Duties OOW period" dataIndex="roTaxAndDutiesOow" renderer={moneyRndr} />
                    <LinkColumn text="Material cost iW period" dataIndex="roMaterialW" renderer={moneyRndr} />
                    <LinkColumn text="Material cost OOW period" dataIndex="roMaterialOow" renderer={moneyRndr} />

                </Column>

            </Grid>

            {this.toolbar()}

            <PlausibilityCheckDialog ref={this.plausiWndRef} />

        </Container>
    }

    private filterRef = (x) => {
        this.filter = x;
    }

    private gridRef = (x) => {
        this.grid = x;
    }

    private plausiWndRef = (x) => {
        this.plausiWnd = x;
    }

    toggleSelectable = field => {
        this.setState({
            selectable: { ...this.state.selectable, [field]: !this.state.selectable[field] }
        });
    };

    setExtensible = extensible => {
        this.setState({ extensible });
    };

    private onMoreDetails = (e) => {
        let target = e.target;
        let action = target.getAttribute('data-action');
        if (!action) {
            return;
        }
        let rowID = target.getAttribute('data-rowid');
        console.log('onMoreDetails', action, rowID);

        this.plausiWnd.show(rowID);
    }

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

        if (this.state && this.state.selectedCountry) {
            const srv = new UserCountryService();
            let cntId = this.state.selectedCountry.id;
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
        this.reRender(); // stub for correct render
    }

    private reRender() {
        this.grid.refresh();
        this.setState({ ______: new Date().getTime() });
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
        let result: boolean = this.approved() && this.state.showInLocalCurrency;
        if (result) {
            const cnt: Country = this.state.selectedCountry;
            result = cnt && cnt.canStoreListAndDealerPrices;
        }
        return result;
    }

    private canEditTC(): boolean {
        let result: boolean = this.approved() && this.state.showInLocalCurrency;
        if (result) {
            const cnt: Country = this.state.selectedCountry;
            result = cnt && cnt.canOverrideTransferCostAndPrice;
        }
        return result;
    }

    private toolbar() {

        let invalid = this.state.disableSearchButton;
        let canedit = this.canEdit();

        let selected = this.approved() ? this.getSelectedRows().length : 0;

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
            {
                selected > 0 && <span>Selected: {selected}record(s)</span>
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
        let stdw = record.get('LocalServiceStandardWarrantyManual');
        for (let i = 0, len = items.count(); i < len; i++) {
            let row = items.getAt(i);
            if (row.get('Country') === cnt && row.get('Wg') === wg) {
                row.set('LocalServiceStandardWarrantyManual', stdw);
            }
        }
    }
}
