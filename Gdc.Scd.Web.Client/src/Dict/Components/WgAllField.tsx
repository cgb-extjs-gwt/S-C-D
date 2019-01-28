import { DictField } from "./DictField";

export class WgAllField extends DictField {
    public getItems() {
        return this.srv.getWgWithMultivendor();
    }
}