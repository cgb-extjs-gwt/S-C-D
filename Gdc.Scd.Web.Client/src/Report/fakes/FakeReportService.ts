import { HwCalcFilterModel } from "../Model/HwCalcFilterModel";
import { HwCalcListModel } from "../Model/HwCalcListModel";
import { IReportService } from "../Services/IReportService";
import { fakeHwCost } from "./FakeHwCost";
import { DataInfo } from "../../Common/States/CommonStates";

export class FakeReportService implements IReportService {

    public getHwCost(filter: HwCalcFilterModel): Promise<DataInfo<HwCalcListModel>> {
        return this.fromResult({ items: fakeHwCost, total: fakeHwCost.length * 5 });
    }

    private fromResult<T>(value: T): Promise<T> {
        return Promise.resolve(value);
    }
}
