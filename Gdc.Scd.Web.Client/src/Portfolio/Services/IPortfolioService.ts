import { PortfolioEditModel } from "../Model/PortfolioEditModel";

export interface IPortfolioService {
    allow(row: PortfolioEditModel): Promise<any>;


    deny(row: PortfolioEditModel): Promise<any>;
}
