import { DataInfo } from "../../Common/States/CommonStates";
import { CapabilityMatrixEditModel } from "../Model/CapabilityMatrixEditModel";
import { CapabilityMatrixFilterModel } from "../Model/CapabilityMatrixFilterModel";
import { CapabilityMatrixListModel } from "../Model/CapabilityMatrixListModel";

export interface ICapabilityMatrixService {
    allowItem(row: CapabilityMatrixEditModel): Promise<any>;

    allowItems(ids: string[]): Promise<any>;

    denyItem(row: CapabilityMatrixEditModel): Promise<any>;

    getAllowed(filter: CapabilityMatrixFilterModel): Promise<DataInfo<CapabilityMatrixListModel>>;

    getDenied(filter: CapabilityMatrixFilterModel): Promise<DataInfo<CapabilityMatrixListModel>>;
}
