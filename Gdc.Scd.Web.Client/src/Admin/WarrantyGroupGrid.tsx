import * as React from 'react';
import { FieldType } from "../CostEditor/States/CostEditorStates";
import { EditItem } from "../CostEditor/States/CostBlockStates";
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField, SelectionColumn, CheckBoxField } from '@extjs/ext-react';
import { NamedId } from '../Common/States/CommonStates';

Ext.require([
    'Ext.grid.plugin.Editable',
    'Ext.grid.plugin.CellEditing',
]);

export default class RoleCodesGrid extends React.Component {
    state = {
        render: false,
        disableSaveButton: true,
        disableCancelButton: true,
        disableManageRoleCodesButton: false,
        selectedRecord: null
    };


    store = Ext.create('Ext.data.Store', {
        fields: ['id','name', 'roleCodeId','roleCode'],

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
            api: {
                read: '/api/WarrantyGroup/GetAll',
                update: '/api/WarrantyGroup/SaveAll'
            }
        },
        listeners: {
            update: (store, record, operation, modifiedFieldNames, details, eOpts) => {
                const modifiedRecordsCount = this.store.getUpdatedRecords().length;
                this.saveButtonHandler(modifiedRecordsCount);
            }
        }
    });

    storeRC = Ext.create('Ext.data.Store', {
        fields:['id','name'],
        autoLoad: false,
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
        },
        listeners: {
            datachanged: (store) => {
                this.setState({ render: true });
            }
        }}
    );

    saveButtonHandler = (modifiedRecordsCount) => {
        if (modifiedRecordsCount > 0) {
            this.setState({ disableSaveButton: false, disableCancelButton:false });
        }
        else {
            this.setState({ disableSaveButton: true, disableCancelButton: true });
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
                    disableCancelButton: true
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

    ManageRoleCodes = () => {
        window.location.href = "/admin/role-code-management";
    }

    cancelChanges = () => {
        this.store.rejectChanges();
        this.setState({ disableCancelButton: true });
        this.store.load();
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

    filterOnChange = (chkBox, newValue, oldValue) => {
        if (newValue)
            this.store.filter('roleCode', 'null');       
        else
            this.store.clearFilter();
    }

    

    private getValueColumn() {
        let selectField;
        let renderer: (value, data: { data }) => string;

        selectField = (
            <SelectField             
                store={this.storeRC}
                valueField="id"
                displayField="name"            
                label="Select role code"
                queryMode="local"
            />
        );
        renderer = (value, { data }) => {
            let result: string;
            if (this.state.render) {
                if (data.roleCodeId > 0) {
                    const selectedItem = this.storeRC.data.items.find(item => item.data.id === data.roleCodeId);
                    result = selectedItem.data.name;
                    console.log('this is callback');
                } else
                    result = "";
            }           
            return result;
        }

        return (
            <Column
                text="Role code"
                dataIndex="roleCodeId"
                flex={1}
                editable={true}
                renderer={renderer.bind(this)}
            >
                {selectField}
            </Column>
        )
    }

    render() {
        if (!this.state.render) {
            this.storeRC.load();
            return null;
        }
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
                <Toolbar docked="top">
                    <CheckBoxField boxLabel="Show only WGs with no Role code" onChange={(chkBox, newValue, oldValue) => this.filterOnChange(chkBox, newValue, oldValue)} />
                </Toolbar>
                <Toolbar docked="bottom">
                    <Button
                        text="Manage role codes"
                        flex={1}
                        iconCls="x-fa fa-users"
                        handler={this.ManageRoleCodes}
                    />
                    <Button
                        text="Cancel"
                        flex={1}
                        iconCls="x-fa fa-trash"
                        handler={this.cancelChanges}
                        disabled={this.state.disableCancelButton}
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