import * as React from "react";
import { Toolbar, Button, FormPanel, TextField } from "@extjs/ext-react";

export interface QualityGateToolbarActions {
    onSave?(explanationMessage: string)
    onCancel?()
}

export interface QualityGateToolbarProps extends QualityGateToolbarActions {
    flex?: number
}

export interface QualityGateToolbarState {
    isValidExplanationForm: boolean
}

export class QualityGateToolbar extends React.Component<QualityGateToolbarProps, QualityGateToolbarState> {
    private explanationTextField
    private explanationForm

    constructor(props){ 
        super(props);

        this.state = {
            isValidExplanationForm: false
        };
    }

    public render() {
        const { onSave, onCancel, flex } = this.props;
        const { isValidExplanationForm } = this.state;

        return (
            <FormPanel defaults={{labelAlign: 'left'}} flex={flex} ref={form => this.explanationForm = form} minHeight="100">
                <TextField 
                    ref={textField => this.explanationTextField = textField}
                    required
                    placeholder="Please enter the reason"
                    onChange={this.onExplanationTextFieldChange}
                />
                <Toolbar docked="bottom">
                    <Button 
                        text="Save" 
                        handler={this.onSave} 
                        flex={1} 
                        disabled={!isValidExplanationForm}
                    />
                    <Button text="Cancel" handler={onCancel} flex={1}/>
                </Toolbar>
            </FormPanel>
        );
    }

    private onExplanationTextFieldChange = (textField, newValue: string, oldValue: string) => {
        this.setState({ isValidExplanationForm: this.explanationForm.isValid() });
    }

    private onSave = () => {
        const { onSave } = this.props;

        onSave && onSave(this.explanationTextField.getValue());
    }
}