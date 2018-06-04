import { Action } from "redux";

export interface ItemSelectedAction extends Action<string> {
    selectedItemId: string;
}