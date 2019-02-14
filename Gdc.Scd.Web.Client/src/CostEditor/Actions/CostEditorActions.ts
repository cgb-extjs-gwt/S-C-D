import { ItemSelectedAction } from "../../Common/Actions/CommonActions";

export const COST_EDITOR_PAGE = 'costEditor';
export const COST_EDITOR_SELECT_APPLICATION = 'COST_EDITOR.SELECT.APPLICATION';

export const selectApplication = (applicationId: string) => (<ItemSelectedAction>{
    type: COST_EDITOR_SELECT_APPLICATION,
    selectedItemId: applicationId
})