import * as React from "react";
import { SaveApprovalToollbar } from "../../Approval/Components/SaveApprovalToollbar";
import { SaveToolbar } from "../../Common/Components/SaveToolbar";
import { AjaxDynamicGridActions, AjaxDynamicGridProps, AjaxDynamicGrid } from "../../Common/Components/AjaxDynamicGrid";

export interface TableViewActions<T=any> extends AjaxDynamicGridActions<T> {
    onApprove?()
}

export interface TableViewProps<T=any> extends AjaxDynamicGridProps<T>, TableViewActions<T> {

}

export class TableView extends AjaxDynamicGrid<TableViewProps> {
    protected getSaveToolbar(hasChanges: boolean, ref: (toolbar: SaveToolbar) => void) {
        return (
            <SaveApprovalToollbar 
                ref={ref}
                isEnableClear={hasChanges} 
                isEnableSave={hasChanges}
                onCancel={this.onCancel}
                onSave={this.onSave}
                onApproval={this.onApproval}
            />
        );
    }

    private onApproval = () => {
        this.saveWithCallback(this.props.onApprove);
    }
}