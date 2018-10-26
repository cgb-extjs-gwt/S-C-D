import { Action } from "redux";
import { CommonAction } from "../../Common/Actions/CommonActions";

export const APP_REMOTE_DEFAULT = "APP.REMOTE.DEFAULT";
export const APP_REMOTE_REPORT = "APP.REMOTE.REPORT";

export interface RemoteAction extends CommonAction<Action> { }

export interface LinkPreparedAction extends RemoteAction { }

export const remoteDefault = (data: any) => (<RemoteAction>{
    type: APP_REMOTE_DEFAULT,
    data
})