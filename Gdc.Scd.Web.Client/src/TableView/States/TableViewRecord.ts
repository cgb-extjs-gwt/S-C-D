import { NamedId } from "../../Common/States/CommonStates";

export interface TableViewRecord {
    // ids: { [key: string]: number}
    // data: { [key: string]: string}

    coordinates: { [key: string]: NamedId }
    data: { [key: string]: { value, count: number } }
}