import * as React from "react";
import { SaveApprovalToollbar } from "../../Approval/Components/SaveApprovalToollbar";
import { AjaxDynamicGrid, AjaxDynamicGridActions, AjaxDynamicGridProps } from "../../Common/Components/AjaxDynamicGrid";
import { SaveToolbar } from "../../Common/Components/SaveToolbar";
import { TableViewRecord } from "../../TableView/States/TableViewRecord";

export interface TableViewGridProps extends AjaxDynamicGridProps<TableViewRecord> {
    onApprove(): void;
}

export class TableViewGrid extends AjaxDynamicGrid<TableViewGridProps> {

    public constructor(props: TableViewGridProps) {
        super(props);
        this.init();
    }

    protected getSaveToolbar(hasChanges: boolean, ref: (toolbar: SaveToolbar) => void) {
        return (
            <SaveApprovalToollbar
                ref={ref}
                isEnableClear={hasChanges}
                isEnableSave={hasChanges}
                onCancel={this.onCancel}
                onSave={this.onSave}
                onApproval={this.onApprove}
            >
            </SaveApprovalToollbar>
        );
    }

    private init() {
        this.onApprove = this.onApprove.bind(this);
        //hack!!!
        //TODO: remove, refactor!
        this.componentWillReceiveProps(this.props);
    }

    private onApprove() {
        this.saveWithCallback(this.props.onApprove);
    }
}