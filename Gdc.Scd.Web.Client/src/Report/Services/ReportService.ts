import { get } from "../../Common/Services/Ajax";
import { DataInfo } from "../../Common/States/CommonStates";
import { AutoGridModel } from "../Model/AutogridModel";
import { HwCostFilterModel } from "../Model/HwCostFilterModel";
import { HwCostListModel } from "../Model/HwCostListModel";
import { ReportModel } from "../Model/ReportModel";
import { IReportService } from "./IReportService";

export class ReportService implements IReportService {

    private static schemas: any = {}; // promises cache, singleton
    private static reports: any; // reports cache, singleton

    private controllerName: string = 'report';

    public getHwCost(filter: HwCostFilterModel): Promise<DataInfo<HwCostListModel>> {
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

    public getSchema(id: string): Promise<AutoGridModel> {

        let p = ReportService.schemas[id];

        if (!p) {
            p = get<AutoGridModel>(this.controllerName, 'schema', { id: id });
            ReportService.schemas[id] = p;
        }

        return p;
    }

    public getSchemaByName(name: string): Promise<AutoGridModel> {

        let p = ReportService.schemas[name];

        if (!p) {
            p = get<AutoGridModel>(this.controllerName, 'schema', { name: name });
            ReportService.schemas[name] = p;
        }

        return p;
    }
}
