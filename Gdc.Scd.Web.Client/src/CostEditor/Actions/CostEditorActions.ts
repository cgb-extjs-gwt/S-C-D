import { CostEditorState } from "../States/CostEditorStates";
import { Action, Dispatch } from "redux";
import { asyncAction } from "../../Common/Actions/AsyncAction";
import { ItemSelectedAction } from "../../Common/Actions/CommonActions";
import { pageInit } from "../../Layout/Actions/AppActions";
import { CommonState } from "../../Layout/States/AppStates";
import { handleRequest } from "../../Common/Helpers/RequestHelper";

export const COST_EDITOR_PAGE = 'costEditor';
export const COST_EDITOR_SELECT_APPLICATION = 'COST_EDITOR.SELECT.APPLICATION';

export const selectApplication = (applicationId: string) => (<ItemSelectedAction>{
    type: COST_EDITOR_SELECT_APPLICATION,
    selectedItemId: applicationId
})