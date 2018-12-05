export interface CountryFilterModel {
    group?: string;
    lut?: string;
    digit?: string;
    iso?: string;

    isMaster?: boolean;
    storeListAndDealer?: boolean;
    overrideTCandTP?: boolean;
}