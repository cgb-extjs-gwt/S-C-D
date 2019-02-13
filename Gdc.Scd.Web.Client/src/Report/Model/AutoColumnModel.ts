import { AutoColumnType } from "./AutoColumnType";

export interface AutoColumnModel {
    text: string;

    name: string;

    type?: AutoColumnType;

    allowNull?: boolean;

    flex?: number;

    format?: string;
}