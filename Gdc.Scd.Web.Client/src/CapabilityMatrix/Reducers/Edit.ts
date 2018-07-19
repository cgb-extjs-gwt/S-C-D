import * as actionTypes from "../Actions/ActionTypes"

export function editReducer(state: any, action: string) {

    switch (action) {

        case actionTypes.MATRIX_EDIT_ALLOW_COMBINATION:
            break;

        case actionTypes.MATRIX_EDIT_DENY_COMBINATION:
            break;

        case actionTypes.MATRIX_EDIT_COUNTRY_CHANGE:
            break;

        default:
            return state;
    }

}