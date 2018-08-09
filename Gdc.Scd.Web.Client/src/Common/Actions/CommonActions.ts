import { Action } from "redux";

export interface ItemSelectedAction extends Action<string> {
    selectedItemId: string;
}

export interface ItemWithParentSelectedAction extends ItemSelectedAction{
    selectedItemParentId: string
}

export interface CommonAction<T = any> extends Action<string> {
    data: T;
}

export interface ItemsSelectedAction extends Action<string>{
    selectedItemIds: string[];
}