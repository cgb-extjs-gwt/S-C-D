import { Reducer, Action } from "redux";
import { 
    OpenPageAction, 
    APP_PAGE_OPEN,
    LoadingAction,
    LoadingAppDataAction,
    APP_LOAD_DATA
} from "../Actions/AppActions";
import { AppState } from "../States/AppStates";

const defaultState = () => (<AppState>{
    appMetaData: null,
    currentPage: null
});

const openPage: Reducer<AppState, OpenPageAction> = (state, action) => ({
    ...state,
    currentPage: {
        id: action.id,
        title: action.title
    }
})

const loadAppData: Reducer<AppState, LoadingAppDataAction> = (state, action) => ({
    ...state,
    appMetaData: action.data.meta,
    userRoles: action.data.userRoles
})

export const appReducer: Reducer<AppState, Action<string>> = (state = defaultState(), action) => {
    switch (action.type) {
        case APP_PAGE_OPEN:
            return openPage(state, <OpenPageAction>action);

        case APP_LOAD_DATA:
            return loadAppData(state, <LoadingAppDataAction>action);

        default:
            return state;
    }
} 