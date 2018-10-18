export interface Model<TData=any> {
    [key: string]: any
    data: TData
    phantom: boolean
    set(fieldName: string, newValue, options?)
    get(fieldName: string)

}

export enum StoreOperation {
    Edit = 'edit',
    Reject = 'reject',
    Commit = 'commit'
}

export interface Collection<T=any> {
    add(item: T)
    remove(item: T)
    each(fn: (item: T) => boolean)
}

export interface Store<T=any> {
    [key: string]: any
    getModifiedRecords(): Model<T>[]
    commitChanges()
    rejectChanges()
    on(eventName: string, fn: Function, scope)
    un(eventName: string, fn: Function)
    add(records: Model<T>[] | T[]):  Model<T>[]
    add(...records: Model<T>[]):  Model<T>[]
    add(...records: T[]):  Model<T>[]
    remove(records: Model<T>[])
    remove(record: Model<T>)
    findBy(fn: (record: Model<T>) => boolean, scope?, start?: number)
    loadData(data: Model<T>[] | { [key: string]: any }[], append?: boolean)
    each(fn: (record: Model<T>) => any, scope?, includeFilterd?: boolean)
    filterBy(fn: (record: Model<T>) => boolean, scope?)
    setFilters(fn: (item: T) => boolean)
    getFilters(): Collection
    removeFilter()
    getTotalCount(): number
    sync(options: { 
        callback?()
    })
}

export interface StoreUpdateEventFn<T=any> {
    (store: Store<T>, record: Model<T>, operation: StoreOperation, modifiedFieldNames: string[], details)
}