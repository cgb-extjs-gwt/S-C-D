export interface NamedId<TId = string> {
    id: TId;
    name: string;
}

export interface DataInfo<T>
{
    items: T[];
    total: number;
}

export interface SelectList<T> {
    selectedItemId: string;
    list: T[]
}

export interface MultiSelectList<T> {
    selectedItemIds: string[];
    list: T[] 
}

export interface ElementWithParent<T=NamedId, U=string>{
    element : T,
    parentId : U
}

export interface PageName {
    pageName: string
}