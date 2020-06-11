import { connect } from "react-redux";
import { CommonState } from "../../Layout/States/AppStates";
import { ProjectListProps, ProjectListActions, ProjectList } from "./ProjectList";
import { Urls } from "../Services/ProjectService";
import { selectProject } from "../Actions/ProjectCalculatorActions";
import { loadProjectItemEditData } from "../Actions/ProjectCalculatorAsyncActions";

export const ProjectListContainer = connect<ProjectListProps, ProjectListActions, {}, CommonState>(
    () => ({
        url: Urls.getBy
    }),
    dispatch => ({
        onInit: () => dispatch(loadProjectItemEditData()),
        onSelectProject: selectedProject => dispatch(selectProject(selectedProject))
    })
)(ProjectList)