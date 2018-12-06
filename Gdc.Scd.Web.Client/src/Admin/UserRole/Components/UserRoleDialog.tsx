import * as React from 'react';
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField, Container, TextField, Dialog, GridCell } from '@extjs/ext-react';
import PickerPanel, { PickerPanelProps } from '../../../Common/Helpers/PickerPanelHelper';

interface UserRoleDialogProps{
    store
    storeUser
    countries
    roles

    selectedRecord
    isVisibleForm
    onHideDialog?()
    saveRecords?()   
}

export class UserRoleDialog extends React.Component<UserRoleDialogProps, any> {
    private userRoleForm: Dialog & any;
    private userPickerPanel: PickerPanel & any;
    private countryComboBox: ComboBoxField & any;
    private roleComboBox: ComboBoxField & any;

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public componentDidMount() {
        const { roles, selectedRecord } = this.props;
        if (selectedRecord) {
            let selectedRole = roles.find(item=>item.id==selectedRecord.data.roleId)
            this.setState({
                ...this.state,
                countryFieldHidden: selectedRole && selectedRole.isGlobal
            })              
        }         
    }

    public init() {       
        this.state = {
            ...this.state,
            countryFieldHidden: true,
            isVisible: false,
            isValid: false
        }     
    }

    render() {
        const { isVisibleForm, selectedRecord } = this.props;
        const { store, storeUser, countries, roles } = this.props;

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
                defaults={{
                    valueField: 'id',
                    displayField: 'name',
                    queryMode: 'local',
                    editable: false,
                    required: true
                }}
            >
                <PickerPanel
                    ref={pickerPanel => this.userPickerPanel = pickerPanel}
                    value={selectedRecord && storeUser.getById(selectedRecord.data.userId)}    
                    onChange={this.isValidInput.bind(this)}
                />
                <ComboBoxField
                    ref={combobox => this.roleComboBox = combobox}
                    options={roles}
                    label="Role"
                    value={selectedRecord && selectedRecord.data.roleId}                  
                    onChange={this.onRoleChange.bind(this)}
                />
                <ComboBoxField
                    ref={combobox => this.countryComboBox = combobox}
                    options={countries}
                    label="Country"
                    value={selectedRecord && selectedRecord.data.countryId}
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
                if (store.findBy(record => record.data.userId == newUser.data.userId &&
                    record.data.roleId == newUser.data.roleId &&
                    record.data.countryId == newUser.data.countryId)==-1) {
                    store.add(newUser);
                    newUser.set({ id: 0 });
                }
                else
                {
                    Ext.Msg.alert('Error', 'This user role already exists')
                }
                
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
        const { roles } = this.props;
        const roleId = combobox.getValue();
        if (roleId && roleId > 0) {
            const selectedRole = roles.find(item => item.id === roleId);
            if (selectedRole.isGlobal) {
                this.countryComboBox.setValue(null);
            }
            this.setState({
                ...this.state,
                countryFieldHidden: selectedRole.isGlobal
            });
        }
        else {
            this.setState({
                ...this.state,
                countryFieldHidden: true
            });
        }
        this.isValidInput();
    }

    private isValidInput = () => {
        if (this.userPickerPanel && this.userPickerPanel.getUserIdentity() &&
            this.roleComboBox && this.roleComboBox.getValue() > 0 &&
            ((!this.state.countryFieldHidden && this.countryComboBox && this.countryComboBox.getValue() > 0) || this.state.countryFieldHidden)) {
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
}