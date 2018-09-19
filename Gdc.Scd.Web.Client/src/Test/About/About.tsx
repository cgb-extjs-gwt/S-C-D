import * as React from 'react';
import { Container, Button } from '@extjs/ext-react';
import PickerWindow from '../PickerWindow';

interface PickerState {
    isVisible: boolean;
}

class About extends React.Component<any, PickerState> {
    public componentWillMount() {
        this.setState({ isVisible: false })
    }

    store = Ext.create('Ext.data.Store', {
        pageSize: 50,
        fields: ['countryName', 'countryId', 'reactionTimeName',
            'reactionTimeId', 'reactionTypeName', 'reactionTypeId',
            'serviceLocatorName', 'serviceLocatorId', 'isApplicable', 'innerId'],
        autoLoad: true,
        proxy: {
            type: 'ajax',
            api: {
                read: '/api/AvailabilityFeeAdmin/GetAll',
                update: '/api/AvailabilityFeeAdmin/SaveAll'
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
                    //this.setState({ disableSaveButton: false });
                }
                else {
                    //this.setState({ disableSaveButton: true });
                }
            }
        }
    });

    saveRecords = () => {

        this.store.sync({
            callback: function (batch, options) {
            },

            success: function (batch, options) {
                //TODO: show successfull message box
            },

            failure: (batch, options) => {
                //TODO: show error
            }

        });
    }
    render() {
        return (
            <Container padding="20">
                <h1>About this App</h1>
                <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam eget leo sed mi imperdiet dictum a id turpis. Suspendisse a ante eu lorem lacinia vestibulum. Suspendisse volutpat malesuada ante, sed fermentum massa auctor in. Praesent semper sodales feugiat. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Mauris mauris ante, suscipit id metus non, venenatis tempor tortor. In ornare tempor ipsum. Sed rhoncus augue urna, ut dapibus odio fringilla vitae. Phasellus malesuada mauris ut nulla varius sodales. Sed et leo luctus, venenatis felis sit amet, vehicula nibh. Curabitur fringilla fringilla nibh, porttitor lacinia urna vestibulum eu. Integer ac aliquet risus. Curabitur imperdiet quis purus at consectetur. Sed ornare vitae felis a scelerisque. Donec mi purus, auctor sit amet molestie nec, imperdiet auctor mauris.</p>
                <Button
                    text="People Picker"
                    handler={() => this.showPickerWindow()}
                />
                <PickerWindow
                    isVisible={this.state.isVisible}
                    onSendClick={this.onSendDialogClick}
                    onCancelClick={this.onCancelClick}
                />
            </Container>

        );
    }

    private onCancelClick = () => {
        this.setState({
            ...this.state,
            isVisible: false
        });
    }
    private showPickerWindow = () => {
        this.setState({
            ...this.state,
            isVisible: true
        });
    }
    private onSendDialogClick = (value: number) => {


        this.setState({
            ...this.state,
            isVisible: false
        });
    }
}

export default About;