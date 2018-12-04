import { ColumnInfo } from "../../States/ColumnInfo";
import { Model } from "../../States/ExtStates";

export interface DynamicGridActions {
    init?()
    onSelectionChange?(grid, records: Model[], selecting: boolean, selectionInfo)
    onCancel?()
    onSave?()
}

export interface DynamicGridProps extends DynamicGridActions {
    columns: ColumnInfo[]
    id?: string
    minHeight?: number
    minWidth?: number
    flex?: number
}