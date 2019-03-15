import { ColumnInfo } from "../States/ColumnInfo";

export const buildReferenceColumnRendered = (column: ColumnInfo) => {
    const getReferenceName = value => value == null || value == ' ' ? value : column.referenceItems.get(value).name;

    return column.rendererFn 
        ? (value, record, dataIndex, cell) => column.rendererFn(getReferenceName(value), record, dataIndex, cell) 
        : (value, record, dataIndex, cell) => getReferenceName(value);
}