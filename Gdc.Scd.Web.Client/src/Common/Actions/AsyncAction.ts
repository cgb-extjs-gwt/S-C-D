import { Action } from "redux";
import { Dispatch } from "react-redux";

export interface AsyncActionHandler<TState> {
    (dispatch: Dispatch, getState: () => TState, action: AsyncAction<TState>): void
}

export class AsyncAction<TState = any> implements Action<string> {
    type = 'ASYNC_ACTION';
    handler: AsyncActionHandler<TState>;

    constructor(handler: AsyncActionHandler<TState>){
        this.handler = handler;
    }
}

export const asyncAction = <TState = any>(handler: AsyncActionHandler<TState>) => new AsyncAction<TState>(handler)