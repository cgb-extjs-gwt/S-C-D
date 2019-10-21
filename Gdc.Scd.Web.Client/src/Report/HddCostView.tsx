import { Button, Column, Container, Grid, NumberColumn, Toolbar } from "@extjs/ext-react";
import * as React from "react";
import { ExtDataviewHelper } from "../Common/Helpers/ExtDataviewHelper";
import { handleRequest } from "../Common/Helpers/RequestHelper";
import { buildMvcUrl, post } from "../Common/Services/Ajax";
import { UserCountryService } from "../Dict/Services/UserCountryService";
import { CalcCostProps } from "./Components/CalcCostProps";
import { setFloatOrEmpty, readonly } from "./Components/GridExts";
import { ddMMyyyyRenderer, emptyRenderer, moneyRenderer, percentRenderer } from "./Components/GridRenderer";
import { HddCostFilter } from "./Components/HddCostFilter";
import { HddCostFilterModel } from "./Model/HddCostFilterModel";
import { ExportService } from "./Services/ExportService";

Ext.require([
    'Ext.grid.plugin.Clipboard'
]);

export class HddCostView extends React.Component<CalcCostProps, any> {

    private grid: Grid;

    private filter: HddCostFilter;

    private store = Ext.create('Ext.data.Store', {

        fields: [
            'wgId', 'listPrice', 'dealerDiscount', 'changeUserName', 'changeUserEmail', 'changeDate',

            { name: 'readonlyWg', calculate: readonly('wg') },
            { name: 'readonlySog', calculate: readonly('sog') },
            { name: 'readonlyHdd', calculate: readonly('hddRetention') },
            { name: 'readonlyChangeDate', calculate: readonly('changeDate') },
            {
                name: 'dealerPriceCalc',
                calculate: function (d) {
                    let result: any;
                    if (d && d.listPrice) {
                        result = d.listPrice;
                        if (d.dealerDiscount) {
                            result = result - (result * d.dealerDiscount / 100);
                        }
                    }
                    return result;
                }
            },
            {
                name: 'changeUserCalc',
                calculate: function (d) {
                    let result: string = '';
                    if (d) {
                        if (d.changeUserName) {
                            result += d.changeUserName;
                        }
                        if (d.changeUserEmail) {
                            result += '[' + d.changeUserEmail + ']';
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
            actionMethods: {
                read: 'POST'
            },
            reader: {
                type: 'json',
                rootProperty: 'items',
                idProperty: "wgId",
                totalProperty: 'total'
            },
            paramsAsJson: true
        },
        listeners: {
            update: (store, record) => {
                const changed = this.store.getUpdatedRecords().length;
                this.toggleToolbar(changed == 0);

                store.suspendEvents(false);
                setFloatOrEmpty(record, 'transferPrice');
                setFloatOrEmpty(record, 'listPrice');
                setFloatOrEmpty(record, 'dealerDiscount');
                store.resumeEvents();
            }
        },

    });

    private selectable: any = {
        extensible: 'both',
        rows: true,
        cells: true,
        columns: true,
        drag: true,
        checkbox: false
    };

    public state = {
        disableSaveButton: true,
        disableCancelButton: true,
        isAdmin: false
    };

    private pluginCfg: any;

    public constructor(props: CalcCostProps) {
        super(props);
        this.init();
    }

    public componentDidMount() {
        new UserCountryService().isAdminUser().then(x => this.setState({ isAdmin: x }));
    }

    public render() {

        let canEdit: boolean = this.canEdit();
        let isAdmin: boolean = this.state.isAdmin;

        return (
            <Container layout="fit">

                <HddCostFilter
                    ref={this.filterRef}
                    docked="right"
                    onSearch={this.onSearch}
                    onDownload={this.onDownload}
                    scrollable={true} />

                <Grid
                    ref={this.gridRef}
                    store={this.store}
                    width="100%"
                    platformConfig={this.pluginCfg}
                    selectable={this.selectable}
                    cls="grid-paging-no-count"
                >

                    { /*dependencies*/}

                    <Column
                        flex="2"
                        isHeaderGroup={true}
                        text="Dependencies"
                        dataIndex=""
                        cls="calc-cost-result-green"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap" }}>

                        <Column text="WG(Asset)" dataIndex="readonlyWg" />
                        <Column text="SOG(Asset)" dataIndex="readonlySog" renderer={emptyRenderer} />

                    </Column>

                    { /*Resulting costs*/}

                    <Column
                        flex="6"
                        isHeaderGroup={true}
                        text="Resulting costs"
                        dataIndex=""
                        cls="calc-cost-result-blue"
                        defaults={{ align: 'center', minWidth: 100, flex: 1, cls: "x-text-el-wrap", renderer: moneyRenderer }}>

                        <NumberColumn text="HDD retention" dataIndex="readonlyHdd" hidden={!isAdmin} />

                        <NumberColumn text="Transfer price" dataIndex="transferPrice" editable={canEdit} />
                        <NumberColumn text="List price" dataIndex="listPrice" editable={canEdit} />
                        <NumberColumn text="Dealer discount in %" dataIndex="dealerDiscount" editable={canEdit} renderer={percentRenderer} />
                        <NumberColumn text="Dealer price" dataIndex="dealerPriceCalc" />

                        <Column flex="2" minWidth="250" text="Change user" dataIndex="changeUserCalc" renderer={emptyRenderer} />
                        <Column text="Change date" dataIndex="readonlyChangeDate" renderer={ddMMyyyyRenderer} />

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

    private init() {
        this.onSearch = this.onSearch.bind(this);
        this.onDownload = this.onDownload.bind(this);
        this.cancelChanges = this.cancelChanges.bind(this);
        this.saveRecords = this.saveRecords.bind(this);
        //
        this.store.on('beforeload', this.onBeforeLoad, this);
        //
        if (this.approved()) { //using CanEdit() does not work cause await http request
            this.pluginCfg = this.editPluginConf();
        } else {
            this.pluginCfg = this.readPluginConf();
        }
    }

    private approved() {
        return this.props.approved;
    }

    private canEdit(): boolean {
        return this.props.approved && this.state.isAdmin;
    }

    private cancelChanges() {
        this.store.rejectChanges();
        this.toggleToolbar(true);
    }

    private onSearch(filter: HddCostFilterModel) {
        this.reload();
    }

    private onDownload(filter: HddCostFilterModel & any) {
        filter = filter || {};
        ExportService.Download('HDD-RETENTION-CALC-RESULT', this.props.approved, filter);
    }

    private onBeforeLoad(s, operation) {
        this.reset();
        //
        let filter = this.filter.getModel() as any;
        filter.approved = this.props.approved;
        let params = Ext.apply({}, operation.getParams(), filter);
        operation.setParams(params);
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

    private reload() {
        ExtDataviewHelper.refreshToolbar(this.grid);
        this.store.load();
    }

    private reset() {
        this.setState({
            disableCancelButton: true,
            disableSaveButton: true
        });
    }

    private saveRecords() {
        let recs = this.store.getModifiedRecords().map(x => x.getData());

        if (recs) {
            let p = post('hdd', 'savecost', recs).then(() => {
                this.reset();
                this.reload();
            });
            handleRequest(p);
        }
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

    private toggleToolbar(disable: boolean) {
        this.setState({ disableSaveButton: disable, disableCancelButton: disable });
    }
}