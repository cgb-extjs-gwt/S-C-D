import { DataInfo } from "../../Common/States/CommonStates";
import { CapabilityMatrixEditModel } from "../Model/CapabilityMatrixEditModel";
import { CapabilityMatrixFilterModel } from "../Model/CapabilityMatrixFilterModel";
import { CapabilityMatrixListModel } from "../Model/CapabilityMatrixListModel";
import { ICapabilityMatrixService } from "../Services/ICapabilityMatrixService";
import { fakeAllowed } from "./FakeAllowed";
import { fakeDenied } from "./FakeDenied";

export class FakeCapabilityMatrixService implements ICapabilityMatrixService {

    public allowItem(row: CapabilityMatrixEditModel): Promise<any> {
        return this.saveItem(row, true);
    }

    public allowItems(ids: string[]): Promise<any> {
        return this.fromResult({});
    }

    public denyItem(row: CapabilityMatrixEditModel): Promise<any> {
        return this.saveItem(row, false);
    }

    public getAllowed(filter: CapabilityMatrixFilterModel): Promise<DataInfo<CapabilityMatrixListModel>> {
        return this.fromResult({ items: fakeAllowed, total: fakeAllowed.length * 5 });
    }

    public getDenied(filter: CapabilityMatrixFilterModel): Promise<DataInfo<CapabilityMatrixListModel>> {
        return this.fromResult({ items: fakeDenied, total: fakeDenied.length * 5 });
    }

    private saveItem(row: CapabilityMatrixEditModel, allow: boolean): Promise<any> {
        throw new Error("Method not implemented.");
    }

    private fromResult<T>(value: T): Promise<T> {
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                resolve(value);
            }, 1);
        });
    }
}
