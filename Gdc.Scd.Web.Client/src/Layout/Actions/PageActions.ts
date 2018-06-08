import { PageState, PageTitle, PageData, PageError } from "../States/PageStates";
import { CommonAction } from "../../Common/Actions/CommonActions";

export const PAGE_OPEN = 'PAGE.OPEN';
export const PAGE_INIT_SUCCESS = 'PAGE.INIT.SUCCESS';
export const PAGE_INIT_ERROR = 'PAGE.INIT.ERROR';

export interface PageAction<T = any> extends CommonAction<T> {
    pageName: string
}

export class PageActionBuilder {
    private pageName: string;
    private pageTitle: string;

    constructor(pageName: string, pageTitle: string) {
        this.pageName = pageName;   
        this.pageTitle = pageTitle;
    }

    public openPage() {
        return this.createPageAction(PAGE_OPEN, <PageTitle>{ title: this.pageTitle })
    }

    public initPageSuccess<TData>(data: TData) {
        return this.createPageAction(PAGE_INIT_SUCCESS, data);
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