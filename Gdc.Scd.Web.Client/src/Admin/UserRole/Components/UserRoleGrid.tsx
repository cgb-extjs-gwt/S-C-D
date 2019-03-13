import { Button, Column, Grid, GridCell, Toolbar } from '@extjs/ext-react';
import * as React from 'react';

interface UserRoleGridProps {
    store
    storeUser
    countries
    roles

    onHideDialog?()
    onShowDialog?()
    onSelectRecord?(record)
    saveRecords?(store)
}

export class UserRoleGrid extends React.Component<UserRoleGridProps> {

    onNewButtonClick = () => {
        const { onShowDialog, onSelectRecord } = this.props;       
        onSelectRecord(null);
        onShowDialog();
    }

    onEditButtonClick = (grid, info) => {
        const { onShowDialog, onSelectRecord } = this.props;
        onSelectRecord(info.record);
        onShowDialog();
    }

    onDeleteButtonClick = (grid, info)  => {
        Ext.Msg.confirm("Confirmation", "Are you sure you want to delete this record?", (buttonId, record) => this.deleteRecord(buttonId, info.record))
    }

    deleteRecord = (buttonId, record) => {
        if (buttonId == 'yes') {
            const { store, saveRecords } = this.props;
            store.remove(record);
            saveRecords(store);
        }     
    }

    private getUserColumn() {
        let renderer: (value, data: { data }) => string;
        const { storeUser } = this.props;
        renderer = (value, { data }) => {
            let result: string;
            if (data.userId > 0) {
                const selectedItem = storeUser.getById(data.userId);
                result = selectedItem.data.name;
            } else
                result = "";
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


    private getCountryColumn() {
        let renderer: (value, data: { data }) => string;
        const { countries } = this.props;
        renderer = (value, { data }) => {
            let result: string;
            if (data.countryId > 0) {
                const selectedItem = countries.find(item => item.id === data.countryId);
                result = selectedItem.name;
            } else
                result = "";
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

    private getRoleColumn() {
        let renderer: (value, data: { data }) => string;
        const { roles } = this.props;
        renderer = (value, { data }) => {
            let result: string;
            if (data.roleId > 0) {
                const selectedItem = roles.find(item => item.id === data.roleId);
                result = selectedItem.name;
            } else
                result = "";           
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

    private getEmailColumn() {
        let renderer: (value, data: { data }) => string;
        const { storeUser } = this.props;
        renderer = (value, { data }) => {
            let result: string;
            if (data.userId > 0) {
                const selectedItem = storeUser.getById(data.userId);
                result = selectedItem.data.email;
            } else
                result = "";
            return result;
        }
        
        return (
            <Column
                text="E-mail"
                dataIndex=""
                flex={1}
                renderer={renderer.bind(this)}
            />
        )
    }

    private getActionColumn() {
         return (
            <Column
                text="Actions"
                flex={0.5}
                dataIndex=""
                renderer={() => { return " " }} //it displays some simbol otherwise
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
                />

            </Column>
        )
    }

    render() {  
        const { store } = this.props;
        return (         
            <Grid
                title={'User roles'}
                store={store}
                cls="filter-grid"
                columnLines={true}
                shadow
            >
                {this.getUserColumn()} 
                {this.getEmailColumn()}
                {this.getRoleColumn()}  
                {this.getCountryColumn()} 
                {this.getActionColumn()}

                <Toolbar docked="top">   
                    <Button
                        text="New"
                        iconCls="x-fa fa-plus"
                        handler={this.onNewButtonClick} 
                        width="100"
                        textAlign="left"
                    />                 
                </Toolbar>
            </Grid>                
        )      
    }

}