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
import { selectApplication, selectScope, init, selectCostBlock, loseChanges, hideDataLoseWarning, selectApplicationLosseDataCheck, selectScopeLosseDataCheck } from '../Actions/InputCostElementActions';
import { NamedId } from '../../Common/States/NamedId';
import { SelectList } from '../../Common/States/SelectList';
import { CostBlockInputState, EditItem, CheckItem, Filter } from '../States/CostBlock';
import { CostBlock as CostBlockComp, CostBlockProps } from './CostBlocks'
import { selectCountry, selectCostElement, selectInputLevel, getFilterItemsByCustomElementSelection, getFilterItemsByInputLevelSelection, reloadFilterBySelectedCountry, changeSelectionCostElementFilter, changeSelectionInputLevelFilter, resetCostElementFilter, resetInputLevelFilter, loadEditItemsByContext, clearEditItems, editItem, saveEditItemsToServer, selectCountryWithReloading, applyFiltersWithReloading } from '../Actions/CostBlockInputActions';

Ext.require('Ext.MessageBox');

export interface CostElementActions {
    onInit?: () => void;
    onApplicationSelected?: (applicationId: string) => void;
    onScopeSelected?: (scopeId: string) => void;
    onCostBlockSelected?: (costBlockId: string) => void;
    onLoseChanges?: () => void
    onCancelDataLose?: () => void
    tabActions: {
        onCountrySelected?: (countryId: string, costBlockId: string) => void
        onCostElementSelected?: (costBlockId: string, costElementId: string) => void
        onInputLevelSelected?: (costBlockId: string, inputLevelId: string) => void
        onCostElementFilterSelectionChanged?: (
            costBlockId: string,
            costElementId: string, 
            filterItemId: string,
            isSelected: boolean) => void
        onInputLevelFilterSelectionChanged?: (
            costBlockId: string,
            inputLevelId: string, 
            filterItemId: string,
            isSelected: boolean) => void
        onCostElementFilterReseted?: (costBlockId: string, costElementId: string) => void
        onInputLevelFilterReseted?: (costBlockId: string, inputLevelId: string) => void
        onEditItemsCleared?: (costBlockId: string) => void
        onItemEdited?: (costBlockId: string, item: EditItem) => void
        onEditItemsSaving?: (costBlockId: string) => void
        onApplyFilters?: (costBlockId: string) => void
    }
}

export interface CostBlockTab extends NamedId {
    costBlock: CostBlockProps
}

export interface CostElementsProps extends CostElementActions {
    application: SelectList<NamedId>
    scope: SelectList<NamedId>
    costBlocks: SelectList<CostBlockTab>
    isDataLossWarningDisplayed: boolean
}

export class CostElementsInput extends React.Component<CostElementsProps> {
    private isShownDataLossWarning = false;

    constructor(props: CostElementsProps){
        super(props);
        props.onInit && props.onInit();
    }

    public componentDidUpdate() {
        const { isDataLossWarningDisplayed } = this.props;

        if (isDataLossWarningDisplayed && !this.isShownDataLossWarning) {
            this.showDataLossWarning();
            this.isShownDataLossWarning = true;
        }
    }

