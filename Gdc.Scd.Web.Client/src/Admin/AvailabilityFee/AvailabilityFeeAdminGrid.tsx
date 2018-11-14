import * as React from 'react';
import { Grid, Column, CheckColumn, Toolbar, Button } from '@extjs/ext-react';
import { buildMvcUrl } from "../../Common/Services/Ajax";

const CONTROLLER_NAME = 'AvailabilityFeeAdmin';

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
                read: buildMvcUrl(CONTROLLER_NAME, 'GetAll'),
                update: buildMvcUrl(CONTROLLER_NAME, 'SaveAll')
            },
            reader: {
                type: 'json',
                rootProperty: 'items',
                totalProperty: 'total'
            },
            writer: {
                type: 'json',
                writeAllFields: true,
                allowSingle: false
            },
            listeners: {
                exception: function(proxy, response, operation){
                    //TODO: show error
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
            },

            success: function(batch, options){
                //TODO: show successfull message box
            },

            failure: (batch, options) =>
            {
                //TODO: show error
            }
            
        });
    }

    render(){
        return (<Grid title={ 'Availability Fee Settings' } store={ this.store } cls="filter-grid" columnLines= {true} plugins={['pagingtoolbar']} >
                    <Column text="Country" dataIndex="countryName" flex={1} />
                    <Column text="Reaction Time" dataIndex="reactionTimeName" flex={1} />
                    <Column text="Reaction Type" dataIndex="reactionTypeName" flex={1} />
                    <Column text="Service Location" dataIndex="serviceLocatorName" flex={1} />
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