import { Action } from "redux";

export const APP_REMOTE_DEFAULT = "APP.REMOTE.DEFAULT";
export const APP_REMOTE_REPORT = "APP.REMOTE.REPORT";

export interface RemoteAction extends Action<string> {
    text: string
}

export interface LinkPreparedAction extends RemoteAction { }

export const remoteDefault = (text: string) => (<RemoteAction>{
    type: APP_REMOTE_DEFAULT,
    text: text
})