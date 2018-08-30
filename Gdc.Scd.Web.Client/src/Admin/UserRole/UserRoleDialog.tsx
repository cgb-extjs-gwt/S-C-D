import * as React from 'react';
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField, Container, TextField, Dialog, GridCell } from '@extjs/ext-react';

interface UserRoleDialogProps {
    store
    storeUser
    storeCountry
    storeRole

    selectedRecord
    isValidForm
    isVisibleForm
    onHideDialog?()
}

interface UserRoleDialogState {
    selectedRecord
    isValidForm
    isVisibleForm
}

export class UserRoleDialog extends React.Component<UserRoleDialogProps, UserRoleDialogState> {
    constructor(props) {
        super(props);

        this. state = {
            selectedRecord: this.props.selectedRecord,
            isValidForm: this.props.isValidForm,
            isVisibleForm: this.props.isVisibleForm
        }; 
    }

    private userRoleForm: Dialog & any;
    private userComboBox: ComboBoxField & any;
    private countryComboBox: ComboBoxField & any;
    private roleComboBox: ComboBoxField & any;

   

    saveRecords = () => {
        const { store } = this.props;
        store.sync({
            scope: this,

            success: function(batch, options) {
                this.setState({
                    disableSaveButton: true,
                    disableDeleteButton: true,
                    disableNewButton: false
                });
                this.store.load();
            },

            failure: (batch, options) => {
                //TODO: show error
                store.rejectChanges();
            }
        });
    }

    render() {
        const { isValidForm, isVisibleForm, selectedRecord } = this.props;
        const { store, storeUser, storeCountry, storeRole } = this.props;

        return (                      
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
                        <ComboBoxField
                            ref={combobox => this.userComboBox = combobox}
                            store={storeUser}
                            valueField="id"
                            displayField="name"
                            label="User"
                            queryMode="local"
                            value={selectedRecord && selectedRecord.data.userId}
                        />
                        <ComboBoxField
                            store={storeCountry}
                            valueField="id"
                            displayField="name"
                            label="Country"
                            queryMode="local"
                            value={selectedRecord && selectedRecord.data.countryId}
                            ref={combobox => this.countryComboBox = combobox}
                        />
                        <ComboBoxField
                            store={storeRole}
                            valueField="id"
                            displayField="name"
                            label="Role"
                            queryMode="local"
                            value={selectedRecord && selectedRecord.data.roleId}
                            ref={combobox => this.roleComboBox = combobox}
                        />
                        <Toolbar docked="bottom">
                            <Button
                                text="Save"
                                handler={this.onFormSave.bind(this)}
                                flex={1}
                            />
                            <Button text="Cancel" handler={this.onFormCancel} flex={1} />
                        </Toolbar>
                    </Dialog>
        )
    }

    private onFormSave = () => {
        const { store, onHideDialog, selectedRecord } = this.props;

        if (selectedRecord) {
            selectedRecord.set({
                userId: this.userComboBox.getValue(),
                countryId: this.countryComboBox.getValue(),
                roleId: this.roleComboBox.getValue()
            });
        }
        else {
            let newItem = Ext.create('UserRole', {
                userId: this.userComboBox.getValue(),
                roleId: this.roleComboBox.getValue(),
                countryId: this.countryComboBox.getValue()
            });
            store.add(newItem);
            newItem.set({ id: 0 });
        };
        this.saveRecords();
        onHideDialog();

    }

    private onFormCancel = () => {
        const { store, onHideDialog } = this.props;
        onHideDialog();
    }
}