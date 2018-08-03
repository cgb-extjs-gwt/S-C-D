import { CapabilityMatrixEditModel } from "../Model/CapabilityMatrixEditModel";
import { CapabilityMatrixListModel } from "../Model/CapabilityMatrixListModel";
import { NamedId } from "../../Common/States/CommonStates";
import { CapabilityMatrixFilterModel } from "../Model/CapabilityMatrixFilterModel";

export interface ICapabilityMatrixService {
    allowItem(row: CapabilityMatrixEditModel): Promise<any>;

    allowItems(ids: string[]): Promise<any>;

    denyItem(row: CapabilityMatrixEditModel): Promise<any>;

    getCountries(): Promise<NamedId[]>;

    getWG(): Promise<NamedId[]>;

    getAvailabilityTypes(): Promise<NamedId[]>;

    getDurationTypes(): Promise<NamedId[]>;

    getReactTypes(): Promise<NamedId[]>;

    getReactionTimeTypes(): Promise<NamedId[]>;

    getServiceLocationTypes(): Promise<NamedId[]>;

    getAllowed(filter: CapabilityMatrixFilterModel): Promise<CapabilityMatrixListModel[]>;

    getDenied(filter: CapabilityMatrixFilterModel): Promise<CapabilityMatrixListModel[]>;
}
