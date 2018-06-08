import { Reducer } from "redux";
import { PageState, PageTitle, PageData, PageError } from "../States/PageStates";
import { PAGE_OPEN, PAGE_INIT_SUCCESS, PAGE_INIT_ERROR, PageAction } from "../Actions/PageActions";

const defaultState = () => (<PageState>{
    title: ''
});

export const pageReducer: Reducer<PageState, PageAction> = (state = defaultState(), action) => {
    switch (action.type) {
        case PAGE_OPEN:
            return <PageState>{ 
                title: (<PageTitle>action.data).title,
                isLoading: true
            };

        case PAGE_INIT_SUCCESS:
            return <PageState>{ 
                ...state,
                isLoading: false
            };

        case PAGE_INIT_ERROR:
            return <PageState>{ 
                ...state,
                isLoading: false,
                error: (<PageError>action.data).error 
            };

        default:
            return state;
    }
} 