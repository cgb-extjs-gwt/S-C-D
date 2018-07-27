import { NamedId } from "../../Common/States/CommonStates";
import { CapabilityMatrixEditModel } from "../Model/CapabilityMatrixEditModel";
import { CapabilityMatrixListModel } from "../Model/CapabilityMatrixListModel";
import { ICapabilityMatrixService } from "../Services/ICapabilityMatrixService";

export class CapabilityMatrixService implements ICapabilityMatrixService {
    allowItem(row: any) {
        throw new Error("Method not implemented.");
    }
    allowItems(ids: string[]): Promise<any> {
        throw new Error("Method not implemented.");
    }
    denyItem(row: any) {
        throw new Error("Method not implemented.");
    }
    getCountries(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }
    getWG(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }
    getAvailabilityTypes(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }
    getDurationTypes(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }
    getReactTypes(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }
    getReactionTimeTypes(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }
    getServiceLocationTypes(): Promise<NamedId<string>[]> {
        throw new Error("Method not implemented.");
    }
    getAllowed(): Promise<CapabilityMatrixListModel[]> {
        throw new Error("Method not implemented.");
    }
    getDenied(): Promise<CapabilityMatrixListModel[]> {
        throw new Error("Method not implemented.");
    }
}





