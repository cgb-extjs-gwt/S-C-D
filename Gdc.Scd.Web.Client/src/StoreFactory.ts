import { createStore, combineReducers, applyMiddleware, Action } from "redux";
import { AsyncAction } from "./Common/Actions/AsyncAction";
import { CostEditorState } from "./CostEditor/States/CostEditorStates";
import { costEditorReducer } from "./CostEditor/Reducers/CostEditorReducer";
import { costBlockReducer } from "./CostEditor/Reducers/CostBlockReducer";
import { appReducer } from "./Layout/Reducers/AppReducer";
import { CommonState, AppState } from "./Layout/States/AppStates";
import { COST_EDITOR_PAGE } from "./CostEditor/Actions/CostEditorActions";
import { COST_APPROVAL_PAGE } from "./CostApproval/Actions/CostApprovalFilterActions";
import { bundleFilterReducer } from "./CostApproval/Reducers/BundleFilterReducer";

const asyncActionHandler = store => next => action => {
    if (action instanceof AsyncAction) {
        (<AsyncAction>action).handler(store.dispatch, store.getState, action);
    } else {
        next(action);
    }
}

export const storeFactory = () => {
    const reducer = combineReducers({
        app: (state: AppState, action: Action<string>) => appReducer(state, action),
        pages: combineReducers({
            [COST_EDITOR_PAGE]: (state: CostEditorState, action: Action<string>) => {
                state = costEditorReducer(state, action);
                state = costBlockReducer(state, action);
            
                return state;
            },
            [COST_APPROVAL_PAGE]: bundleFilterReducer
        })
    });

    return createStore(
        reducer,
        applyMiddleware(asyncActionHandler)
    );
}
