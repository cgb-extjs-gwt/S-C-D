import * as React from 'react';
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField, Container, TextField, Dialog, GridCell } from '@extjs/ext-react';
import { UserRoleDialog } from './UserRoleDialog'
import { UserRoleGrid } from './UserRoleGrid'
import { buildMvcUrl } from "../../Common/Services/Ajax";

const CONTROLLER_NAME = 'UserRole';
const USER_CONTROLLER_NAME = 'User';
const ROLE_CONTROLLER_NAME = 'Role';
const COUNTRY_CONTROLLER_NAME = 'Country';


Ext.define('UserRole', {
    extend: 'Ext.data.Model',
    fields: [
        'id', 'userId', 'countryId', 'roleId'
    ]
});

export default class RoleCodesContainer extends React.Component {

    state = {
        selectedRecord: null,

        storeUserReady: false,
        storeCountryReady: false,
        storeRoleReady: false,

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
                create: buildMvcUrl(CONTROLLER_NAME, 'SaveAll'),
                read: buildMvcUrl(CONTROLLER_NAME, 'GetAll'),
                update: buildMvcUrl(CONTROLLER_NAME, 'SaveAll'),
                destroy: buildMvcUrl(CONTROLLER_NAME, 'DeleteAll')
            }
        }
    });

    saveRecords = () => {
        this.store.sync({
            scope: this,

            success: function (batch, options) {
                this.store.load();
            },

            failure: (batch, options) => {
                //TODO: show error
                this.store.rejectChanges();
            }
        });
    }

    private onHideDialog = () => {
        this.setState({ isVisibleForm: false })
    }

    private onShowDialog = () => {
        this.setState({ isVisibleForm: true })
    }

    private onSelectRecord = (record) => {
        this.setState({ selectedRecord: record })
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
                read: buildMvcUrl(USER_CONTROLLER_NAME, 'GetAll')
            }
        },
        listeners: {
            datachanged: (store) => {
                this.setState({ storeUserReady: true });
            }
        }
    }
    );   

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
                read: buildMvcUrl(COUNTRY_CONTROLLER_NAME, 'GetAll')
            }
        },
        listeners: {
            datachanged: (store) => {
                this.setState({ storeCountryReady: true });
            }
        }
    }
    );

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
                read: buildMvcUrl(ROLE_CONTROLLER_NAME, 'GetAll')
            }
        },
        listeners: {
            datachanged: (store) => {
                this.setState({ storeRoleReady: true });
            }
        }
    }
    );

    render() {
        const { isVisibleForm, selectedRecord } = this.state;

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
                <UserRoleGrid
                    store={this.store}
                    storeUser={this.storeUser}
                    storeRole={this.storeRole}
                    storeCountry={this.storeCountry}

                    onHideDialog={this.onHideDialog}
                    onShowDialog={this.onShowDialog}
                    onSelectRecord={this.onSelectRecord}
                    saveRecords={this.saveRecords}
                />
                {isVisibleForm &&
                    <UserRoleDialog
                        store={this.store}
                        storeUser={this.storeUser}
                        storeRole={this.storeRole}
                        storeCountry={this.storeCountry}

                        selectedRecord={selectedRecord}
                        isVisibleForm={isVisibleForm}
                        onHideDialog={this.onHideDialog}
                        saveRecords={this.saveRecords}
                    />
                }
            </Container>
        )
    }

}