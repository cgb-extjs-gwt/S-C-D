import { DictField } from "./DictField";
import { NamedId } from "../../Common/States/CommonStates";

export class WgSogField extends DictField<NamedId> {
    public getItems() {
        return this.srv.getWgWithSog();
    }
}