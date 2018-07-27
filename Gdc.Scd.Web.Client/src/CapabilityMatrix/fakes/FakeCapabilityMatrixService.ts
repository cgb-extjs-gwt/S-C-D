import { NamedId } from "../../Common/States/CommonStates";
import { CapabilityMatrixEditModel } from "../Model/CapabilityMatrixEditModel";
import { CapabilityMatrixListModel } from "../Model/CapabilityMatrixListModel";
import { ICapabilityMatrixService } from "../Services/ICapabilityMatrixService";
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

    public allowItem(row: CapabilityMatrixEditModel) {
        return this.saveItem(row, true);
    }

    public allowItems(ids: string[]): Promise<any> {
        return this.fromResult({});
    }

    public denyItem(row: CapabilityMatrixEditModel) {
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

    public getAllowed(): Promise<CapabilityMatrixListModel[]> {
        return this.fromResult(fakeAllowed);
    }

    public getDenied(): Promise<CapabilityMatrixListModel[]> {
        return this.fromResult(fakeDenied);
    }

    private saveItem(row: CapabilityMatrixEditModel, allow: boolean) {
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
