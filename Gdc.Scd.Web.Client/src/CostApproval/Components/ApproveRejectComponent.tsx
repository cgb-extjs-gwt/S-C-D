import * as React from "react";
import { Container, Toolbar, Button, FormPanel, TextField } from "@extjs/ext-react";

export interface ApproveRejectActions {
    onApprove?();
    onSendBackToRequestor?(message: string)
}

export interface ApproveRejectState {
    isVisibleRejectForm: boolean
    isValidRejectForm: boolean
}

export class ApproveRejectComponent extends React.Component<ApproveRejectActions, ApproveRejectState> {
    private rejectMessageTextField: TextField & any;
    private rejectForm: FormPanel & any;

    constructor(props: ApproveRejectActions) {
        super(props);

        this.state = {
            isVisibleRejectForm: false,
            isValidRejectForm: false,
        }
    }

    public render() {
        const { isValidRejectForm, isVisibleRejectForm } = this.state;

        return (
            <Container layout="hbox">
                {
                    !isVisibleRejectForm &&
                    <Toolbar flex={1}>
                        <Button text="Approve" handler={this.onApprove} flex={1}/>
                        <Button text="Reject" handler={this.onReject} flex={1}/>
                    </Toolbar>
                }

                {
                    isVisibleRejectForm &&
                    <FormPanel defaults={{labelAlign: 'left'}} flex={1} ref={form => this.rejectForm = form}>
                        <TextField 
                            ref={textField => this.rejectMessageTextField = textField}
                            required
                            placeholder="Please enter the reason for rejection"
                            onChange={this.onRejectMessageTextFieldChange}
                        />
                        <Toolbar docked="bottom">
                            <Button 
                                text="Send back to requestor" 
                                handler={this.onSendBackToRequestor} 
                                flex={1} 
                                disabled={!isValidRejectForm}
                            />
                            <Button text="Cancel" handler={this.onRejectCancel} flex={1}/>
                        </Toolbar>
                    </FormPanel>
                }
            </Container>
        );
    }

    private onReject = () => {
        this.setState({isVisibleRejectForm: true})
    }

    private onApprove = () => {
        const { onApprove } = this.props;
        
        this.setState({isVisibleRejectForm: false});

        onApprove && onApprove();
    }

    private onSendBackToRequestor = () => {
        const { onSendBackToRequestor } = this.props;

        onSendBackToRequestor && onSendBackToRequestor(this.rejectMessageTextField.getValue());
    }

    private onRejectCancel = () => {
        this.setState({
            isVisibleRejectForm: false,
            isValidRejectForm: false
        });
    }

    private onRejectMessageTextFieldChange = (textField, newValue: string, oldValue: string) => {
        this.setState({ isValidRejectForm: this.rejectForm.isValid() });
    }
}