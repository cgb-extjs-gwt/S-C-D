import { buildMvcUrl } from "../../Common/Services/Ajax";
import { AlertHelper } from "../../Common/Helpers/AlertHelper";

export class ExportService
{
    public static Download(report: string, approved: boolean, filter: any) {

        filter = filter || {};
        filter.report = report;
        filter.approved = approved ? 1 : 0;
        filter._dc = new Date().getTime();

        let url = buildMvcUrl('report', 'exportbyname');

        AlertHelper.autoload(url, report, filter);
    }
}
