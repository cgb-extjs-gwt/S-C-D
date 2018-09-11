import { Action } from "redux";

export interface PageAction extends Action<string> {
    pageName: string
}

export interface ItemSelectedAction extends Action<string> {
    selectedItemId: string;
}

export interface PageItemSelectedAction extends ItemSelectedAction, PageAction {

}

export interface ItemWithParentSelectedAction extends ItemSelectedAction{
    selectedItemParentId: string
}

export interface PageItemWithParentSelectedAction extends PageAction, ItemWithParentSelectedAction {
    
}

export interface CommonAction<T = any> extends Action<string> {
    data: T;
}

export interface PageCommonAction<T = any> extends CommonAction<T>, PageAction {
    
}
