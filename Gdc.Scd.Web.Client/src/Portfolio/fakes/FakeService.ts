import { PortfolioEditModel } from "../Model/PortfolioEditModel";
import { IPortfolioService } from "../Services/IPortfolioService";

export class FakeService implements IPortfolioService {

    public allowItem(row: PortfolioEditModel): Promise<any> {
        return this.saveItem(row, true);
    }

    public denyItem(row: PortfolioEditModel): Promise<any> {
        return this.saveItem(row, false);
    }

    private saveItem(row: PortfolioEditModel, allow: boolean): Promise<any> {
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
