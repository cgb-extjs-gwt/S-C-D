import { NamedId } from "../../Common/States/CommonStates";

export interface IDictService {
    getCountries(): Promise<NamedId[]>;

    getWG(): Promise<NamedId[]>;

    getSog(): Promise<NamedId[]>;

    getAvailabilityTypes(): Promise<NamedId[]>;

    getDurationTypes(): Promise<NamedId[]>;

    getYears(): Promise<NamedId[]>;

    getReactTypes(): Promise<NamedId[]>;

    getReactionTimeTypes(): Promise<NamedId[]>;

    getServiceLocationTypes(): Promise<NamedId[]>;
}
