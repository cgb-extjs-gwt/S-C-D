import { ProjectItemEditData } from "../States/ProjectCalculatorState"
import { CommonAction } from "../../Common/Actions/CommonActions"
import { Project } from "../States/Project"

export const PROJECT_CALCULATOR_SET_PROJECT_ITEM_EDIT_DATA = 'PROJECT_CALCULATOR.SET.PROJECT_ITEM_EDIT_DATA'
export const PROJECT_CALCULATOR_SELECT_PROJECT = 'PROJECT_CALCULATOR.SELECT.PROJECT'

export const setProjectItemEditData = (data: ProjectItemEditData) =>(<CommonAction<ProjectItemEditData>>{
    type: PROJECT_CALCULATOR_SET_PROJECT_ITEM_EDIT_DATA,
    data
})

export const selectProject = (project: Project) => (<CommonAction<Project>>{
    type: PROJECT_CALCULATOR_SELECT_PROJECT,
    data: project
})