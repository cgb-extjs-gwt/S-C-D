import * as React from 'react';
import { Grid, Column, CheckColumn, Toolbar, Button } from '@extjs/ext-react';


class AvailabilityFeeAdminGrid extends React.Component{

    state = {
        disableSaveButton: true
    };

    store = Ext.create('Ext.data.Store', {
        pageSize: 50,
        fields: ['countryName', 'countryId', 'reactionTimeName', 
                 'reactionTimeId', 'reactionTypeName', 'reactionTypeId',
                'serviceLocatorName', 'serviceLocatorId', 'isApplicable', 'innerId'],
        autoLoad: true,
        proxy: {
            type: 'ajax',
            api: {
                read: '/api/AvailabilityFeeAdmin/GetAll',
                update: '/api/AvailabilityFeeAdmin/SaveAll'
            },
            reader: {
                type: 'json',
                rootProperty: 'combinations',
                totalProperty: 'totalCount'
            },
            writer: {
                type: 'json',
                writeAllFields: true,
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
        console.log(this.store);
        return ( <Grid title={ 'Availability Fee Settings' } store={ this.store } cls="filter-grid" columnLines= {true} plugins={['pagingtoolbar']} >
                    <Column text="Country" dataIndex="countryName" flex={1} />
                    <Column text="Reaction Time" dataIndex="reactionTimeName" flex={1} />
                    <Column text="Reaction Type" dataIndex="reactionTypeName" flex={1} />
                    <Column text="Service Locator" dataIndex="serviceLocatorName" flex={1} />
                    <CheckColumn text="Is Applicable" dataIndex="isApplicable" flex={1} />

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

export default AvailabilityFeeAdminGrid;