import { buildMvcUrl, get } from "../../Common/Services/Ajax"
import { ProjectItemEditData } from "../States/ProjectCalculatorState"

export const PROJECT_CONTROLLER_NAME = 'Project'

export const Urls = {
    getBy: buildMvcUrl(PROJECT_CONTROLLER_NAME, 'GetBy')
}

export const getProjectItemEditData = () => get<ProjectItemEditData>(PROJECT_CONTROLLER_NAME, 'GetProjectItemEditData')