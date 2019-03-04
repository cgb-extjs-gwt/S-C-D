import * as React from 'react';
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField, Container, TextField, Dialog, GridCell } from '@extjs/ext-react';
import { UserRoleDialog } from '../Components/UserRoleDialog'
import { UserRoleGrid } from '../Components/UserRoleGrid'
import { buildMvcUrl } from "../../../Common/Services/Ajax";
import { UserRoleFilterPanel } from "../Filter/UserRoleFilterPanel"; 
import { UserRoleFilterModel } from "../Filter/UserRoleFilterModel";
import { UserRoleService } from "../Services/UserRoleService";
import { ExportService } from "../../../Report/Services/ExportService";

const CONTROLLER_NAME = 'UserRole';
const USER_CONTROLLER_NAME = 'User';
const ROLE_CONTROLLER_NAME = 'Role';
const COUNTRY_CONTROLLER_NAME = 'Country';


Ext.define('UserRole', {
    extend: 'Ext.data.Model',
    fields: [
        'id', 'userId', 'roleId',
        {
            name: 'countryId', type: 'int',
            convert: function (val, row) {
                if (!val)
                    return '';
                return val;
            }
        }
    ]
});

Ext.define('User', {
    extend: 'Ext.data.Model',
    fields: [
        'id', 'name', 'email', 'login'
    ]
});

export default class RoleCodesContainer extends React.Component {
    private filter: UserRoleFilterPanel;
    private srv: UserRoleService;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    private init() {
        this.srv = new UserRoleService();
        this.onSearch = this.onSearch.bind(this);
    }

    public componentDidMount() {
        this.filter = this.refs.filter as UserRoleFilterPanel;

        Promise.all([
            this.srv.getRoles(),
            this.srv.getCountries()
        ]).then(x => {
            this.setState({
                ...this.state,
                roles: x[0].sort(this.compare),
                countries: x[1].sort(this.compare),
                storeCountryReady: true,
                storeRoleReady:true
            });
        });

        this.store.load();
    }

    private compare = (a, b) => {
        if (a.name > b.name) {
            return 1;
        }
        if (a.name < b.name) {
            return -1;
        }
        return 0;
    }

    state = {
        users: [],
        roles: [],
        countries: [],

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

    private onSearch(filter: UserRoleFilterModel) {
        this.store.clearFilter()
        filter.user ? this.store.filterBy(record => record.data.userId == filter.user) : false
        filter.role ? this.store.filterBy(record => record.data.roleId == filter.role) : false
        filter.country ? this.store.filterBy(record => record.data.countryId == filter.country) : false
    }

    private onDownload(filter: UserRoleFilterModel & any) {
        filter = filter || {};
        ExportService.Download('User-Roles', null, filter);
    }

    storeUser = Ext.create('Ext.data.Store', {
        model: 'User',
        autoLoad: false,
        pageSize: 0,
        sorters: [{
            property: 'name',
            direction: 'ASC'
        }],
        proxy: {
            type: 'ajax',
            writer: {
                type: 'json',
                writeAllFields: true,
                allowSingle: false,
                idProperty: "id"
            },
            reader: {
                type: 'json'
            },
            api: {
                create: buildMvcUrl(USER_CONTROLLER_NAME, 'SaveAll'),
                read: buildMvcUrl(USER_CONTROLLER_NAME, 'GetAll'),
                update: buildMvcUrl(CONTROLLER_NAME, 'SaveAll'),
                destroy: buildMvcUrl(CONTROLLER_NAME, 'DeleteAll')
            }
        },
        listeners: {
            datachanged: (store) => {
                this.setState({ storeUserReady: true });
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
        if (!this.state.storeCountryReady || !this.state.storeRoleReady) {
            return null;
        }

        return (
            <Container layout="fit">
                <UserRoleFilterPanel
                    ref="filter"
                    docked="right"
                    onSearch={this.onSearch.bind(this)}
                    onDownload={this.onDownload.bind(this)}
                    storeUser={this.storeUser}
                    roles={this.state.roles}
                    countries={this.state.countries}
                    scrollable={true}
                />
                <UserRoleGrid
                    store={this.store}
                    storeUser={this.storeUser}
                    roles={this.state.roles}
                    countries={this.state.countries}

                    onHideDialog={this.onHideDialog}
                    onShowDialog={this.onShowDialog}
                    onSelectRecord={this.onSelectRecord}
                    saveRecords={this.saveRecords}
                />
                {isVisibleForm &&
                    <UserRoleDialog
                        store={this.store}
                        storeUser={this.storeUser}
                        roles={this.state.roles}
                        countries={this.state.countries}

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