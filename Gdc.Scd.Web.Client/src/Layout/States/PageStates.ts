export const PAGE_STATE_KEY = 'page';

export interface PageTitle {
    title: string;
}

export interface PageData<TData = any> {
    data: TData
}

export interface PageError {
    error: any;
}

// export interface PageName {
//     name: string;
// }

export interface PageState<TData = any> extends PageTitle, PageData<TData>, PageError {
    isLoading: boolean
}

// export interface PageState {
//     name: string;
//     title: string;
//     error: any;
//     data: any;
// }

export interface PageCommonState<TData = any> {
    page: PageState
}