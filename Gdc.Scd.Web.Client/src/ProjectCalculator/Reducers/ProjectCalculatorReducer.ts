import { Reducer, Action } from "redux";
import { ProjectCalculatorState, ProjectItemEditData } from "../States/ProjectCalculatorState";
import { PROJECT_CALCULATOR_SET_PROJECT_ITEM_EDIT_DATA, PROJECT_CALCULATOR_SELECT_PROJECT } from "../Actions/ProjectCalculatorActions";
import { CommonAction } from "../../Common/Actions/CommonActions";
import { Project } from "../States/Project";

const defaultState = () => (<ProjectCalculatorState>{
    projectItemEditData: null,
    selectedProject: null
})

const setProjectItemEditData: Reducer<ProjectCalculatorState, CommonAction<ProjectItemEditData>> = (state, { data }) => ({
    ...state,
    projectItemEditData: data
})

const selectProject: Reducer<ProjectCalculatorState, CommonAction<Project>> = (state, { data }) => ({
    ...state,
    selectedProject: data
})

export const projectCalculatorReducer: Reducer<ProjectCalculatorState, Action<string>> = (state = defaultState(), action) => {
    switch (action.type) {
        case PROJECT_CALCULATOR_SET_PROJECT_ITEM_EDIT_DATA:
            return setProjectItemEditData(state, <CommonAction<ProjectItemEditData>>action)

        case PROJECT_CALCULATOR_SELECT_PROJECT:
            return selectProject(state, <CommonAction<Project>>action)

        default:
            return state;
    }
}