import * as React from 'react';
import { FormPanel, NumberField, Button, ComboBoxField, Grid, Column } from '@extjs/ext-react';
import { buildMvcUrl } from "../Common/Services/Ajax";
Ext.require('Ext.grid.plugin.PagingToolbar');
export interface PickerPanelProps {
    value?: number;
    onSendClick: (value: number) => void;
    onCancelClick: () => void;
}

const CONTROLLER_NAME = 'Users';
Ext.define('User', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'abbr', type: 'string' },
        { name: 'name', type: 'string' }
    ]
});
export default class PickerPanel extends React.Component<PickerPanelProps, any> {
    private pickerField: ComboBoxField & any;
    private numberField: NumberField & any;
    private sendButton: Button & any;
    state = {
        disableSendButton: true
    };
    private userList = [
        
    ];
    store = Ext.create('Ext.data.Store', {
        autoLoad: true,
        fields: ['abbr', 'name'],
        data: [
            
        ],
        proxy: {
            type: 'ajax',
            api: {
                read: buildMvcUrl(CONTROLLER_NAME, 'SearchUser'),
            },
            reader: {
                type: 'json',
                successProperty: 'success',
                messageProperty: 'message'
            },
            writer: {
                type: 'json',
                encode: 'true',
            }
        },
        listeners: {
            exception: function (proxy, response, operation) {
                //TODO: show error
                console.log(operation.getError());
            }
        }
    });

    loadUsers = () => {
        this.store.load({
            params: {
                searchString: this.pickerField.getValue()
            },
            callback: function (records, operation, success) {
                if (records[0].data.total > 0)
                    records[0].data.items.forEach(function (element) {
                        var user = Ext.create('User', {
                            abbr: element.SamAccountName, name: element.DisplayName
                        });
                        var userStore = Ext.getStore(this);
                        userStore.add(user)
                        //this.userList = [
                            
                        //];
                    });
            },
            scope: this
        });
    }

    public render() {
        const { value, onSendClick, onCancelClick } = this.props;
        return (
            <FormPanel>
                <ComboBoxField
                    ref={combobox => this.pickerField = combobox}
                    store={this.store}
                    //options={this.userList}
                    width={200}
                    label="Find user name"
                    displayField="name"
                    valueField="code"
                    queryMode="local"
                    labelAlign="placeholder"
                    onKeyUp={() => this.loadUsers()}
                    hideTrigger 
                    typeAhead
                />
                <Button
                    ref={button => this.sendButton = button}
                    text="Send"
                    handler={() => onSendClick(this.pickerField.getValue())}
                    disabled={this.state.disableSendButton}
                />
                <Button text="Cancel" handler={onCancelClick} />
            </FormPanel>
        );
    }
}


