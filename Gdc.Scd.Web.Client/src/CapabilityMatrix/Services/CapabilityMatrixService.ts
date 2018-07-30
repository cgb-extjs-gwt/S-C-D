import { NamedId } from "../../Common/States/CommonStates";
import { CapabilityMatrixEditModel } from "../Model/CapabilityMatrixEditModel";
import { CapabilityMatrixListModel } from "../Model/CapabilityMatrixListModel";
import { ICapabilityMatrixService } from "../Services/ICapabilityMatrixService";

import { AvailabilityService } from "../../Dict/Services/AvailabilityService";
import { CountryService } from "../../Dict/Services/CountryService";
import { DurationService } from "../../Dict/Services/DurationService";
import { ReactionTimeService } from "../../Dict/Services/ReactionTimeService";
import { ReactionTypeService } from "../../Dict/Services/ReactionTypeService";
import { ServiceLocationService } from "../../Dict/Services/ServiceLocationService";
import { WgService } from "../../Dict/Services/WgService";

import { get, post } from "../../Common/Services/Ajax";

export class CapabilityMatrixService implements ICapabilityMatrixService {

    private controllerName: string;

    public constructor() {
        this.controllerName = 'capabilitymatrix';
    }

    public allowItem(row: CapabilityMatrixEditModel) : Promise<any> {
        return post(this.controllerName, 'allow', row);
    }

    public allowItems(ids: string[]): Promise<any> {
        return post(this.controllerName, 'allowbyid', ids);
    }

    public denyItem(row: CapabilityMatrixEditModel) {
        return post(this.controllerName, 'deny', row);
    }

    public getCountries(): Promise<NamedId<string>[]> {
        return new CountryService().getAll();
    }

    public getWG(): Promise<NamedId<string>[]> {
        return new WgService().getAll();
    }

    public getAvailabilityTypes(): Promise<NamedId<string>[]> {
        return new AvailabilityService().getAll();
    }

    public getDurationTypes(): Promise<NamedId<string>[]> {
        return new DurationService().getAll();
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

    public getAllowed(): Promise<CapabilityMatrixListModel[]> {
        return get<CapabilityMatrixListModel[]>(this.controllerName, 'allowed');
    }

    public getDenied(): Promise<CapabilityMatrixListModel[]> {
        return get<CapabilityMatrixListModel[]>(this.controllerName, 'denied');
    }
}





