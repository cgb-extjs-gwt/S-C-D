import * as React from 'react';
import { FormPanel, NumberField, Button } from '@extjs/ext-react';

export interface EditCellsPanelProps {
    value?: number;
    onEditClick: (value: number) => void;
    onCancelClick: () => void;
}

export default class EditCellsPanel extends React.Component<EditCellsPanelProps, any> {
    private numberField: NumberField & any;

    public render() {
        const { value, onEditClick, onCancelClick } = this.props;

        return (
            <FormPanel>
                <NumberField ref={field => this.numberField = field} label="Value:" value={value}/>
                <Button text="Edit" handler={() => onEditClick(this.numberField.getValue())} />
                <Button text="Cancel" handler={onCancelClick} />
            </FormPanel>
        );
    }
}
