import { Country } from "../Model/Country";
import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class UserCountryField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getUserCountries(this.canCache());
    }

    public getSelectedModel(): Country {
        return super.getSelectedModel() as Country;
    }
}