import { NamedId } from "./CommonStates";
import { Model, Store } from "./ExtStates";

export enum ColumnType {
    Text,
    CheckBox,
    Numeric,
    Reference,
    Percent
}

export interface FilterItem {
    checked: boolean
    value
}

export interface ColumnFilter {
    store: Store<FilterItem>
    checkedDataIndex: string
    valueDataIndex: string
}

export interface ColumnInfo<T=any> {
    dataIndex: string
    title: string
    type: ColumnType
    isEditable?: boolean
    referenceItems?: Map<string, NamedId>
    isInvisible?: boolean
    filter?: ColumnFilter
    mappingFn?(data: T): any
    editMappingFn?(data: Model<T>, dataIndex: string)
    rendererFn?(value, record: Model<T>): any
}