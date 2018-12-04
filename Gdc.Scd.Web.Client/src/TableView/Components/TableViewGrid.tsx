import * as React from "react";
import { SaveApprovalToollbar } from "../../Approval/Components/SaveApprovalToollbar";
import { AjaxDynamicGrid, AjaxDynamicGridActions, AjaxDynamicGridProps } from "../../Common/Components/AjaxDynamicGrid";
import { SaveToolbar } from "../../Common/Components/SaveToolbar";
import { TableViewRecord } from "../States/TableViewRecord";

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