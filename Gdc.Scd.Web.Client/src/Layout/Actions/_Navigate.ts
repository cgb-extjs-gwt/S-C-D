import { Action } from "redux";

export const NAVIGATE_ACTION_NAME = 'NAVIGATE';

export interface History {
    push: (pathName: string) => void;
}

export interface NavigateAction extends Action<string> {
    pathName: string;
    history: History;
}

export const navigateActionCreator = (pathName: string, history: History) => (<NavigateAction>{
    type: NAVIGATE_ACTION_NAME,
    pathName,
    history
});