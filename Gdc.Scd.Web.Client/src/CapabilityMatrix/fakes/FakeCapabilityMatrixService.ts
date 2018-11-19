import { DataInfo, NamedId } from "../../Common/States/CommonStates";
import { FakeDictService } from "../../Dict/fakes/FakeDictService";
import { CapabilityMatrixEditModel } from "../Model/CapabilityMatrixEditModel";
import { CapabilityMatrixFilterModel } from "../Model/CapabilityMatrixFilterModel";
import { CapabilityMatrixListModel } from "../Model/CapabilityMatrixListModel";
import { ICapabilityMatrixService } from "../Services/ICapabilityMatrixService";
import { fakeAllowed } from "./FakeAllowed";
import { fakeDenied } from "./FakeDenied";

export class FakeCapabilityMatrixService implements ICapabilityMatrixService {

    private dictSrv: FakeDictService;

    public allowItem(row: CapabilityMatrixEditModel): Promise<any> {
        return this.saveItem(row, true);
    }

    public allowItems(ids: string[]): Promise<any> {
        return this.fromResult({});
    }

    public denyItem(row: CapabilityMatrixEditModel): Promise<any> {
        return this.saveItem(row, false);
    }

    public getCountries(): Promise<NamedId[]> {
        return this.dictSrv.getCountries();
    }

    public getWG(): Promise<NamedId[]> {
        return this.dictSrv.getWG();
    }

    public getAvailabilityTypes(): Promise<NamedId[]> {
        return this.dictSrv.getAvailabilityTypes();
    }

    public getDurationTypes(): Promise<NamedId[]> {
        return this.dictSrv.getDurationTypes();
    }

    public getReactTypes(): Promise<NamedId[]> {
        return this.dictSrv.getReactionTypes();
    }

    public getReactionTimeTypes(): Promise<NamedId[]> {
        return this.dictSrv.getReactionTimeTypes();
    }

    public getServiceLocationTypes(): Promise<NamedId[]> {
        return this.dictSrv.getServiceLocationTypes();
    }

    public getAllowed(filter: CapabilityMatrixFilterModel): Promise<DataInfo<CapabilityMatrixListModel>> {
        return this.fromResult({ items: fakeAllowed, total: fakeAllowed.length * 5 });
    }

    public getDenied(filter: CapabilityMatrixFilterModel): Promise<DataInfo<CapabilityMatrixListModel>> {
        return this.fromResult({ items: fakeDenied, total: fakeDenied.length * 5 });
    }

    private saveItem(row: CapabilityMatrixEditModel, allow: boolean): Promise<any> {
        throw new Error("Method not implemented.");
    }

    private fromResult<T>(value: T): Promise<T> {
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                resolve(value);
            }, 1);
        });
    }
}
