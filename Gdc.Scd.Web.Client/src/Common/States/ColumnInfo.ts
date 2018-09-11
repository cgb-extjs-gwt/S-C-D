export enum ColumnType {
    Simple,
    Checkbox
}

export interface ColumnInfo {
    dataIndex: string
    title: string
    type: ColumnType
}