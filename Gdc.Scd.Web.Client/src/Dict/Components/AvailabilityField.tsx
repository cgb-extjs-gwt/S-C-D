import { DictField } from "./DictField";

export class AvailabilityField extends DictField {
    public getItems() {
        return this.srv.getAvailabilityTypes();
    }
}