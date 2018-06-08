import * as React from 'react';
import { Dialog } from '@extjs/ext-react';
import EditCellsPanel, { EditCellsPanelProps } from './EditCellsPanel';

interface EditCellsWindowProps extends EditCellsPanelProps {
    isVisible: boolean;
}

export default class EditCellsWindow extends React.Component<EditCellsWindowProps, any> {
    private dialog: Dialog;

    public render() {
        const { isVisible, value, onEditClick, onCancelClick } = this.props;

        return (
            <Dialog 
                displayed={isVisible} 
                title="Edit Cells" 
                closeAction="hide"
                ref={dialog => this.dialog = dialog}
            >
                <EditCellsPanel 
                    value={value} 
                    onEditClick={onEditClick}
                    onCancelClick={onCancelClick}
                />
            </Dialog>
        );
    }
}