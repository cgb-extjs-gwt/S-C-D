import { CapabilityMatrixEditModel } from "../Model/CapabilityMatrixEditModel";
import { CapabilityMatrixListModel } from "../Model/CapabilityMatrixListModel";
import { NamedId } from "../../Common/States/CommonStates";

export interface ICapabilityMatrixService {
    allowItem(row: CapabilityMatrixEditModel);

    allowItems(ids: string[]): Promise<any>;

    denyItem(row: CapabilityMatrixEditModel);

    getCountries(): Promise<NamedId[]>;

    getWG(): Promise<NamedId[]>;

    getAvailabilityTypes(): Promise<NamedId[]>;

    getDurationTypes(): Promise<NamedId[]>;

    getReactTypes(): Promise<NamedId[]>;

    getReactionTimeTypes(): Promise<NamedId[]>;

    getServiceLocationTypes(): Promise<NamedId[]>;

    getAllowed(): Promise<CapabilityMatrixListModel[]>;

    getDenied(): Promise<CapabilityMatrixListModel[]>;
}
