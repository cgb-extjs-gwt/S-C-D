export interface CountryFilterModel {
    country?: string;
    group?: string;
    lut?: string;
    digit?: string;
    iso?: string;

    isMaster?: boolean;
    storeListAndDealer?: boolean;
    overrideTCandTP?: boolean;
}