import { IPortfolioService } from "./IPortfolioService"
import { PortfolioService } from "./PortfolioService";

export class PortfolioServiceFactory {

    public static getPortfolioService(): IPortfolioService {
        return new PortfolioService();
    }

}