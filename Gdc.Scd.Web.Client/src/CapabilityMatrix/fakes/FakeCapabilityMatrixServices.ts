import { CapabilityMatrixDto } from "../States/CapabilityMatrixStates";

export const allowItem = (row: CapabilityMatrixDto) => {
    return saveItem(row);
}

export const denyItem = (row: CapabilityMatrixDto) => {
    return saveItem(row);
}

export const saveItem = (editItems: CapabilityMatrixDto) => {
    //const key = createEditItemsStorageKey(context);
    //const storageItemsJson = localStorage.getItem(key);
    //const storageItems: EditItem[] = storageItemsJson && JSON.parse(storageItemsJson) || [];
    //const saveItems = storageItems.filter(
    //    storageItem => editItems.findIndex(item => storageItem.id === item.id) === -1
    //).concat(editItems);

    //localStorage.setItem(
    //    key,
    //    JSON.stringify(saveItems)
    //);

    return Promise.resolve();
}

export const getCapabilityMatrixItems = () => Promise.resolve(<CapabilityMatrixDto>{
    countries: [
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
    ],
    warrantyGroups: [
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
    ],
    availabilityTypes: [
        { id: '1000', name: '24x7' },
        { id: '1001', name: '8x5' },
        { id: '1002', name: '3x2' },
        { id: '1003', name: '365 days in year' },
        { id: '1004', name: 'any time' },
        { id: '1005', name: 'None' }
    ],
    durationTypes: [
        { id: '1000', name: '2h' },
        { id: '1001', name: '3h' },
        { id: '1002', name: '8h' },
        { id: '1003', name: '12h' },
        { id: '1004', name: '24h' },
        { id: '1005', name: '48h' }
    ],
    reactTypes: [
        { id: '1000', name: 'recover' },
        { id: '1001', name: 'response' },
        { id: '1002', name: 'none' }
    ],
    reactionTimeTypes: [
        { id: '1000', name: '2h' },
        { id: '1001', name: '3h' },
        { id: '1002', name: '4h' },
        { id: '1003', name: '5h' },
        { id: '1004', name: '6h' },
        { id: '1005', name: '7h' }
    ],
    serviceLocationTypes: [
        { id: '1000', name: 'on side' },
        { id: '1001', name: 'off side' },
        { id: '1002', name: 'exit' },
        { id: '1003', name: 'none' },
        { id: '1004', name: 'any' }
    ]
});
