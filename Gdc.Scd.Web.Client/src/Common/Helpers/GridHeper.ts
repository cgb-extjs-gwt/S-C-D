import { ColumnInfo } from "../States/ColumnInfo";

export const buildReferenceColumnRendered = (column: ColumnInfo) => {
    const getReferenceName = function (value) {
        if (value == null || value == ' ') {
            return value;
        }
        let item = column.referenceItems.get(value);
        if (item) {
            return item.name;
        }
    };

    return column.rendererFn
        ? (value, record, dataIndex, cell) => column.rendererFn(getReferenceName(value), record, dataIndex, cell)
        : (value, record, dataIndex, cell) => getReferenceName(value);
}