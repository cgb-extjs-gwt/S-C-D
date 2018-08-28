import * as React from "react";
import { Grid, Column, Container, TextField, FormPanel, Toolbar, Button } from "@extjs/ext-react";
import { ColumnInfo } from "../../Common/States/ColumnInfo";
import { DynamicGrid } from "../../Common/Components/DynamicGrid";

export interface ApprovalValuesActions {
    onApprove?();
    onSendBackToRequestor?(message: string)
}

export interface ApprovalValuesProps extends ApprovalValuesActions {
    dataLoadUrl?: string
    columns?: ColumnInfo[]
    id?: string
}

interface ApprovalValuesState {
    isVisibleRejectForm: boolean
    isValidRejectForm: boolean
}

export class ApprovalValuesViewComponent extends React.Component<ApprovalValuesProps, ApprovalValuesState> {
    private readonly store;
    private rejectMessageTextField: TextField & any;
    private rejectForm: FormPanel & any;

    constructor(props: ApprovalValuesProps) {
        super(props);

        this.state = {
            isVisibleRejectForm: false,
            isValidRejectForm: false
        }

        this.store = this.getStore(props.columns, props.dataLoadUrl);
    }

    public render() {
        const { columns, id } = this.props;
        const { isValidRejectForm, isVisibleRejectForm } = this.state;

        return (
            <Container layout="vbox">
                <DynamicGrid store={this.store} columns={columns} height={400}>
                    {
                        !isVisibleRejectForm &&
                        <Toolbar docked="bottom">
                            <Button text="Approve" handler={this.onApprove} flex={1}/>
                            <Button text="Reject" handler={this.onReject} flex={1}/>
                        </Toolbar>
                    }
                </DynamicGrid>
                
                {
                    isVisibleRejectForm &&
                    <Container layout="hbox">
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
                    </Container>
                }
            </Container>
        );
    }

    private getStore(columns: ColumnInfo[], dataLoadUrl: string) {
        return Ext.create('Ext.data.Store', {
            fields: columns.map(column => ({ name: column.dataIndex })),
            autoLoad: true,
            proxy: {
                type: 'ajax',
                url: dataLoadUrl,
                reader: { 
                    type: 'json',
                }
            }
        });
    }

    private onReject = () => {
        this.setState({isVisibleRejectForm: true})
    }

    private onApprove = () => {
        const { onApprove } = this.props;
        
        this.setState({isVisibleRejectForm: false})

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