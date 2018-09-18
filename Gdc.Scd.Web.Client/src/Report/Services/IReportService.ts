import { DataInfo } from "../../Common/States/CommonStates";
import { HwCalcFilterModel } from "../Model/HwCalcFilterModel";
import { HwCalcListModel } from "../Model/HwCalcListModel";
import { AutoGridModel } from "../Model/AutogridModel";

export interface IReportService {
    getHwCost(filter: HwCalcFilterModel): Promise<DataInfo<HwCalcListModel>>;

    getSchema(type: string): Promise<AutoGridModel>;
}
