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

        if (row.IsLocalPortfolio()) {
            let countries = row.countries;
            return this.getWgOrDefault(row.wgs).then(x => this.postSequentialByCountry(action, countries, x, row))
        }
        else {
            return this.getWgOrDefault(row.wgs).then(x => this.postSerial(action, null, x, row));
        }
    }

    private postSequentialByCountry(action: string, countries: string[], wgs: string[], row: PortfolioEditModel): Promise<any> {

        let p = Promise.resolve();

        for (let i = 0, len = countries.length; i < len; i++) {
            let cnt = countries[i];
            let wgCopy = [...wgs];
            p = p.then(x => this.postSerial(action, cnt, wgCopy, row));
        }

        return p;
    }

    private postSerial(action: string, cnt: string, wgs: string[], row: PortfolioEditModel): Promise<any> {

        //send batch update by 8wg only

        let p = Promise.resolve();

        let max = 8, k = 0, part = [];

        for (let i = 0, len = wgs.length; i < len; i++) {

            if (k == max) {
                let partCopy = part;
                p = p.then(() => this.send(action, cnt, partCopy, row));
                part = [];
                k = 0;
            }

            part.push(wgs[i]);
            k++;
        }

        return p.then(() => this.send(action, cnt, part, row));
    }

    private send(action: string, cnt: string, wg: string[], row: PortfolioEditModel): Promise<any> {
        return post(this.controllerName, action, { ...row, CountryId: cnt, wgs: wg });
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
