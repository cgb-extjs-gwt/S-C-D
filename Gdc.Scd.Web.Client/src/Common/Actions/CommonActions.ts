import { Action } from "redux";

export interface PageAction extends Action<string> {
    pageName: string
}

export interface ItemSelectedAction extends Action<string> {
    selectedItemId: string;
}

export interface MultiItemSelectedAction extends Action<string> {
    selectedItemIds: string[];
}

export interface PageItemSelectedAction extends ItemSelectedAction, PageAction {

}

export interface PageMultiItemSelectedAction extends MultiItemSelectedAction, PageAction {

}

export interface CommonAction<T = any> extends Action<string> {
    data: T;
}

export interface PageCommonAction<T = any> extends CommonAction<T>, PageAction {
    
}
