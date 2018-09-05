import { NamedId } from "../../Common/States/CommonStates";
import { AvailabilityService } from "../../Dict/Services/AvailabilityService";
import { CountryService } from "../../Dict/Services/CountryService";
import { DurationService } from "../../Dict/Services/DurationService";
import { ReactionTimeService } from "../../Dict/Services/ReactionTimeService";
import { ReactionTypeService } from "../../Dict/Services/ReactionTypeService";
import { ServiceLocationService } from "../../Dict/Services/ServiceLocationService";
import { WgService } from "../../Dict/Services/WgService";
import { IDictService } from "./IDictService";

export class DictService implements IDictService {

    public getCountries(): Promise<NamedId<string>[]> {
        return new CountryService().getAll();
    }

    public getWG(): Promise<NamedId<string>[]> {
        return new WgService().getAll();
    }

    public getSog(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }

    public getAvailabilityTypes(): Promise<NamedId<string>[]> {
        return new AvailabilityService().getAll();
    }

    public getDurationTypes(): Promise<NamedId<string>[]> {
        return new DurationService().getAll();
    }

    public getYears(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }

    public getReactTypes(): Promise<NamedId<string>[]> {
        return new ReactionTypeService().getAll();
    }

    public getReactionTimeTypes(): Promise<NamedId<string>[]> {
        return new ReactionTimeService().getAll();
    }

    public getServiceLocationTypes(): Promise<NamedId<string>[]> {
        return new ServiceLocationService().getAll();
    }
}
