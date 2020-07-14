import { ColumnInfo } from "../States/ColumnInfo";
import { ExpandTrigger } from "@extjs/ext-react";
import { NamedId } from "../States/CommonStates";

export const buildGetReferenceNameFn = (referenceItems: Map<number, NamedId<number>>) => value => {
    if (value == null || value === ' ') {
        return value;
    }
    let item = referenceItems.get(value);
    if (item) {
        return item.name;
    }
}

export const buildReferenceColumnRendered = (column: ColumnInfo) => {
    const getReferenceName = buildGetReferenceNameFn(column.referenceItems);

    return column.rendererFn
        ? (value, record, dataIndex, cell) => column.rendererFn(getReferenceName(value), record, dataIndex, cell)
        : (value, record, dataIndex, cell) => getReferenceName(value);
}