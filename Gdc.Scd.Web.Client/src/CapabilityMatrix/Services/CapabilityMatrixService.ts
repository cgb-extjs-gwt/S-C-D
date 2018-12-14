import { get, post } from "../../Common/Services/Ajax";
import { DataInfo } from "../../Common/States/CommonStates";
import { CapabilityMatrixEditModel } from "../Model/CapabilityMatrixEditModel";
import { CapabilityMatrixFilterModel } from "../Model/CapabilityMatrixFilterModel";
import { CapabilityMatrixListModel } from "../Model/CapabilityMatrixListModel";
import { ICapabilityMatrixService } from "../Services/ICapabilityMatrixService";



export class CapabilityMatrixService implements ICapabilityMatrixService {

    private controllerName: string;

    public constructor() {
        this.controllerName = 'capabilitymatrix';
    }

    public allowItem(row: CapabilityMatrixEditModel): Promise<any> {
        throw new Error('not implemented');
        //return post(this.controllerName, 'allow', row);
    }

    public allowItems(ids: string[]): Promise<any> {
        return post(this.controllerName, 'allow', ids);
    }

    public denyItem(row: CapabilityMatrixEditModel) {
        return post(this.controllerName, 'deny', row);
    }

    public getAllowed(filter: CapabilityMatrixFilterModel): Promise<DataInfo<CapabilityMatrixListModel>> {
        return get<DataInfo<CapabilityMatrixListModel>>(this.controllerName, 'allowed', filter);
    }

    public getDenied(filter: CapabilityMatrixFilterModel): Promise<DataInfo<CapabilityMatrixListModel>> {
        return get<DataInfo<CapabilityMatrixListModel>>(this.controllerName, 'denied', filter);
    }
}
