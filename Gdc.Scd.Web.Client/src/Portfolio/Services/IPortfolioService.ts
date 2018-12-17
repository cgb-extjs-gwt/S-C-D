import { PortfolioEditModel } from "../Model/PortfolioEditModel";

export interface IPortfolioService {
    allowItem(row: PortfolioEditModel): Promise<any>;


    denyItem(row: PortfolioEditModel): Promise<any>;
}
