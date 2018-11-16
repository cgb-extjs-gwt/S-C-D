import * as React from "react";
import { ColumnInfo, ColumnType } from "../../Common/States/ColumnInfo";
import { Container, FormPanel, TextField, Toolbar, Button, Grid, Column, GridCell, CheckColumn } from "@extjs/ext-react";
import { DynamicGrid } from "../../Common/Components/DynamicGrid";
import { BundleDetailGroup } from "../States/QualityGateResult";
import { Model } from "../../Common/States/ExtStates";
import { CostElementMeta } from "../../Common/States/CostMetaStates";
import { QualityGateGridProps, QualityGateGrid } from "./QualityGateGrid";

export interface QualityGateErrorActions {
    onSave?(explanationMessage: string)
    onCancel?()
}

export interface QualityGateErrorProps extends QualityGateGridProps, QualityGateErrorActions {

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
        const { onCancel } = this.props;
        const { isValidExplanationForm } = this.state;

        return (
            <Container layout="vbox" scrollable={true}>
                <QualityGateGrid {...this.props} />
                
                <FormPanel defaults={{labelAlign: 'left'}} flex={1} ref={form => this.explanationForm = form} minHeight="100">
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

    private onExplanationTextFieldChange = (textField, newValue: string, oldValue: string) => {
        this.setState({ isValidExplanationForm: this.explanationForm.isValid() });
    }

    private onSave = () => {
        const { onSave } = this.props;

        onSave && onSave(this.explanationTextField.getValue());
    }
}