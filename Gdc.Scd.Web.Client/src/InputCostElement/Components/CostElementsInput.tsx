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
import { CostElementInputState, CostBlockMeta } from '../States/CostElementState';
import { getCostElementInput } from '../Services/CostElementService';
import { selectApplication, selectScope, init, selectCostBlock } from '../Actions/InputCostElementActions';
import data from '../../Test/Home/data';
import { NamedId } from '../../Common/States/NamedId';
import { SelectList } from '../../Common/States/SelectList';
import { CostBlockInputState } from '../States/CostBlock';
import { CostBlock as CostBlockComp, CostBlockProps } from './CostBlocks'
import { selectCountry, selectCostElement, selectInputLevel, getFilterItemsByCustomElementSelection, getFilterItemsByInputLevelSelection } from '../Actions/CostBlockInputActions';

export interface CostElementActions {
    onInit();
    onApplicationSelected(aplicationId: string);
    onScopeSelected(scopeId: string);
    onCostBlockSelected(costBlockId: string);
    tabActions: {
        onCountrySelected(countryId: string, costBlockId: string)
        onCostElementSelected(costBlockId: string, costElementId: string)
        onInputLevelSelected(costBlockId: string, inputLevelId: string)
    }
}

export interface CostBlockTab extends NamedId {
    costBlock: CostBlockProps
}

export interface CostElementsProps {
    application: SelectList<NamedId>
    scope: SelectList<NamedId>
    costBlocks: SelectList<CostBlockTab>
}

export class CostElementsInput extends React.Component<CostElementsProps & CostElementActions> {
    constructor(props: CostElementsProps & CostElementActions){
        super(props);
        props.onInit();
    }

    public render() {
        const { application, scope, costBlocks } = this.props;

        return (
            <Container layout="vbox">
                <FormPanel defaults={{labelAlign: 'left'}}>
                    {this.applicationCombobox(application)}

                    <ContainerField label="Scope" layout={{type: 'vbox', align: 'left'}}>
                        { 
                            scope && 
                            scope.list && 
                            scope.list.map(item => this.scopeRadioFild(item, scope.selectedItemId))
                        }
                    </ContainerField>
                </FormPanel>

                <Container title="Cost Blocks:">
                    {
                        costBlocks && 
                        costBlocks.list &&
                        <FixedTabPanel 
                            tabBar={{
                                layout: { pack: 'left' }
                            }}
                            activeTab={
                                costBlocks.list.findIndex(costBlock => costBlock.id === costBlocks.selectedItemId)
                            }
                            onActiveItemChange={this.onActiveTabChange}
                        >
                            {costBlocks.list.map(item => this.costBlockTab(item, costBlocks.selectedItemId))}
                        </FixedTabPanel>
                    }
                </Container>
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
        const { onScopeSelected } = this.props;

        return (
            <RadioField 
                key={scopeItem.id} 
                boxLabel={scopeItem.name} 
                name="scope" 
                checked={scopeItem.id === selectedScopeId}
                onCheck={radioField => onScopeSelected(scopeItem.id)}
            />
        );
    }

    private costBlockTab(costBlockTab: CostBlockTab, selectedCostBlockId: string) {
        const { 
            onCountrySelected, 
            onCostElementSelected, 
            onInputLevelSelected 
        } = this.props.tabActions;

        return (
            <Container key={costBlockTab.id} title={costBlockTab.name}>
                <CostBlockComp 
                    {...costBlockTab.costBlock} 
                    onCountrySelected={countryId => onCountrySelected(countryId, costBlockTab.id)} 
                    onCostElementSelected={costElementId => onCostElementSelected(costBlockTab.id, costElementId)}
                    onInputLevelSelected={inputLevelId => onInputLevelSelected(costBlockTab.id, inputLevelId)}
                />
            </Container>
        );
    }
}

const costBlockTabListMap = (
    costBlockInput: CostBlockInputState, 
    countries: NamedId[], 
    costBlockMeta: CostBlockMeta,
    inputLevel:  Map<string, NamedId> 
): CostBlockTab => {
    const costElementInput = 
        costBlockInput.costElement.list.find(
            item => item.costElementId === costBlockInput.costElement.selectedItemId);

    const costElementMeta = 
        costBlockMeta.costElements.find(
            item => item.id === costBlockInput.costElement.selectedItemId);

    const selectedInputLevel = inputLevel.get(costBlockInput.inputLevel.selectedId);
    
    return {
        id: costBlockInput.costBlockId,
        name: costBlockMeta.name,
        costBlock: {
            country: {
                selectedItemId: costBlockInput.selectedCountryId,
                list: countries
            },
            costElement: {
                selectList: {
                    selectedItemId: costBlockInput.costElement.selectedItemId,
                    list: costBlockMeta.costElements.filter(
                        costElement => costBlockInput.visibleCostElementIds.includes(costElement.id))
                },
                filter: costElementInput && costElementInput.filter,
                filterName: costElementMeta && costElementMeta.dependency && costElementMeta.dependency .name,
                description: costElementMeta && costElementMeta.description,
                isVisibleFilter: costElementInput && costElementInput.filter && costElementInput.filter.length > 0
            },
            inputLevel: {
                selectList: {
                    selectedItemId: costBlockInput.inputLevel.selectedId,
                    list: Array.from(inputLevel.values())
                },
                filter: costElementInput && costElementInput.filter,
                filterName: selectedInputLevel && selectedInputLevel.name,
                isVisibleFilter: costElementInput && costElementInput.filter && costElementInput.filter.length > 0
            },
            editItems: []
        }
    }
}

export const CostElementsInputContainer = connect<CostElementsProps,CostElementActions,{},PageCommonState>(
    state => {
        const { 
            applications, 
            selectedApplicationId,
            scopes,  
            selectedScopeId,
            visibleCostBlockIds,
            selectedCostBlockId,
            costBlocksInputs,
            countries: countryMap,
            costBlockMetas,
            inputLevels
        } = state.page.data as CostElementInputState;

        const countryArray = countryMap && Array.from(countryMap.values()) || [];

        return {
            application: {
                selectedItemId: selectedApplicationId,
                list: applications && Array.from(applications.values())
            },
            scope: {
                selectedItemId: selectedScopeId,
                list: scopes && Array.from(scopes.values())
            },
            costBlocks: {
                selectedItemId: selectedCostBlockId,
                list: costBlocksInputs && 
                      costBlocksInputs.filter(costBlockInput => visibleCostBlockIds.includes(costBlockInput.costBlockId))
                                      .map(costBlockInput => 
                                        costBlockTabListMap(
                                            costBlockInput, 
                                            countryArray, 
                                            costBlockMetas.get(costBlockInput.costBlockId), 
                                            inputLevels))
            }
        } as CostElementsProps;
    },
    dispatch => ({
        onInit: () => dispatch(init()),
        onApplicationSelected: applicationId => dispatch(selectApplication(applicationId)),
        onScopeSelected: scopeId => dispatch(selectScope(scopeId)),
        onCostBlockSelected: costBlockId => dispatch(selectCostBlock(costBlockId)),
        tabActions: {
            onCountrySelected: (countryId, costBlockId) => dispatch(selectCountry(countryId, costBlockId)),
            onCostElementSelected: (costBlockId, costElementId) => dispatch(
                getFilterItemsByCustomElementSelection(costBlockId, costElementId)
            ),
            onInputLevelSelected: (costBlockId, inputLevelId) => dispatch(
                getFilterItemsByInputLevelSelection(costBlockId, inputLevelId)
            )
        }
    })
)(CostElementsInput);