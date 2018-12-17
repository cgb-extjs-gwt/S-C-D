import { IPortfolioService } from "./IPortfolioService"
import { PortfolioService } from "./PortfolioService";

export class MatrixFactory {

    public static getMatrixService(): IPortfolioService {
        return new PortfolioService();
    }

}