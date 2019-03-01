import { post } from "../../Common/Services/Ajax";
import { PortfolioEditModel } from "../Model/PortfolioEditModel";
import { IPortfolioService } from "./IPortfolioService";
import { DictFactory } from "../../Dict/Services/DictFactory";

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

    private postSequential(action: string, row: PortfolioEditModel): Promise<any> {
        return this.getWgOrDefault(row.wgs).then(x => this.postSerial(action, x, row))
    }

    private postSerial(action: string, arr: string[], row: PortfolioEditModel): Promise<any> {

        //send batch update by 8wg only

        let p = Promise.resolve();

        let max = 8, k = 0, part = [];

        for (let i = 0, len = arr.length; i < len; i++) {

            if (k == max) {
                let partCopy = part;
                p = p.then(() => this.send(action, partCopy, row));
                part = [];
                k = 0;
            }

            part.push(arr[i]);
            k++;
        }

        return p.then(() => this.send(action, part, row));
    }

    private send(action: string, wg: string[], row: PortfolioEditModel): Promise<any> {
        return post(this.controllerName, action, { ...row, wgs: wg });
    }

    private getWgOrDefault(wg: string[]): Promise<string[]> {
        if (wg && wg.length > 0) {
            return Promise.resolve(wg);
        }
        else {
            return DictFactory.getDictService().getWG().then(x => x.map(y => y.id));
        }
    }
}
