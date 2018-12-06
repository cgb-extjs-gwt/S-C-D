import * as React from 'react';
import { Grid, Column, CheckColumn, Toolbar, Button, TextField, Container } from '@extjs/ext-react';
import { FilterPanel } from "./CountryFilterPanel";
import { CountryFilterModel } from "./CountryFilterModel";
import { buildMvcUrl } from "../../Common/Services/Ajax";

const CONTROLLER_NAME = 'CountryManagement';

export class CountryGrid extends React.Component {

    private filter: FilterPanel;

    state = {
        disableSaveButton: true
    };

    public constructor(props: any) {
        super(props);
        this.init();
    }

    renderer = (value) => value ? value : " ";

    store = Ext.create('Ext.data.Store', {
        fields: [
            'countryId',
            'countryGroup',
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

    saveRecords = () => {

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


    render() {
        return (
            <Container scrollable={true} >

                <FilterPanel ref="filter" docked="right" onSearch={this.onSearch} />

                <Grid
                    title={'Country Settings'}
                    store={this.store}
                    cls="filter-grid"
                    columnLines={true}
                    width="100%"
                    height="100%"
                    plugins={['pagingtoolbar', 'gridcellediting']}>
                <Column text="Country" dataIndex="countryName" flex={1} />
                <Column text="Group" dataIndex="countryGroup" flex={1} />
                <Column text="LUT" dataIndex="lutCode" flex={1} renderer={this.renderer.bind(this)} />
                <Column text="Digit" dataIndex="countryDigit" flex={1} renderer={this.renderer.bind(this)} />
                <Column text="ISO Code" dataIndex="isO3Code" flex={1} renderer={this.renderer.bind(this)} />
                <Column text="Is Master" dataIndex="isMaster" flex={1} />
                <CheckColumn text="Store List and Dealer Prices" dataIndex="canStoreListAndDealerPrices" flex={2} />
                <CheckColumn text="Override TC and TP" dataIndex="canOverrideTransferCostAndPrice" flex={2} />
                <Column text="Quality Group" dataIndex="qualityGroup" flex={1} editable
                    renderer={this.renderer.bind(this)} >
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
        this.filter = this.refs.filter as FilterPanel;
        //
        this.reload();
    }

    private init() {
        this.onSearch = this.onSearch.bind(this);
        this.onBeforeLoad = this.onBeforeLoad.bind(this);
        //
        this.store.on('beforeload', this.onBeforeLoad);
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
