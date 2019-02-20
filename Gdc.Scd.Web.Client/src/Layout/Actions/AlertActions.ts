import { Action } from "redux";

export const APP_ALERT_DEFAULT = "APP.ALERT.DEFAULT";
export const APP_ALERT_ERROR = "APP.ALERT.ERROR";
export const APP_ALERT_INFO = "APP.ALERT.INFO";
export const APP_ALERT_LINK = "APP.ALERT.LINK";
export const APP_ALERT_REPORT = "APP.ALERT.REPORT";
export const APP_ALERT_SUCCESS = "APP.ALERT.SUCCESS";
export const APP_ALERT_WARNING = "APP.ALERT.WARNING";

export interface AlertAction extends Action<string> {
    text: string,
    caption: string
}

export interface LinkAction extends AlertAction {
    url: string
}

export interface ReportAction extends AlertAction {
    url: string,
    report: string,
    filter: any
}

export const remoteDefault = (text: string) => (<AlertAction>{
    type: APP_ALERT_DEFAULT,
    text: text
})

export const report = (caption: string, text: string, url: string, report: string, filter: any) => (<ReportAction>{
    type: APP_ALERT_REPORT,
    caption: caption || 'Report',
    text: text,

    url: url,

    report: report,
    filter: filter
})