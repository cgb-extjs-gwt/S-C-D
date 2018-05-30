export interface SelectList<T> {
    selectedItemId: string;
    list: T[]
}

export interface MultiSelectList<T> {
    selectedItemIds: string[];
    list: T[] 
}