import { DictField } from "./DictField";
import { Country } from "../Model/Country";

export class UserCountryField extends DictField {
    public getItems() {
        return this.srv.getUserCountries(this.canCache());
    }

    public getSelectedModel(): Country {
        return super.getSelectedModel() as Country;
    }
}