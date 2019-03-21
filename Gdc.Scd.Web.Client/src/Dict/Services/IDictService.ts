import { NamedId, SortableNamedId } from "../../Common/States/CommonStates";
import { Country } from "../Model/Country";

export interface IDictService {
    getCountries(): Promise<NamedId<string>[]>;

    getMasterCountries(cache: boolean): Promise<Country[]>;

    getMasterCountriesNames(): Promise<NamedId<string>[]>;

    getUserCountries(cache: boolean): Promise<Country[]>;

    getUserCountryNames(): Promise<NamedId[]>;

    getCountryGroups(): Promise<NamedId[]>;

    getCountryGroupDigits(): Promise<NamedId[]>;

    getCountryGroupLuts(): Promise<NamedId[]>;

    getCountryGroupIsoCode(): Promise<NamedId[]>;

    getCountryQualityGroup(): Promise<NamedId[]>;

    getRegions(): Promise<NamedId[]>;

    getCurrencies(): Promise<NamedId[]>;

    getWG(): Promise<NamedId[]>;

    getWgWithMultivendor(): Promise<NamedId[]>;

    getStandardWg(): Promise<NamedId[]>;

    getHardwareWg(): Promise<NamedId[]>;

    getPla(): Promise<NamedId[]>;

    getSog(): Promise<NamedId[]>;

    getSwDigit(): Promise<NamedId[]>;

    getSwDigitSog(): Promise<NamedId[]>;

    getAvailabilityTypes(): Promise<NamedId[]>;

    getDurationTypes(): Promise<NamedId[]>;

    getYears(): Promise<NamedId[]>;

    getReactionTypes(): Promise<NamedId[]>;

    getReactionTimeTypes(): Promise<NamedId[]>;

    getServiceLocationTypes(): Promise<SortableNamedId[]>;

    getProActive(): Promise<NamedId[]>;

    getRoles(): Promise<NamedId[]>;
}
