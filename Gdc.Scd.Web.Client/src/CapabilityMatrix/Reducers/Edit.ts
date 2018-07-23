import * as actionTypes from "../Actions/ActionTypes"
import { Action } from "redux";

export interface countryChangeAction extends Action<string> {
    country: string
}

export function capabilityMatrixEditReducer(state: any = {}, action: countryChangeAction) {

    switch (action.type) {

        case actionTypes.MATRIX_EDIT_ALLOW_COMBINATION:
            return state;

        case actionTypes.MATRIX_EDIT_DENY_COMBINATION:
            return state;

        case actionTypes.MATRIX_EDIT_COUNTRY_CHANGE:
            return { ...state, isPortfolio: !action.country };

        default:
            return state;
    }

}