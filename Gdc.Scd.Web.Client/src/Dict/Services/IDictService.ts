import { NamedId } from "../../Common/States/CommonStates";
import { Country } from "../Model/Country";

export interface IDictService {
    getCountries(): Promise<NamedId<string>[]>;

    getMasterCountries(cache: boolean): Promise<Country[]>;

    getUserCountries(cache: boolean): Promise<Country[]>;

    getCountryGroups(): Promise<NamedId[]>;

    getCountryGroupDigits(): Promise<NamedId[]>;

    getCountryGroupLuts(): Promise<NamedId[]>;

    getCountryGroupIsoCode(): Promise<NamedId[]>;

    getCountryQualityGroup(): Promise<NamedId[]>;

    getWG(): Promise<NamedId[]>;

    getPla(): Promise<NamedId[]>;

    getSog(): Promise<NamedId[]>;

    getAvailabilityTypes(): Promise<NamedId[]>;

    getDurationTypes(): Promise<NamedId[]>;

    getYears(): Promise<NamedId[]>;

    getReactionTypes(): Promise<NamedId[]>;

    getReactionTimeTypes(): Promise<NamedId[]>;

    getServiceLocationTypes(): Promise<NamedId[]>;

    getProActive(): Promise<NamedId[]>;

    getRoles(): Promise<NamedId[]>;
}
