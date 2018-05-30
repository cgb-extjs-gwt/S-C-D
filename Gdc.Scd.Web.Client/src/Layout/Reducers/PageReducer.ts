import { Reducer } from "redux";
import { PageState, PageTitle, PageData, PageError } from "../States/PageStates";
import { CommonAction } from "../../Common/CommonAction";
import { PAGE_OPEN, PAGE_INIT_SUCCESS, PAGE_INIT_ERROR, PageAction } from "../Actions/PageActions";

const defaultState = () => (<PageState>{
    title: ''
});

export const pageReducer: Reducer<PageState, PageAction> = (state = defaultState(), action) => {
    switch (action.type) {
        case PAGE_OPEN:
            return <PageState>{ 
                title: (<PageTitle>action.data).title 
            };

        case PAGE_INIT_SUCCESS:
            return <PageState>{ 
                ...state,
                data: (<PageData>action.data).data 
            };

        case PAGE_INIT_ERROR:
            return <PageState>{ 
                ...state,
                error: (<PageError>action.data).error 
            };

        default:
            return state;
    }
} 