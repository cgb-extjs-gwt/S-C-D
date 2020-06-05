import * as React from "react";
import { Container, FileField, Grid, Column } from "@extjs/ext-react";
import { SaveApprovalToollbar } from "../../Approval/Components/SaveApprovalToollbar";
import { ImportResult } from "../States/TableViewState";
import { Store } from "../../Common/States/ExtStates";
import { QualtityGateSetImportWindowContainer } from "./QualtityGateSetImportWindowContainer";

const STATUS_DATA_INDEX = 'status';

export interface ImportFromExcelViewActions {
    onSave?(file)
    onApproveSave?(file)
    onCancel?()
}

export interface ImportFromExcelViewProps extends ImportFromExcelViewActions {
    importResults?: ImportResult[]
}

export interface ImportFromExcelViewState {
    isEnableSave: boolean
}

export class ImportFromExcelView extends React.Component<ImportFromExcelViewProps, ImportFromExcelViewState> {
    private fileField;
    private statusStore: Store<ImportResult> = Ext.create('Ext.data.Store', {
        fields: [
            { name: STATUS_DATA_INDEX, type: 'string' },
        ]
    })

    constructor(props: ImportFromExcelViewProps) {
        super(props);

        this.state = {
            isEnableSave: false
        }
    }

    public render() {
        this.updateStatuses();

        return (
            <Container layout="vbox">
                <Container layout="vbox" docked="top" height="20%" minWidth="200" maxWidth="30%" padding="10" defaults={{labelAlign: 'left'}}>
                    <FileField label="Excel file" ref={this.setRefFileField} onChange={this.onFileFieldChange}/>
                </Container>
                
                <Grid store={this.statusStore} sortable={false} grouped={false} flex={1}>
                    <Column text="Status" dataIndex={STATUS_DATA_INDEX} flex={1}/>
                    <SaveApprovalToollbar 
                        isEnableClear={true} 
                        isEnableSave={this.state.isEnableSave}
                        disableDialogs={true}
                        cancelButtonText="Back to Central Data Input"
                        onCancel={this.onCancel}
                        onSave={this.onSave}
                        onApproval={this.onApproveSave}
                    />
                </Grid>
                <QualtityGateSetImportWindowContainer position={{ top: '25%', left: '25%'}}/>
            </Container>
        );
    }

    private setRefFileField = field => {
        this.fileField = field;
    }

    private updateStatuses() {
        this.statusStore.removeAll();
        
        if (this.props.importResults) {
            this.statusStore.loadData(this.props.importResults);
        }
    }

    private getFile() {
        return this.fileField.getFiles()[0];
    }

    private onSave = () => {
        const file = this.getFile();
        const { onSave } = this.props;
        
        onSave && onSave(file);
    }

    private onApproveSave = () => {
        const file = this.getFile();
        const { onApproveSave } = this.props;
        
        onApproveSave && onApproveSave(file);
    }

    private onCancel = () => {
        const { onCancel } = this.props;

        onCancel && onCancel();
    }

    private onFileFieldChange = (fileField, newValue, oldValue) => {
        this.setState({ isEnableSave: !!newValue });
    }
}