import * as React from 'react';
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField, Container, TextField, Dialog, GridCell } from '@extjs/ext-react';

interface UserRoleDialogProps {
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
    private userComboBox: ComboBoxField & any;
    private countryComboBox: ComboBoxField & any;
    private roleComboBox: ComboBoxField & any;

    state = {
        countryFieldHidden: true,
        isValid: false
    }

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
                        <ComboBoxField
                            ref={combobox => this.userComboBox = combobox}
                            store={storeUser}
                            valueField="id"
                            displayField="name"
                            label="User"
                            queryMode="local"
                            value={selectedRecord && selectedRecord.data.userId}
                            editable={false}
                            required={true}
                            onChange={this.isValidInput.bind(this)}
                        />
                        <ComboBoxField
                            store={storeRole}
                            valueField="id"
                            displayField="name"
                            label="Role"
                            queryMode="local"
                            value={selectedRecord && selectedRecord.data.roleId}
                            ref={combobox => this.roleComboBox = combobox}
                            editable={false}
                            required={true}
                            onChange={this.onRoleChange}
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
                            hidden={this.state.countryFieldHidden}
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
        const { store, onHideDialog, selectedRecord, saveRecords } = this.props;

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
        saveRecords();
        onHideDialog();

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
            this.setState({ countryFieldHidden: selectedRole.data.isGlobal });
        }
        else {
            this.setState({ countryFieldHidden: true });
        }
        this.isValidInput();
    }

    private isValidInput = () => {
        if (this.userComboBox && this.userComboBox.getValue() > 0 &&
            this.roleComboBox && this.roleComboBox.getValue() > 0 &&
            ((!this.state.countryFieldHidden && this.countryComboBox && this.countryComboBox.getValue() > 0) || this.state.countryFieldHidden)) {

            this.setState({ isValid: true })
        }
        else {
            this.setState({ isValid: false })
        }
    }
}