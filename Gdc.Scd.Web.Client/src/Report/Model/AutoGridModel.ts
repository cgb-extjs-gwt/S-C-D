import { AutoFilterModel } from "./AutoFilterModel";
import { AutoColumnModel } from "./AutoColumnModel";

export interface AutoGridModel {

    id: string;

    name: string;

    caption?: string;

    fields: AutoColumnModel[];

    filter: AutoFilterModel[];

}