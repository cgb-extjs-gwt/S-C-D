import { CheckBoxField } from "@extjs/ext-react";

export class BooleanFilter extends CheckBoxField {
    public getValue(): string {
        return (this as any).getChecked();
    }
}