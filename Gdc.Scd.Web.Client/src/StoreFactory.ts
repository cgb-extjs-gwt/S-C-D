import { createStore, combineReducers, applyMiddleware, Action, compose } from "redux";
import { AsyncAction } from "./Common/Actions/AsyncAction";
import { CostEditorState } from "./CostEditor/States/CostEditorStates";
import { costEditorReducer } from "./CostEditor/Reducers/CostEditorReducer";
import { costBlockReducer } from "./CostEditor/Reducers/CostBlockReducer";
import { appReducer } from "./Layout/Reducers/AppReducer";
import { CommonState, AppState } from "./Layout/States/AppStates";
import { COST_EDITOR_PAGE } from "./CostEditor/Actions/CostEditorActions";
import { buildApprovalCostElementsReducer } from "./CostApproval/Reducers/ApprovalCostElementsReducer";
import { COST_APPROVAL_PAGE, OWN_COST_APPROVAL_PAGE } from "./CostApproval/Constants/CostApprovalConstants";
import { qualityGateErrorsReducer } from "./CostApproval/Reducers/QualityGateErrorsReducer";

const asyncActionHandler = store => next => action => {
    if (action instanceof AsyncAction) {
        (<AsyncAction>action).handler(store.dispatch, store.getState, action);
    } else {
        next(action);
    }
}

const composeEnhancers = (<any>window).__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose;

export const storeFactory = () => {
    const reducer = combineReducers({
        app: (state: AppState, action: Action<string>) => appReducer(state, action),
        pages: combineReducers({
            [COST_EDITOR_PAGE]: (state: CostEditorState, action: Action<string>) => {
                state = costEditorReducer(state, action);
                state = costBlockReducer(state, action);
            
                return state;
            },
            [COST_APPROVAL_PAGE]: buildApprovalCostElementsReducer(COST_APPROVAL_PAGE),
            [OWN_COST_APPROVAL_PAGE]: buildApprovalCostElementsReducer(OWN_COST_APPROVAL_PAGE, {
                qualityGateErrors: qualityGateErrorsReducer
            })
        })
    });

    return createStore(
        reducer,
        composeEnhancers(applyMiddleware(asyncActionHandler))
    );
}
