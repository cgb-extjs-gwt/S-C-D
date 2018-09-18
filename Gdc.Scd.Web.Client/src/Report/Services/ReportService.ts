import { DataInfo } from "../../Common/States/CommonStates";
import { HwCalcFilterModel } from "../Model/HwCalcFilterModel";
import { HwCalcListModel } from "../Model/HwCalcListModel";
import { IReportService } from "./IReportService";
import { AutoGridModel } from "../Model/AutogridModel";

export class ReportService implements IReportService {

    public getHwCost(filter: HwCalcFilterModel): Promise<DataInfo<HwCalcListModel>> {
        throw new Error("Method not implemented.");
    }

    public getSchema(): Promise<AutoGridModel> {
        throw new Error("Method not implemented.");
    }
}
