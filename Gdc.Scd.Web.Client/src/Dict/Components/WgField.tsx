import { DictField } from "./DictField";

export class WgField extends DictField {
    public getItems() {
        return this.srv.getWG();
    }
}