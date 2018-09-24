import * as React from 'react';
import { Dialog, Button } from '@extjs/ext-react';
import PickerPanel, { PickerPanelProps } from './PickerPanelHelper';

export interface PickerWindowProps extends PickerPanelProps {
    isVisible: boolean;
}

export default class PickerWindowHelper extends React.Component<PickerWindowProps, any> {
    private dialog: Dialog;
    private pickerPanel: PickerPanel;

    state = {
        disableSendButton: true
    };

    public render() {
        const { isVisible, value, onSendClick, onCancelClick } = this.props;

        return (
            <Dialog
                displayed={isVisible}
                title="Edit Cells"
                closeAction="hide"
                ref={dialog => this.dialog = dialog}
            >
                <PickerPanel
                    ref={pickerPanel => this.pickerPanel = pickerPanel}
                    value={value}
                    onSendClick={onSendClick}
                    onCancelClick={onCancelClick}
                />
                <Button                   
                    text="Send"
                    handler={() => onSendClick(this.pickerPanel.getUserIdentity())}
                />
                <Button text="Cancel" handler={onCancelClick} />
            </Dialog>
        );
    }
}