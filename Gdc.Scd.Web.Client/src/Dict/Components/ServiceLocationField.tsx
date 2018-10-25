import { DictField } from "./DictField";

export class ServiceLocationField extends DictField {
    public getItems() {
        return this.srv.getServiceLocationTypes();
    }
}