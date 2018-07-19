import * as actionTypes from "./ActionTypes"

export const allowCombination = () => {
    return {
        type: actionTypes.MATRIX_EDIT_ALLOW_COMBINATION
    };
}

export const denyCombination = () => {
    return {
        type: actionTypes.MATRIX_EDIT_DENY_COMBINATION
    };
}

export const countryChange = () => {
    return {
        type: actionTypes.MATRIX_EDIT_COUNTRY_CHANGE
    };
}



