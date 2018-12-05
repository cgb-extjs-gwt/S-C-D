import { Action } from "redux";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { CommonAction } from "../../Common/Actions/CommonActions";
import { getAppData } from "../Services/AppService";
import { AppData } from "../States/AppStates";

export const APP_PAGE_OPEN = 'APP.PAGE.OPEN';
export const APP_PAGE_INIT = 'APP.PAGE.INIT';
export const APP_LOAD_DATA = "APP.LOAD.DATA";

export interface OpenPageAction extends Action<string> {
    id: string
}

export interface PageInitAction<T = any> extends Action<string> {
    pageId: string
    data: T
}

export interface LoadingAction extends Action<string> {
    isLoading: boolean
}

export interface LoadingAppDataAction extends CommonAction<AppData> {
}

export const openPage = (id: string) => (<OpenPageAction>{
    type: APP_PAGE_OPEN,
    id
})

export const pageInit = (pageId: string, data) => (<PageInitAction>{
    type: APP_PAGE_INIT,
    pageId,
    data
})

export const loadAppData = (data: AppData) => (<LoadingAppDataAction>{
    type: APP_LOAD_DATA,
    data
})

export const loadMetaDataFromServer = () => asyncAction(
    dispatch => {
        getAppData().then(
            data => {
                dispatch(loadAppData(data));
            }
        );
    }
)