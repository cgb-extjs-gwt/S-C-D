import * as React from 'react';
import { FormPanel, NumberField, Button, ComboBoxField, Grid, Column } from '@extjs/ext-react';
import { buildMvcUrl } from "../Common/Services/Ajax";
Ext.require('Ext.grid.plugin.PagingToolbar');
export interface PickerPanelProps {
    value?: string;
    onSendClick: (value: string) => void;
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

    enableSend = () => {
        this.setState({ disableSendButton: false });
    }

    loadUsers = () => {
        this.store.load({
            params: {
                searchString: this.pickerField.getValue()
            },
            callback: function (records, operation, success) {
                var userStore = this.pickerField.getStore();
                userStore.removeAll();
                if (records[0].data.total > 0) {
                    this.setState({ disableSendButton: true });
                    for (var i = 0; i < records[0].data.total; i++) {

                        userStore.add([{ abbr: records[0].data.items[i].userSamAccount, name: records[0].data.items[i].username }]);
                    };
                    this.pickerField.expand();
                }

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
                    width={500}
                    label="Find user name"
                    displayField="name"
                    valueField="code"
                    queryMode="local"
                    labelAlign="placeholder"
                    onKeyUp={() => this.loadUsers()}
                    onChange={() => this.enableSend()}
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


