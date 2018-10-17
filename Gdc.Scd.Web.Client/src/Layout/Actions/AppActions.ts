import { CommonAction } from "../../Common/Actions/CommonActions";
import { Action } from "redux";
import { CostMetaData } from "../../Common/States/CostMetaStates";
import { getAppData } from "../Services/AppService";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { AppData } from "../States/AppStates";

export const APP_PAGE_OPEN = 'APP.PAGE.OPEN';
export const APP_PAGE_INIT = 'APP.PAGE.INIT';
export const APP_LOAD_DATA = "APP.LOAD.DATA";

export interface OpenPageAction extends Action<string> {
    id: string
    title: string
}

export interface PageInitAction<T = any> extends Action<string> {
    pageId: string
    data: T
}

export interface LoadingAction extends Action<string> {
    isLoading: boolean
}

export interface LoadingAppDataAction extends CommonAction<AppData>{
}

export const openPage = (id: string, title: string) => (<OpenPageAction>{
    type: APP_PAGE_OPEN,
    id,
    title
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