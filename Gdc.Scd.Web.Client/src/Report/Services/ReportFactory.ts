import { DictService } from "../../Dict/Services/DictService";
import { IDictService } from "../../Dict/Services/IDictService";
import { FakeReportService } from "../fakes/FakeReportService";
import { IReportService } from "./IReportService";

export class ReportFactory {

    public static getReportService(): IReportService {
        return new FakeReportService();
    }

    public static getDictService(): IDictService {
        return new DictService();
    }

}