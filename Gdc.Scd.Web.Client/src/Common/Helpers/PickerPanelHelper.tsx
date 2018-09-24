import * as React from 'react';
import { FormPanel, NumberField, Button, ComboBoxField, Grid, Column, ComboBox } from '@extjs/ext-react';
import { buildMvcUrl } from "../Services/Ajax"
Ext.require('Ext.grid.plugin.PagingToolbar');
export interface PickerPanelProps {
    value?: string;
    onSendClick: (value: string) => void;
    onCancelClick: () => void;
}

const CONTROLLER_NAME = 'User';

export default class PickerPanelHelper extends React.Component<PickerPanelProps, any> {
    private pickerField: ComboBoxField & any;
    private numberField: NumberField & any;
    private sendButton: Button & any;

    private userList = [

    ];
    store = Ext.create('Ext.data.Store', {
        autoLoad: true,
        fields: ['item', 'name'],
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
                this.pickerField.collapse();
                var userStore = this.pickerField.getStore();
                userStore.removeAll();
                if (records[0].data.total > 0) {
                    this.setState({ disableSendButton: true });
                    for (var i = 0; i < records[0].data.total; i++) {

                        userStore.add([{
                            name: records[0].data.items[i].name,
                            item: records[0].data.items[i]
                        }]);
                    };
                    this.pickerField.expand();
                }

            },
            scope: this
        });
    }
    getUserIdentity = () => {
        return this.pickerField.getValue();
    }
    public render() {
        const { value, onSendClick, onCancelClick } = this.props;
        return (
                <ComboBox
                    ref={combobox => this.pickerField = combobox}
                    store={this.store}
                    label="Find user name"
                    displayField="name"
                    valueField="item"
                    queryMode="remote"
                    labelAlign="placeholder"
                    onKeyUp={() => this.loadUsers()}
                    onChange={() => this.enableSend()}
                    valueNotFoundText="no results"
                    hideTrigger
                    typeAhead
                />
        );
    }
}


