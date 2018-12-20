import { DictField } from "./DictField";

export class CountryField extends DictField {
    public getItems() {
        return this.srv.getMasterCountries();
    }
}