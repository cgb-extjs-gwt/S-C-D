import * as React from "react";
import { ColumnInfo } from "../../Common/States/ColumnInfo";
import { Container, FormPanel, TextField, Toolbar, Button } from "@extjs/ext-react";
import { DynamicGrid } from "../../Common/Components/DynamicGrid";

export interface QualityGateErrorActions {
    onSave?(explanationMessage: string)
    onCancel?()
}

export interface QualityGateErrorProps extends QualityGateErrorActions {
    columns?: ColumnInfo[]
    errors: {[key: string]: any}[]
}

interface QualityGateErrorState {
    isValidExplanationForm: boolean
}

export class QualityGateErrorView extends React.Component<QualityGateErrorProps, QualityGateErrorState> {
    private explanationTextField
    private explanationForm

    constructor(props: QualityGateErrorProps){ 
        super(props);

        this.state = {
            isValidExplanationForm: false
        };
    }

    render() {
        const { columns, onCancel } = this.props;
        const { isValidExplanationForm } = this.state;
        const store = this.buildStore();

        return (
            <Container layout="vbox">
                <DynamicGrid store={store} columns={columns} height={400}/>
                
                <FormPanel defaults={{labelAlign: 'left'}} flex={1} ref={form => this.explanationForm = form}>
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
            </Container>
        );
    }

    private buildStore() {
        const { columns, errors } = this.props;

        return Ext.create('Ext.data.Store', {
            fields: columns.map(column => ({ name: column.dataIndex })),
            data: errors
        });
    }

    private onExplanationTextFieldChange = (textField, newValue: string, oldValue: string) => {
        this.setState({ isValidExplanationForm: this.explanationForm.isValid() });
    }

    private onSave = () => {
        const { onSave } = this.props;

        onSave && onSave(this.explanationTextField.getValue());
    }
}