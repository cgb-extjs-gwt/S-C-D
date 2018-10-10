import * as React from 'react';
import { Grid, Column, CheckColumn, Toolbar, Button, TextField } from '@extjs/ext-react';
import { buildMvcUrl } from "../../../Common/Services/Ajax";

const CONTROLLER_NAME = 'CountryManagement';

Ext.require([
    'Ext.grid.plugin.Editable',
    'Ext.grid.plugin.CellEditing',
]);

class CountryGrid extends React.Component{

    state = {
        disableSaveButton: true
    };

    renderer = (value) => value ? value : " ";

    store = Ext.create('Ext.data.Store', {
        fields: ['countryId',
            'countryGroup', 'countryName', 'lUTCode', 'countryDigit',
            'iSO3Code', 'isMaster',
            'canStoreListAndDealerPrices',
            'canOverrideTransferCostAndPrice', 'qualityGroup'],
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
                exception: function(proxy, response, operation){
                    //TODO: show error
                    console.log(operation.getError());
                }
            }
        },
        listeners:{
            update: (store, record, operation, modifiedFieldNames, details, eOpts) => {
                const modifiedRecords = this.store.getUpdatedRecords();
                if (modifiedRecords.length > 0){
                    this.setState({ disableSaveButton: false});
                }
                else{
                    this.setState({ disableSaveButton: true});
                }
            }
        }
    });

    saveRecords = () => {

        this.store.sync({
            callback: function(batch, options){
                console.log('this is callback');
            },

            success: function(batch, options){
                //TODO: show successfull message box
                console.log('this is success');
            },

            failure: (batch, options) =>
            {
                //TODO: show error
                console.log('this is failure');
            }
            
        });
    }

    
    render(){
        return (<Grid title={'Country Settings'} store={this.store} cls="filter-grid" columnLines={true}
            plugins={['pagingtoolbar', 'gridcellediting']}>
            <Column text="Group" dataIndex="countryGroup" flex={1} />
            <Column text="Country" dataIndex="countryName" flex={1} />
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
                      flex = {1} 
                      handler = { this.saveRecords }
                      iconCls = "x-fa fa-save"
                      disabled = { this.state.disableSaveButton }
                />
             </Toolbar>
        </Grid>);
    }
}

export default CountryGrid;