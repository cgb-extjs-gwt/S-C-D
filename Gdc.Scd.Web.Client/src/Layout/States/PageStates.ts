export const PAGE_STATE_KEY = 'page';

export interface PageTitle {
    title: string;
}

export interface PageData {
    data: any
}

export interface PageError {
    error: any;
}

// export interface PageName {
//     name: string;
// }

export interface PageState extends PageTitle, PageData, PageError {
}

// export interface PageState {
//     name: string;
//     title: string;
//     error: any;
//     data: any;
// }

export interface PageCommonState {
    page: PageState
}