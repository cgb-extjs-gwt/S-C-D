import { DataInfo } from "../../Common/States/CommonStates";
import { AutoGridModel } from "../Model/AutogridModel";
import { HwCalcFilterModel } from "../Model/HwCalcFilterModel";
import { HwCalcListModel } from "../Model/HwCalcListModel";
import { IReportService } from "../Services/IReportService";
import { fakeHwCost } from "./FakeHwCost";
import { fakeSchema } from "./FakeSchema";

export class FakeReportService implements IReportService {

    public getSchema(): Promise<AutoGridModel> {
        return this.fromResult(fakeSchema);
    }

    public getHwCost(filter: HwCalcFilterModel): Promise<DataInfo<HwCalcListModel>> {
        return this.fromResult({ items: fakeHwCost, total: fakeHwCost.length * 5 });
    }

    private fromResult<T>(value: T): Promise<T> {
        return Promise.resolve(value);
    }
}
