import * as React from 'react';
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField, Container, TextField, FormPanel, Dialog } from '@extjs/ext-react';

Ext.require([
    'Ext.grid.plugin.Editable',
    'Ext.grid.plugin.CellEditing',
]);

Ext.define('UserRole', {
    extend: 'Ext.data.Model',
    fields: [
       'id', 'userId', 'countryId', 'roleId'
    ]
});

export default class RoleCodesGrid extends React.Component {
    private userRoleForm: FormPanel & any;

    state = {
        disableSaveButton: true,
        disableDeleteButton: true,
        disableNewButton: false,
        selectedRecord: null,
       
        
        storeUserReady: false,
        storeCountryReady: false,
        storeRoleReady: false,

        isValidForm: false,
        isVisibleForm: false
    };


    store = Ext.create('Ext.data.Store', {
        model: 'UserRole',
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
                create: '/api/userrole/SaveAll',
                read: '/api/userrole/GetAll',
                update: '/api/userrole/SaveAll',
                destroy: '/api/userrole/DeleteAll'
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

    reloadStore = () => {
        this.store.load();
    }

    newRecord = () => {
        this.setState({ isVisibleForm: true });
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

    storeUser = Ext.create('Ext.data.Store', {
        fields: ['id', 'name'],
        autoLoad: false,
        pageSize: 0,
        sorters: [{
            property: 'name',
            direction: 'ASC'
        }],
        proxy: {
            type: 'ajax',
            reader: {
                type: 'json'
            },
            api: {
                read: '/api/User/GetAll'
            }
        },
        listeners: {
            datachanged: (store) => {
                this.setState({ storeUserReady: true });
            }
        }
    }
    );

    private getUserColumn() {
        let selectField;
        let renderer: (value, data: { data }) => string;

        renderer = (value, { data }) => {
            let result: string;
            if (this.state.storeUserReady) {
                if (data.userId > 0) {
                    const selectedItem = this.storeUser.data.items.find(item => item.data.id === data.userId);
                    result = selectedItem.data.name;
                } else
                    result = "";
            }
            return result;
        }

        return (
            <Column
                text="User"
                dataIndex="userId"
                flex={1}
                renderer={renderer.bind(this)}
            />
        )
    }

    storeCountry = Ext.create('Ext.data.Store', {
        fields: ['id', 'name'],
        autoLoad: false,
        pageSize: 0,
        sorters: [{
            property: 'name',
            direction: 'ASC'
        }],
        proxy: {
            type: 'ajax',
            reader: {
                type: 'json'
            },
            api: {
                read: '/api/Country/GetAll'
            }
        },
        listeners: {
            datachanged: (store) => {
                this.setState({ storeCountryReady: true });
            }
        }
    }
    );

    private getCountryColumn() {
        let selectField;
        let renderer: (value, data: { data }) => string;

        renderer = (value, { data }) => {
            let result: string;
            if (this.state.storeCountryReady) {
                if (data.countryId > 0) {
                    const selectedItem = this.storeCountry.data.items.find(item => item.data.id === data.countryId);
                    result = selectedItem.data.name;
                } else
                    result = "";
            }
            return result;
        }

        return (
            <Column
                text="Country"
                dataIndex="countryId"
                flex={1}
                renderer={renderer.bind(this)}
            />
        )
    }

    storeRole = Ext.create('Ext.data.Store', {
        fields: ['id', 'name'],
        autoLoad: false,
        pageSize: 0,
        sorters: [{
            property: 'name',
            direction: 'ASC'
        }],
        proxy: {
            type: 'ajax',
            reader: {
                type: 'json'
            },
            api: {
                read: '/api/Role/GetAll'
            }
        },
        listeners: {
            datachanged: (store) => {
                this.setState({ storeRoleReady: true });
            }
        }
    }
    );

    private getRoleColumn() {
        let selectField;
        let renderer: (value, data: { data }) => string;

        renderer = (value, { data }) => {
            let result: string;
            if (this.state.storeRoleReady) {
                if (data.roleId > 0) {
                    const selectedItem = this.storeRole.data.items.find(item => item.data.id === data.roleId);
                    result = selectedItem.data.name;
                } else
                    result = "";
            }
            return result;
        }

        return (
            <Column
                text="Role"
                dataIndex="roleId"
                flex={1}
                renderer={renderer.bind(this)}
            />
        )
    }

    render() {
        const { isValidForm, isVisibleForm, selectedRecord } = this.state;

        if (!this.state.storeUserReady) {
            this.storeUser.load();
            return null;
        }
        if (!this.state.storeCountryReady) {
            this.storeCountry.load();
            return null;
        }
        if (!this.state.storeRoleReady) {
            this.storeRole.load();
            return null;
        }
       
        return (
            <Container layout="fit">            
            <Grid
                title={'User roles'}
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
                {this.getUserColumn()} 
                {this.getCountryColumn()} 
                {this.getRoleColumn()}  
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
            {
            isVisibleForm &&
                        <Dialog
                        displayed={isVisibleForm}
                        closable
                        closeAction="hide"
                        ref={form => this.userRoleForm = form}
                        platformConfig={{
                            "!phone": {
                                maxHeight: 500,
                                width: 350
                            }
                        }}
                        onHide={this.onFormCancel}
                        >
                        <TextField
                            //ref={textField => this.rejectMessageTextField = textField}
                                label="Name"
                                required
                                //onChange={this.onRejectMessageTextFieldChange}
                                value={selectedRecord && selectedRecord.userId}                              
                        />
                        <TextField
                            //ref={textField => this.rejectMessageTextField = textField}
                            label="Country"
                            required
                            //onChange={this.onRejectMessageTextFieldChange}
                            value={selectedRecord && selectedRecord.countryId}
                        />
                        <TextField
                            //ref={textField => this.rejectMessageTextField = textField}
                            label="Role"
                            required
                            //onChange={this.onRejectMessageTextFieldChange}
                            value={selectedRecord && selectedRecord.roleId}
                        />
                        <Toolbar docked="bottom">
                            <Button
                                text="Save"
                                handler={this.onFormSave}
                                flex={1}
                                disabled={!isValidForm}
                            />
                            <Button text="Cancel" handler={this.onFormCancel} flex={1} />
                        </Toolbar>
                    </Dialog>
            }
            </Container>
        )      
    }

    private onFormSave = () => {
        this.setState({ isVisibleForm: false })
    }

    private onFormCancel = () => {
        this.setState({ isVisibleForm: false })
    }
}