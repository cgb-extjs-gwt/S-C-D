import * as React from 'react';
import { EditItem } from "../States/CostBlockStates";
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField, Dialog, Container} from '@extjs/ext-react';
import { NamedId } from '../../Common/States/CommonStates';
import { ValueColumnProps, EditGrid, EditGridProps } from './EditGrid';
import { QualityGateWindowContainer } from './QualityGateWindowContainer';
import { SaveToolbar } from '../../Common/Components/SaveToolbar';
import { SaveApprovalToollbar } from '../../Approval/Components/SaveApprovalToollbar';
import { HistroryButtonContainer } from './HistroryButtonContainer';
import { BundleDetailGroup } from '../../QualityGate/States/QualityGateResult';

Ext.require([
    'Ext.grid.plugin.CellEditing', 
    'Ext.panel.Resizer'
]);

export interface EditGridToolActions {
    onItemEdited?(item: EditItem)
    onCleared?()
    onSaving?(forApproval: boolean)
}

export interface EditGridToolProps extends EditGridToolActions {
    isEnableSave: boolean
    isEnableClear: boolean
    flex?: number
    editGrid: EditGridProps
    qualityGateErrors: BundleDetailGroup[]
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

        let isEnabledHistoryButton = false;
        let editItem: string = null;

        if (this.state.selectedItems.length == 1) {
            isEnabledHistoryButton = true;
            editItem = this.state.selectedItems[0].id;
        }

        return (
            <Container layout="vbox" flex={props.flex}>
                <Toolbar docked="top">
                    <HistroryButtonContainer 
                        editItemId={editItem}
                        isEnabled={isEnabledHistoryButton}
                        flex={1} 
                        windowPosition={{ left: '40%', top: '20%' }}
                    />
                </Toolbar>

                <EditGrid 
                    {...props.editGrid} 
                    onItemEdited={props.onItemEdited} 
                    onSelected={this.onSelectGrid}
                    onApprove={() => this.props.onSaving(true)}
                    onSave={() => this.props.onSaving(false)}
                    onCancel={this.props.onCleared}
                />

                {/* <SaveApprovalToollbar
                    isEnableClear={props.isEnableClear} 
                    isEnableSave={props.isEnableSave}
                    onCancel={() => this.props.onCleared()}
                    onSave={() => this.props.onSaving(false)}
                    onApproval={() => this.props.onSaving(true)}
                /> */}

                <QualityGateWindowContainer 
                    applicationId={props.applicationId} 
                    costBlockId={props.costBlockId} 
                    costElementId={props.costElementId}
                    errors={props.qualityGateErrors} 
                    position={{ left: '20%', top: '20%' }}
                />
            </Container>
        );
    }

    private onSelectGrid = (items: EditItem[]) => {
        this.setState({
            selectedItems: items
        })
    }
}