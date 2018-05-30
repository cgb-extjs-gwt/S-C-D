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
import { connect } from 'react-redux';
import { PageCommonState, PageState, PAGE_STATE_KEY } from '../../Layout/States/PageStates';
import { CostElementInput } from '../States/CostElementState';
import { get } from '../Services/CostElementService';
import { actionBuilder, selectApplication, selectScope, init, selectCostBlock } from '../Actions/InputCostElementActions';
import data from '../../Test/Home/data';
import { NamedId } from '../../Common/States/NamedId';
import { SelectList } from '../../Common/States/SelectList';
import { CostBlockInput } from '../States/CostBlock';
import { CostBlock as CostBlockComp } from './CostBlocks'

export interface CostElementsDispatch {
    onInit();
    onApplicationSelected(aplicationId: string);
    onScopeSelected(scopeId: string);
    onCostBlockSelected(costBlockId: string);
}

export interface CostElementsProps extends CostElementInput, CostElementsDispatch {
    
}

export class CostElementsInput extends React.Component<CostElementsProps> {
    constructor(props: CostElementsProps){
        super(props);
        props.onInit();
    }

    public render() {
        const { applications, scopes, costBlocks } = this.props;

        return (
            <Container layout="vbox">
                <FormPanel defaults={{labelAlign: 'left'}}>
                    {this.applicationCombobox(applications)}

                    <ContainerField label="Scope" layout={{type: 'vbox', align: 'left'}}>
                        { scopes && scopes.list.map(item => this.scopeRadioFild(item, scopes.selectedItemId))}
                    </ContainerField>
                </FormPanel>

                <Panel title="Cost Blocks:">
                    {
                        costBlocks && 
                        costBlocks.list && 
                        <FixedTabPanel 
                            tabBar={{
                                layout: { pack: 'left' }
                            }}
                            activeTab={
                                costBlocks.selectedItemId && 
                                costBlocks.list.findIndex(item => item.id === costBlocks.selectedItemId)
                            }
                            onActiveItemChange={this.onActiveTabChange}
                        >
                            {costBlocks.list.map(item => this.costBlockTab(item, costBlocks.selectedItemId))}

                            {/* <Container title="Tab 1">
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
                            <Container title="Tab 2" data={{id: 'tab2'}}>
                                This is content for Tab 2!
                            </Container>
                            <Container title="Tab 3">
                                This is content for Tab 3!
                            </Container> */}
                        </FixedTabPanel>
                    }
                </Panel>
            </Container>
        );
    }

    private onActiveTabChange = (tabPanel, newValue, oldValue) => {
        const activeTabIndex = tabPanel.getActiveItemIndex();
        const selectedCostBlockId = this.props.costBlocks.list[activeTabIndex].id;

        this.props.onCostBlockSelected(selectedCostBlockId);
    }

    private applicationCombobox(application: SelectList<NamedId>) {
        const applicatonStore = Ext.create('Ext.data.Store', {
            data: application && application.list
        });

        const selectedApplication = 
            applicatonStore.getData()
                           .findBy(item => (item.data as NamedId).id === application.selectedItemId);

        return (
            <ComboBoxField 
                label="Application"
                width="25%"
                displayField="name"
                valueField="id"
                queryMode="local"
                store={applicatonStore}
                selection={selectedApplication}
                onChange={(combobox, newValue, oldValue) => this.props.onApplicationSelected(newValue)}
            />
        );
    }

    private scopeRadioFild(scopeItem: NamedId, selectedScopeId: string) {
        return (
            <RadioField 
                key={scopeItem.id} 
                boxLabel={scopeItem.name} 
                name="scope" 
                checked={scopeItem.id === selectedScopeId}
            />
        );
    }

    private costBlockTab(costBlock: CostBlockInput, selectedCostBlockId: string) {
        const { countries } = this.props;

        return (
            <Container key={costBlock.id} title={costBlock.name}>
                {
                    costBlock.id === selectedCostBlockId && costBlock.costElements != null
                        ? <CostBlockComp costBlock={costBlock} countries={countries} onCountrySelected={() => ''}/>
                        : null
                }
            </Container>
        );
    }
}

export const CostElementsInputContainer = connect<{},CostElementsDispatch,{},{}>(
    state => (state[PAGE_STATE_KEY] as PageState).data,
    dispatch => ({
        onInit: () => dispatch(init()),
        onApplicationSelected: applicationId => dispatch(selectApplication(applicationId)),
        onScopeSelected: scopeId => dispatch(selectScope(scopeId)),
        onCostBlockSelected: costBlockId => dispatch(selectCostBlock(costBlockId)),
    })
)(CostElementsInput);