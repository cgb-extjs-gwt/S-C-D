import { NamedId } from "../../Common/States/CommonStates";

export interface IDictService {
    getCountries(): Promise<NamedId[]>;

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

    getRoles(): Promise<NamedId[]>;
}
