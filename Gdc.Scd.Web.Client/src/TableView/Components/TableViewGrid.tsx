import * as React from "react";
import { AjaxDynamicGrid, AjaxDynamicGridProps, ApiUrls } from "../../Common/Components/AjaxDynamicGrid";
import { LocalDynamicGridActions } from "../../Common/Components/LocalDynamicGrid";
import { TableViewRecord } from "../States/TableViewRecord";
import { SaveToolbar } from "../../Common/Components/SaveToolbar";
import { SaveApprovalToollbar } from "../../Approval/Components/SaveApprovalToollbar";
import { DynamicGrid } from "../../Common/Components/DynamicGrid";
import { DynamicGridProps, DynamicGridActions } from "../../Common/Components/Props/DynamicGridProps";
import { ColumnInfo } from "../../Common/States/ColumnInfo";
import { objectPropsEqual } from "../../Common/Helpers/CommonHelpers";
import { Model } from "../../Common/States/ExtStates";

export interface TableViewGridActions extends LocalDynamicGridActions<TableViewRecord>, DynamicGridActions {
    onApprove?()
    onSelectionChange?(grid, records: Model[], selecting: boolean, selectionInfo)
}

export interface TableViewGridProps extends TableViewGridActions {
    columns: ColumnInfo[]
    apiUrls: ApiUrls
    hasChanges: boolean
}

export class TableViewGrid extends React.Component<TableViewGridProps> {
    private innerGrid: AjaxDynamicGrid

    public componentWillReceiveProps(nextProps: TableViewGridProps) {
        if (this.innerGrid && !nextProps.hasChanges) {
            this.innerGrid.commitChanges();
        }
    }

    public shouldComponentUpdate(nextProps: TableViewGridProps) {
        return (
            this.props != nextProps && 
            (this.props.apiUrls != nextProps.apiUrls || this.props.columns != nextProps.columns)
        );
    }

    public render() {
        const gridProps = this.props as AjaxDynamicGridProps

        return (
            <AjaxDynamicGrid 
                { ...gridProps } 
                ref={this.innerGridRef}
                getSaveToolbar={this.getSaveToolbar} 
            />
        ); 
    }

    private innerGridRef = (grid: AjaxDynamicGrid) => {
        this.innerGrid = grid;
    }

    private getSaveToolbar = (
        hasChanges: boolean, 
        ref: (toolbar: SaveToolbar) => void, 
        { cancel, save, saveWithCallback }: DynamicGrid
    ) => {
        const { onApprove, onSave } = this.props;

        return (
            <SaveApprovalToollbar 
                ref={ref}
                isEnableClear={hasChanges} 
                isEnableSave={hasChanges}
                onCancel={cancel}
                onSave={onSave}
                onApproval={onApprove}
            />
        );
    }
}