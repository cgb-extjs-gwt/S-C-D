import { DictField } from "./DictField";
import { Country } from "../Model/Country";
import { NamedId } from "../../Common/States/CommonStates";

export class CountryField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getMasterCountries(this.canCache());
    }

    public getSelectedModel(): Country {
        return super.getSelectedModel() as Country;
    }
}