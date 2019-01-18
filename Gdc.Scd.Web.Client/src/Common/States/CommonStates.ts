export interface NamedId<TId = string> {
    id: TId;
    name: string;
}

export interface DataInfo<T>
{
    items: T[];
    total: number;
}

export interface SelectList<T, TId=string> {
    selectedItemId: TId;
    list: T[]
}

export interface SelectListAdvanced<T, TId=string> extends SelectList<T, TId> {
    onItemSelected?(id: TId)
}

export interface MultiSelectList<T> {
    selectedItemIds: string[];
    list: T[] 
}

export interface PageName {
    pageName: string
}