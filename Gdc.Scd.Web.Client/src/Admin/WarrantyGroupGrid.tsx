import * as React from 'react';
import { FieldType } from "../CostEditor/States/CostEditorStates";
import { EditItem } from "../CostEditor/States/CostBlockStates";
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField, SelectionColumn } from '@extjs/ext-react';
import { NamedId } from '../Common/States/CommonStates';

Ext.require([
    'Ext.grid.plugin.Editable',
    'Ext.grid.plugin.CellEditing',
]);

Ext.define('WarrantyGroup', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'id', type: 'int' },
        { name: 'name', type: 'string' }
    ],

    hasOne: {model:'RoleCode', name:'roleCode'}

});

export default class RoleCodesGrid extends React.Component {
    state = {
        disableSaveButton: true,
        disableDeleteButton: true,
        disableNewButton: false,
        selectedRecord: null
    };


    store = Ext.create('Ext.data.Store', {
        //model: 'WarrantyGroup',
        field: ['id','name', 
            {
                name: 'roleCode',
                mapping: 'roleCode'
        }],

        autoLoad: true,
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
                read: '/api/WarrantyGroup/GetAll',
                update: '/api/WarrantyGroup/SaveAll'
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

    storeRG = Ext.create('Ext.data.Store', {
        fields:['id','name'],
        autoLoad: true,
        pageSize: 0,
        sorters: [ {
            property: 'name',
            direction: 'ASC'
        }],
        proxy: {
            type: 'ajax',
            reader: {
                type: 'json'
            },
            api: {
                read: '/api/RoleCode/GetAll'
            }
        }}
    );

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
            scope: this,
            callback: function (batch, options) {
                console.log('this is callback');
            },

            success: function (batch, options) {
                //TODO: show successfull message box
                console.log('this is success');
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
                console.log('this is failure');
            }

        });
    }

    reloadStore = () => {
        this.store.load();
    }

    newRecord = () => {
        this.store.add(Ext.create('RoleCode', { id: 0, name: 'new' }));
        this.setState({ disableNewButton: true });
        console.log(this.store);
    }

    deleteRecord = () => {
        this.store.remove(this.state.selectedRecord);
        this.setState({ disableDeleteButton: true });
    }

    selectRowHandler = (dataView, records, selected, selection) => {
        if (records[0]) {
            this.setState({
                selectedRecord: records[0],
                disableDeleteButton: false
            });
        }
        else {
            this.setState({ disableDeleteButton: true });
        }
    }

    private getValueColumn() {
        let columnOptions;
        let renderer: (value, data: { data }) => string;
        var options = this.storeRG.data.items.map(item => ({ text: item.data.name, value: item.data })).slice();
        if(options.length)
        columnOptions = (
            <SelectField
                options={options}
                valueField="value"
                queryMode="local"
            />
        );
        console.log(this.store);
        renderer = (value, { data }) => {
        let result: string;

        if (data.roleCode) {
            const selectedItem = this.storeRG.data.items.find(item => item.data.id == data.roleCode.id);

            result = selectedItem.data.name;
        } 

        return result;
    }

        return (
            <Column
                text="Role code"
                dataIndex="roleCode"
                flex={1}
                editable={true}
                renderer={renderer.bind(this)}
            >
                {columnOptions}
            </Column>
        )
    }

    render() {
        const props = this.props;
        const store = this.store;
        return (
            <Grid
                title={'Warranty groups'}
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
                    text="WG"
                    dataIndex="name"
                    flex={1}
                />
                {this.getValueColumn()}
                


                <Toolbar docked="bottom">
                    <Button
                        text="New"
                        flex={1}
                        iconCls="x-fa fa-plus"
                        handler={this.newRecord}
                        disabled={this.state.disableNewButton}
                    />
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
                        disabled={this.state.disableSaveButton}
                    />
                </Toolbar>
            </Grid>
        )
    }
}