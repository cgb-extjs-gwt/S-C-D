import { ReportView } from "../../Report";
import { buildMvcUrl } from "../../Common/Services/Ajax";
import { PROJECT_CONTROLLER_NAME } from "../Services/ProjectService";

export interface ProjectReportProps {
    projectId: number
}

export class ProjectReport extends ReportView<ProjectReportProps> {
    public getUrl(): string {
        return buildMvcUrl(PROJECT_CONTROLLER_NAME, 'ReportView', this.getParams());
    }

    public getDownloadUrl(): string {
        return buildMvcUrl(PROJECT_CONTROLLER_NAME, 'ReportExport', this.getParams());
    }

    private getParams() {
        return {
            id: this.state.id,
            projectId: this.props.projectId
        }
    }
}