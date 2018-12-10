import * as React from "react";
import { AjaxDynamicGrid, AjaxDynamicGridProps } from "../../Common/Components/AjaxDynamicGrid";
import { LocalDynamicGridActions } from "../../Common/Components/LocalDynamicGrid";
import { TableViewRecord } from "../States/TableViewRecord";
import { SaveToolbar } from "../../Common/Components/SaveToolbar";
import { SaveApprovalToollbar } from "../../Approval/Components/SaveApprovalToollbar";
import { DynamicGrid } from "../../Common/Components/DynamicGrid";

export interface TableViewGridActions extends LocalDynamicGridActions<TableViewRecord> {
    onApprove?()
}

export interface TableViewGridProps extends AjaxDynamicGridProps<TableViewRecord>, TableViewGridActions {
}

export class TableViewGrid extends React.Component<TableViewGridProps> {
    public render() {
        return (
            <AjaxDynamicGrid 
                getSaveToolbar={this.getSaveToolbar}
                {...this.props}
            />
        );
    }

    private getSaveToolbar = (
        hasChanges: boolean, 
        ref: (toolbar: SaveToolbar) => void, 
        { cancel, save, saveWithCallback }: DynamicGrid
    ) => {
        return (
            <SaveApprovalToollbar 
                ref={ref}
                isEnableClear={hasChanges} 
                isEnableSave={hasChanges}
                onCancel={cancel}
                onSave={save}
                onApproval={() => saveWithCallback(this.props.onApprove)}
            >
            </SaveApprovalToollbar>
        );
    }
}