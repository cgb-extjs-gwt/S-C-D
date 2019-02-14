import { NamedId } from "../../Common/States/CommonStates";
import { Region } from "./CostBlockStates";

export interface CostElementData {
    regions: Region[] 
    referenceValues: NamedId<number>[] 
}