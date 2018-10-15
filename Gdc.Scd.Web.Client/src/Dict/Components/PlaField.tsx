import { DictField } from "./DictField";

export class PlaField extends DictField {
    public getItems() {
        return this.srv.getPla();
    }
}