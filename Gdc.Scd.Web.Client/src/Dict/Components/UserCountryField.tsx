import { Country } from "../Model/Country";
import { DictField } from "./DictField";

export class UserCountryField extends DictField {
    public getItems() {
        return this.srv.getUserCountries(this.canCache());
    }

    public getSelectedModel(): Country {
        return super.getSelectedModel() as Country;
    }
}