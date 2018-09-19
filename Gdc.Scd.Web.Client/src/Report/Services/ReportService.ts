import { get } from "../../Common/Services/Ajax";
import { DataInfo } from "../../Common/States/CommonStates";
import { AutoGridModel } from "../Model/AutogridModel";
import { HwCalcFilterModel } from "../Model/HwCalcFilterModel";
import { HwCalcListModel } from "../Model/HwCalcListModel";
import { ReportModel } from "../Model/ReportModel";
import { IReportService } from "./IReportService";

export class ReportService implements IReportService {

    private static schemas: any = {}; // promises cache, singleton
    private static reports: any; // reports cache, singleton

    private controllerName: string = 'report';

    public getHwCost(filter: HwCalcFilterModel): Promise<DataInfo<HwCalcListModel>> {
        throw new Error("Method not implemented.");
    }

    public getReports(): Promise<DataInfo<ReportModel>> {

        let p = ReportService.reports;

        if (!p) {
            p = get<DataInfo<ReportModel>>(this.controllerName, 'getall');
            ReportService.reports = p;
        }

        return p;
    }

    public getSchema(type: string): Promise<AutoGridModel> {

        let p = ReportService.schemas[type];

        if (!p) {
            p = get<AutoGridModel>(this.controllerName, 'schema', { type: type });
            ReportService.schemas[type] = p;
        }

        return p;
    }
}
