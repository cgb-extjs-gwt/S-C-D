import { DictField } from "./DictField";

export class YearField extends DictField {
    public getItems() {
        return this.srv.getYears();
    }
}