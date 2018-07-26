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

export const allowItem = (row: CapabilityMatrixItem) => {
    return saveItem(row, true);
}

export const denyItem = (row: CapabilityMatrixItem) => {
    return saveItem(row, false);
}

export const saveItem = (row: CapabilityMatrixItem, allow: boolean) => {
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

export function getCountries(): Promise<NamedId[]> {
    return fromResult(fakeCountries);
}

export function getWG(): Promise<NamedId[]> {
    return fromResult(fakeWG);
}

export function getAvailabilityTypes(): Promise<NamedId[]> {
    return fromResult(fakeAvailability);
}

export function getDurationTypes(): Promise<NamedId[]> {
    return fromResult(fakeDuration);
}

export function getReactTypes(): Promise<NamedId[]> {
    return fromResult(fakeReactTypes);
}

export function getReactionTimeTypes(): Promise<NamedId[]> {
    return fromResult(fakeReactTimeTypes);
}

export function getServiceLocationTypes(): Promise<NamedId[]> {
    return fromResult(fakeServiceLocationTypes);
}

export function getAllowed(): Promise<CapabilityMatrixListModel[]> {
    return fromResult(fakeAllowed);
}

export function getDenied(): Promise<CapabilityMatrixListModel[]> {
    return fromResult(fakeDenied);
}

function fromResult<T>(value: T): Promise<T> {
    return new Promise((resolve, reject) => {
        setTimeout(() => {
            resolve(value);
        }, 1);
    });
}

