import * as React from 'react';
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField, Container, TextField, Dialog, GridCell } from '@extjs/ext-react';
import { UserRoleDialog } from './UserRoleDialog'

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
    private userRoleForm: Dialog & any;
    private userComboBox: ComboBoxField & any;
    private countryComboBox: ComboBoxField & any;
    private roleComboBox: ComboBoxField & any;

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
            scope: this,

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

    onNewButtonClick = () => {
        this.setState({ isVisibleForm: true, selectedRecord: null });
    }

    onEditButtonClick = (grid, info) => {
        this.setState({ isVisibleForm: true, selectedRecord: info.record });
    }

    onDeleteButtonClick = (grid, info) => {
        Ext.Msg.confirm("Confirmation", "Are you sure you want to do that?", () => this.deleteRecord(info.record))
    }

    deleteRecord = (record) => {
        this.store.remove(record);
        this.saveRecords();
        this.setState({ disableDeleteButton: true });
    }

    private onHideDialog = () => {
        this.setState({ isVisibleForm: false })
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
                >
                    {this.getUserColumn()}
                    {this.getCountryColumn()}
                    {this.getRoleColumn()}
                    <Column
                        text="Actions"
                        flex={1}
                    >
                        <GridCell
                            tools={{
                                gear: {
                                    tooltip: "Edit",
                                    handler: this.onEditButtonClick
                                },
                                close: {
                                    tooltip: "Delete",
                                    handler: this.onDeleteButtonClick
                                }
                            }}
                            value=""
                        />
                    </Column>
                    <Toolbar docked="top">
                        <Button
                            text="New"
                            flex={1}
                            iconCls="x-fa fa-plus"
                            handler={this.onNewButtonClick}
                            disabled={this.state.disableNewButton}
                        />
                    </Toolbar>
                </Grid>
                {isVisibleForm &&
                    <UserRoleDialog
                        store={this.store}
                        storeUser={this.storeUser}
                        storeRole={this.storeRole}
                        storeCountry={this.storeCountry}
                        selectedRecord={selectedRecord}
                        isValidForm={isValidForm}
                        isVisibleForm={isVisibleForm}
                        onHideDialog={this.onHideDialog}
                    />
                }
            </Container>
        )
    }

}