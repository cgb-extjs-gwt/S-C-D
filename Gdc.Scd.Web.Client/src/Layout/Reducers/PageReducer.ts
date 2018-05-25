import { Reducer } from "redux";
import { PageState, PageTitle, PageData } from "../States/PageStates";
import { CommonAction } from "../../Common/CommonAction";
import { PAGE_OPEN } from "../Actions/PageActions";

const defaultState = () => (<PageState>{
    title: '',
    data: null
});

export const pageReducer: Reducer<PageState, CommonAction> = (state = defaultState(), action) => {
    switch (action.type) {
        case PAGE_OPEN:
            return <PageState>{ 
                title: (<PageTitle>action.data).title 
            };

        case PAGE_OPEN:
            return <PageState>{ 
                ...state,
                title: (<PageData>action.data).data 
            };

        default:
            return state;
    }
} 