import * as React from 'react';
import { FormPanel, NumberField, Button, ComboBoxField, Grid, Column } from '@extjs/ext-react';
Ext.require('Ext.grid.plugin.PagingToolbar');
export interface PickerPanelProps {
    value?: number;
    onSendClick: (value: number) => void;
    onCancelClick: () => void;
}

export default class PickerPanel extends React.Component<PickerPanelProps, any> {
    ComboBoxField: any;
    private numberField: NumberField & any;
    state = {
        disableSendButton: true
    };

    store = Ext.create('Ext.data.Store', {
        pageSize: 50,
        fields: ['Username', 'SamAccount'],
        autoLoad: true,
        proxy: {
            type: 'ajax',
            api: {
                read: '/api/Users/SearchUser',
                update: '/api/Users/SelectUser'
            },
            reader: {
                type: 'json',
                rootProperty: 'items',
                totalProperty: 'total'
            },
            writer: {
                type: 'json',
                writeAllFields: true,
                allowSingle: false
            },
            listeners: {
                exception: function (proxy, response, operation) {
                    //TODO: show error
                }
            }
        },
        listeners: {
            update: (store, record, operation, modifiedFieldNames, details, eOpts) => {
                const modifiedRecords = this.store.getUpdatedRecords();
                if (modifiedRecords.length > 0) {
                    this.setState({ disableSaveButton: false });
                }
                else {
                    this.setState({ disableSaveButton: true });
                }
            }
        }
    });

    loadUsers = () => {

    }

    public render() {
        const { value, onSendClick, onCancelClick } = this.props;
        const data = [
            { "name": "AAA", "abbr": "AE" },
            { "name": "BBB", "abbr": "BE" },
        ]
        return (
            <FormPanel>
                <ComboBoxField
                    //store={this.store}
                    options={data}
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
                    text="Send"
                    handler={() => onSendClick(this.ComboBoxField.getValue())}
                    disabled={this.state.disableSendButton}
                />
                <Button text="Cancel" handler={onCancelClick} />
            </FormPanel>
        );
    }
}


