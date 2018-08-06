import { ICapabilityMatrixService } from "./ICapabilityMatrixService"
import { CapabilityMatrixService } from "./CapabilityMatrixService";
//import { FakeCapabilityMatrixService } from "../fakes/FakeCapabilityMatrixService";

export class MatrixFactory {

    public static getMatrixService(): ICapabilityMatrixService {
        return new CapabilityMatrixService();

        //return new FakeCapabilityMatrixService();
    }

}