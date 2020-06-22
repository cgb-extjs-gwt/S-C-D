import { buildMvcUrl, get, deleteItem, post } from "../../Common/Services/Ajax"
import { ProjectItemEditData } from "../States/ProjectCalculatorState"
import { Project } from "../States/Project"

export const PROJECT_CONTROLLER_NAME = 'Project'

export const Urls = {
    getBy: buildMvcUrl(PROJECT_CONTROLLER_NAME, 'GetBy')
}

export const getProjectItemEditData = () => get<ProjectItemEditData>(PROJECT_CONTROLLER_NAME, 'GetProjectItemEditData')

export const getProject = (id: number) => get<Project>(PROJECT_CONTROLLER_NAME, 'Get', { id })

export const deleteProject = (id: number) => deleteItem(PROJECT_CONTROLLER_NAME, 'Delete', { id })

export const saveProject = (project: Project) => post<Project>(PROJECT_CONTROLLER_NAME, 'Save', project)

export const saveProjectWhithInterpolation = (project: Project) => post<Project, Project>(PROJECT_CONTROLLER_NAME, 'SaveWithInterpolation', project)

