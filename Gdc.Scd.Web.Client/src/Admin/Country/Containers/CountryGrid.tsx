import * as React from 'react';
import { Grid, Column, CheckColumn, Toolbar, Button } from '@extjs/ext-react';
import * as CountryManagementState from '../../../Dict/States/CountryStates';


class CountryGrid extends React.Component{

    state = {
        disableSaveButton: true
    };

    store = new Ext.data.Store({
        fields: ['id', 'name', 'canOverrideListAndDealerPrices', 'showDealerPrice', 'canOverrideTransferCostAndPrice'],
        autoLoad: true,
        proxy: {
            type: 'ajax',
            api: {
                read: '/api/CountryManagement/GetAll',
                update: '/api/CountryManagement/SaveAll'
            },
            reader: {
                type: 'json',
                idProperty: 'id'
            },
            writer: {
                type: 'json',
                writeAllFields: true,
                idProperty: 'id',
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
                this.store.rejectChanges();
                console.log('this is failure');
            }
            
        });
    }

    
    render(){
        return ( <Grid title={ 'Country Settings' } store={ this.store } cls="filter-grid" columnLines= {true} >
                    <Column text="Country Name" dataIndex="name" flex={1} />
                    <CheckColumn text="Can Override List and Dealer Price" dataIndex="canOverrideListAndDealerPrices" flex={1} />
                    <CheckColumn text="Show Dealer Price" dataIndex="showDealerPrice" flex={1} />
                    <CheckColumn text="Can Override TC and TP" dataIndex="canOverrideTransferCostAndPrice" flex={1} />

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