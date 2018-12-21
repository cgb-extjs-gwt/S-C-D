import * as React from "react";
import { AjaxDynamicGrid, AjaxDynamicGridProps, ApiUrls } from "../../Common/Components/AjaxDynamicGrid";
import { LocalDynamicGridActions } from "../../Common/Components/LocalDynamicGrid";
import { TableViewRecord } from "../States/TableViewRecord";
import { SaveToolbar } from "../../Common/Components/SaveToolbar";
import { SaveApprovalToollbar } from "../../Approval/Components/SaveApprovalToollbar";
import { DynamicGrid } from "../../Common/Components/DynamicGrid";
import { DynamicGridProps } from "../../Common/Components/Props/DynamicGridProps";
import { ColumnInfo } from "../../Common/States/ColumnInfo";
import { objectPropsEqual } from "../../Common/Helpers/CommonHelpers";

export interface TableViewGridActions extends LocalDynamicGridActions<TableViewRecord> {
    onApprove?()
}

export interface TableViewGridProps extends TableViewGridActions {
    columns: ColumnInfo[]
    apiUrls: ApiUrls
}

export class TableViewGrid extends React.PureComponent<TableViewGridProps> {
    public render() {
        const gridProps = this.props as AjaxDynamicGridProps

        return (
            <AjaxDynamicGrid 
                { ...gridProps } 
                width="2200px"
                height="100%"
                isScrollable={true}
                getSaveToolbar={this.getSaveToolbar} 
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
            />
        );
    }
}