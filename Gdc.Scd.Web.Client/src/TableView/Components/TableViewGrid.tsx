import * as React from "react";
import { SaveApprovalToollbar } from "../../Approval/Components/SaveApprovalToollbar";
import { SaveToolbar } from "../../Common/Components/SaveToolbar";
import { AjaxDynamicGridActions, AjaxDynamicGridProps, AjaxDynamicGrid } from "../../Common/Components/AjaxDynamicGrid";
import { HistoryButtonView, HistoryButtonViewProps } from "../../History/Components/HistoryButtonView";
import { Model } from "../../Common/States/ExtStates";
import { TableViewRecord } from "../States/TableViewRecord";
import { QualtityGateSetWindowContainer } from "./QualtityGateSetWindowContainer";

export interface TableViewGridActions extends AjaxDynamicGridActions<TableViewRecord> {
    onApprove?()
}

export interface TableViewGridProps extends AjaxDynamicGridProps<TableViewRecord>, TableViewGridActions {
}

export class TableViewGrid extends AjaxDynamicGrid<TableViewGridProps> {
    protected getSaveToolbar(hasChanges: boolean, ref: (toolbar: SaveToolbar) => void) {
        return (
            <SaveApprovalToollbar 
                ref={ref}
                isEnableClear={hasChanges} 
                isEnableSave={hasChanges}
                onCancel={this.onCancel}
                onSave={this.onSave}
                onApproval={this.onApproval}
            >
            </SaveApprovalToollbar>
        );
    }

    private onApproval = () => {
        this.saveWithCallback(this.props.onApprove);
    }
}