export interface Model<TData=any> {
    [key: string]: any
    data: TData
    phantom: boolean
    set(fieldName: string, newValue, options?)
    set(obj: { [key: string]: any }, options?)
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
    getProxy()
    reload()
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
    removeAll()
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
    getById(id)
    getAt(index: number)
}

export interface StoreUpdateEventFn<T=any> {
    (store: Store<T>, record: Model<T>, operation: StoreOperation, modifiedFieldNames: string[], details)
}

export interface MenuItem {
    id: string
    text?: string
    iconCls?: string
    leaf?: boolean
    disabled?: boolean
    children?: MenuItem[]
}

export interface Position {
    top?: string | number
    bottom?: string | number
    left?: string | number
    right?: string | number
}