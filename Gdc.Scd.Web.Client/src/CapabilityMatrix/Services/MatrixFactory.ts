import { FakeCapabilityMatrixService } from "../fakes/FakeCapabilityMatrixService"
import { ICapabilityMatrixService } from "./ICapabilityMatrixService"

export class MatrixFactory {

    public static getMatrixService(): ICapabilityMatrixService {
        return new FakeCapabilityMatrixService();
    }

}