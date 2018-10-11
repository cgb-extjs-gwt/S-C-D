import { DictService } from "../../Dict/Services/DictService";
import { IDictService } from "../../Dict/Services/IDictService";
import { IReportService } from "./IReportService";
import { ReportService } from "./ReportService";

export class ReportFactory {

    public static getReportService(): IReportService {
        return new ReportService();
    }

    public static getDictService(): IDictService {
        return new DictService();
    }

}