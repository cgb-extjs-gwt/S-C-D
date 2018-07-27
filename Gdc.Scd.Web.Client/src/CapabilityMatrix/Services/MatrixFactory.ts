import { ICapabilityMatrixService } from "./ICapabilityMatrixService"
import { CapabilityMatrixService } from "./CapabilityMatrixService";

export class MatrixFactory {

    public static getMatrixService(): ICapabilityMatrixService {
        return new CapabilityMatrixService();
    }

}