// import { PageState, PageTitle, PageData, PageError } from "../../Layout/States/PageStates";
// import { CommonAction } from "../CommonAction";
// import { Reducer } from "redux";

// export abstract class BasePageReducerBuilder<T> {
//     public toReducer(): Reducer<PageState, CommonAction> {
//         return this.reduce.bind(this);
//     }

//     protected reduce(state: PageState = this.getDefaultState(), action: CommonAction) {
//         switch (action.type) {
//             case PAGE_OPEN:
//                 return <PageState>{ 
//                     title: (<PageTitle>action.data).title 
//                 };
    
//             case PAGE_INIT_SUCCESS:
//                 return <PageState>{ 
//                     ...state,
//                     data: (<PageData>action.data).data 
//                 };
    
//             case PAGE_INIT_ERROR:
//                 return <PageState>{ 
//                     ...state,
//                     error: (<PageError>action.data).error 
//                 };
    
//             default:
//                 return state;
//         }
//     }

//     protected getDefaultState() {
//         return <PageState>{
//             title: ''
//         }
//     }

//     protected abstract getPageName();

//     protected abstract getPageTitle();
// }