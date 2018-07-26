import { CapabilityMatrixDto, CapabilityMatrixItem } from "../States/CapabilityMatrixStates";
import { error } from "../../Layout/Actions/AppActions";
import { NamedId } from "../../Common/States/CommonStates";

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
    return fromResult([
        { id: 'Algeria', name: 'Algeria' },
        { id: 'Austria', name: 'Austria' },
        { id: 'Balkans', name: 'Balkans' },
        { id: 'Belgium', name: 'Belgium' },
        { id: 'CIS & Russia', name: 'CIS & Russia' },
        { id: 'Czech Republic', name: 'Czech Republic' },
        { id: 'Denmark', name: 'Denmark' },
        { id: 'Egypt', name: 'Egypt' },
        { id: 'Finland', name: 'Finland' },
        { id: 'France', name: 'France' },
        { id: 'Germany', name: 'Germany' },
        { id: 'Greece', name: 'Greece' },
        { id: 'Hungary', name: 'Hungary' },
        { id: 'India', name: 'India' },
        { id: 'Italy', name: 'Italy' },
        { id: 'Luxembourg', name: 'Luxembourg' },
        { id: 'Middle East', name: 'Middle East' },
        { id: 'Morocco', name: 'Morocco' },
        { id: 'Netherlands', name: 'Netherlands' },
        { id: 'Norway', name: 'Norway' },
        { id: 'Poland', name: 'Poland' },
        { id: 'Portugal', name: 'Portugal' },
        { id: 'South Africa', name: 'South Africa' },
        { id: 'Spain', name: 'Spain' },
        { id: 'Sweden', name: 'Sweden' },
        { id: 'Switzerland', name: 'Switzerland' },
        { id: 'Tunisia', name: 'Tunisia' },
        { id: 'Turkey', name: 'Turkey' },
        { id: 'UK & Ireland', name: 'UK & Ireland' }
    ]);
}

export function getWG(): Promise<NamedId[]> {
    return fromResult([
        { id: 'WG0', name: 'WG0' },
        { id: 'WG1', name: 'WG1' },
        { id: 'WG2', name: 'WG2' },
        { id: 'WG3', name: 'WG3' },
        { id: 'WG4', name: 'WG4' },
        { id: 'WG4', name: 'WG4' },
        { id: 'WG5', name: 'WG5' },
        { id: 'WG6', name: 'WG6' },
        { id: 'WG7', name: 'WG7' },
        { id: 'WG8', name: 'WG8' }
    ]);
}

export function getAvailabilityTypes(): Promise<NamedId[]> {
    return fromResult([
        { id: '1000', name: '24x7' },
        { id: '1001', name: '8x5' },
        { id: '1002', name: '3x2' },
        { id: '1003', name: '365 days in year' },
        { id: '1004', name: 'any time' },
        { id: '1005', name: 'None' }
    ]);
}

export function getDurationTypes(): Promise<NamedId[]> {
    return fromResult([
        { id: '1000', name: '2h' },
        { id: '1001', name: '3h' },
        { id: '1002', name: '8h' },
        { id: '1003', name: '12h' },
        { id: '1004', name: '24h' },
        { id: '1005', name: '48h' }
    ]);
}

export function getReactTypes(): Promise<NamedId[]> {
    return fromResult([
        { id: '1000', name: 'recover' },
        { id: '1001', name: 'response' },
        { id: '1002', name: 'none' }
    ]);
}

export function getReactionTimeTypes(): Promise<NamedId[]> {
    return fromResult([
        { id: '1000', name: '2h' },
        { id: '1001', name: '3h' },
        { id: '1002', name: '4h' },
        { id: '1003', name: '5h' },
        { id: '1004', name: '6h' },
        { id: '1005', name: '7h' }
    ]);
}

export function getServiceLocationTypes(): Promise<NamedId[]> {
    return fromResult([
        { id: '1000', name: 'on side' },
        { id: '1001', name: 'off side' },
        { id: '1002', name: 'exit' },
        { id: '1003', name: 'none' },
        { id: '1004', name: 'any' }
    ]);
}

function fromResult<T>(value: T): Promise<T> {
    return new Promise((resolve, reject) => {
        setTimeout(() => {
            resolve(value);
        }, 1);
    });
}