    public render() {
        const { application, scope, costBlocks } = this.props;

        return (
            <Container layout="vbox">
                <FormPanel defaults={{labelAlign: 'left'}}>
                    {this.applicationCombobox(application)}
                    {this.scopeCombobox(scope)}

                    {/* <ContainerField label="Scope" layout={{type: 'vbox', align: 'left'}}>
                        { 
                            scope && 
                            scope.list && 
                            scope.list.map(item => this.scopeRadioFild(item, scope.selectedItemId))
                        }
                    </ContainerField> */}
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
                            onActiveItemChange={
                                (tabPanel, newValue, oldValue) => this.onActiveTabChange(tabPanel, newValue, oldValue)
                            }
                        >
                            {costBlocks.list.map(item => this.costBlockTab(item, costBlocks.selectedItemId))}
                        </FixedTabPanel>
                    }
                </Container>
            </Container>
        );
    }

    private onActiveTabChange = (tabPanel, newValue, oldValue) => {
        if (this.props.costBlocks.list && this.props.costBlocks.list.length > 0) {
            const { onCostBlockSelected } = this.props;

            const activeTabIndex = tabPanel.getActiveItemIndex();
            const selectedCostBlockId = this.props.costBlocks.list[activeTabIndex].id;

            onCostBlockSelected && onCostBlockSelected(selectedCostBlockId);
        }
    }

    private applicationCombobox(application: SelectList<NamedId>) {
        const { onApplicationSelected } = this.props;

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
                onChange={(combobox, newValue, oldValue) => 
                    onApplicationSelected && onApplicationSelected(newValue)
                }
            />
        );
    }

    // private scopeRadioFild(scopeItem: NamedId, selectedScopeId: string) {
    //     const { onScopeSelected } = this.props;

    //     return (
    //         <RadioField 
    //             key={scopeItem.id} 
    //             itemId={scopeItem.id}
    //             boxLabel={scopeItem.name} 
    //             name="scope" 
    //             checked={scopeItem.id === selectedScopeId}
    //             onCheck={() => onScopeSelected && onScopeSelected(scopeItem.id) }
    //         />
    //     );
    // }

    private scopeCombobox(scopes: SelectList<NamedId>) {
        const { onScopeSelected } = this.props;

        const scopeStore = Ext.create('Ext.data.Store', {
            data: scopes && scopes.list
        });

        const selectedScope = 
            scopeStore.getData()
                      .findBy(item => (item.data as NamedId).id === scopes.selectedItemId);

        return (
            <ComboBoxField 
                label="Scope"
                width="25%"
                displayField="name"
                valueField="id"
                queryMode="local"
                store={scopeStore}
                selection={selectedScope}
                onChange={(combobox, newValue, oldValue) => 
                    onScopeSelected && onScopeSelected(newValue)
                }
            />
        );
    }

    private costBlockTab(costBlockTab: CostBlockTab, selectedCostBlockId: string) {
        const { 
            onCountrySelected, 
            onCostElementSelected, 
            onInputLevelSelected,
            onCostElementFilterSelectionChanged,
            onInputLevelFilterSelectionChanged,
            onCostElementFilterReseted,
            onInputLevelFilterReseted,
            onEditItemsCleared,
            onItemEdited,
            onEditItemsSaving,
            onApplyFilters
        } = this.props.tabActions;

        return (
            <Container key={costBlockTab.id} title={costBlockTab.name}>
                <CostBlockComp 
                    {...costBlockTab.costBlock} 
                    onCountrySelected={
                        countryId => 
                            onCountrySelected && onCountrySelected(countryId, costBlockTab.id)
                    } 
                    onCostElementSelected={
                        costElementId => 
                            onCostElementSelected && onCostElementSelected(costBlockTab.id, costElementId)
                    }
                    onInputLevelSelected={
                        inputLevelId => 
                            onInputLevelSelected && onInputLevelSelected(costBlockTab.id, inputLevelId)
                    }
                    onCostElementFilterSelectionChanged={
                        (costElementId, filterItemId, isSelected) =>
                            onCostElementFilterSelectionChanged && 
                            onCostElementFilterSelectionChanged(costBlockTab.id, costElementId, filterItemId, isSelected)
                    }
                    onInputLevelFilterSelectionChanged={
                        (inputLevelId, filterItemId, isSelected) =>
                            onInputLevelFilterSelectionChanged && 
                            onInputLevelFilterSelectionChanged(costBlockTab.id, inputLevelId, filterItemId, isSelected)
                    }
                    onCostElementFilterReseted={
                        costElementId => 
                            onCostElementFilterReseted && 
                            onCostElementFilterReseted(costBlockTab.id, costElementId)
                    }
                    onInputLevelFilterReseted={
                        inputLevelId =>
                            onInputLevelFilterReseted && 
                            onInputLevelFilterReseted(costBlockTab.id, inputLevelId)
                    }
                    onEditItemsCleared={
                        () => onEditItemsCleared && onEditItemsCleared(costBlockTab.id)
                    }
                    onItemEdited={
                        item => onItemEdited && onItemEdited(costBlockTab.id, item)
                    }
                    onEditItemsSaving={
                        () => onEditItemsSaving && onEditItemsSaving(costBlockTab.id)
                    }
                    onApplyFilters={
                        () => onApplyFilters && onApplyFilters(costBlockTab.id)
                    }
                />
            </Container>
        );
    }

    private showDataLossWarning() {
        const { onLoseChanges, onCancelDataLose } = this.props;
        const me = this;

        const messageBox = Ext.Msg.confirm(
            'Warning', 
            'You have unsaved changes. If you continue, you will lose changes. Continue?',
            (buttonId: string) => {
                switch(buttonId) {
                    case 'yes':
                        onLoseChanges && onLoseChanges();
                        break;
                    case 'no':
                        onCancelDataLose && onCancelDataLose();
                        break;
                }

                me.isShownDataLossWarning = false
            }
        );
    }
}

const isSetContainsAllCheckedItems = (set: Set<string>, filterObj: Filter) => {
    let result = true;

    if (filterObj && filterObj.filter) {
        const checkedItems = filterObj.filter.filter(item => item.isChecked);

        result = checkedItems.length == set.size && checkedItems.every(item => set.has(item.id))
    } 

    return result
}

