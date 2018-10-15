import { DictField } from "./DictField";

export class SogField extends DictField {
    public getItems() {
        return this.srv.getSog();
    }
}