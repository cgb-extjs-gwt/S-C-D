import * as React from 'react';
import { EditItem } from "../../CostEditor/States/CostBlockStates";
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField, TextField } from '@extjs/ext-react';
import { NamedId } from '../../Common/States/CommonStates';
import { buildMvcUrl } from "../../Common/Services/Ajax";
import { ExtMsgHelper } from "../../Common/Helpers/ExtMsgHelper";

const CONTROLLER_NAME = 'RoleCode';

Ext.require([
    'Ext.grid.plugin.Editable',
    'Ext.grid.plugin.CellEditing',
    'Ext.data.validator.Presence'
]);

Ext.define('RoleCode', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'id', type: 'int' },
        { name: 'name', type: 'string' }
    ],
    belongsTo: 'WarrantyGroup'
});

export default class RoleCodesGrid extends React.Component {
    private grid;

    state = {
        disableSaveButton: true,
        disableDeleteButton: true,
        disableNewButton: false
    };


    store = Ext.create('Ext.data.Store', {
        model: 'RoleCode',
        autoLoad: true,
        //autoSync: true,
        pageSize: 0,
        proxy: {
            type: 'ajax',
            writer: {
                type: 'json',        
                writeAllFields: true,             
                allowSingle: false,
                idProperty: "id"
            },
            reader: {
                type: 'json',        
                idProperty: "id"
            },          
            listeners: {
                exception: function (proxy, response, operation) {
                    if (response.status != 200) {
                        let message = JSON.parse(response.responseText).Message
                        Ext.Msg.alert('Error', message)
                    }
                }               
            },
            api: {             
                create: buildMvcUrl(CONTROLLER_NAME, 'SaveAll'),
                read: buildMvcUrl(CONTROLLER_NAME, 'GetAllActive'),
                update: buildMvcUrl(CONTROLLER_NAME, 'SaveAll'),
                destroy: buildMvcUrl(CONTROLLER_NAME, 'DeactivateAll')
            }
        },
        listeners: {
            update: (store, record, operation, modifiedFieldNames, details, eOpts) => {
                const modifiedRecordsCount = this.store.getUpdatedRecords().length;
                this.saveButtonHandler(modifiedRecordsCount);              
            },
            datachanged: (store) => {
                const modifiedRecordsCount = this.store.getModifiedRecords().length + this.store.getRemovedRecords().length;
                this.saveButtonHandler(modifiedRecordsCount);
            }
        }
    });

    saveButtonHandler = (modifiedRecordsCount) => {
        if (modifiedRecordsCount > 0) {
            this.setState({ disableSaveButton: false });
        }
        else {
            this.setState({ disableSaveButton: true });
        }
    }

    saveRecords = () => {
        this.store.sync({
            scope:this,

            success: function (batch, options) {
                this.setState({
                    disableSaveButton: true,
                    disableDeleteButton: true,
                    disableNewButton: false
                });
                this.store.load();
            },

            failure: (batch, options) => {
                //TODO: show error
                this.store.rejectChanges();
            }      
        });
    }

    newRecord = () => {
        this.store.add(Ext.create('RoleCode', { id: 0, name: 'new' }));
        this.saveRecords();
        this.setState({ disableNewButton: true });
    }

    onDeleteRecords = () => {
        let selected = this.grid.getSelections();
        if (selected.length > 0) {
            ExtMsgHelper.confirm('Delete role code', 'Do you want to remove role code(s)?', () => this.deleteRecords(selected));
        }
    }

    deleteRecords = (selected) => {
        this.store.remove(selected);
        this.saveRecords();
    }

    selectRowHandler = (dataView, records, selected, selection) => {
        if (records) {
            this.setState({
                disableDeleteButton: false
            });
        }
        else {
            this.setState({ disableDeleteButton: true });
        }          
    }

    render() {
        return (
            <Grid
                ref={x => this.grid = x}
                title={'Role codes'}
                store={this.store}
                cls="filter-grid"
                columnLines={true}
                shadow
                onSelectionChange={(dataView, records, selected, selection) => this.selectRowHandler(dataView, records, selected, selection)}
                platformConfig={{
                    desktop: {
                        plugins: {
                            gridcellediting: true
                        }
                    },
                    '!desktop': {
                        plugins: {
                            grideditable: true
                        }
                    }
                }
                }
            >
                <Column
                    text="ID"                 
                    dataIndex="id"
                    width="80"
                />
                <Column
                    text="Role code"
                    flex={1}
                    dataIndex="name"
                    editable                 
                >
                    <TextField required validators={ value => value.trim().length > 0 }/> 
                </Column>

                <Toolbar docked="top">
                    <Button
                        text="New"
                        iconCls="x-fa fa-plus"
                        handler={this.newRecord}
                        disabled={this.state.disableNewButton}
                        width="100"
                        textAlign="left"
                    />               
                </Toolbar>
                <Toolbar docked="bottom">   
                    <Button
                        text="Delete"
                        flex={1}
                        iconCls="x-fa fa-trash"
                        handler={this.onDeleteRecords}
                        disabled={this.state.disableDeleteButton}
                    />
                    <Button
                        text="Save"
                        flex={1}
                        iconCls="x-fa fa-save"
                        handler={this.saveRecords}
                        disabled={this.state.disableSaveButton}
                    />
                </Toolbar>
            </Grid>
        )
    }
}