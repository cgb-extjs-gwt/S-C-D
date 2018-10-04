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