import { AutoFilterModel } from "./AutoFilterModel";
import { AutoColumnModel } from "./AutoColumnModel";

export interface AutoGridModel {

    caption?: string;

    fields: AutoColumnModel[];

    filter: AutoFilterModel[];

}