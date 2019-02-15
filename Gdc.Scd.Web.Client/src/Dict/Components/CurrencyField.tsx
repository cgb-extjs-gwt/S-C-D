import { NamedId } from "../../Common/States/CommonStates";
import { DictField } from "./DictField";

export class CurrencyField extends DictField<NamedId> {

    protected valueField: string = 'name';

    public getItems() {
        return this.srv.getCurrencies();
    }
}