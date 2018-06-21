import { createStore, combineReducers, applyMiddleware } from "redux";
import { PageState, PAGE_STATE_KEY } from "./Layout/States/PageStates";
import { pageReducer } from "./Layout/Reducers/PageReducer";
import { PageAction } from "./Layout/Actions/PageActions";
import { AsyncAction } from "./Common/Actions/AsyncAction";
import { CostEditorState } from "./CostEditor/States/CostEditorStates";
import { costEditorReducer } from "./CostEditor/Reducers/CostEditorReducer";
import { costBlockReducer } from "./CostEditor/Reducers/CostBlockReducer";

const asyncActionHandler = store => next => action => {
    if (action instanceof AsyncAction) {
        (<AsyncAction>action).handler(store.dispatch, store.getState, action);
    } else {
        next(action);
    }
}

const pageDataReducer = (state: PageState<CostEditorState>, action: PageAction) => {
    let data = state.data;

    data = costEditorReducer(data, action);
    data = costBlockReducer(data, action);

    return data;
}

export const storeFactory = () => {
    const reducer = combineReducers({ 
        [PAGE_STATE_KEY]: (state: PageState, action: PageAction) => {
            const newState = pageReducer(state, action);

            return <PageState>{
                ...newState,
                data: pageDataReducer(newState, action)
            }
        }
    });  

    return createStore(
        reducer,
        applyMiddleware(asyncActionHandler)
    );
}
