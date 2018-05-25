import { NAVIGATE_ACTION_NAME, NavigateAction } from "../Actions/_Navigate";
import { Action } from "redux";

export const navigationReducer = (state: string = '/', action: Action<string>) => {
    switch (action.type) {
        case NAVIGATE_ACTION_NAME:
            navigateActionHandler(state, <NavigateAction>action);
            break;
        
        default:
            return state;
    }
}

const navigateActionHandler = (state: string, { pathName, history }: NavigateAction) => {
    history.push(pathName);

    return pathName;
}

