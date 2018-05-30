import { PageState, PageTitle, PageData, PageError } from "../States/PageStates";
import { CommonAction } from "../../Common/CommonAction";

export const PAGE_OPEN = 'PAGE.OPEN';
export const PAGE_INIT_SUCCESS = 'PAGE.INIT.SUCCESS';
export const PAGE_INIT_ERROR = 'PAGE.INIT.ERROR';

export interface PageAction<T = any> extends CommonAction<T> {
    pageName: string
}

// const createPageAction = <T>(pageName: string) => (<PageAction<T>>{

// });

// export const openPage = (title: string) => (<CommonAction<PageTitle>>{
//     type: PAGE_OPEN,
//     data: { title }
// });

// export const initPageSuccess = data => (<CommonAction<PageData>>{
//     type: PAGE_INIT_SUCCESS,
//     data: { data }
// });

// export const initPageError = error => (<CommonAction<PageError>>{
//     type: PAGE_INIT_ERROR,
//     data: { error }
// });

export class PageActionBuilder<TData> {
    private pageName: string;
    private pageTitle: string;

    constructor(pageName: string, pageTitle: string) {
        this.pageName = pageName;   
        this.pageTitle = pageTitle;
    }

    public openPage() {
        return this.createPageAction(PAGE_OPEN, <PageTitle>{ title: this.pageTitle })
    }

    public initPageSuccess(data: TData) {
        return this.createPageAction(PAGE_INIT_SUCCESS, <PageData>{ data });
    }

    public initPageError(error) {
        return this.createPageAction(PAGE_INIT_ERROR, error);
    }

    private createPageAction<T>(type: string, data: T) {
        return <PageAction<T>>{
            type,
            data,
            pageName: this.pageName
        }
    }
}