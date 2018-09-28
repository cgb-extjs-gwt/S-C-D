import { DataInfo } from "../../Common/States/CommonStates";
import { HwCalcFilterModel } from "../Model/HwCalcFilterModel";
import { HwCalcListModel } from "../Model/HwCalcListModel";
import { IReportService } from "./IReportService";

export class ReportService implements IReportService {

    getHwCost(filter: HwCalcFilterModel): Promise<DataInfo<HwCalcListModel>> {
        throw new Error("Method not implemented.");
    }

    
}
