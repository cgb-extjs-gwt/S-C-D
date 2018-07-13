import * as React from "react";
import { Container, Button, FormPanel, CheckBoxField, SelectField, ExtReact, List } from "@extjs/ext-react";

export class CapabilityMatrixEditView extends React.Component<{}> {

    store = new Ext.data.Store({
        data: [
            { title: 'Item 1' },
            { title: 'Item 2' },
            { title: 'Item 3' },
            { title: 'Item 4' }
        ]
    });

    public render() {
        return (
            <Container layout="vbox">
                <Container>

                    <ExtReact>
                        <List
                            itemTpl="{title}"
                            store={this.store}
                        />
                    </ExtReact>

                    <FormPanel shadow layout={{ type: 'vbox', align: 'left' }}>
                        <CheckBoxField boxLabel="Fujitsu Global Portfolio" />
                        <CheckBoxField boxLabel="Master Portfolio" />
                        <CheckBoxField boxLabel="Core Portfolio" />
                        <Container>
                            <Button text="Deny combinations" ui="decline" />
                            <Button text="Allow combinations" />
                        </Container>
                    </FormPanel>
                </Container>
            </Container>
        );
    }
}