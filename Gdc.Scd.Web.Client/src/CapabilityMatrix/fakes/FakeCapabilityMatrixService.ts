import { NamedId, DataInfo } from "../../Common/States/CommonStates";
import { CapabilityMatrixEditModel } from "../Model/CapabilityMatrixEditModel";
import { CapabilityMatrixListModel } from "../Model/CapabilityMatrixListModel";
import { ICapabilityMatrixService } from "../Services/ICapabilityMatrixService";
import { CapabilityMatrixFilterModel } from "../Model/CapabilityMatrixFilterModel";
import { fakeAllowed } from "./FakeAllowed";
import { fakeCountries } from "./FakeCountries";
import { fakeWG } from "./FakeWG";
import { fakeAvailability } from "./FakeAvailability";
import { fakeDuration } from "./FakeDuration";
import { fakeReactTypes } from "./FakeReactTypes";
import { fakeReactTimeTypes } from "./FakeReactTimeTypes";
import { fakeServiceLocationTypes } from "./FakeServiceLocationTypes";
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

    public getCountries(): Promise<NamedId[]> {
        return this.fromResult(fakeCountries);
    }

    public getWG(): Promise<NamedId[]> {
        return this.fromResult(fakeWG);
    }

    public getAvailabilityTypes(): Promise<NamedId[]> {
        return this.fromResult(fakeAvailability);
    }

    public getDurationTypes(): Promise<NamedId[]> {
        return this.fromResult(fakeDuration);
    }

    public getReactTypes(): Promise<NamedId[]> {
        return this.fromResult(fakeReactTypes);
    }

    public getReactionTimeTypes(): Promise<NamedId[]> {
        return this.fromResult(fakeReactTimeTypes);
    }

    public getServiceLocationTypes(): Promise<NamedId[]> {
        return this.fromResult(fakeServiceLocationTypes);
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
