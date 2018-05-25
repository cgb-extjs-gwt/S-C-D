import { Action } from "redux";

export interface CommonAction<T = any> extends Action<string> {
    data: T;
}