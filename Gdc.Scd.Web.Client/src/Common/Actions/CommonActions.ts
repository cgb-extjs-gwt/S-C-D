import { Action } from "redux";

export interface ItemSelectedAction extends Action<string> {
    selectedItemId: string;
}

export interface CommonAction<T = any> extends Action<string> {
    data: T;
}