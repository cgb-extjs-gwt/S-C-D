import { Reducer, Action } from "redux";
import { PageAction } from "../Actions/CommonActions";

export const buildPageReducer = (pageName: string, reducer: Reducer) => (state, action) => {
    const pageAction = <PageAction>action;

    return pageAction.pageName == null || pageAction.pageName == pageName
        ? reducer(state, action) 
        : state;
}