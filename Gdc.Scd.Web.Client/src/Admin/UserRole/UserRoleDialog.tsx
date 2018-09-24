import * as React from 'react';
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField, Container, TextField, Dialog, GridCell } from '@extjs/ext-react';
import PickerPanel, { PickerPanelProps } from '../../Common/Helpers/PickerPanelHelper';

interface UserRoleDialogProps{
    store
    storeUser
    storeCountry
    storeRole

    selectedRecord
    isVisibleForm
    onHideDialog?()
    saveRecords?()   
}

export class UserRoleDialog extends React.Component<UserRoleDialogProps> {
    private userRoleForm: Dialog & any;
    private userPickerPanel: PickerPanel & any;
    private countryComboBox: ComboBoxField & any;
    private roleComboBox: ComboBoxField & any;


    private countryFieldHidden = true;
    private isVisible = false;

    state={
        ...this.state,
        isValid: false
    };

    render() {
        const { isVisibleForm, selectedRecord } = this.props;
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
                <PickerPanel
                    ref={pickerPanel => this.userPickerPanel = pickerPanel}
                    onSendClick={this.onSendDialogClick}
                    onCancelClick={this.onCancelClick}
                />
                <ComboBoxField
                    ref={combobox => this.roleComboBox = combobox}
                    store={storeRole}
                    valueField="id"
                    displayField="name"
                    label="Role"
                    queryMode="local"
                    value={selectedRecord && selectedRecord.data.roleId}                  
                    editable={false}
                    required={true}
                    onChange={this.onRoleChange.bind(this)}
                />
                <ComboBoxField
                    ref={combobox => this.countryComboBox = combobox}
                    store={storeCountry}
                    valueField="id"
                    displayField="name"
                    label="Country"
                    queryMode="local"
                    value={selectedRecord && selectedRecord.data.countryId}
                    editable={false}
                    required={true}
                    hidden={this.countryFieldHidden}
                    onChange={this.isValidInput.bind(this)}
                />                  
                <Toolbar docked="bottom">
                    <Button
                        text="Save"
                        handler={this.onFormSave.bind(this)}
                        flex={1}
                        disabled={!this.state.isValid}
                    />
                    <Button text="Cancel" handler={this.onFormCancel} flex={1} />
                </Toolbar>
            </Dialog>
        )
    }

    private onFormSave = () => {
        const { store, storeUser, onHideDialog, selectedRecord, saveRecords } = this.props;
        this.getUserId(this).then((userId) => {
            if (selectedRecord) {
                selectedRecord.set({
                    userId: userId,
                    countryId: this.countryComboBox.getValue(),
                    roleId: this.roleComboBox.getValue()
                });
            }
            else {
                let newUser = Ext.create('UserRole', {
                    userId: userId,
                    roleId: this.roleComboBox.getValue(),
                    countryId: this.countryComboBox.getValue()
                });
                store.add(newUser);
                newUser.set({ id: 0 });
            }
            saveRecords();
            onHideDialog();
        })}        

    private getUserId = (context) => {
        return new Promise(function (resolve, reject) {
            const { storeUser } = context.props;
            let pickedUser = context.userPickerPanel.getUserIdentity();
            let userIndex = storeUser.find('email', pickedUser.email)
            let user = null;
            if (userIndex == -1) {
                user = Ext.create('User', {
                    name: pickedUser.name,
                    login: pickedUser.login,
                    email: pickedUser.email
                })
                storeUser.add(user)
                user.set({ id: 0 })
                storeUser.sync({
                    scope: this,

                    success: function (batch, options) {
                        storeUser.load(function (records, operation, success) {
                            userIndex = storeUser.find('email', pickedUser.email)
                            user = storeUser.data.items[userIndex]

                            resolve(user.id)
                        });
                    },
                    failure: (batch, options) => {
                        //TODO: show error
                        storeUser.rejectChanges();
                        reject();
                    }
                });
            }
            else {
                user = storeUser.data.items[userIndex]

                resolve(user.id)
            }
        })
    }

    private onFormCancel = () => {
        const { onHideDialog } = this.props;
        onHideDialog();
    }

    private onRoleChange = (combobox) => {
        const { storeRole } = this.props;
        const roleId = combobox.getValue();
        if (roleId && roleId > 0) {
            const selectedRole = storeRole.data.items.find(item => item.data.id === roleId);
            if (selectedRole.data.isGlobal) {
                this.countryComboBox.setValue(null);
            }
            this.countryFieldHidden = selectedRole.data.isGlobal
        }
        else {
            this.countryFieldHidden = true
        }
        this.isValidInput();
    }

    private isValidInput = () => {
        if (this.userPickerPanel && this.userPickerPanel.getUserIdentity() &&
            this.roleComboBox && this.roleComboBox.getValue() > 0 &&
            ((!this.countryFieldHidden && this.countryComboBox && this.countryComboBox.getValue() > 0) || this.countryFieldHidden)) {
            this.setState({
                ...this.state,
                isValid: true
            });
        }
        else {
            this.setState({
                ...this.state,
                isValid: false
            });
        }
    }

    private onCancelClick = () => {
        this.setState({
            ...this.state,
            isVisible: false
        });
    }

    private onSendDialogClick = (value: string) => {
        this.isVisible = false
    }
}