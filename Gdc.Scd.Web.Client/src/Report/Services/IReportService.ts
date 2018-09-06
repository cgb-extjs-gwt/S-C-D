import { DataInfo } from "../../Common/States/CommonStates";
import { HwCalcFilterModel } from "../Model/HwCalcFilterModel";
import { HwCalcListModel } from "../Model/HwCalcListModel";

export interface IReportService {
    getHwCost(filter: HwCalcFilterModel): Promise<DataInfo<HwCalcListModel>>;
}
