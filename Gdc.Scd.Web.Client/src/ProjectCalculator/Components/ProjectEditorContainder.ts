import { connect } from "react-redux";
import { CommonState } from "../../Layout/States/AppStates";
import { ProjectEditor, ProjectEditorProps, ProjectEditorActions } from "./ProjectEditor";
import { getProject, saveProject } from "../Services/ProjectService";
import { selectProject } from "../Actions/ProjectCalculatorActions";
import { buildComponentUrl } from "../../Common/Services/Ajax";
import { Paths } from "../../Layout/Components/LayoutContainer";
import { loadProjectItemEditData } from "../Actions/ProjectCalculatorAsyncActions";

const goToList = history => {
    history.push(buildComponentUrl(Paths.projectCalculatorList));
}

export const ProjectEditorContainder = connect<ProjectEditorProps, ProjectEditorActions, any, CommonState>(
    ({ pages: { projectCalculator } }) => ({
        project: projectCalculator.selectedProject,
        projectItemEditData: projectCalculator.projectItemEditData
    }),
    (dispatch, { match, history }) => ({
        onInit: () => {
            const projectId: number = +match.params.id;

            if (projectId) {
                getProject(match.params.id).then(project => dispatch(selectProject(project)));
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
            saveProject(project).then(() => {
                goToList(history);
            })
        }
    })
)(ProjectEditor)