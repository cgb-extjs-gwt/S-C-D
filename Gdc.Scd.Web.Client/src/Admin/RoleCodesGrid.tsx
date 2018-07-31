import * as React from 'react';
import { FieldType } from "../CostEditor/States/CostEditorStates";
import { EditItem } from "../CostEditor/States/CostBlockStates";
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField } from '@extjs/ext-react';
import { NamedId } from '../Common/States/CommonStates';

Ext.require([
    'Ext.grid.plugin.Editable',
    'Ext.grid.plugin.CellEditing',
]);

Ext.define('RoleCode', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'id', type: 'int' },
        { name: 'name', type: 'string' }
    ]
});

export default class RoleCodesGrid extends React.Component {
    state = {
        disableSaveButton: true,
        disableDeleteButton: true,
        selectedRecord: null
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
                    console.log(operation.getError());
                }
            },
            api: {             
                create: '/api/rolecode/SaveAll',
                read: '/api/rolecode/GetAll',
                update: '/api/rolecode/SaveAll',
                destroy: '/api/rolecode/DeleteAll'
            }
        },
        listeners: {
            update: (store, record, operation, modifiedFieldNames, details, eOpts) => {
                const modifiedRecords = this.store.getModifiedRecords();
                
                if (modifiedRecords.length > 0) {
                    this.setState({ disableSaveButton: false });
                }
                else {
                    this.setState({ disableSaveButton: true });
                }
                console.log(this.state);
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
                this.store.rejectChanges();
                console.log('this is failure');
            }

        });
    }

    newRecord = () => {
        this.store.add(Ext.create('RoleCode', { id: -1, name: 'new' }));
    }

    deleteRecord = () => {
        this.store.remove(this.state.selectedRecord);
        console.log(this.state.selectedRecord);
    }

    selectRowHandler = (dataView, records) => {
        if (records[0]) {
            this.setState({
                selectedRecord: records[0],
                disableDeleteButton: false
            });
        }
        else {
            this.setState({ disableDeleteButton: true });
        }          
        console.log(this.state);
    }

    render() {
        const props = this.props;
        const store = this.store;
        return (
            <Grid
                title={'Role codes'}
                store={this.store}
                cls="filter-grid"
                columnLines={true}
                shadow
                onSelect={(dataView, records) => this.selectRowHandler(dataView, records)}
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
                    flex={1}
                    dataIndex="id"                   
                />
                <Column
                    text="Role code"
                    flex={1}
                    dataIndex="name"
                    editable
                />       
                <Toolbar docked="bottom">   
                    <Button
                        text="New"
                        flex={1}
                        iconCls="x-fa fa-plus"
                        handler={ this.newRecord }
                    />
                    <Button
                        text="Delete"
                        flex={1}
                        iconCls="x-fa fa-trash"
                        handler={ this.deleteRecord }
                    />
                    <Button
                        text="Save"
                        flex={1}
                        iconCls="x-fa fa-save"
                        disabled= { this.store.disableSaveButton }
                        handler= { this.saveRecords }
                    />
                </Toolbar>
            </Grid>
        )
    }
}