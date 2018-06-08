export interface NamedId {
    id: string;
    name: string;
}

export interface SelectList<T> {
    selectedItemId: string;
    list: T[]
}

export interface MultiSelectList<T> {
    selectedItemIds: string[];
    list: T[] 
}