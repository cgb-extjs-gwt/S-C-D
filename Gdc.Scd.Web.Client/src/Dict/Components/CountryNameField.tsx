import { DictField } from "./DictField";

export class CountryNameField extends DictField {
    public getItems() {
        return this.srv.getCountries();
    }
}