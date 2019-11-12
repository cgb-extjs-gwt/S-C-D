import * as React from "react";
import { RadioField, Container } from "@extjs/ext-react";

export class FspRadioField extends React.Component<any> {

    private withFsp: RadioField & any;

    private noFsp: RadioField & any;

    public render() {
        return <Container layout={{ type: 'vbox', align: 'left' }} margin="0" defaults={{ padding: '0' }} >
            <RadioField name="hasFsp" boxLabel="All" checked />
            <RadioField ref={this.setWithFsp} name="hasFsp" boxLabel="With FSP" />
            <RadioField ref={this.setNoFsp} name="hasFsp" boxLabel="No FSP" />
        </Container>
    }

    private setWithFsp = (x) => {
        this.withFsp = x;
    }

    private setNoFsp = (x) => {
        this.noFsp = x;
    }

    public getValue(): boolean {
        if (this.withFsp.getChecked()) {
            return true;
        }
        else if (this.noFsp.getChecked()) {
            return false;
        }
        else {
            return null;
        }
    }
}
