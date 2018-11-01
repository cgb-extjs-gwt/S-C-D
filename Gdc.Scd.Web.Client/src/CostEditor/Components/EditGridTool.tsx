import * as React from 'react';
import { EditItem } from "../States/CostBlockStates";
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField, Dialog, Container} from '@extjs/ext-react';
import { NamedId } from '../../Common/States/CommonStates';
import { HistoryValuesGridContainer } from './HistoryValuesGridContainer';
import { ValueColumnProps, EditGrid, EditGridProps } from './EditGrid';
import { QualityGateWindowContainer } from './QualityGateWindowContainer';
import { SaveToolbar } from '../../Common/Components/SaveToolbar';
import { SaveApprovalToollbar } from '../../Approval/Components/SaveApprovalToollbar';

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
    applicationId: string
    costBlockId: string
    costElementId: string
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
                        handler={this.showHistoryWindow}
                    />
                </Toolbar>

                <EditGrid 
                    {...props.editGrid} 
                    onItemEdited={props.onItemEdited} 
                    onSelected={this.onSelectGrid}
                />

                <SaveApprovalToollbar
                    isEnableClear={props.isEnableClear} 
                    isEnableSave={props.isEnableSave}
                    onCancel={() => this.props.onCleared()}
                    onSave={() => this.props.onSaving(false)}
                    onApproval={() => this.props.onSaving(true)}
                />

                {this.getHistoryWindow()}

                <QualityGateWindowContainer 
                    applicationId={props.applicationId} 
                    costBlockId={props.costBlockId} 
                    costElementId={props.costElementId}
                    errors={props.qualityGateErrors} 
                />
            </Container>
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