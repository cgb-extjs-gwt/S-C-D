import { createStore, combineReducers, applyMiddleware } from "redux";
import thunk from "redux-thunk";
//import { navigationReducer } from "../Layout/Reducers/Navigation";

export const storeFactory = () => {
    //const reducer = combineReducers({currentPath: navigationReducer});

    // const reducer = combineReducers({ 
    // });  

    // return createStore(
    //     reducer, 
    //     null, 
    //     applyMiddleware(thunk)
    // );
}
