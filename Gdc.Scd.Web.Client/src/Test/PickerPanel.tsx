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
        autoLoad: true,
        fields: ['abbr', 'name'],
        data: [
            //{ "abbr": "AL", "name": "Alabama" },
            //{ "abbr": "BG", "name": "Bbbbb" }
        ],
        proxy: {
            type: 'ajax',
            api: {
                read: '/api/Users/SearchUser',
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
        console.log("1");
        this.store.load({
            params: {
                searchString: "B"
            },
            callback: function (records, operation, success) {

            },
            scope: this
        });
    }

    public render() {
        const { value, onSendClick, onCancelClick } = this.props;
        return (
            <FormPanel>
                <ComboBoxField
                    store={this.store}
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


