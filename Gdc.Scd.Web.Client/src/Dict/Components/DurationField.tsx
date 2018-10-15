import { DictField } from "./DictField";

export class DurationField extends DictField {
    public getItems() {
        return this.srv.getDurationTypes();
    }
}