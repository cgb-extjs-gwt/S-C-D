export interface PageTitle {
    title: string;
}

export interface PageData<TData = any> {
    data: any
}

export interface PageState<TData = any> extends PageTitle, PageData<TData> {
    
}

export interface PageCommonState<TData = any> {
    page: PageState<TData>
}