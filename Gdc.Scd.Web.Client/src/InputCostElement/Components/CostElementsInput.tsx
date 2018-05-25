import * as React from 'react';
import { 
    Container, 
    FormPanel,
    ComboBoxField, 
    ContainerField, 
    RadioField, 
    Label,
    Panel,
    TabPanel
} from '@extjs/ext-react';
import FixedTabPanel from '../../Common/Components/FixedTabPanel';

export interface CostElementsProps {
    title: string
    isLoading: boolean
}

export class CostElementsInput extends React.Component<CostElementsProps> {
    public render() {
        const { isLoading } = this.props;

        return (
            <Container layout="vbox">
                <FormPanel defaults={{labelAlign: 'left'}}>
                    <ComboBoxField 
                        label="Application"
                        width="25%"/>

                    <ContainerField label="Scope" layout={{type: 'vbox', align: 'left'}}>
                        <RadioField boxLabel="Compact" name="priority"/>
                        <RadioField boxLabel="Mid-size" name="priority"/>
                        <RadioField boxLabel="SUV" name="priority"/>
                    </ContainerField>
                </FormPanel>

                <Panel title="Cost Blocks:">
                    <FixedTabPanel 
                        tabBar={{
                            layout: { pack: 'left' }
                        }}
                    >
                        <Container title="Tab 1">
                            <FormPanel defaults={{labelAlign: 'left'}}>
                                <ComboBoxField 
                                    label="Application"
                                    width="25%"/>

                                <ContainerField label="Scope" layout={{type: 'vbox', align: 'left'}}>
                                    <RadioField boxLabel="Compact" name="priority"/>
                                    <RadioField boxLabel="Mid-size" name="priority"/>
                                    <RadioField boxLabel="SUV" name="priority"/>
                                </ContainerField>
                            </FormPanel>
                        </Container>
                        <Container title="Tab 2">
                            This is content for Tab 2!
                        </Container>
                        <Container title="Tab 3">
                            This is content for Tab 3!
                        </Container>
                    </FixedTabPanel>
                </Panel>
            </Container>
        );
    }
}