 import { createStore, combineReducers, applyMiddleware, compose } from "redux";
import { pageReducer } from "../Layout/Reducers/PageReducer";
import { costElementReducer } from "../InputCostElement/Reducers/CostElementReducer";
import { PageState } from "../Layout/States/PageStates";
import { CommonAction } from "./CommonAction";

export const storeFactory = () => {
    const reducer = combineReducers({ 
        //page: compose(pageReducer, costElementReducer)
        page: (state: PageState, action: CommonAction) => ({
                ...pageReducer(state, action),
                data: compose(costElementReducer)
        })
    });  

    return createStore(reducer);
}
