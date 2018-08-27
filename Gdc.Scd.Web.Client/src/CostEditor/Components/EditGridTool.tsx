import * as React from 'react';
import { FieldType } from "../States/CostEditorStates";
import { EditItem } from "../States/CostBlockStates";
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField, Dialog, Container} from '@extjs/ext-react';
import { NamedId } from '../../Common/States/CommonStates';
import { HistoryValuesGridContainer } from './HistoryValuesGridContainer';
import { ValueColumnProps, EditGrid, EditGridProps } from './EditGrid';
import { QualityGateErrorContainer } from './QualityGateErrorContainer';

Ext.require([
    'Ext.grid.plugin.CellEditing', 
    'Ext.panel.Resizer'
]);

export interface EditGridToolActions {
    onItemEdited?(item: EditItem)
    onApplyFilters?()
    onCleared?()
    onSaving?(forApproval: boolean)
}

export interface EditGridToolProps extends EditGridToolActions {
    isEnableSave: boolean
    isEnableClear: boolean
    isEnableApplyFilters: boolean
    flex?: number
    editGrid: EditGridProps
    qualityGateErrors: {[key: string]: any}[]
    costBlockId: string
}

export interface EditGridToolState {
    isVisibleHistoryWindow: boolean
    selectedItems: EditItem[]
}

export class EditGridTool extends React.Component<EditGridToolProps, EditGridToolState> {
    constructor(props: EditGridToolProps) {
        super(props);

        this.state = {
            isVisibleHistoryWindow: false,
            selectedItems: []
        }
    }

    public render() {
        const props = this.props;

        return (
            <Container layout="vbox" flex={props.flex}>
                <Toolbar docked="top">
                    <Button 
                        text="Apply filters" 
                        flex={1} 
                        disabled={!props.isEnableApplyFilters}
                        handler={props.onApplyFilters}
                    />

                    <Button 
                        text="History" 
                        flex={1} 
                        disabled={this.state.selectedItems.length != 1}
                        //ref={button => this.historyButton = button}
                        handler={this.showHistoryWindow}
                    />
                </Toolbar>

                <EditGrid 
                    {...props.editGrid} 
                    onItemEdited={props.onItemEdited} 
                    onSelected={this.onSelectGrid}
                />

                <Toolbar docked="bottom">
                    <Button 
                        text="Cancel" 
                        flex={1} 
                        disabled={!props.isEnableClear}
                        handler={() => this.showClearDialog()}
                    />
                    <Button 
                        text="Save" 
                        flex={1} 
                        disabled={!props.isEnableSave}
                        handler={() => this.showSaveDialog(false)}
                    />
                    <Button 
                        text="Save and send for approval" 
                        flex={1} 
                        disabled={!props.isEnableSave}
                        handler={() => this.showSaveDialog(true)}
                    />
                </Toolbar>

                { this.getHistoryWindow() }
                { this.getQualityGateErrorWindow() }
            </Container>
        );
    }

    private showSaveDialog(forApproval: boolean) {
        const { onSaving } = this.props;
    
        Ext.Msg.confirm(
          'Saving changes', 
          'Do you want to save the changes?',
          (buttonId: string) => onSaving && onSaving(forApproval)
        );
    }
    
    private showClearDialog() {
        const { onCleared } = this.props;

        Ext.Msg.confirm(
            'Clearing changes', 
            'Do you want to clear the changes?',
            (buttonId: string) => onCleared && onCleared()
        );
    }

    private getHistoryWindow() {
        const { isVisibleHistoryWindow, selectedItems } = this.state;
        const editItemId = selectedItems.length > 0 ? selectedItems[0].id : null;

        return (
            isVisibleHistoryWindow && selectedItems.length == 1 &&
            <Dialog 
                displayed={isVisibleHistoryWindow} 
                title="History" 
                closable 
                maximizable
                resizable={{
                    dynamic: true,
                    edges: 'all'
                }}
                minHeight="600"
                minWidth="700"
                onClose={this.closeHistoryWindow}
                layout="fit"
            >
                <HistoryValuesGridContainer editItemId={editItemId} />
            </Dialog>
        );
    }

    private getQualityGateErrorWindow() {
        const { qualityGateErrors, costBlockId } = this.props;

        return (
            <Dialog 
                displayed={qualityGateErrors && qualityGateErrors.length > 0} 
                title="Quality gate errors" 
                closable 
                maximizable
                resizable={{
                    dynamic: true,
                    edges: 'all'
                }}
                minHeight="600"
                minWidth="700"
                layout="fit"
            >
                <QualityGateErrorContainer costBlockId={costBlockId} errors={qualityGateErrors} />
            </Dialog>
        );
    }

    private showHistoryWindow = () => {
        this.setState({
            isVisibleHistoryWindow: true
        });
    }

    private closeHistoryWindow = () => {
        this.setState({ 
            isVisibleHistoryWindow: false 
        });
    }

    private onSelectGrid = (items: EditItem[]) => {
        this.setState({
            selectedItems: items
        })
    }
}