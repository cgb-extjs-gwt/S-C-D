import { NamedId } from "../../Common/States/CommonStates";

export interface TableViewRecord {
    coordinates: { [key: string]: NamedId<number> }
    data: { 
        [key: string]: { 
            value, 
            count: number,
        } 
    }
}