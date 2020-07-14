import { connect } from "react-redux";
import { ProjectReportProps, ProjectReport } from "./ProjectReport";
import { CommonState } from "../../Layout/States/AppStates";

export const ProjectReportContainer = connect<ProjectReportProps, {}, any, CommonState>(
    (state, { match }) => ({
        projectId: match.params.projectId
    })
)(ProjectReport)