import * as React from "react";
import { SaveApprovalToollbar } from "../../Approval/Components/SaveApprovalToollbar";
import { SaveToolbar } from "../../Common/Components/SaveToolbar";
import { AjaxDynamicGridActions, AjaxDynamicGridProps, AjaxDynamicGrid } from "../../Common/Components/AjaxDynamicGrid";
import { HistoryButtonView, HistoryButtonViewProps } from "../../History/Components/HistoryButtonView";
import { Model } from "../../Common/States/ExtStates";
import { TableViewRecord } from "../States/TableViewRecord";

export interface TableViewActions extends AjaxDynamicGridActions<TableViewRecord> {
    onApprove?()
}

export interface TableViewProps extends AjaxDynamicGridProps<TableViewRecord>, TableViewActions {
    buildHistotyDataLoadUrl(selection: Model<TableViewRecord>[], selectedDataIndex: string): string
}

export interface TableViewState {
    selection: Model<TableViewRecord>[]
    selectedDataIndex: string
    isEnableHistoryButton: boolean
}

export class TableView extends AjaxDynamicGrid<TableViewProps, TableViewState> {
    constructor(props) {
        super(props)

        this.state = {
            selection: [],
            selectedDataIndex: null,
            isEnableHistoryButton: false
        };
    }

    protected getSaveToolbar(hasChanges: boolean, ref: (toolbar: SaveToolbar) => void) {
        const { selection, selectedDataIndex, isEnableHistoryButton } = this.state;
        const { buildHistotyDataLoadUrl } = this.props;
        
        return (
            <SaveApprovalToollbar 
                ref={ref}
                isEnableClear={hasChanges} 
                isEnableSave={hasChanges}
                onCancel={this.onCancel}
                onSave={this.onSave}
                onApproval={this.onApproval}
            >
                <HistoryButtonView  
                    isEnabled={isEnableHistoryButton}
                    flex={1}
                    buidHistoryUrl={() => buildHistotyDataLoadUrl(selection, selectedDataIndex)}
                />
            </SaveApprovalToollbar>
        );
    }

    protected onSelectionChange(grid, records: Model<TableViewRecord>[], selecting: boolean, selectionInfo) {
        const column = selectionInfo.startCell.column;

        this.setState({
            selection: records,
            selectedDataIndex: column.getDataIndex(),
            isEnableHistoryButton: !!column.getEditable()
        });

        super.onSelectionChange(grid, records, selecting, selectionInfo);
    }

    private onApproval = () => {
        this.saveWithCallback(this.props.onApprove);
    }
}