const costBlockTabListMap = (
    costBlockInput: CostBlockInputState, 
    countries: NamedId[], 
    costBlockMeta: CostBlockMeta,
    inputLevel:  Map<string, NamedId> 
): CostBlockTab => {
    const { edit } = costBlockInput;

    const costElementInput = 
        costBlockInput.costElement.list.find(
            item => item.costElementId === costBlockInput.costElement.selectedItemId);

    const selectedCostElementMeta = 
        costBlockMeta.costElements.find(
            item => item.id === costBlockInput.costElement.selectedItemId);

    const selectedInputLevelMeta = inputLevel.get(costBlockInput.inputLevel.selectedItemId);
    const selectedInputLevel = 
        costBlockInput.inputLevel.list && 
        costBlockInput.inputLevel.list.find(
            item => item.inputLevelId === costBlockInput.inputLevel.selectedItemId)
    
    const isEnableEditButtons = edit.editedItems && edit.editedItems.length > 0;
    const isEnableList = !edit.editedItems || edit.editedItems.length == 0;

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
                filterName: selectedCostElementMeta && 
                            selectedCostElementMeta.dependency && 
                            selectedCostElementMeta.dependency .name,
                description: selectedCostElementMeta && selectedCostElementMeta.description,
                isVisibleFilter: costElementInput && costElementInput.filter && costElementInput.filter.length > 0,
                isEnableList: isEnableList
            },
            inputLevel: {
                selectList: {
                    selectedItemId: costBlockInput.inputLevel.selectedItemId,
                    list: Array.from(inputLevel.values())
                },
                filter: selectedInputLevel && selectedInputLevel.filter,
                filterName: selectedInputLevelMeta && selectedInputLevelMeta.name,
                isVisibleFilter: selectedInputLevel && selectedInputLevel.filter && selectedInputLevel.filter.length > 0,
                isEnableList: isEnableList
            },
            edit: {
                nameColumnTitle: selectedInputLevelMeta && selectedInputLevelMeta.name,
                valueColumnTitle: selectedCostElementMeta && selectedCostElementMeta.name,
                isVisible: costBlockInput.costElement.selectedItemId != null && 
                           costBlockInput.inputLevel.selectedItemId !=null,
                items: edit.originalItems && edit.originalItems.map(originalItem => ({
                    ...edit.editedItems.find(editedItem => editedItem.id === originalItem.id) || originalItem
                })),
                isEnableClear: isEnableEditButtons,
                isEnableSave: isEnableEditButtons,
                isEnableApplyFilters: !isSetContainsAllCheckedItems(edit.appliedFilter.costElementsItemIds, costElementInput) ||
                                      !isSetContainsAllCheckedItems(edit.appliedFilter.inputLevelItemIds, selectedInputLevel)
            }
        }
    }
}

export const CostElementsInputContainer = connect<CostElementsProps,CostElementActions,{},PageCommonState<CostElementInputState>>(
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
            inputLevels,
            dataLossInfo,
        } = state.page.data;

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
            isDataLossWarningDisplayed: dataLossInfo.isWarningDisplayed,
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
        onApplicationSelected: applicationId => dispatch(selectApplicationLosseDataCheck(applicationId)),
        onScopeSelected: scopeId => dispatch(selectScopeLosseDataCheck(scopeId)),
        onCostBlockSelected: costBlockId => dispatch(selectCostBlock(costBlockId)),
        onLoseChanges: () => dispatch(loseChanges()),
        onCancelDataLose: () => dispatch(hideDataLoseWarning()),
        tabActions: {
            onCountrySelected: (countryId, costBlockId) => {
                dispatch(selectCountryWithReloading(costBlockId, countryId));
            },
            onCostElementSelected: (costBlockId, costElementId) => {
                dispatch(getFilterItemsByCustomElementSelection(costBlockId, costElementId));
                dispatch(loadEditItemsByContext());
            },
            onInputLevelSelected: (costBlockId, inputLevelId) => {
                dispatch(getFilterItemsByInputLevelSelection(costBlockId, inputLevelId));
                dispatch(loadEditItemsByContext());
            },
            onCostElementFilterSelectionChanged: (costBlockId, costElementId, filterItemId, isSelected) => {
                dispatch(changeSelectionCostElementFilter(costBlockId, costElementId, filterItemId, isSelected));
                //dispatch(loadEditItemsByContext());
            },
            onInputLevelFilterSelectionChanged: (costBlockId, inputLevelId, filterItemId, isSelected) => {
                dispatch(changeSelectionInputLevelFilter(costBlockId, inputLevelId, filterItemId, isSelected));
                //dispatch(loadEditItemsByContext());
            },
            onCostElementFilterReseted: (costBlockId, costElementId) => {
                dispatch(resetCostElementFilter(costBlockId, costElementId));
                dispatch(loadEditItemsByContext());
            },
            onInputLevelFilterReseted: (costBlockId, inputLevelId) => {
                dispatch(resetInputLevelFilter(costBlockId, inputLevelId))
                dispatch(loadEditItemsByContext());
            },
            onEditItemsCleared: costBlockId => dispatch(clearEditItems(costBlockId)),
            onItemEdited: (costBlockId, item) => dispatch(editItem(costBlockId, item)),
            onEditItemsSaving: costBlockId => dispatch(saveEditItemsToServer(costBlockId)),
            onApplyFilters: costBlockId => dispatch(applyFiltersWithReloading(costBlockId))
        }
    })
)(CostElementsInput);