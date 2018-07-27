import { NamedId } from "../../Common/States/CommonStates";
import { CapabilityMatrixListModel } from "../Model/CapabilityMatrixListModel";
import { CapabilityMatrixItem } from "../Model/CapabilityMatrixItem";
import { fakeAllowed } from "./FakeAllowed";
import { fakeCountries } from "./FakeCountries";
import { fakeWG } from "./FakeWG";
import { fakeAvailability } from "./FakeAvailability";
import { fakeDuration } from "./FakeDuration";
import { fakeReactTypes } from "./FakeReactTypes";
import { fakeReactTimeTypes } from "./FakeReactTimeTypes";
import { fakeServiceLocationTypes } from "./FakeServiceLocationTypes";
import { fakeDenied } from "./FakeDenied";
import { ICapabilityMatrixService } from "../Services/ICapabilityMatrixService";

export class FakeCapabilityMatrixService implements ICapabilityMatrixService {

    public allowItem(row: CapabilityMatrixItem) {
        return this.saveItem(row, true);
    }

    public denyItem(row: CapabilityMatrixItem) {
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

    private saveItem(row: CapabilityMatrixItem, allow: boolean) {
        const key = 'DENY_MATRIX';

        let json = localStorage.getItem(key);
        let allDeny: any = json ? JSON.parse(json) : {};

        let hash = row.hash();
        if (allow) {
            allDeny[hash] = row;
        }
        else {
            allDeny[hash] = null;
            delete allDeny[hash];
        }

        localStorage.setItem(key, JSON.stringify(allDeny));

        return Promise.resolve();
    }

    private fromResult<T>(value: T): Promise<T> {
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                resolve(value);
            }, 1);
        });
    }
}
