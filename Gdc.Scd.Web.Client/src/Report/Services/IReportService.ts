import { DataInfo } from "../../Common/States/CommonStates";
import { AutoGridModel } from "../Model/AutogridModel";
import { HwCostFilterModel } from "../Model/HwCostFilterModel";
import { HwCostListModel } from "../Model/HwCostListModel";
import { ReportModel } from "../Model/ReportModel";

export interface IReportService {
    getHwCost(filter: HwCostFilterModel): Promise<DataInfo<HwCostListModel>>;

    getSchema(id: string): Promise<AutoGridModel>;

    getSchemaByName(name: string): Promise<AutoGridModel>;

    getReports(): Promise<DataInfo<ReportModel>>;
}
