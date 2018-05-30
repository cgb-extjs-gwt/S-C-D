import { Action } from "redux";
import { Dispatch } from "react-redux";

export interface AsyncActionHandler<TState> {
    (dispatch: Dispatch, state: TState): void
}

export class AsyncAction<TState = any> implements Action<string> {
    type = 'ASYNC_ACTION';
    handler: (dispatch: Dispatch, state: TState) => void
    constructor(handler: AsyncActionHandler<TState>){
        this.handler = handler;
    }
}

export const asyncAction = <TState = any>(handler: AsyncActionHandler<TState>) => new AsyncAction<TState>(handler)