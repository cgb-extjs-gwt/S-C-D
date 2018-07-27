import { CapabilityMatrixItem } from "../Model/CapabilityMatrixItem";
import { NamedId } from "../../Common/States/CommonStates";
import { CapabilityMatrixListModel } from "../Model/CapabilityMatrixListModel";

export interface ICapabilityMatrixService {
    allowItem(row: CapabilityMatrixItem);

    allowItems(ids: string[]): Promise<any>;

    denyItem(row: CapabilityMatrixItem);

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
