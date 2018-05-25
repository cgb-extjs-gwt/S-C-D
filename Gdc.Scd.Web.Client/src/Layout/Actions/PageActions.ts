import { PageState, PageTitle, PageData } from "../States/PageStates";
import { CommonAction } from "../../Common/CommonAction";

export const PAGE_OPEN = 'PAGE.OPEN';
export const PAGE_INIT = 'PAGE.INIT';

export const openPage = (title: string) => (<CommonAction<PageTitle>>{
    type: PAGE_OPEN,
    data: { title }
});

export const initPage = data => (<CommonAction<PageData>>{
    type: PAGE_INIT,
    data: { data }
});