import * as React from 'react';
import { FieldType } from "../../CostEditor/States/CostEditorStates";
import { EditItem } from "../../CostEditor/States/CostBlockStates";
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField, TextField } from '@extjs/ext-react';
import { NamedId } from '../../Common/States/CommonStates';
import { buildMvcUrl } from "../../Common/Services/Ajax";

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
    state = {
        disableSaveButton: true,
        disableDeleteButton: true,
        disableNewButton: false,
        deletedRecord: null,
        selectedRecord: null,
        isValid: true
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
                    //TODO: show error
                    if (response.responseText.includes("The DELETE statement conflicted with the REFERENCE constraint")) {
                        Ext.Msg.alert('Error', 'This item cannot be deleted because it is still referenced by other items.')
                    }
                }               
            },
            api: {             
                create: buildMvcUrl(CONTROLLER_NAME, 'SaveAll'),
                read: buildMvcUrl(CONTROLLER_NAME, 'GetAll'),
                update: buildMvcUrl(CONTROLLER_NAME, 'SaveAll'),
                destroy: buildMvcUrl(CONTROLLER_NAME, 'DeleteAll')
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

    deleteRecord = () => {
        this.state.deletedRecord = this.state.selectedRecord
        this.store.remove(this.state.selectedRecord);
        this.setState({ disableDeleteButton: true, disableNewButton: false });
    }

    selectRowHandler = (dataView, records, selected, selection) => {
        if (!this.state.deletedRecord && records[0]) {
            this.setState({
                selectedRecord: records[0],
                disableDeleteButton: false
            });
        }
        else {
            this.setState({ disableDeleteButton: true });
        }          
    }

    render() {
        let isValid = this.state.isValid;

        return (
            <Grid
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
                        handler={this.deleteRecord}
                        disabled={this.state.disableDeleteButton}
                    />
                    <Button
                        text="Save"
                        flex={1}
                        iconCls="x-fa fa-save"
                        handler={this.saveRecords}
                        disabled={this.state.disableSaveButton || !isValid}
                    />
                </Toolbar>
            </Grid>
        )
    }
}