import { post } from "../../Common/Services/Ajax";
import { PortfolioEditModel } from "../Model/PortfolioEditModel";
import { IPortfolioService } from "./IPortfolioService";

export class PortfolioService implements IPortfolioService {

    private controllerName: string;

    public constructor() {
        this.controllerName = 'portfolio';
    }

    public allowItem(row: PortfolioEditModel): Promise<any> {
        return post(this.controllerName, 'allow', row);
    }

    public denyItem(row: PortfolioEditModel) {
        return post(this.controllerName, 'deny', row);
    }
}
