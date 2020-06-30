import { connect } from "react-redux";
import { CommonState } from "../../Layout/States/AppStates";
import { ProjectEditor, ProjectEditorProps, ProjectEditorActions } from "./ProjectEditor";
import { getProject, saveProjectWhithInterpolation } from "../Services/ProjectService";
import { selectProject } from "../Actions/ProjectCalculatorActions";
import { buildComponentUrl } from "../../Common/Services/Ajax";
import { Paths } from "../../Layout/Components/LayoutContainer";
import { loadProjectItemEditData } from "../Actions/ProjectCalculatorAsyncActions";
import { handleRequest } from "../../Common/Helpers/RequestHelper";
import { StoreOperation } from "../../Common/States/ExtStates";

const goToList = history => {
    history.push(buildComponentUrl(Paths.projectCalculatorList));
}

export const ProjectEditorContainer = connect<ProjectEditorProps, ProjectEditorActions, any, CommonState>(
    ({ pages: { projectCalculator } }) => ({
        project: projectCalculator.selectedProject,
        projectItemEditData: projectCalculator.projectItemEditData
    }),
    (dispatch, { match, history }) => ({
        onInit: () => {
            const projectId: number = +match.params.id;

            if (projectId) {
                handleRequest(
                    getProject(match.params.id).then(project => dispatch(selectProject(project)))
                );
            }

            dispatch(loadProjectItemEditData());
        },
        onBackToList: project => {
            if (project.id == 0) {
                selectProject(null);
            }

            goToList(history);
        },
        onSave: project => {
            handleRequest(
                saveProjectWhithInterpolation(project).then(
                    updatedProject => dispatch(selectProject(updatedProject))
                )
            );
        },
        onUpdateProjectItems: (records, operation, dataIndex) => {
            if (operation == StoreOperation.Edit) {
                for (const record of records) {
                    switch (dataIndex) {
                        case 'wgId':
                        case 'countryId':    
                        case 'availability':
                        case 'reactionTypeId': 
                        case 'serviceLocationId':
                        case 'duration.value':    
                        case 'duration.periodType':
                        case 'reactionTime.value':    
                        case 'reactionTime.periodType':
                            record.data.isRecalculation = true;
                            break;
                    }
                }
            }
        }
    })
)(ProjectEditor)