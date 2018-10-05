export interface Model<TData=any> {
    [key: string]: any
    data: TData
    set(fieldName: string, newValue, options?)
    get(fieldName: string)
}

export enum StoreOperation {
    Edit = 'edit',
    Reject = 'reject',
    Commit = 'commit'
}

export interface Store<T=any> {
    [key: string]: any
    getModifiedRecords(): Model<T>[]
    commitChanges()
    rejectChanges()
    on(eventName: string, fn: Function, scope)
    un(eventName: string, fn: Function)
    loadData(data: Model<T>[] | { [key: string]: any }[], append?: boolean)
    each(fn: (record: Model<T>) => boolean)
    filterBy(fn: (record: Model<T>) => boolean, scope?)
    sync(options: { 
        callback?()
    })
}

export interface StoreUpdateEventFn<T=any> {
    (store: Store<T>, record: Model<T>, operation: StoreOperation, modifiedFieldNames: string[], details)
}