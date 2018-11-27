import { NamedId } from "../../Common/States/CommonStates";

export interface TableViewRecord {
    coordinates: { [key: string]: NamedId }
    data: { 
        [key: string]: { 
            value, 
            count: number,
        } 
    }
    additionalData: {
        [key: string]: {
            value
        }
    }
}