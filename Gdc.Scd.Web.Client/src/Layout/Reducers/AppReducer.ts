import { Reducer, Action } from "redux";
import { 
    OpenPageAction, 
    ErrorAction, 
    APP_PAGE_OPEN,
    APP_LOADING, 
    APP_ERROR, 
    LoadingAction
} from "../Actions/AppActions";
import { AppState } from "../States/AppStates";

const defaultState = () => (<AppState>{
    error: null,
    isLoading: false
});

const openPage: Reducer<AppState, OpenPageAction> = (state, action) => ({
    ...state,
    currentPage: {
        id: action.id,
        title: action.title
    }
})

const error: Reducer<AppState, ErrorAction> = (state, action) => ({
    ...state
})

const loading: Reducer<AppState, LoadingAction> = (state, action) => ({
    ...state,
    isLoading: action.isLoading
})

export const appReducer: Reducer<AppState, Action<string>> = (state = defaultState(), action) => {
    switch (action.type) {
        case APP_PAGE_OPEN:
            return openPage(state, <OpenPageAction>action);

        case APP_LOADING:
            return loading(state, <LoadingAction>action)

        case APP_ERROR:
            return error(state, <ErrorAction>action)

        default:
            return state;
    }
} 