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
import { CostElementInputState } from '../States/CostElementState';
import { get } from '../Services/CostElementService';
import { selectApplication, selectScope, init, selectCostBlock } from '../Actions/InputCostElementActions';
import data from '../../Test/Home/data';
import { NamedId } from '../../Common/States/NamedId';
import { SelectList } from '../../Common/States/SelectList';
import { CostBlockInputState } from '../States/CostBlock';
import { CostBlock as CostBlockComp } from './CostBlocks'

export interface CostElementsDispatch {
    onInit();
    onApplicationSelected(aplicationId: string);
    onScopeSelected(scopeId: string);
    onCostBlockSelected(costBlockId: string);
}

export interface CostElementsProps extends CostElementInputState, CostElementsDispatch {
    
}

export class CostElementsInput extends React.Component<CostElementsProps> {
    constructor(props: CostElementsProps){
        super(props);
        props.onInit();
    }

    public render() {
        const { 
            applications, 
            selectedApplicationId, 
            scopes, 
            selectedScopeId,
            visibleCostBlockIds,
            selectedCostBlockId,
            costBlocksInputs
        } = this.props;

        return (
            <Container layout="vbox">
                <FormPanel defaults={{labelAlign: 'left'}}>
                    {this.applicationCombobox(applications, selectedApplicationId)}

                    <ContainerField label="Scope" layout={{type: 'vbox', align: 'left'}}>
                        { 
                            scopes && 
                            Array.from(scopes.values())
                                 .map(item => this.scopeRadioFild(item, selectedScopeId))
                        }
                    </ContainerField>
                </FormPanel>

                <Panel title="Cost Blocks:">
                    {
                        <FixedTabPanel 
                            tabBar={{
                                layout: { pack: 'left' }
                            }}
                            activeTab={
                                visibleCostBlockIds && visibleCostBlockIds.indexOf(selectedCostBlockId) 
                            }
                            onActiveItemChange={this.onActiveTabChange}
                        >
                            { 
                                costBlocksInputs && 
                                Array.from(costBlocksInputs.values())
                                     .map(item => this.costBlockTab(item, selectedCostBlockId)) 
                            }

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
        const selectedCostBlockId = this.props.visibleCostBlockIds[activeTabIndex];

        this.props.onCostBlockSelected(selectedCostBlockId);
    }

    private applicationCombobox(applications: Map<string, NamedId>, selectedApplicationId: string) {
        const applicatonStore = Ext.create('Ext.data.Store', {
            data: applications && Array.from(applications.values())
        });

        const selectedApplication = 
            applicatonStore.getData()
                           .findBy(item => (item.data as NamedId).id === selectedApplicationId);

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

    private costBlockTab(costBlockInput: CostBlockInputState, selectedCostBlockId: string) {
        const { countries, costBlockMetas } = this.props;
        const costBlockMeta = costBlockMetas.get(costBlockInput.costBlockId);

        return (
            <Container key={costBlockInput.costBlockId} title={costBlockMeta.name}>
                {/* {
                    costBlockInput.costBlockId === selectedCostBlockId
                        ? <CostBlockComp costBlock={costBlockInput} countries={countries} onCountrySelected={() => ''}/>
                        : null
                } */}
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