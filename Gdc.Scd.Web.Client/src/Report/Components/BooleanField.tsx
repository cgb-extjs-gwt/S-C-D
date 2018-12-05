import { CheckBoxField } from "@extjs/ext-react";

export class BooleanField extends CheckBoxField {
    public getValue(): string {
        return (this as any).getChecked();
    }
}