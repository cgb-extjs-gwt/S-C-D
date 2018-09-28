import { ColumnInfo } from "../../States/ColumnInfo";

export interface DynamicGridActions {
    init?()
    onSelectionChange?(grid, records: any[])
    onCancel?()
    onSave?()
}

export interface DynamicGridProps extends DynamicGridActions {
    columns: ColumnInfo[]
    id?: string
    minHeight?: number
    minWidth?: number
}