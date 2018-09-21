import * as React from 'react';
import { Dialog } from '@extjs/ext-react';
import PickerPanel, { PickerPanelProps } from './PickerPanelHelper';

interface PickerWindowProps extends PickerPanelProps {
    isVisible: boolean;
}

export default class PickerWindowHelper extends React.Component<PickerWindowProps, any> {
    private dialog: Dialog;

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
                    value={value}
                    onSendClick={onSendClick}
                    onCancelClick={onCancelClick}
                />
            </Dialog>
        );
    }
}