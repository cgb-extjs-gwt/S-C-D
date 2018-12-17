import { post } from "../../Common/Services/Ajax";
import { PortfolioEditModel } from "../Model/PortfolioEditModel";
import { IPortfolioService } from "./IPortfolioService";

export class PortfolioService implements IPortfolioService {

    private controllerName: string;

    public constructor() {
        this.controllerName = 'capabilitymatrix';
    }

    public allowItem(row: PortfolioEditModel): Promise<any> {
        throw new Error('not implemented');
    }

    public denyItem(row: PortfolioEditModel) {
        return post(this.controllerName, 'deny', row);
    }
}
