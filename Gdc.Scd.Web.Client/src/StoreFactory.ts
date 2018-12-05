import { createStore, combineReducers, applyMiddleware, Action, compose, Reducer } from "redux";
import { AsyncAction } from "./Common/Actions/AsyncAction";
import { CostEditorState } from "./CostEditor/States/CostEditorStates";
import { costEditorReducer } from "./CostEditor/Reducers/CostEditorReducer";
import { appReducer } from "./Layout/Reducers/AppReducer";
import { CommonState, AppState } from "./Layout/States/AppStates";
import { COST_EDITOR_PAGE } from "./CostEditor/Actions/CostEditorActions";
import { buildApprovalCostElementsReducer } from "./CostApproval/Reducers/ApprovalCostElementsReducer";
import { COST_APPROVAL_PAGE, OWN_COST_APPROVAL_PAGE } from "./CostApproval/Constants/CostApprovalConstants";
import { qualityGateErrorsReducer } from "./CostApproval/Reducers/QualityGateErrorsReducer";
import { tableViewReducer } from "./TableView/Reducers/TableViewReducer";
import { CostMetaData } from "./Common/States/CostMetaStates";

const asyncActionHandler = store => next => action => {
    if (action instanceof AsyncAction) {
        (<AsyncAction>action).handler(store.dispatch, store.getState, action);
    } else {
        next(action);
    }
}

const composeEnhancers = (<any>window).__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose;

// const buildRootReducer = () => {
//     const combinedCostEditorReducer = (state: CommonState, action: Action<string>) => {
//         state = costEditorReducer(state, action);
//         state = costBlockReducer(state, action);
    
//         return state;
//     };

//     const reducer = combineReducers({
//         app: appReducer,
//         pages: combineReducers({
//             [COST_APPROVAL_PAGE]: buildApprovalCostElementsReducer(COST_APPROVAL_PAGE),
//             [OWN_COST_APPROVAL_PAGE]: buildApprovalCostElementsReducer(OWN_COST_APPROVAL_PAGE, {
//                 qualityGateErrors: qualityGateErrorsReducer
//             }),
//             tableView: tableViewReducer
//         })
//     });

//     return (state: CommonState, action: Action<string>) => {
//         state = <CommonState>reducer(state, action);
//         state = combinedCostEditorReducer(state, action);

//         return state;
//     }
// }

// const buildRootReducer = () => {
//     const pagesReducer = combineReducers({
//         [COST_EDITOR_PAGE]: (state = {}) => state,
//         [COST_APPROVAL_PAGE]: buildApprovalCostElementsReducer(COST_APPROVAL_PAGE),
//         [OWN_COST_APPROVAL_PAGE]: buildApprovalCostElementsReducer(OWN_COST_APPROVAL_PAGE, {
//             qualityGateErrors: qualityGateErrorsReducer
//         }),
//         tableView: tableViewReducer
//     });

//     const rootReducer: Reducer<CommonState, Action<string>> = (state = <any>{}, action) => { 
//         const app = appReducer(state.app, action);

//         return <CommonState>{
//             app,
//             pages: {
//                 ...pagesReducer(state.pages, action),
//                 costEditor: costEditorReducer(state.pages && state.pages.costEditor, action, app.appMetaData)
//             }
//         }
//     }

//     return rootReducer;
// }

export const storeFactory = () => {
    const reducer = combineReducers({
        app: appReducer,
        pages: combineReducers({
            [COST_EDITOR_PAGE]:costEditorReducer,
            [COST_APPROVAL_PAGE]: buildApprovalCostElementsReducer(COST_APPROVAL_PAGE),
            [OWN_COST_APPROVAL_PAGE]: buildApprovalCostElementsReducer(OWN_COST_APPROVAL_PAGE, {
                qualityGateErrors: qualityGateErrorsReducer
            }),
            tableView: tableViewReducer
        })
    });

    return createStore(
        reducer,
        composeEnhancers(applyMiddleware(asyncActionHandler))
    );
}
