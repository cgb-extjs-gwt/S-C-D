import { DataInfo } from "../../Common/States/CommonStates";
import { AutoGridModel } from "../Model/AutogridModel";
import { HwCalcFilterModel } from "../Model/HwCalcFilterModel";
import { HwCalcListModel } from "../Model/HwCalcListModel";
import { ReportModel } from "../Model/ReportModel";

export interface IReportService {
    getHwCost(filter: HwCalcFilterModel): Promise<DataInfo<HwCalcListModel>>;

    getSchema(id: string): Promise<AutoGridModel>;

    getSchemaByName(name: string): Promise<AutoGridModel>;

    getReports(): Promise<DataInfo<ReportModel>>;
}
