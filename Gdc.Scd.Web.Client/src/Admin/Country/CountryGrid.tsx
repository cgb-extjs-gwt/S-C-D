import { Button, CheckColumn, Column, ComboBoxField, Container, Grid, TextField, Toolbar } from '@extjs/ext-react';
import * as React from 'react';
import { buildMvcUrl } from "../../Common/Services/Ajax";
import { CurrencyService } from '../../Dict/Services/CurrencyService';
import { UserCountryService } from '../../Dict/Services/UserCountryService';
import { CountryFilterModel } from "./CountryFilterModel";
import { FilterPanel } from "./CountryFilterPanel";

const CONTROLLER_NAME = 'CountryManagement';

export class CountryGrid extends React.Component {

    private filter: FilterPanel;

    public state = {
        disableSaveButton: true,
        isAdmin: false,
        curs: null
    };

    public constructor(props: any) {
        super(props);
        this.init();
    }

    private renderer = (value) => value ? value : " ";

    private store = Ext.create('Ext.data.Store', {
        fields: [
            'countryId',
            'countryGroup',
            'region',
            'countryName',
            'lUTCode',
            'countryDigit',
            'iSO3Code',
            'isMaster',
            'canStoreListAndDealerPrices',
            'canOverrideTransferCostAndPrice',
            'qualityGroup'
        ],
        autoLoad: true,
        proxy: {
            type: 'ajax',
            api: {
                read: buildMvcUrl(CONTROLLER_NAME, 'GetAll'),
                update: buildMvcUrl(CONTROLLER_NAME, 'SaveAll'),
            },
            reader: {
                type: 'json',
                rootProperty: 'items',
                totalProperty: 'total'
            },
            writer: {
                type: 'json',
                writeAllFields: true,
                idProperty: 'countryId',
                allowSingle: false
            },
            listeners: {
                exception: function (proxy, response, operation) {
                    //TODO: show error
                    console.log(operation.getError());
                }
            }
        },
        listeners: {
            update: (store, record, operation, modifiedFieldNames, details, eOpts) => {
                const modifiedRecords = this.store.getUpdatedRecords();
                if (modifiedRecords.length > 0) {
                    this.setState({ disableSaveButton: false });
                }
                else {
                    this.setState({ disableSaveButton: true });
                }
            }
        }
    });

    private saveRecords = () => {

        this.store.sync({
            callback: function (batch, options) {
                console.log('this is callback');
            },

            success: function (batch, options) {
                //TODO: show successfull message box
                console.log('this is success');
            },

            failure: (batch, options) => {
                //TODO: show error
                console.log('this is failure');
            }

        });
    }

    public render() {
        return (
            <Container scrollable={true} >

                <FilterPanel ref={x => this.filter = x} docked="right" onSearch={this.onSearch} scrollable={true} />

                <Grid
                    title={'Country Settings'}
                    store={this.store}
                    cls="filter-grid"
                    columnLines={true}
                    width="100%"
                    height="100%"
                    plugins={['gridcellediting']}>
                    <Column text="Country" dataIndex="countryName" flex={1} />
                    <Column text="Country Group" dataIndex="countryGroup" flex={1} />
                    <Column text="Region" dataIndex="region" flex={1} />
                    <Column text="LUT" dataIndex="lutCode" flex={1} renderer={this.renderer} />
                    <Column text="Digit" dataIndex="countryDigit" flex={1} renderer={this.renderer} />
                    <Column text="ISO Code" dataIndex="isO3Code" flex={1} renderer={this.renderer} />

                    <Column text="Currency Code" dataIndex="currency" flex={1} renderer={this.renderer} editable={this.canEditCurrency()}>
                        {this.createCurrecyField()}
                    </Column>

                    <CheckColumn text="Is Master" dataIndex="isMaster" flex={1} disabled={true} />
                    <CheckColumn text="Store List and Dealer Prices" dataIndex="canStoreListAndDealerPrices" flex={2} />
                    <CheckColumn text="Override TC and TP" dataIndex="canOverrideTransferCostAndPrice" flex={1} />
                    <Column text="Quality Group" dataIndex="qualityGroup" flex={1} editable renderer={this.renderer} >
                        <TextField />
                    </Column>

                    <Toolbar docked="bottom">
                        <Button
                            text="Save"
                            flex={1}
                            handler={this.saveRecords}
                            iconCls="x-fa fa-save"
                            disabled={this.state.disableSaveButton}
                        />
                    </Toolbar>
                </Grid>
            </Container>);
    }

    public componentDidMount() {
        this.reload();
        new CurrencyService().getAll().then(x => this.setState({ curs: x }));
        new UserCountryService().isAdminUser().then(x => this.setState({ isAdmin: x }));
    }

    private createCurrecyField() {
        let cmp = null;

        if (this.canEditCurrency()) {
            cmp = <ComboBoxField
                store={this.state.curs}
                valueField="name"
                displayField="name"
                queryMode="local"
                editable={false}
                height="100%"
                width="100%"
            />;
        }

        return cmp;
    }

    private canEditCurrency() {
        return this.state.isAdmin && this.state.curs;
    }

    private init() {
        this.onSearch = this.onSearch.bind(this);
        this.store.on('beforeload', this.onBeforeLoad, this);
    }

    private onSearch(filter: CountryFilterModel) {
        this.reload();
    }

    private reload() {
        this.store.currentPage = 1
        this.store.load();
    }

    private onBeforeLoad(s, operation) {
        let filter = this.filter.getModel();
        let params = Ext.apply({}, operation.getParams(), filter);
        operation.setParams(params);
    }
}
