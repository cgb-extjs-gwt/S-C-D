import { TableViewRecord } from "../States/TableViewRecord";

export const isEqualCoordinates = 
    ({ coordinates: coord1 }: TableViewRecord, { coordinates: coord2 }: TableViewRecord) => 
        Object.keys(coord1).every(key => coord1[key].id === coord2[key].id)