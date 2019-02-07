import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class CountryNameField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getCountries();
    }
}