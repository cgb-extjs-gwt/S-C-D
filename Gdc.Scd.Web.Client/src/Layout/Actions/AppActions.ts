import { CommonAction } from "../../Common/Actions/CommonActions";
import { Action } from "redux";

export const APP_PAGE_OPEN = 'APP.PAGE.OPEN';
export const APP_LOADING = 'APP.LOADING';
export const APP_ERROR = 'APP.ERROR';
export const APP_PAGE_INIT = 'APP.PAGE.INIT';

export interface OpenPageAction extends Action<string> {
    id: string
    title: string
}

export interface ErrorAction extends Action<string> {
    error: any
}

export interface PageInitAction<T = any> extends Action<string> {
    pageId: string
    data: T
}

export interface LoadingAction extends Action<string> {
    isLoading: boolean
}

export const openPage = (id: string, title: string) => (<OpenPageAction>{
    type: APP_PAGE_OPEN,
    id,
    title
})

export const loading = (isLoading: boolean) => (<LoadingAction>{
    type: APP_LOADING,
    isLoading
})

export const error = error => (<ErrorAction>{
    type: APP_ERROR,
    error
})

export const pageInit = (pageId: string, data) => (<PageInitAction>{
    type: APP_PAGE_INIT,
    pageId, 
    data
})