import { post } from "../../Common/Services/Ajax";
import { PortfolioEditModel } from "../Model/PortfolioEditModel";
import { IPortfolioService } from "./IPortfolioService";

export class PortfolioService implements IPortfolioService {
    private controllerName: string;

    public constructor() {
        this.controllerName = 'portfolio';
    }

    public allow(row: PortfolioEditModel): Promise<any> {
        return this.postSequential('allow', row);
    }

    public deny(row: PortfolioEditModel) {
        return this.postSequential('deny', row);
    }

    public denyById(cnt: string[], ids: string[]): Promise<any> {
        return post(this.controllerName, 'denylocal', { countryId: cnt, items: ids });
    }

    private postSequential(action: string, row: PortfolioEditModel) {

        var p = Promise.resolve();

        let wg = row.wgs;

        if (wg == null || wg.length === 0) {
            wg = 
        }

        for (let i = 0, len = wg.length; i < len; i++) {

        }





        return post(this.controllerName, action, row);
    }
}
