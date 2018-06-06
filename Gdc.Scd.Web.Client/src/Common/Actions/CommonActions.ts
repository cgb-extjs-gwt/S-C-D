import { Action } from "redux";

export interface ItemSelectedAction extends Action<string> {
    selectedItemId: string;
}

// export interface ItemSelectedWithOldValueAction extends Action<string> {
//     newSelectedItemId: string;
//     oldSelectedItemId: string;
// }